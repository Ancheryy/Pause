using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public abstract class Singleton<T> where T : Singleton<T>
{
    private static T _instance;
    
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
