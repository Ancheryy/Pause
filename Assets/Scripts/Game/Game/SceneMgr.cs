using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneMgr : MonoSingleton<SceneMgr>
{
    private SceneInstance _currentScene;
    private static Image _uiMask;


    /// <summary>
    /// 加载关卡场景
    /// </summary>
    /// <param name="checkpointName"></param>
    /// <param name="callback"></param>
    /// <typeparam name="T">关卡加载事件的类型（比如：Checkpoint1_1.LoadCheckpoint1_1Event）</typeparam>
    public void LoadScene<T>(string checkpointName, Action callback = null) where T : EventCenter.IEvent, new()
    {
        AddressableMgr.LoadSceneAsync(checkpointName, LoadSceneMode.Single, true, (obj) =>
        {
            _currentScene = obj.Result;
            SceneInstance newScene = _currentScene;
            SceneManager.SetActiveScene(newScene.Scene);
            
            EventCenter.Publish<T>(new T());
            callback?.Invoke();
        });
    }
    
    /// <summary>
    /// 卸载关卡场景
    /// </summary>
    /// <param name="checkpointName"></param>
    /// <typeparam name="T"></typeparam>
    public void UnloadScene<T>(string checkpointName) where T : Checkpoint, new()
    {
        AddressableMgr.UnloadSceneAsync(checkpointName, (obj) =>
        {
            Debug.Log($"已经卸载 {checkpointName} 场景");
        });
    }
    
    
    
    
    public void EnterCheckpoint(Checkpoint checkpoint)
    {
        MonoMgr.StartGlobalCoroutine(DoEnterCheckpoint(checkpoint));
    }
    
    private IEnumerator DoEnterCheckpoint(Checkpoint checkpoint)
    {
        // 查找当前场景中的 UIMask，通过场景当中的物体 Tag 寻找
        GameObject uiMaskObj = GameObject.FindWithTag("UIMask");
        if (uiMaskObj == null)
        {
            // 如果没有找到，创建一个默认的
            uiMaskObj = CreateDefaultUIMask();
        }
        _uiMask = uiMaskObj.GetComponent<Image>();
        
        // 让 UI 遮罩不会影响关卡交互
        _uiMask.gameObject.SetActive(true);
        _uiMask.GetComponent<CanvasGroup>().interactable = false;
        _uiMask.GetComponent<CanvasGroup>().blocksRaycasts = false;
        // 入场策略执行
        yield return checkpoint.EnterStrategy.ExecuteEnter(_uiMask);
        _uiMask.gameObject.SetActive(false);
        
        // 发布结束入场事件（订阅方法：关卡开始时需要进行自启动的方法，比如开局显示蓝色手指提示）
        EventCenter.Publish(new EnterSceneCompleteEvent(checkpoint));
    }
    
    private GameObject CreateDefaultUIMask()
    {
        // 创建Canvas
        GameObject canvasObj = new GameObject("UIMaskCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
        canvas.sortingLayerName = "UI";
        canvas.sortingOrder = 999;
    
        // 创建Image
        GameObject imageObj = new GameObject("UIMask");
        imageObj.tag = "UIMask"; // 设置标签
        imageObj.transform.SetParent(canvasObj.transform);
        Image image = imageObj.AddComponent<Image>();
    
        // 设置Image属性
        image.color = Color.white;
        RectTransform rect = image.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    
        // 添加CanvasGroup
        CanvasGroup canvasGroup = imageObj.AddComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    
        return imageObj;
    }
    
    
    // 播放出场动效并销毁UI
    public void ExitLevel(Checkpoint checkpoint)
    {
        if (_uiMask == null)
        {
            // 发布结束退场事件（订阅方法：关卡结束后需要进行自销毁的方法，比如注销关卡加载的订阅，删除音乐等）
            EventCenter.Publish(new ExitCompleteEvent(checkpoint));
            return;
        }
        
        MonoMgr.StartGlobalCoroutine(DoExitLevel(checkpoint));
    }

    private IEnumerator DoExitLevel(Checkpoint checkpoint)
    {
        _uiMask.gameObject.SetActive(true);
        _uiMask.GetComponent<CanvasGroup>().interactable = true;
        _uiMask.GetComponent<CanvasGroup>().alpha = 0f;
        _uiMask.GetComponent<CanvasGroup>().blocksRaycasts = true;
        
        yield return checkpoint.ExitStrategy.ExecuteExit(_uiMask);
        // Destroy(_uiMask);   // 一般来说，跨场景后一定会销毁
        _uiMask = null;
        // 发布结束退场事件（订阅方法：关卡结束后需要进行自销毁的方法，比如注销关卡加载的订阅，删除音乐等）
        EventCenter.Publish(new ExitCompleteEvent(checkpoint));
    }
    
    

    // 关卡入场结束 事件
    public class EnterSceneCompleteEvent : EventCenter.IEvent
    {
        public Checkpoint TriggerCheckpoint;

        public EnterSceneCompleteEvent(Checkpoint triggerCheckpoint)
        {
            TriggerCheckpoint = triggerCheckpoint;
        }
    }
    // 关卡出场结束 事件
    public class ExitCompleteEvent : EventCenter.IEvent
    {
        public Checkpoint TriggerCheckpoint;

        public ExitCompleteEvent(Checkpoint triggerCheckpoint)
        {
            TriggerCheckpoint = triggerCheckpoint;
        }
    }
}
