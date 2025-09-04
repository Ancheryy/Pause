using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

public class ResMgr
{
    private static Dictionary<string, ResInfoBase> _loadedResDic = new Dictionary<string, ResInfoBase>();
    
    /// <summary>
    /// 同步加载资源
    /// </summary>
    /// <param name="path">资源路径</param>
    /// <typeparam name="T">加载资源的类型</typeparam>
    /// <returns>返回所需资源</returns>
    public static T Load<T>(string path) where T : UnityEngine.Object
    {
        string resName = path + "_" + typeof(T).Name;
        ResInfo<T> info;
        
        if (!_loadedResDic.ContainsKey(resName))
        {
            T resObj = Resources.Load<T>(resName);
            info = new ResInfo<T>();
            info.Asset = resObj;
            _loadedResDic.Add(resName, info);
        }
        else
        {
            info = _loadedResDic[resName] as ResInfo<T>;
            if (info.Asset == null)
            {
                MonoMgr.StopGlobalCoroutine(info.LoadAsyncCoroutine);
                info.Asset = Resources.Load<T>(resName);
                info.Callback?.Invoke(info.Asset);
                info.Callback = null;
                info.LoadAsyncCoroutine = null;
            }
        }
        
        // 增加引用计数
        info.AddRefCount();
        return info.Asset;
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <param name="path">资源路径</param>
    /// <param name="callback">加载完成后触发的回调函数，通过其参数传出资源</param>
    /// <typeparam name="T">传出（加载）资源的类型</typeparam>
    public static void LoadAsync<T>(string path, UnityAction<T> callback) where T : UnityEngine.Object
    {
        string resPath = path + "_" + typeof(T).Name;
        ResInfo<T> info;

        if (!_loadedResDic.ContainsKey(resPath))
        {
            info = new ResInfo<T>();
            _loadedResDic.Add(resPath, info);
            info.Callback += callback;
            info.LoadAsyncCoroutine = MonoMgr.StartGlobalCoroutine(DoLoadAsync<T>(path));
        }
        else
        {
            info = (ResInfo<T>)_loadedResDic[resPath];
            if (info.Asset == null)
            {
                // 没有加载完时，添加委托，以待后续统一调用
                info.Callback += callback;
            }
            else
            {
                // 资源已经加载完成，直接调用回调函数，传出资源
                callback(info.Asset);
            }
        }
        
        // 增加引用计数
        info.AddRefCount();
        
    }

    private static IEnumerator DoLoadAsync<T>(string path) where T : UnityEngine.Object
    {
        ResourceRequest request = Resources.LoadAsync(path);
        yield return request;
        
        string resPath = path + "_" + typeof(T).Name;
        if (_loadedResDic.ContainsKey(resPath))
        {
            // 取出资源信息，将加载过程中因重复调用而加入的callback及自身callback一同调用
            ResInfo<T> info = _loadedResDic[resPath] as ResInfo<T>;
            info.Asset = request.asset as T;

            // 如果发现加载过程中需要删除，则调用Unload方法进行资源移除
            if (info.RefCount == 0)
            {
                // 无回调函数，不减少计数
                Unload<T>(path, null, info.IsDel, false);
            }
            else
            {
                info.Callback?.Invoke(info.Asset);
                info.Callback = null;
                info.LoadAsyncCoroutine = null;
            }
        }
    }

    /// <summary>
    /// 卸载资源
    /// </summary>
    /// <param name="path">资源路径</param>
    /// <param name="callback">想要移除的回调函数</param>
    /// <param name="isDel">是否从内存中移除资源</param>
    /// <param name="isSubRef">[主要供内部使用]是否减少引用计数</param>
    /// <typeparam name="T">资源类型</typeparam>
    public static void Unload<T>(string path, UnityAction<T> callback = null, bool isDel = false, bool isSubRef = true) where T : UnityEngine.Object
    {
        string resPath = path + "_" + typeof(T).Name;
        // 判断是否存在资源
        if (!_loadedResDic.ContainsKey(resPath))
        {
            ResInfo<T> info = _loadedResDic[resPath] as ResInfo<T>;
            // 减少引用计数
            if(isSubRef)
                info!.SubRefCount();
            // 记录引用计数为0时是否移除
            info.IsDel = isDel;
            // 资源已经加载完成
            if (info.Asset != null && info.RefCount == 0 && info.IsDel)
            {
                _loadedResDic.Remove(resPath);
                Resources.UnloadAsset(info.Asset);
            }
            // 资源正在加载中（不确定加载到什么程度）
            else if (info.Asset == null)
            {
                // 直接删除（可能出错）
                // MonoMgr.StopGlobalCoroutine(info.LoadAsyncCoroutine);
                // _loadedResDic.Remove(resPath);
                
                // 无引用计数：采用标记清除
                // info.IsDel = true;
                
                // 有引用计数：清除传入回调
                if(callback != null)
                    info.Callback -= callback;
            }
        }
    }

    public void UnloadUnused(UnityAction callback)
    {
        MonoMgr.StartGlobalCoroutine(DoUnloadUnused(callback));
    }

    private IEnumerator DoUnloadUnused(UnityAction callback)
    {
        List<string> list = new List<string>();
        foreach (var pair in _loadedResDic)
        {
            if(pair.Value.RefCount == 0)
                list.Add(pair.Key);
        }

        foreach (var pair in list)
        {
            _loadedResDic.Remove(pair);
        }
        
        AsyncOperation ao = Resources.UnloadUnusedAssets();
        yield return ao;
        // 卸载完成后通知外部
        callback?.Invoke();
    }
    

    public static int GetRefCount<T>(string path)
    {
        string resPath = path + "_" + typeof(T).Name;
        if (_loadedResDic.ContainsKey(resPath))
        {
            return ((ResInfo<T>)_loadedResDic[resPath]).RefCount;
        }
        return 0;
    }



    private abstract class ResInfoBase
    {
        // 引用计数
        private int _refCount;

        public int RefCount => _refCount;

        public void AddRefCount()
        {
            _refCount++;
        }

        public void SubRefCount()
        {
            _refCount--;
            if (_refCount < 0)
            {
                Debug.LogError("RefCount 小于 0，检查引用计数是否成对使用");
            }
        }
    }

    private class ResInfo<T> : ResInfoBase
    {
        // 资源
        public T Asset;
        // 主要用于异步加载之后，传递资源到外部的委托
        public UnityAction<T> Callback;
        // 存储异步加载时开启的协程
        public Coroutine LoadAsyncCoroutine;
        // 是否需要被真正移除
        public bool IsDel = false;
    } 
}
