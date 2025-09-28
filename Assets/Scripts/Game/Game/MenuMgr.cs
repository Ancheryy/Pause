using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuMgr : MonoSingleton<MenuMgr>
{
    public Button startButton;
    public GameObject menu;
    
    private MenuMgr() { }

    public void Start()
    {
        
    }
    
    // 点击游戏开始
    // 淡出菜单界面，关闭当前 MenuScene（LoadScene 方法自动实现），加载新关卡
    public void OnClickGameStart()
    {
        Canvas uiCanvas = GameMgr.Instance.UICanvas;
        
        AnimSequence anim = AnimMgr.Instance.CreateSequence();
        anim.AddNode(() =>
        {
            startButton.GetComponent<UIFade>().FadeOut(0.4f);
            startButton.interactable = false;
            uiCanvas.GetComponent<UIFade>().FadeOut(1.0f);
        }).AddWait(1.0f).AddNode(() =>
        {
            uiCanvas.gameObject.SetActive(false);
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
    }

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
