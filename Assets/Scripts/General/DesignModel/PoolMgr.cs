using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 缓存池（对象池）管理器
/// </summary>
public class PoolMgr : Singleton<PoolMgr>
{
    private readonly Dictionary<string, PoolData> _poolDic = new Dictionary<string, PoolData>();

    private GameObject _poolObj;

    // 是否开启 hierarchy 窗口布局功能
    private static bool IsOpenLayout => true;
    
    private PoolMgr() { }

    /// <summary>
    /// 获取对象
    /// </summary>
    /// <param name="poolName">所取对象名称</param>
    /// <param name="maxNum">最大限制数量</param>
    /// <returns>对象池中取得（或创建）的对象</returns>
    public GameObject GetObj(string poolName, int maxNum = 50)
    {
        if(_poolObj == null && IsOpenLayout)
            _poolObj = new GameObject(name: "Pool");
        
        GameObject obj = null;

        #region 加入了上限后的逻辑
        
        if (!_poolDic.ContainsKey(poolName) || (_poolDic[poolName].Count == 0 && _poolDic[poolName].UsingCount < maxNum))
        {
            obj = GameObject.Instantiate(Resources.Load<GameObject>(poolName));
            obj.name = poolName;
            
            if(!_poolDic.ContainsKey(poolName))
                _poolDic.Add(poolName, new PoolData(_poolObj, obj.name, obj));
            else
            {
                _poolDic[poolName].PushUsingList(obj);
            }
        }
        else if (_poolDic[poolName].Count > 0 || _poolDic[poolName].UsingCount >= maxNum)
        {
            obj = _poolDic[poolName].Pop();
        }
        
        #endregion
        
        #region 没有加入上限时的逻辑
        
        // if (_poolDic.ContainsKey(poolName) && _poolDic[poolName].Count > 0)
        // {
        //     obj = _poolDic[poolName].Pop();
        // }
        // else
        // {
        //     obj = GameObject.Instantiate(Resources.Load<GameObject>(poolName));
        //     obj.name = poolName;
        // }
        
        #endregion
        
        return obj;
    }

    /// <summary>
    /// 压入对象
    /// </summary>
    /// <param name="poolName">压入对象名称</param>
    /// <param name="obj">压入对象</param>
    public void PushObj(string poolName, GameObject obj)
    {
        if(_poolObj == null && IsOpenLayout)
            _poolObj = new GameObject(name: "Pool");
        
        obj.SetActive(false);
        // if (!_poolDic.ContainsKey(poolName))
        // {
        //     _poolDic.Add(poolName, new PoolData(_poolObj, $"{poolName} data"));
        // }
        _poolDic[poolName].Push(obj);
        obj.transform.SetParent(IsOpenLayout ? _poolObj.transform : null);
    }

    /// <summary>
    /// 压入对象（默认名称）
    /// </summary>
    /// <param name="obj">压入对象</param>
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
        private readonly Stack<GameObject> _availableObjs = new Stack<GameObject>();
        
        private readonly LinkedList<GameObject> _usingObjs = new LinkedList<GameObject>();
        
        private readonly GameObject _rootObj;
        
        public int Count => _availableObjs.Count;
        public int UsingCount => _usingObjs.Count;

        /// <summary>
        /// 创建抽屉，同时关联其父对象
        /// </summary>
        /// <param name="poolObj">对象池父物体</param>
        /// <param name="rootName">柜子名称</param>
        /// <param name="usedObj">该柜子中第一个使用中的物体</param>
        public PoolData(GameObject poolObj, string rootName, GameObject usedObj)
        {
            if (IsOpenLayout)
            {
                _rootObj = new GameObject(name: rootName);
                _rootObj.transform.SetParent(poolObj.transform);
            }
            
            _usingObjs.AddLast(usedObj);
        }

        /// <summary>
        /// 数据对象压入
        /// </summary>
        /// <param name="obj">压入对象</param>
        public void Push(GameObject obj)
        {
            obj.SetActive(false);
            if (IsOpenLayout)
            {
                obj.transform.SetParent(_rootObj.transform);
            }
            _availableObjs.Push(obj);
            _usingObjs.Remove(obj);
        }

        /// <summary>
        /// 使用中对象压入
        /// </summary>
        /// <param name="obj">压入使用中对象</param>
        public void PushUsingList(GameObject obj)
        {
            obj.SetActive(false);
            if (IsOpenLayout)
            {
                obj.transform.SetParent(_rootObj.transform);
            }
            _usingObjs.AddLast(obj);
        }

        /// <summary>
        /// 数据对象弹出
        /// </summary>
        /// <returns>所需对象</returns>
        public GameObject Pop()
        {
            GameObject obj;
            
            if(Count > 0)
                obj = _availableObjs.Pop();
            else
            {
                obj = _usingObjs.First.Value;
                _usingObjs.RemoveFirst();
            }
            _usingObjs.AddLast(obj);
                    
            obj.SetActive(true);
            if (IsOpenLayout)
            {
                obj.transform.SetParent(null);
            }
            
            return obj;
        }
    }
}
