using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMgr : MonoSingleton<GameMgr>
{
    [SerializeField] public GameObject gameTips;
    [SerializeField] private Image background;
    
    protected override void Awake()
    {
        base.Awake();

        // LoadManager.Instance.LoadGameInitResources();
        EnterGame();
    }
    
    public void EnterGame()
    {
        MonoMgr.StartGlobalCoroutine(DoEnterGame());
    }

    IEnumerator DoEnterGame()
    {
        CanvasMgr.Instance.uiCanvas.gameObject.SetActive(true);
        // GameObject gameTips = GameManager.Instance.gameTips;
        GameObject menu = MenuMgr.Instance.menu;

        gameTips.SetActive(true);
        menu.SetActive(false);
        gameTips.GetComponent<UIFade>().FadeIn(0.8f);

        yield return new WaitForSeconds(3.0f);
        background.gameObject.SetActive(true);
        gameTips.GetComponent<UIFade>().FadeOut(0.8f);

        yield return new WaitForSeconds(1.0f);
        gameTips.SetActive(false);
        menu.SetActive(true);
        menu.GetComponent<UIFade>().FadeIn(0.8f);
    }


    public void ExitGame()
    {
        DoExitGame();
    }

    public void DoExitGame()
    {
        // SaveManager.Instance.SaveGameOnExit();

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    
}
