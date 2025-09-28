using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 播片流程核心控制器 - 简化版
/// </summary>
public class AnimMgr : Singleton<AnimMgr>
{
    private const bool _enableLogging = false;

    private readonly List<AnimSequence> _activeSequences = new List<AnimSequence>();
    
    private AnimMgr() { }
    
    /// <summary>
    /// 创建并返回一个新的动画序列
    /// </summary>
    public AnimSequence CreateSequence(string sequenceName = "UnnamedSequence")
    {
        var sequence = new AnimSequence(sequenceName, this);
        _activeSequences.Add(sequence);
        return sequence;
    }
    
    /// <summary>
    /// 停止所有序列
    /// </summary>
    public void StopAllSequences()
    {
        for (int i = _activeSequences.Count - 1; i >= 0; i--)
        {
            _activeSequences[i].Stop();
        }
        _activeSequences.Clear();
    }
    
    /// <summary>
    /// 从活动序列列表中移除序列
    /// </summary>
    public void RemoveSequence(AnimSequence sequence)
    {
        _activeSequences.Remove(sequence);
    }
    
    public void Log(string message)
    {
        if (_enableLogging) Debug.Log($"[AnimMgr] {message}");
    }
}

/// <summary>
/// 动画序列状态 - 简化版
/// </summary>
public enum SequenceStatus
{
    Idle = 0,
    Playing = 1,
    Completed = 2
}

/// <summary>
/// 动画序列类 - 简化版
/// </summary>
public class AnimSequence
{
    public string Name { get; private set; }
    public SequenceStatus Status { get; private set; }
    public int CurrentNodeIndex { get; private set; }
    
    // 序列回调（一般用于衔接外部方法）
    public event Action OnSequenceStarted;
    public event Action OnSequenceCompleted;
    // 节点回调（一般用作每个节点结束后的检查或提示性方法）
    public event Action<string> OnNodeStarted; // 节点名称
    public event Action<string> OnNodeCompleted; // 节点名称
    
    private readonly List<IEnumerator> _nodeCoroutines = new List<IEnumerator>();
    private readonly List<string> _nodeNames = new List<string>();
    private Coroutine _executionCoroutine;
    private readonly AnimMgr _manager;

    public AnimSequence(string name, AnimMgr manager)
    {
        Name = name;
        Status = SequenceStatus.Idle;
        _manager = manager;
    }

    /// <summary>
    /// 添加动画节点
    /// </summary>
    public AnimSequence AddNode(IEnumerator animationCoroutine, string nodeName = "UnnamedNode")
    {
        _nodeCoroutines.Add(animationCoroutine);
        _nodeNames.Add(nodeName);
        return this;    // 这里方便递归调用
    }

    /// <summary>
    /// 添加动画节点（传入委托的方式）
    /// </summary>
    public AnimSequence AddNode(Action action, string nodeName = "UnnamedNode")
    {
        // 将 Action 包装成协程
        IEnumerator ActionCoroutine()
        {
            action.Invoke();
            yield break;
        }
    
        return AddNode(ActionCoroutine(), nodeName);
    }

    /// <summary>
    /// 添加等待节点
    /// </summary>
    public AnimSequence AddWait(float seconds, string nodeName = null)
    {
        return AddNode(WaitCoroutine(seconds), nodeName ?? $"Wait_{seconds}s");
    }

    /// <summary>
    /// 添加回调节点
    /// </summary>
    public AnimSequence AddCallback(Action callback, string nodeName = "Callback")
    {
        return AddNode(CallbackCoroutine(callback), nodeName);
    }

    /// <summary>
    /// 添加序列动画开始回调
    /// </summary>
    /// <param name="onSequenceStarted"></param>
    /// <returns></returns>
    public AnimSequence AddSequenceStartedAction(Action onSequenceStarted)
    {
        OnSequenceStarted += onSequenceStarted;
        return this;
    }

    /// <summary>
    /// 添加动画序列结束回调
    /// </summary>
    /// <param name="onSequenceCompleted"></param>
    /// <returns></returns>
    public AnimSequence AddSequenceCompletedAction(Action onSequenceCompleted)
    {
        OnSequenceCompleted += onSequenceCompleted;
        return this;
    }

    /// <summary>
    /// 添加结点开始回调
    /// </summary>
    /// <param name="onNodeStarted"></param>
    /// <returns></returns>
    public AnimSequence AddNodeStartedAction(Action<string> onNodeStarted)
    {
        OnNodeStarted += onNodeStarted;
        return this;
    }

    /// <summary>
    /// 添加结点结束回调
    /// </summary>
    /// <param name="onNodeCompleted"></param>
    /// <returns></returns>
    public AnimSequence AddNodeCompletedAction(Action<string> onNodeCompleted)
    {
        OnNodeCompleted += onNodeCompleted;
        return this;
    }

    /// <summary>
    /// 开始播放序列
    /// </summary>
    public void Play()
    {
        if (Status == SequenceStatus.Playing)
        {
            Debug.LogWarning($"序列 '{Name}' 正在播放中");
            return;
        }

        if (_nodeCoroutines.Count == 0)
        {
            Debug.LogWarning($"序列 '{Name}' 没有可播放的节点");
            return;
        }

        Status = SequenceStatus.Playing;
        CurrentNodeIndex = 0;
        _executionCoroutine = MonoMgr.StartGlobalCoroutine(ExecuteSequence());
        
        OnSequenceStarted?.Invoke();
        _manager.Log($"序列 '{Name}' 开始播放");
    }

    /// <summary>
    /// 停止序列
    /// </summary>
    public void Stop()
    {
        if (Status != SequenceStatus.Playing)
            return;

        if (_executionCoroutine != null)
        {
            MonoMgr.StopGlobalCoroutine(_executionCoroutine);
            _executionCoroutine = null;
        }

        Status = SequenceStatus.Completed;
        _manager.RemoveSequence(this);
        _manager.Log($"序列 '{Name}' 已停止");
    }

    /// <summary>
    /// 执行序列的主协程
    /// </summary>
    private IEnumerator ExecuteSequence()
    {
        for (CurrentNodeIndex = 0; CurrentNodeIndex < _nodeCoroutines.Count; CurrentNodeIndex++)
        {
            yield return PlayNode(CurrentNodeIndex);
        }

        CompleteSequence();
    }

    /// <summary>
    /// 播放单个节点
    /// </summary>
    private IEnumerator PlayNode(int index)
    {
        string nodeName = _nodeNames[index];
        OnNodeStarted?.Invoke(nodeName);
        _manager.Log($"开始播放节点 [{index}] {nodeName}");

        // 直接执行节点协程
        yield return MonoMgr.StartGlobalCoroutine(_nodeCoroutines[index]);

        OnNodeCompleted?.Invoke(nodeName);
        _manager.Log($"完成播放节点 [{index}] {nodeName}");
    }

    /// <summary>
    /// 完成序列
    /// </summary>
    private void CompleteSequence()
    {
        Status = SequenceStatus.Completed;
        _executionCoroutine = null;
        
        OnSequenceCompleted?.Invoke();
        _manager.RemoveSequence(this);
        _manager.Log($"序列 '{Name}' 播放完成");
    }

    /// <summary>
    /// 等待协程
    /// </summary>
    private IEnumerator WaitCoroutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    /// <summary>
    /// 回调协程
    /// </summary>
    private IEnumerator CallbackCoroutine(Action callback)
    {
        callback?.Invoke();
        yield return null;
    }
}