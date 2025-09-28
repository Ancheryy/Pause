using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMgr : MonoSingleton<GameMgr>
{
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private GameObject gameTips;
    [SerializeField] private Image background;
    
    public Canvas UICanvas => uiCanvas;

    private IDisposable _subscription1;

    // 全局游戏初始化入口，在 Awake() 前调用，且自动调用
    [RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
        FlowController.CreateChapters();
    }
    
    
    protected override void Awake()
    {
        base.Awake();

        // GameMgr 为继承了 MonoBehaviour 的懒汉式单例类，只适合用于初始化游戏进程中 逻辑层 的相关数据
        _subscription1 = EventCenter.Subscribe<EnterGameEvent>(ShowGameTips);
        
    }

    private void Start()
    {
        EventCenter.Publish(new EnterGameEvent());
    }
    
    public void ShowGameTips(EnterGameEvent evt)
    {
        GameObject menu = MenuMgr.Instance.menu;
        
        AnimSequence anim = AnimMgr.Instance.CreateSequence();
        anim.AddNode(() =>
        {
            uiCanvas.gameObject.SetActive(true);
            gameTips.SetActive(true);
            menu.SetActive(false);
            gameTips.GetComponent<UIFade>().FadeIn(0.8f);
        }).AddWait(3.0f).AddNode(() =>
        {
            background.gameObject.SetActive(true);
            gameTips.GetComponent<UIFade>().FadeOut(0.8f);
        }).AddWait(1.0f).AddNode(() =>
        {
            gameTips.SetActive(false);
            menu.SetActive(true);
            menu.GetComponent<UIFade>().FadeIn(0.8f);
            _subscription1.Dispose();
            _subscription1 = null;
        }).AddWait(0.8f).AddNode(() =>
        {
            // 恢复菜单按键功能
            MenuMgr.Instance.startButton.gameObject.SetActive(true);
            MenuMgr.Instance.startButton.interactable = true;
            MenuMgr.Instance.startButton.GetComponent<CanvasGroup>().alpha = 1f;
        });
        
        anim.Play();
    }


    public void ExitGame()
    {
        DoExitGame();
    }

    private void DoExitGame()
    {
        // SaveManager.Instance.SaveGameOnExit();

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // 游戏开始事件
    public class EnterGameEvent : EventCenter.IEvent
    {
        
    }
}
