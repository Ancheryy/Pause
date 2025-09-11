using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MonoMgr : MonoSingleton<MonoMgr>
{
    private event UnityAction UpdateEvent;
    private event UnityAction FixedUpdateEvent;
    private event UnityAction LateUpdateEvent;

    /// <summary>
    /// 添加 Update 帧更新监听函数
    /// </summary>
    /// <param name="action"></param>
    public void AddUpdateListener(UnityAction action)
    {
        UpdateEvent += action;
    }

    /// <summary>
    /// 移除 Update 帧更新监听函数
    /// </summary>
    /// <param name="action"></param>
    public void RemoveUpdateListener(UnityAction action)
    {
        UpdateEvent -= action;
    }
    
    /// <summary>
    /// 添加 FixedUpdate 帧更新监听函数
    /// </summary>
    /// <param name="action"></param>
    public void AddFixedUpdateListener(UnityAction action)
    {
        FixedUpdateEvent += action;
    }

    /// <summary>
    /// 移除 FixedUpdate 帧更新监听函数
    /// </summary>
    /// <param name="action"></param>
    public void RemoveFixedUpdateListener(UnityAction action)
    {
        FixedUpdateEvent -= action;
    }
    
    /// <summary>
    /// 添加 LateUpdate 帧更新监听函数
    /// </summary>
    /// <param name="action"></param>
    public void AddLateUpdateListener(UnityAction action)
    {
        LateUpdateEvent += action;
    }

    /// <summary>
    /// 移除 LateUpdate 帧更新监听函数
    /// </summary>
    /// <param name="action"></param>
    public void RemoveLateUpdateListener(UnityAction action)
    {
        LateUpdateEvent -= action;
    }
    
    private void Update()
    {
        UpdateEvent?.Invoke();
    }
    
    private void FixedUpdate()
    {
        FixedUpdateEvent?.Invoke();
    }
    
    private void LateUpdate()
    {
        LateUpdateEvent?.Invoke();
    }

    /// <summary>
    /// 开启全局特定协程
    /// </summary>
    /// <param name="routine"></param>
    /// <returns></returns>
    public static Coroutine StartGlobalCoroutine(IEnumerator routine)
    {
        return Instance.StartCoroutine(Instance.WrapCoroutine(routine));
    }
    
    private IEnumerator WrapCoroutine(IEnumerator routine)
    {
        yield return routine;
    }

    /// <summary>
    /// 关闭全局特定协程
    /// </summary>
    /// <param name="coroutine"></param>
    public static void StopGlobalCoroutine(Coroutine coroutine)
    {
        if (coroutine != null)
        {
            Instance.StopCoroutine(coroutine);
        }
    }

    /// <summary>
    /// 停止所有全局协程
    /// </summary>
    public static void StopAllGlobalCoroutines()
    {
        Instance.StopAllCoroutines();
    }
}
