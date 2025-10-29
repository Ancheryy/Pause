using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Checkpoint1_1Gameplay : PrefabSingleton<Checkpoint1_1Gameplay>
{
    [Header("ParentGameObject")]
    [SerializeField] private GameObject checkpointGameObject;
    [SerializeField] private GameObject seasonsGameObject;
    [SerializeField] private GameObject targetZonesGameObject;
    [SerializeField] private bool isDebug;
    
    [Header("ChapterCanvas——UI")]
    [SerializeField] private GameObject chapterCanvas;
    [SerializeField] private Image title;
    [SerializeField] private Image intro;
    [SerializeField] private Button nextButton;
    
    [Header("Checkpoint1_1——UI")]
    [SerializeField] private Canvas gameCanvas;
    [SerializeField] private TMP_Text hintText;
    
    [Header("Checkpoint1_1——GameObject")]
    [SerializeField] private GameObject circleHint;
    [SerializeField] private GameObject fingerHint;
    [SerializeField] private List<TargetZone1_1> targetZones;
    [SerializeField] private List<Season1_1> seasons;

    private List<IDisposable> _subscriptions;
    private IDisposable _hintSubscription;
    private bool _needHint = true;
    private Coroutine _showHintsCoroutine;
    
    
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
            EventCenter.Publish(new Checkpoint1_1.LoadCheckpoint1_1Event());
        }
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
        _subscriptions = new List<IDisposable>();
        
        _subscriptions.Add(EventCenter.Subscribe<SceneMgr.EnterStrategyCompleteEvent>(ShowChapter1Title));
        // subscriptions.Add(EventCenter.Subscribe<Checkpoint1_1.InitCheckpoint1_1Event>(InitGameplay));
        _subscriptions.Add(EventCenter.Subscribe<Dragger.OnDragEndEvent>(CheckSwitch));
        _subscriptions.Add(EventCenter.Subscribe<AttachableZone1_1.AfterAttachEvent>(CheckPassCheckpoint));
        
        checkpointGameObject.SetActive(true);
        seasonsGameObject.SetActive(false);
        targetZonesGameObject.SetActive(false);
        
        chapterCanvas.gameObject.SetActive(true);
        title.gameObject.SetActive(false);
        intro.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        
        gameCanvas.gameObject.SetActive(false);
        hintText.gameObject.SetActive(false);
        circleHint.gameObject.SetActive(false);
        fingerHint.gameObject.SetActive(false);

        _hintSubscription = EventCenter.Subscribe<Dragger.OnDragEndEvent>(AfterFirstDrag);
        _needHint = true;
        _showHintsCoroutine = null;

        for (int i = 0; i < 4; i++)
        {
            targetZones[i].GetComponent<SpriteRenderer>().sprite = null;
            seasons[i].gameObject.SetActive(false);
        }
    }
    
    private void ShowChapter1Title(SceneMgr.EnterStrategyCompleteEvent evt)
    {
        if (evt.TriggerCheckpoint != null && evt.TriggerCheckpoint.ID != 101)
        {
            return;
        }

        if (evt.TriggerCheckpoint == null && !isDebug)
        {
            return;
        }
        
        chapterCanvas.SetActive(true);
        AnimSequence anim = AnimMgr.Instance.CreateSequence();
        anim.AddNode(() =>
        {
            title.gameObject.SetActive(true);
            intro.gameObject.SetActive(false);
            title.GetComponent<UIFade>().FadeIn(0.8f);
        }).AddWait(2.8f).AddNode(() =>
        {
            title.GetComponent<UIFade>().FadeOut(0.8f);
        }).AddWait(1.0f).AddNode(() =>
        {
            title.gameObject.SetActive(false);
            intro.gameObject.SetActive(true);
            intro.GetComponent<UIFade>().FadeIn(0.8f);
        }).AddWait(2.8f).AddNode(() =>
        {
            nextButton.gameObject.SetActive(true);
            nextButton.GetComponent<UIFade>().FadeIn(0.4f);
        });
        
        anim.Play();
    }    
    
    // 点击下一关按钮
    public void OnClickNextButton()
    {
        nextButton.interactable = false;
        
        AnimSequence anim = AnimMgr.Instance.CreateSequence();
        anim.AddNode((() =>
        {
            nextButton.GetComponent<UIFade>().FadeOut(0.4f);
        })).AddWait(0.8f).AddNode(() =>
        {
            intro.GetComponent<UIFade>().FadeOut(0.8f);
        }).AddWait(1.0f).AddNode(() =>
        {
            intro.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);
            chapterCanvas.gameObject.SetActive(false);
            // 正式进入第一关内容（四季淡入）
            Debug.Log("正式进入第一关内容");
            MonoMgr.StartGlobalCoroutine(SeasonsFadeIn());
        }).AddWait(3.0f).AddNode(() =>
        {
            nextButton.gameObject.SetActive(true);
            nextButton.interactable = true;
        });
        
        anim.Play();
    }

    // 四季淡入
    IEnumerator SeasonsFadeIn()
    {
        checkpointGameObject.SetActive(true);
        seasonsGameObject.SetActive(true);
        targetZonesGameObject.SetActive(true);
        
        gameCanvas.gameObject.SetActive(true);
        foreach (var season in seasons)
        {
            season.gameObject.SetActive(true);
            season.GetComponent<Dragger>().enableDrag = false;
            season.GetComponent<SpriteFade>().FadeIn(0.5f);
            yield return new WaitForSeconds(0.5f);
        }
        yield return new WaitForSeconds(1f);
        foreach (var season in seasons)
        {
            season.GetComponent<Dragger>().enableDrag = true;
        }
        hintText.gameObject.SetActive(true);
        hintText.GetComponent<UIFade>().FadeIn(0.5f);
        _showHintsCoroutine = MonoMgr.StartGlobalCoroutine(DoShowHints());
    }

    IEnumerator DoShowHints()
    {
        while (_needHint)
        {
            circleHint.gameObject.SetActive(true);
            fingerHint.gameObject.SetActive(true);
            circleHint.GetComponent<SpriteFade>().FadeIn(0.5f);
            fingerHint.GetComponent<SpriteFade>().FadeIn(0.5f);
            yield return new WaitForSeconds(0.5f);
            circleHint.GetComponent<SpriteFade>().FadeOut(0.5f);
            fingerHint.GetComponent<SpriteFade>().FadeOut(0.5f);
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    private void AfterFirstDrag(Dragger.OnDragEndEvent evt)
    {
        _needHint = false;
        MonoMgr.StopGlobalCoroutine(_showHintsCoroutine);
        circleHint.gameObject.SetActive(false);
        fingerHint.gameObject.SetActive(false);
        _hintSubscription?.Dispose();
        _hintSubscription = null;
    }
    
    // 检查交换
    private void CheckSwitch(Dragger.OnDragEndEvent evt)
    {
        bool needReset = false;
        foreach (var zone in targetZones)
        {
            zone.GetComponent<AttachableZone1_1>().CheckSnap(evt.GameObject, out bool need);
            if (need)
            {
                needReset = true;
            }
        }
        // 延迟重置
        if (needReset)
        {
            evt.GameObject.GetComponent<Season1_1>().ResetPosition();
        }
        // 
        EventCenter.Publish(new AttachableZone1_1.AfterAttachEvent(this.gameObject));
    }

    // 检查通关（在每次吸附成功后）
    private void CheckPassCheckpoint(AttachableZone1_1.AfterAttachEvent evt)
    {
        bool isPassCheckpoint = true;
        bool[] matches = new bool[4];
        for(int i = 0; i < targetZones.Count; i++)
        {
            matches[i] = targetZones[i].GetComponent<TargetZone1_1>().IsMatch();
            if (!matches[i])
            {
                isPassCheckpoint = false;
            }
        }

        if (!isPassCheckpoint)
        {
            for (int i = 0; i < 4; i++)
            {
                if (matches[i])
                {
                    targetZones[i].GetComponent<TargetZone1_1>().ShowGoldenLight();
                }
            }
        }
        else
        {
            // 代表通关
            // EventCenter.Publish(new Checkpoint1_1.PassCheckpoint1_1Event());
            MonoMgr.StartGlobalCoroutine(DelayPublish());
        }
    }
    
    private bool IsAllMatched()
    {
        return false;
    }

    IEnumerator DelayPublish()
    {
        // 四个季节一同发光
        yield return DoShine();

        yield return new WaitForSeconds(1.8f);
        EventCenter.Publish(new Checkpoint1_1.PassCheckpoint1_1Event());
    }
    
    IEnumerator DoShine()
    {
        foreach (var targetZone in targetZones)
        {
            targetZone.gameObject.SetActive(true);
            var sr2 = targetZone.GetComponent<SpriteRenderer>();
            sr2.color = new Color(sr2.color.r, sr2.color.g, sr2.color.b, 1f);
            // targetZone.GetComponent<SpriteRenderer>().color = sr2.color;
            targetZone.GetComponent<AttachableZone1_1>().attachedObject.GetComponent<SpriteFade>().SetAlphaImmediate(0f);
            targetZone.GetComponent<AttachableZone1_1>().attachedObject.gameObject.SetActive(false);
            targetZone.GetComponent<AttachableZone1_1>().attachedObject.GetComponent<Dragger>().enableDrag = false;
            targetZone.GetComponent<SpriteRenderer>().sprite = targetZone.goldenSeason;
            targetZone.GetComponent<SpriteFade>().FadeIn(0.4f);
            targetZone.isAvailable = false;
        }            
        // 保持金光
        yield return new WaitForSeconds(1.5f);
    }

}
