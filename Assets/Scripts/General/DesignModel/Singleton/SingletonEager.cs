using System;
using System.Reflection;
using UnityEngine;

// 饿汉式单例模板 - 在类加载时就创建实例
public abstract class SingletonEager<T> where T : SingletonEager<T>
{
    // 在静态构造函数中立即创建实例
    private static readonly T _instance = CreateInstance();
    
    // 为子类提供一个保证线程安全的锁
    protected static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            return _instance;
        }
    }

    // 静态构造函数，确保线程安全地创建实例
    static SingletonEager()
    {
        // 静态字段初始化已经是线程安全的，这里不需要额外锁
    }

    // 私有方法用于创建实例
    private static T CreateInstance()
    {
        Type type = typeof(T);
        ConstructorInfo constructor = type.GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance, 
            null, 
            Type.EmptyTypes, 
            null);
            
        if (constructor != null)
        {
            return constructor.Invoke(null) as T;
        }
        else
        {
            Debug.LogError($"Can't find non-public parameterless constructor for {typeof(T)}");
            return null;
        }
    }

    // 保护的无参构造函数，防止外部实例化
    protected SingletonEager()
    {
        // 防止重复创建检查
        if (_instance != null)
        {
            throw new Exception($"Singleton instance of {typeof(T)} already exists!");
        }
        
        Initialize();
    }

    // 可选的初始化方法，子类可以重写
    protected virtual void Initialize()
    {
        // 子类可以在这里进行初始化操作
    }
}