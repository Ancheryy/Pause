using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuMgr : MonoSingleton<MenuMgr>
{
    public Button startButton;
    public GameObject menu;
    
    private MenuMgr() { }
    
    public void OnClickGameStart()
    {
        MonoMgr.StartGlobalCoroutine(DoStartGame());
    }

    public void OnClickSetting()
    {

    }

    public void OnClickAbout()
    {

    }



    IEnumerator DoStartGame()
    {
        Canvas gameCanvas = CanvasMgr.Instance.GameCanvas;
        Canvas uiCanvas = CanvasMgr.Instance.UICanvas;
        
        startButton.GetComponent<UIFade>().FadeOut(0.4f);

        // 2. 进行Canvas间的切换
        startButton.interactable = false;
        uiCanvas.GetComponent<UIFade>().FadeOut(1.0f);

        yield return new WaitForSeconds(1.0f);
        uiCanvas.gameObject.SetActive(false);
        gameCanvas.gameObject.SetActive(true);
        gameCanvas.GetComponent<UIFade>().FadeIn(0.8f);

        // 3. 等待章节标题显现，然后进入当前章节游戏
        yield return new WaitForSeconds(1.0f);
        yield return ChapterMgr.Instance.DoShowTitle();

        yield return new WaitForSeconds(3.0f);
        startButton.gameObject.SetActive(true);
        startButton.interactable = true;
        startButton.GetComponent<CanvasGroup>().alpha = 1f;
    }

    public void ResetMenuCanvas()
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
        MonoMgr.StartGlobalCoroutine(DoShowMenu());
    }

    IEnumerator DoShowMenu()
    {
        GameObject menuScene = MenuMgr.Instance.menu;
        Canvas menuCanvas = CanvasMgr.Instance.uiCanvas;
        // 重新设置按键等状态
        ResetMenuCanvas();

        CanvasMgr.Instance.GameCanvas.GetComponent<UIFade>().FadeOut(0.8f);

        yield return new WaitForSeconds(1.0f);
        CanvasMgr.Instance.GameCanvas.gameObject.SetActive(false);
        menuCanvas.gameObject.SetActive(true);
        menuCanvas.enabled = true;
        menuScene.SetActive(true);
        menuCanvas.GetComponent<UIFade>().FadeIn(0.8f);
        menuScene.GetComponent<UIFade>().FadeIn(0.8f);

        yield return new WaitForSeconds(1.0f);
        menuScene.GetComponent<CanvasGroup>().alpha = 1f;
    }
}
