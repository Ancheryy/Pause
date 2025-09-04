using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// 继承时需注意在单例类中添加 无参构造函数
public abstract class Singleton<T> where T : Singleton<T>
{
    private static T _instance;

    // 为子类提供一个保证线程安全的锁
    protected static readonly object _lock = new object();
    
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        Type type = typeof(T);
                        ConstructorInfo constructor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,null,Type.EmptyTypes,null);
                        if (constructor != null)
                            _instance = constructor.Invoke(null) as T;
                        else
                            Debug.LogError($"Can't find constructor for {typeof(T)}");
                    }
                }
            }
            return _instance;
        }
    }
}
