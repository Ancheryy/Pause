using UnityEngine;

/// <summary>
/// 挂载在Prefab上的场景级单例基类
/// 必须主动挂载到Prefab上，随Prefab实例化时自动生成单例
/// </summary>
public abstract class PrefabSingleton<T> : MonoBehaviour where T : PrefabSingleton<T>
{
    private static T _instance;
    private static bool _isApplicationQuitting = false;

    public static T Instance
    {
        get
        {
            if (_isApplicationQuitting)
            {
                Debug.LogWarning($"单例[{typeof(T)}]已被销毁，应用程序正在退出");
                return null;
            }

            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    // 强制要求必须挂载在Prefab上
                    throw new System.InvalidOperationException(
                        $"未找到[{typeof(T)}]的实例！请确保该单例已挂载到场景Prefab上");
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            // 不跨场景保留（与MonoSingleton不同）
        }
        else if (_instance != this)
        {
            Debug.LogError($"检测到重复的单例[{typeof(T)}]，已销毁：{gameObject.name}");
            DestroyImmediate(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void OnApplicationQuit()
    {
        _isApplicationQuitting = true;
    }
}