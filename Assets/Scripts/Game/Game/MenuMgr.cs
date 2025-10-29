using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MenuMgr : MonoSingleton<MenuMgr>
{
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private GameObject gameTips;
    [SerializeField] private Image background;
    
    [SerializeField] public Button startButton;
    [SerializeField] public GameObject menu;
    
    public Canvas MenuCanvas => menuCanvas;
    
    private IDisposable _subscription1;
    
    private MenuMgr() { }

    // 全局游戏初始化入口，在 Awake() 前调用，且自动调用
    [RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
        // FlowController.CreateChapters();
    }
    
    protected override void Awake()
    {
        base.Awake();
        
        _subscription1 = EventCenter.Subscribe<EnterGameEvent>(ShowGameTips);
    }

    void Start()
    {
        EventCenter.Publish(new EnterGameEvent());
    }
    
    // 显示游戏提示
    public void ShowGameTips(EnterGameEvent evt)
    {
        GameObject menu = MenuMgr.Instance.menu;
        
        AnimSequence anim = AnimMgr.Instance.CreateSequence();
        anim.AddNode(() =>
        {
            menuCanvas.gameObject.SetActive(true);
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
    
    // 点击游戏开始
    // 淡出菜单界面，关闭当前 MenuScene（LoadScene 方法自动实现），加载新关卡
    public void OnClickGameStart()
    {
        startButton.interactable = false;
        
        AnimSequence anim = AnimMgr.Instance.CreateSequence();
        anim.AddNode(() =>
        {
            startButton.GetComponent<UIFade>().FadeOut(0.4f);
            menuCanvas.GetComponent<UIFade>().FadeOut(1.0f);
        }).AddWait(1.0f).AddNode(() =>
        {
            menuCanvas.gameObject.SetActive(false);
            // 这里的章节过场应该与每个章节第一关结合在一块，不再单独显现
            SceneMgr.Instance.LoadScene<Checkpoint1_1.LoadCheckpoint1_1Event>("Checkpoint1_1"); // 或者说加载之前的存档记录
            // 这里应该先出现章节过场，然后再加载关卡场景
        });
        
        anim.Play();
    }

    public void OnClickSetting()
    {

    }

    public void OnClickAbout()
    {

    }
    
    

    private void ResetMenuCanvas()
    {
        startButton.gameObject.SetActive(true);
        startButton.interactable = true;
        startButton.GetComponent<CanvasGroup>().alpha = 1f;
    }
    
    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void BackToMenu()
    {
        // MonoMgr.StartGlobalCoroutine(DoShowMenu());
        
        // IEnumerator DoShowMenu()
        // {
        //     GameObject menuScene = MenuMgr.Instance.menu;
        //     Canvas menuCanvas = CanvasMgr.Instance.uiCanvas;
        //     // 重新设置按键等状态
        //     ResetMenuCanvas();
        //
        //     CanvasMgr.Instance.GameCanvas.GetComponent<UIFade>().FadeOut(0.8f);
        //
        //     yield return new WaitForSeconds(1.0f);
        //     CanvasMgr.Instance.GameCanvas.gameObject.SetActive(false);
        //     menuCanvas.gameObject.SetActive(true);
        //     menuCanvas.enabled = true;
        //     menuScene.SetActive(true);
        //     menuCanvas.GetComponent<UIFade>().FadeIn(0.8f);
        //     menuScene.GetComponent<UIFade>().FadeIn(0.8f);
        //
        //     yield return new WaitForSeconds(1.0f);
        //     menuScene.GetComponent<CanvasGroup>().alpha = 1f;
        // }
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
