using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Checkpoint1_3Gameplay : PrefabSingleton<Checkpoint1_3Gameplay>
{   
    [SerializeField] private bool isDebug;

    [Header("ParentGameObject")]
    [SerializeField] private GameObject checkpointGameObject;
    
    [Header("Checkpoint1_3——UI")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private Image uiMask;
    [SerializeField] private Canvas gameCanvas;
    [SerializeField] private Image summerTitle;
    [SerializeField] private TMP_Text text1Image;
    [SerializeField] private Button nextPartButton;
    
    [Header("Checkpoint1_3——GameObject")] 
    [SerializeField] private GameObject background;
    [SerializeField] private List<GlassDome1_3> domes;
    [SerializeField] private AttachableZone1_3 targetZone;

    public bool isPassed = false;
    private GameObject _lastDome;
    private List<IDisposable> _subscriptions;
    public Dictionary<int, string> Musics;
    
    
    protected override void Awake()
    {
        base.Awake();
        
        InitGameplay();
    }
    
    void Start()
    {
        if (isDebug)
        {
            EventCenter.Publish(new SceneMgr.EnterSceneCompleteEvent(null));
            EventCenter.Publish(new Checkpoint1_3.LoadCheckpoint1_3Event());
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
        // Checkpoint1_3 UI 初始化
        uiCanvas.gameObject.SetActive(true);
        gameCanvas.gameObject.SetActive(true);
        summerTitle.gameObject.SetActive(true);
        text1Image.gameObject.SetActive(false);
        nextPartButton.gameObject.SetActive(false);

        // Checkpoint1_3 GameObject 初始化
        background.gameObject.SetActive(true);
        
        isPassed = false;
        _lastDome = null;
        _subscriptions = new List<IDisposable>();
        _subscriptions.Add(EventCenter.Subscribe<SceneMgr.EnterSceneCompleteEvent>(ShowSummerTitle));
        Musics = new Dictionary<int, string>();

    }

    // 夏日标题缓缓显现
    private void ShowSummerTitle(SceneMgr.EnterSceneCompleteEvent evt)
    {
        if (evt.TriggerCheckpoint != null && evt.TriggerCheckpoint.ID != 103)
        {
            return;
        }
        if (evt.TriggerCheckpoint == null && !isDebug)
        {
            return;
        }
        
        AnimSequence anim = AnimMgr.Instance.CreateSequence();
        anim.AddWait(2.5f)
            .AddNode(() =>
            {
                // SoundManager.Instance.StopAll();
                text1Image.gameObject.SetActive(false);
                summerTitle.GetComponent<UIFade>().FadeOut(0.5f);
            })
            .AddWait(0.6f)
            .AddNode(() =>
            {
                summerTitle.gameObject.SetActive(false);
                gameCanvas.gameObject.SetActive(true);
                text1Image.gameObject.SetActive(true);
                text1Image.GetComponent<UIFade>().FadeIn(0.8f);
            })
            .AddWait(2.8f)
            .AddNode(() =>
            {
                nextPartButton.gameObject.SetActive(true);
                nextPartButton.GetComponent<UIFade>().FadeIn(0.4f);
            });

        anim.Play();
    }

    // 点击下一章节按键
    public void OnClickNextPart()
    {
        nextPartButton.interactable = false;
        
        AnimSequence anim = AnimMgr.Instance.CreateSequence();
        anim.AddNode(() =>
            {
                nextPartButton.GetComponent<UIFade>().FadeOut(0.4f);
            })
            .AddWait(0.8f)
            .AddNode(() =>
            {
                nextPartButton.interactable = true;
                nextPartButton.gameObject.SetActive(false);
                text1Image.GetComponent<UIFade>().FadeOut(0.8f);
            })
            .AddWait(1.0f)
            .AddNode(() =>
            {
                text1Image.gameObject.SetActive(false);
                gameCanvas.gameObject.SetActive(false);
                checkpointGameObject.SetActive(true);
                checkpointGameObject.GetComponent<UIFade>().FadeIn(0.5f);
            });

        anim.Play();
    }
    
    // 集中判断，并选择执行的方法
    // 1.先判断当前游戏状态，即：isPassed
    //   如果为 false，执行一般的音乐播放 PlayCorrespondingMusic
    private void AfterDrag(Dragger.OnDragStartEvent evt)
    {
        // 1.获取当前操作的物体
        bool isFound = false;
        GlassDome1_3 dome = null;
        foreach (var d in domes)
        {
            if (evt.GameObject == d)
            {
                dome = d.GetComponent<GlassDome1_3>();
                isFound = true;
                break;
            }
        }
        if (!isFound) return;
        
        // 2.判断当前操作的物体是否处于目标区域上
        if (targetZone.IsAttachable(dome))
        {
            // 处于目标区域上
            
        }
        else
        {
            // 不在目标区域上
            PlayCorrespondingMusic(dome);
        }
    }
    
    // 播放音乐（不在目标区域上）
    private void PlayCorrespondingMusic(GlassDome1_3 glassDome)
    {
        // 2.重置上一个播放了音效的玻璃罩状态（如有）
        if(_lastDome != null)
        {
            _lastDome.GetComponent<GlassDome1_3>().CloseGlassDome();
            StopSFX(Checkpoint1_3Gameplay.Instance._lastDome.GetComponent<GlassDome1_3>().glassDomeId);
        }
            
        MonoMgr.StartGlobalCoroutine(OpenAndPlaySFX());
        
        IEnumerator OpenAndPlaySFX()
        {
            // 保证该协程执行慢于CheckAdsorb()
            // while (!isChecked) { yield return null; }

            // 延迟一帧，防止被提前设置为SetActive(false)，从而丢失当前对象（或当前对象被禁用）
            yield return new WaitForNextFrameUnit();
            // 再延迟一帧，保证该协程执行慢于CheckAdsorb()
            // yield return new WaitForNextFrameUnit();
            if (isPassed) yield break;
            glassDome.OpenGlassDome();
            PlaySFX(id);
            Debug.Log("播放：" + this.name);
            _lastDome = this.gameObject;

            yield return new WaitForSeconds(3.0f);
            glassDome.CloseGlassDome();
        }
        
    }


    private void StopSFX(int id)
    {
        // SoundManager.Instance.StopSFX(musics[id]);
    }
    
}
