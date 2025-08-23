using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 缓存池（对象池）管理器
/// </summary>
public class PoolMgr : Singleton<PoolMgr>
{
    private Dictionary<string, PoolData> _poolDic = new Dictionary<string, PoolData>();

    private GameObject _poolObj;

    // 是否开启 hierarchy 窗口布局功能
    private static bool IsOpenLayout => true;
    
    private PoolMgr() { }
    
    /// <summary>
    /// 获取对象
    /// </summary>
    /// <param name="poolName">所取对象名称</param>
    /// <returns>对象池中取得（或创建）的对象</returns>
    public GameObject GetObj(string poolName)
    {
        GameObject obj = null;
        if (_poolDic.ContainsKey(poolName) && _poolDic[poolName].Count > 0)
        {
            obj = _poolDic[poolName].Pop();
        }
        else
        {
            obj = GameObject.Instantiate(Resources.Load<GameObject>(poolName));
            obj.name = poolName;
        }
        
        return obj;
    }

    /// <summary>
    /// 释放对象
    /// </summary>
    /// <param name="poolName">释放对象名称</param>
    /// <param name="obj">释放对象</param>
    public void PushObj(string poolName, GameObject obj)
    {
        if(_poolObj == null && IsOpenLayout)
            _poolObj = new GameObject(name: "Pool");
        
        obj.SetActive(false);
        if (!_poolDic.ContainsKey(poolName))
        {
            _poolDic.Add(poolName, new PoolData(_poolObj, $"{poolName} data"));
        }
        _poolDic[poolName].Push(obj);
        obj.transform.SetParent(IsOpenLayout ? _poolObj.transform : null);
    }

    /// <summary>
    /// 释放对象（默认名称）
    /// </summary>
    /// <param name="obj">释放对象</param>
    public void PushObj(GameObject obj)
    {
        PushObj(obj.name, obj);
    }

    /// <summary>
    /// 清除对应类型抽屉
    /// </summary>
    /// <param name="poolName">对象所属抽屉名称</param>
    public void ClearPool(string poolName)
    {
        _poolDic.Remove(poolName);
    }

    /// <summary>
    /// 清除所有对象类型抽屉
    /// </summary>
    public void ClearAllPools()
    {
        _poolDic.Clear();
    }


    /// <summary>
    /// 抽屉类
    /// </summary>
    private class PoolData
    {
        private Stack<GameObject> _objs = new Stack<GameObject>();
        private GameObject _rootObj;
        
        public int Count => _objs.Count;

        /// <summary>
        /// 创建抽屉，同时关联其父对象
        /// </summary>
        /// <param name="poolObj">对象池父物体</param>
        /// <param name="rootName">柜子名称</param>
        public PoolData(GameObject poolObj, string rootName)
        {
            if (IsOpenLayout)
            {
                _rootObj = new GameObject(name: rootName);
                _rootObj.transform.SetParent(poolObj.transform);
            }
        }

        /// <summary>
        /// 压入数据对象
        /// </summary>
        /// <param name="obj">压入对象</param>
        public void Push(GameObject obj)
        {
            obj.SetActive(false);
            if (IsOpenLayout)
            {
                obj.transform.SetParent(_rootObj.transform);
            }
            _objs.Push(obj);
        }

        /// <summary>
        /// 弹出数据对象
        /// </summary>
        /// <returns>所需对象</returns>
        public GameObject Pop()
        {
            GameObject obj = _objs.Pop();
            obj.SetActive(true);
            if (IsOpenLayout)
            {
                obj.transform.SetParent(null);
            }
            
            return obj;
        }
    }
}
