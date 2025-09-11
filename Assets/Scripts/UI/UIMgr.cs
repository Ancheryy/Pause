using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1. 用作实例化每关 UI
// 2. 同时实现每关 UI 的进入与退出动效
public class UIMgr : MonoSingleton<UIMgr>
{
    private static GameObject _currentUI;


    public void EnterCheckpoint(GameObject uiPrefab, IEnterStrategy enterStrategy, Action onInitCheckpoint, Action onComplete = null)
    {
        StartCoroutine(DoEnterCheckpoint(uiPrefab, enterStrategy, onInitCheckpoint, onComplete));
    }
    
    private static IEnumerator DoEnterCheckpoint(GameObject uiPrefab, IEnterStrategy enterStrategy, Action onInitCheckpoint, Action onComplete = null)
    {
        // 销毁旧 UI（如果有）
        if (_currentUI != null)
        {
            Destroy(_currentUI);
        }

        // 实例化新 UI
        _currentUI = Instantiate(uiPrefab, CanvasMgr.Instance.GameCanvas.transform);

        // 初始化关卡（包含 各游戏物体状态初始化 + SFX 加载 等）
        onInitCheckpoint?.Invoke();
        
        // 入场策略执行
        _currentUI.GetComponent<CanvasGroup>().interactable = false;
        yield return enterStrategy.ExecuteEnter(_currentUI);
        _currentUI.GetComponent<CanvasGroup>().interactable = true;
        
        // 入场结束触发回调（如 蓝色手指提示）
        onComplete?.Invoke();
    }
    
    
    // 播放出场动效并销毁UI
    public void ExitLevel(IExitStrategy exitStrategy, Action onComplete = null)
    {
        if (_currentUI == null)
        {
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(DoExitLevel(exitStrategy, onComplete));
    }

    private IEnumerator DoExitLevel(IExitStrategy exitStrategy, Action onComplete)
    {
        yield return exitStrategy.ExecuteExit(_currentUI);
        Destroy(_currentUI);
        _currentUI = null;
        onComplete?.Invoke();
    }

    // 获取当前UI中的特定元素（供策略使用）
    public T GetUIComponent<T>(string path) where T : Component
    {
        Transform target = _currentUI.transform.Find(path);
        return target != null ? target.GetComponent<T>() : null;
    }
}
