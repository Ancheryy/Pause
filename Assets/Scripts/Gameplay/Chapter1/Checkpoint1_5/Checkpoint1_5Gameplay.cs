using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Checkpoint1_5Gameplay : PrefabSingleton<Checkpoint1_5Gameplay>
{
    [SerializeField] private bool isDebug;

    [Header("ParentGameObject")]
    [SerializeField] private GameObject checkpointGameObject;
    
    [Header("Checkpoint1_5——UI")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private Image uiMask;
    [SerializeField] private Canvas separateCanvas;
    [SerializeField] private Image separateLayer;
    
    [Header("Checkpoint1_5——GameObject")] 
    [SerializeField] private GameObject background;
    
    [Header("Part1")] 
    [SerializeField] private GameObject part1;
    [SerializeField] private GameObject wholeBg;
    [SerializeField] private GameObject thermometerHot;
    [SerializeField] private GameObject stuffExpressions_go;
    [SerializeField] private GameObject stuff1_go;
    [SerializeField] private GameObject stuff2_go;
    [SerializeField] private GameObject stuff3_go;
    [SerializeField] private GameObject girlMovements_go;
    [SerializeField] private GameObject girl1_go;
    [SerializeField] private GameObject girl2_go;
    [SerializeField] private GameObject girl3_go;
    
    [Header("Part2")] 
    [SerializeField] private GameObject part2;
    [SerializeField] private GameObject bubblePaper_new;
    [SerializeField] private GameObject girlGiveStuffBubblePaper;
    [SerializeField] private GameObject stuffHot;
    [SerializeField] private GameObject stuffCool;
    
    [Header("Part3")] 
    [SerializeField] private GameObject part3;
    [SerializeField] private GameObject server_go;
    [SerializeField] private GameObject server1_go;
    [SerializeField] private GameObject server2_go;
    [SerializeField] private GameObject server3_go;
    [SerializeField] private GameObject server4_go;
    [SerializeField] private GameObject book;
    [SerializeField] private Animator bookHint;
    [SerializeField] private AnimationClip bookHintClip;
    // [SerializeField] private GameObject fingerHint;
    
    [SerializeField] private GameObject openBook;
    [SerializeField] private GameObject stuffAndBubblePaper_go;
    [SerializeField] private GameObject stuffIconPart3;
    [SerializeField] private GameObject bubblePaperIconPart3;
    [SerializeField] private GameObject buildConnection_go;
    [SerializeField] private GameObject fatherIcon;
    [SerializeField] private GameObject whiteLine;
    [SerializeField] private GameObject girlIcon;
    [SerializeField] private GameObject blueLine;
    
    [Header("Animations")] 
    [SerializeField] private Animator girlAndStuff;
    [SerializeField] private AnimationClip girlAndStuffClip;
    
    [SerializeField] private Animator stuff1;
    [SerializeField] private AnimationClip stuff1Clip;
    [SerializeField] private Animator stuff2;
    [SerializeField] private AnimationClip stuff2Clip;
    [SerializeField] private Animator stuff3;
    [SerializeField] private AnimationClip stuff3Clip;
    
    [SerializeField] private Animator girl1;
    [SerializeField] private AnimationClip girl1Clip;
    [SerializeField] private Animator girl2;
    [SerializeField] private AnimationClip girl2Clip;
    [SerializeField] private Animator girl3;
    [SerializeField] private AnimationClip girl3Clip;
    
    [SerializeField] private Animator server1;
    [SerializeField] private AnimationClip server1Clip;
    [SerializeField] private Animator server2;
    [SerializeField] private AnimationClip server2Clip;
    [SerializeField] private Animator server3;
    [SerializeField] private AnimationClip server3Clip;
    [SerializeField] private Animator server4;
    [SerializeField] private AnimationClip server4Clip;
    
    private AnimBound AB_girlAndStuff;
    private AnimBound AB_stuff1;
    private AnimBound AB_stuff2;
    private AnimBound AB_stuff3;
    private AnimBound AB_girl1;
    private AnimBound AB_girl2;
    private AnimBound AB_girl3;
    private AnimBound AB_server1;
    private AnimBound AB_server2;
    private AnimBound AB_server3;
    private AnimBound AB_server4;
    private AnimBound AB_bookHint;
    private AnimBound AB_fingerHint;

    // part2部分按下的泡泡数量
    private int count = 0;
    private bool _isPart2Passed = false;
    private bool _needHint_part2 = true;
    private bool isPassed = false;
    private List<IDisposable> _subscriptions;
    
    
    
    protected override void Awake()
    {
        base.Awake();
        
        InitGameplay();
    }
    
    void Start()
    {
        if (isDebug)
        {
            // 主动触发原本应该在 SceneMgr.LoadScene() 方法中触发的 两个 事件
            EventCenter.Publish(new SceneMgr.EndLoadSceneEvent());
            EventCenter.Publish(new Checkpoint1_5.LoadCheckpoint1_5Event());
        }
    }

    void Update()
    {
        
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        foreach (var subscription in _subscriptions)
        {
            subscription.Dispose();
        }
        _subscriptions.Clear();
    }

    private void InitGameplay()
    {
        checkpointGameObject.SetActive(true);
        
        // Checkpoint1_5 UI 初始化
        uiCanvas.gameObject.SetActive(true);

        // Checkpoint1_5 GameObject 初始化
        background.gameObject.SetActive(true);
        
        // Part1 初始化
        part1.SetActive(true);
        wholeBg.SetActive(true);
        thermometerHot.SetActive(false);
        stuffExpressions_go.SetActive(false);
        stuff1_go.SetActive(false);
        stuff2_go.SetActive(false);
        stuff3_go.SetActive(false);
        girlMovements_go.SetActive(false);
        girl1_go.SetActive(false);
        girl2_go.SetActive(false);
        girl3_go.SetActive(false);
        AB_girlAndStuff = new AnimBound(girlAndStuff, girlAndStuffClip);
        AB_stuff1 = new AnimBound(stuff1, stuff1Clip);
        AB_stuff2 = new AnimBound(stuff2, stuff2Clip);
        AB_stuff3 = new AnimBound(stuff3, stuff3Clip);
        AB_girl1 = new AnimBound(girl1, girl1Clip);
        AB_girl2 = new AnimBound(girl2, girl2Clip);
        AB_girl3 = new AnimBound(girl3, girl3Clip);
        AB_girlAndStuff.ResetAnim();
        AB_stuff1.ResetAnim();
        AB_stuff2.ResetAnim();
        AB_stuff3.ResetAnim();
        AB_girl1.ResetAnim();
        AB_girl2.ResetAnim();
        AB_girl3.ResetAnim();
        AB_girlAndStuff.SetEnable(false);
        AB_stuff1.SetEnable(false);
        AB_stuff2.SetEnable(false);
        AB_stuff3.SetEnable(false);
        AB_girl1.SetEnable(false);
        AB_girl2.SetEnable(false);
        AB_girl3.SetEnable(false);
        
        // Part2 初始化
        part2.SetActive(false);
        bubblePaper_new.gameObject.SetActive(true);
        stuffHot.SetActive(true);
        stuffCool.SetActive(false);
        
        // Part3 初始化
        part3.SetActive(false);
        server_go.SetActive(false);
        server1_go.SetActive(false);
        server2_go.SetActive(false);
        server3_go.SetActive(false);
        server4_go.SetActive(false);
        bookHint.gameObject.SetActive(false);
        AB_server1 = new AnimBound(server1, server1Clip);
        AB_server2 = new AnimBound(server2, server2Clip);
        AB_server3 = new AnimBound(server3, server3Clip);
        AB_server4 = new AnimBound(server4, server4Clip);
        AB_bookHint = new AnimBound(bookHint, bookHintClip);
        AB_server1.ResetAnim();
        AB_server2.ResetAnim();
        AB_server3.ResetAnim();
        AB_server4.ResetAnim();
        AB_bookHint.ResetAnim();
        AB_server1.SetEnable(false);
        AB_server2.SetEnable(false);
        AB_server3.SetEnable(false);
        AB_server4.SetEnable(false);
        AB_bookHint.SetEnable(false);
        
        stuffAndBubblePaper_go.SetActive(false);
        openBook.SetActive(false);
        stuffIconPart3.SetActive(false);
        bubblePaperIconPart3.SetActive(false);
        buildConnection_go.SetActive(false);
        fatherIcon.SetActive(false);
        whiteLine.SetActive(false);
        girlIcon.SetActive(false);
        blueLine.SetActive(false);
        
        // 其他初始化
        count = 0;
        isPassed = false;
        _needHint_part2 = true;
        _subscriptions = new List<IDisposable>();
        _subscriptions.Add(EventCenter.Subscribe<SceneMgr.EnterStrategyCompleteEvent>(ShowStuffWorking));
    }
    
    // -------- Part 1 方法集 --------
    // 镜头摇到右边
    private void ShowStuffWorking(SceneMgr.EnterStrategyCompleteEvent evt)
    {
        if (evt.TriggerCheckpoint != null && evt.TriggerCheckpoint.ID != 105)
        {
            return;
        }

        if (evt.TriggerCheckpoint == null && !isDebug)
        {
            return;
        }
        
        AnimSequence anim = AnimMgr.Instance.CreateSequence();
        anim.AddNode(() =>
            {
                AB_girlAndStuff.PlayAnim();
            })
            .AddWait(AB_girlAndStuff.GetPlayTime())
            .AddNode(() =>
            {
                Debug.Log("镜头移动结束");
                thermometerHot.SetActive(true);
                thermometerHot.GetComponent<SpriteFade>().FadeIn(0.5f);
            })
            .AddWait(2.3f)
            .AddNode(() =>
            {
                uiMask.gameObject.SetActive(true);
                uiMask.GetComponent<UIFade>().FadeIn(0.5f);
            })
            .AddWait(1.3f)
            .AddNode(() =>
            {
                // wholeBg.gameObject.SetActive(false);
                thermometerHot.SetActive(false);
                stuffExpressions_go.SetActive(true);
                uiMask.GetComponent<UIFade>().FadeOut(0.5f);
            })
            .AddWait(1.3f)
            .AddNode(() =>
            {
                AB_stuff1.PlayAnim();
            })
            .AddWait(AB_stuff1.GetPlayTime() + 0.5f)
            .AddNode(() =>
            {
                AB_stuff2.PlayAnim();
            })
            .AddWait(AB_stuff2.GetPlayTime() + 0.5f)
            .AddNode(() =>
            {
                AB_stuff3.PlayAnim();
            })
            .AddWait(AB_stuff3.GetPlayTime() + 2.3f)
            .AddNode(() =>
            {
                AB_stuff1.SetEnable(false);
                AB_stuff2.SetEnable(false);
                AB_stuff3.SetEnable(false);
                stuff1_go.GetComponent<SpriteFade>().FadeOut(0.5f);
                stuff2_go.GetComponent<SpriteFade>().FadeOut(0.5f);
                stuff3_go.GetComponent<SpriteFade>().FadeOut(0.5f);
            })
            .AddWait(1.0f)
            .AddNode(() =>
            {
                stuffExpressions_go.gameObject.SetActive(false);
                girlMovements_go.gameObject.SetActive(true);
                AB_girl1.PlayAnim();
            })
            .AddWait(AB_girl1.GetPlayTime() + 0.5f)
            .AddNode(() =>
            {
                AB_girl2.PlayAnim();
            })
            .AddWait(AB_girl2.GetPlayTime() + 0.5f)
            .AddNode(() =>
            {
                AB_girl3.PlayAnim();
            })
            .AddWait(AB_girl3.GetPlayTime() + 2.3f)
            .AddNode(() =>
            {
                AB_girl1.SetEnable(false);
                AB_girl2.SetEnable(false);
                AB_girl3.SetEnable(false);
                uiMask.GetComponent<UIFade>().FadeIn(0.5f);
            })
            .AddWait(1.3f)
            .AddNode(() =>
            {
                part1.SetActive(false);
                part2.SetActive(true);
                girlGiveStuffBubblePaper.SetActive(true);
                stuffHot.SetActive(false);
                stuffCool.SetActive(false);
                uiMask.GetComponent<UIFade>().FadeOut(0.5f);
            });

        anim.Play();
        
    }
    
    public void PressBubble()
    {
        count++;
        MonoMgr.StartGlobalCoroutine(DoPlayBubbleSFX());
        if (count >= 10 && !_isPart2Passed)
        {
            _isPart2Passed = true;
            bubblePaper_new.GetComponent<BubblePaperCutter>().EnableCutting(false);
            PassPart2();
        }
        
        IEnumerator DoPlayBubbleSFX()
        {
            // SoundManager.Instance.PlaySFX("Audio/Chapter1/1_5_bubblePaper/bubble-wrap-pop-87928", true, false);

            yield return new WaitForSeconds(1.5f);
            // SoundManager.Instance.StopSFX("Audio/Chapter1/1_5_bubblePaper/bubble-wrap-pop-87928");
        }
    }
    
    public void PassPart2()
    {
        Debug.Log("Part2 完成，进入 Part3");
        
        AnimSequence anim = AnimMgr.Instance.CreateSequence();
        anim.AddWait(0.5f)
            .AddNode(() =>
            {
                uiMask.gameObject.SetActive(true);
                uiMask.GetComponent<CanvasGroup>().alpha = 0;
                uiMask.GetComponent<CanvasGroup>().blocksRaycasts = true;
                girlGiveStuffBubblePaper.GetComponent<SpriteFade>().FadeOut(0.5f);
            })
            .AddWait(0.8f)
            .AddNode(() =>
            {
                girlGiveStuffBubblePaper.SetActive(false);
                stuffHot.SetActive(true);
                stuffHot.GetComponent<SpriteFade>().FadeIn(0.5f);
            })
            .AddWait(2.3f)
            .AddNode(() =>
            {
                stuffHot.GetComponent<SpriteFade>().FadeOut(0.5f);
            })
            .AddWait(0.8f)
            .AddNode(() =>
            {
                stuffHot.SetActive(false);
                stuffCool.SetActive(true);
                stuffCool.GetComponent<SpriteFade>().FadeIn(0.5f);
            })
            .AddWait(2.3f).AddNode(() =>
            {
                uiMask.GetComponent<UIFade>().FadeIn(0.5f);
            })
            .AddWait(1.3f)
            .AddNode(() =>
            {
                part2.SetActive(false);
                part3.SetActive(true);
                
                server_go.SetActive(true);
                server1_go.SetActive(false);
                server2_go.SetActive(false);
                server3_go.SetActive(false);
                server4_go.SetActive(false);
                
                uiMask.GetComponent<UIFade>().FadeOut(0.5f);
            })
            .AddWait(1.3f)
            .AddNode(() =>
            {
                uiMask.GetComponent<CanvasGroup>().blocksRaycasts = false;
                AB_server1.PlayAnim();
            })
            .AddWait(AB_server1.GetPlayTime() + 0.5f)
            .AddNode(() =>
            {
                AB_server2.PlayAnim();
            })
            .AddWait(AB_server2.GetPlayTime() + 0.5f)
            .AddNode(() =>
            {
                AB_server3.PlayAnim();
            })
            .AddWait(AB_server3.GetPlayTime() + 0.5f)
            .AddNode(() =>
            {
                book.GetComponent<SpriteButton>().SetInteractable(false);
                AB_server4.PlayAnim();
            })
            .AddWait(AB_server4.GetPlayTime())
            .AddNode(() =>
            {
                book.GetComponent<SpriteButton>().SetInteractable(true);
            })
            .AddWait(2.3f)
            .AddNode(JudgeHintOrNot());

        anim.Play();

        IEnumerator JudgeHintOrNot()
        {
            if (!_needHint_part2) yield break;

            bookHint.gameObject.SetActive(true);
            // fingerHint.gameObject.SetActive(true);
            AB_bookHint.PlayAnim();
            // AB_fingerHint.PlayAnim();
        }
    }

    public void OnClickBook()
    {
        _needHint_part2 = false;
        book.GetComponent<SpriteButton>().SetInteractable(false);

        AB_bookHint.StopAnim();
        // AB_fingerHint.StopAnim();

        OpenBook();
    }

    private void OpenBook()
    {
        AnimSequence anim = AnimMgr.Instance.CreateSequence();
        anim.AddNode(() =>
            {
                isPassed = true;
                separateLayer.gameObject.SetActive(true);
                separateLayer.GetComponent<UIFade>().FadeFromTo(0f, 0.3f, 0.8f);
            })
            .AddWait(0.5f)
            .AddNode(() =>
            {
                stuffAndBubblePaper_go.SetActive(true);
                openBook.SetActive(true);
                openBook.GetComponent<SpriteFade>().FadeIn(0.5f); // 此处应该是播放翻书动画
            })
            .AddWait(1.8f, "显现书中内容（打工人 + 泡泡板）")
            .AddNode(() =>
            {
                stuffIconPart3.gameObject.SetActive(true);
                bubblePaperIconPart3.gameObject.SetActive(true);
                stuffIconPart3.GetComponent<SpriteFade>().FadeIn(0.5f);
                bubblePaperIconPart3.GetComponent<SpriteFade>().FadeIn(0.5f);
            })
            .AddWait(3.3f, "书中内容（打工人 + 泡泡板）淡出")
            .AddNode(() =>
            {
                stuffIconPart3.GetComponent<SpriteFade>().FadeOut(0.5f);
                bubblePaperIconPart3.GetComponent<SpriteFade>().FadeOut(0.5f);
            })
            .AddWait(1.3f, "buildConnection上的白线、父亲图标显现")
            .AddNode((() =>
            {
                buildConnection_go.gameObject.SetActive(true);
                fatherIcon.gameObject.SetActive(true);
                whiteLine.gameObject.SetActive(true);
                fatherIcon.GetComponent<SpriteFade>().FadeIn(0.8f);
                whiteLine.GetComponent<SpriteFade>().FadeIn(0.8f);
            }))
            .AddWait(1.8f, "蓝线、女孩图标显现，表示连接建立")
            .AddNode(() =>
            {
                girlIcon.gameObject.SetActive(true);
                blueLine.gameObject.SetActive(true);
                girlIcon.GetComponent<SpriteFade>().FadeIn(0.8f);
                blueLine.GetComponent<SpriteFade>().FadeIn(0.8f);
            })
            .AddWait(3.3f)
            .AddNode(() =>
            {
                uiMask.GetComponent<UIFade>().FadeIn(0.8f);
            })
            .AddWait(1.8f)
            .AddNode((() =>
            {
                EventCenter.Publish(new Checkpoint1_5.PassCheckpoint1_5Event());
            }));

        anim.Play();
    }
    
}
