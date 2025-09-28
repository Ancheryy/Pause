using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class AddressableMgr : MonoSingleton<AddressableMgr>
{
    private static Dictionary<string, IEnumerator> resDic = new Dictionary<string, IEnumerator>();

    private static Dictionary<string, AsyncOperationHandle<SceneInstance>> sceneHandles = new Dictionary<string, AsyncOperationHandle<SceneInstance>>();
    
    // 直接通过资源的名称（ key ）进行加载
    public static void LoadAssetAsync<T>(string name, Action<AsyncOperationHandle<T>> callback)
    {
        string resName = name + "_" + typeof(T).Name;
        AsyncOperationHandle<T> handle;
        if (resDic.ContainsKey(resName))
        {
            handle = (AsyncOperationHandle<T>)resDic[resName];

            if (handle.IsDone)
            {
                callback(handle);
            }
            else
            {
                handle.Completed += (obj) =>
                {
                    if (obj.Status == AsyncOperationStatus.Succeeded)
                        callback(obj);
                };
            }

            return;
        }
        
        handle = Addressables.LoadAssetAsync<T>(resName);
        handle.Completed += (obj) =>
        {
            if (obj.Status == AsyncOperationStatus.Succeeded)
                callback(obj);
            else
            {
                Debug.LogWarning(resName + " 资源加载失败");
                if(resDic.ContainsKey(resName))
                    resDic.Remove(resName);
            }
        };
        resDic[resName] = handle;
    }

    // 加载场景方法（通过 callback 获取加载后的场景 SceneInstance）
    public static void LoadSceneAsync(string sceneName, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, Action<AsyncOperationHandle<SceneInstance>> callback = null)
    {
        // 检查是否已经在加载或已加载
        if (sceneHandles.ContainsKey(sceneName))
        {
            var handle = sceneHandles[sceneName];
            if (handle.IsDone)
            {
                callback?.Invoke(handle);
            }
            else
            {
                handle.Completed += (obj) =>
                {
                    if (obj.Status == AsyncOperationStatus.Succeeded)
                        callback?.Invoke(obj);
                };
            }
            return;
        }

        // 开始加载场景（当参数 loadMode = LoadSceneMode.Single，activateOnLoad = true 的情况下，场景默认激活）
        var sceneHandle = Addressables.LoadSceneAsync(sceneName, loadMode, activateOnLoad);
        sceneHandle.Completed += (obj) =>
        {
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                callback?.Invoke(obj);
            }
            else
            {
                Debug.LogWarning(sceneName + " 场景加载失败");
                if (sceneHandles.ContainsKey(sceneName))
                    sceneHandles.Remove(sceneName);
            }
        };
        
        sceneHandles[sceneName] = sceneHandle;
    }

    // 卸载场景方法
    public static void UnloadSceneAsync(string sceneName, Action<AsyncOperationHandle<SceneInstance>> callback = null)
    {
        if (sceneHandles.ContainsKey(sceneName))
        {
            var handle = sceneHandles[sceneName];
            var unloadHandle = Addressables.UnloadSceneAsync(handle);
            
            unloadHandle.Completed += (obj) =>
            {
                if (obj.Status == AsyncOperationStatus.Succeeded)
                {
                    sceneHandles.Remove(sceneName);
                    callback?.Invoke(obj);
                }
                else
                {
                    Debug.LogWarning(sceneName + " 场景卸载失败");
                }
            };
        }
        else
        {
            Debug.LogWarning("场景 " + sceneName + " 未被Addressables管理或已卸载");
        }
    }

    public static void ReleaseAsset<T>(string name)
    {
        string resName = name + "_" + typeof(T).Name;
        if (resDic.ContainsKey(resName))
        {
            AsyncOperationHandle<T> handle = (AsyncOperationHandle<T>)resDic[resName];
            Addressables.Release(handle);
            resDic.Remove(resName);
        }
    }

    public static void Clear()
    {
        resDic.Clear();
        AssetBundle.UnloadAllAssetBundles(true);
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }
    
}
