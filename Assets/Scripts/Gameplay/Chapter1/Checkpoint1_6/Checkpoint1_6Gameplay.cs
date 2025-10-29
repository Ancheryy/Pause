using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Checkpoint1_6Gameplay : PrefabSingleton<Checkpoint1_6Gameplay>
{
    [SerializeField] private bool isDebug;

    [Header("ParentGameObject")]
    [SerializeField] private GameObject checkpointGameObject;
    
    [Header("Checkpoint1_5——UI")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private Image uiMask;
    
    [Header("Checkpoint1_5——GameObject")] 
    [SerializeField] private GameObject background;
    
    [Header("Part1")] 
    [SerializeField] private GameObject part1;
    [SerializeField] private List<VoicingObject1_6> allVoicingObjects;
    [SerializeField] private Collider2D targetZoneCollider2D;
    [SerializeField] private GameObject goldenDome;

    
    
    private bool isPassed = false;
    private GameObject lastDome;
    private List<IDisposable> _subscriptions;
    private Dictionary<int, string> musics;
    
        
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
            EventCenter.Publish(new Checkpoint1_6.LoadCheckpoint1_6Event());
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
        checkpointGameObject.SetActive(true);
        
        // Checkpoint1_5 UI 初始化
        uiCanvas.gameObject.SetActive(true);

        // Checkpoint1_5 GameObject 初始化
        background.gameObject.SetActive(true);
        
        // Part1 初始化
        part1.SetActive(true);
        for (int i = 0; i < allVoicingObjects.Count; i++)
        {
            allVoicingObjects[i].gameObject.SetActive(true);
            allVoicingObjects[i].GetComponent<Dragger>().enableDrag = true;
        }
        targetZoneCollider2D.gameObject.SetActive(true);
        goldenDome.gameObject.SetActive(false);
        
        // 其他初始化
        isPassed = false;
        _subscriptions = new List<IDisposable>();
        _subscriptions.Add(EventCenter.Subscribe<Dragger.OnDragEndEvent>(AfterObjectDragged));
    }
    
    // -------- Part 1 方法集 --------
    private void AfterObjectDragged(Dragger.OnDragEndEvent evt)
    {
        VoicingObject1_6 voicingObject = evt.GameObject.GetComponent<VoicingObject1_6>();
        
        // 1.检查是否拖拽到了目标区域
        if (voicingObject.IsAttachable(targetZoneCollider2D) && voicingObject.voicingObjectId == 4)
        {
            // 附着成功
            voicingObject.SnapToTarget(targetZoneCollider2D);
            PlayCorrespondingVoice(voicingObject);
        }
        else
        {
            // 附着失败
            voicingObject.ResetPosition();
            PlaySFX(voicingObject.voicingObjectId);
        }
        
    }
    
    private void PlayCorrespondingVoice(VoicingObject1_6 voicingObject)
    {
        AnimSequence anim = AnimMgr.Instance.CreateSequence();
        anim.AddNode((() =>
            {
                isPassed = true;
                voicingObject.gameObject.SetActive(false);
                targetZoneCollider2D.gameObject.SetActive(false);
                goldenDome.SetActive(true);
                goldenDome.GetComponent<SpriteFade>().FadeIn(0.3f);
            }))
            .AddWait(0.3f)
            .AddNode(() =>
            {
                // 播放对应的音乐
                PlayMusic(voicingObject.voicingObjectId);
                Debug.Log("播放选择的音乐：" + voicingObject.name);
            })
            .AddWait(4.0f)
            .AddNode(() =>
            {
                EventCenter.Publish(new Checkpoint1_6.PassCheckpoint1_6Event());
            });

        anim.Play();
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    private void PlaySFX(int id)
    {
        // 这里需要一个这样的逻辑：检查当前是否有正在播放的音效，如果有则停止它再播放新的（暂时不写这个逻辑）
        
        // SoundManager.Instance.PlaySFX(musics[id], false);
        Debug.Log("Play SFX: " + id);
    }

    private void PlayMusic(int id)
    {
        // MonoBehaviourHelper.StartGlobalCoroutine(DoStopMusicLater(id));
    }

    IEnumerator DoStopMusicLater(int id)
    {
        // SoundManager.Instance.PlaySFX(musics[id], false);

        // yield return new WaitForSeconds(5.8f);
        yield return new WaitForSeconds(0.3f);
        // SoundManager.Instance.PlayMusic(musics[id]);
        Debug.Log("Play Music" + id);

        yield return new WaitForSeconds(3.8f);
        // SoundManager.Instance.StopMusic();
        Debug.Log("Stop Music" + id);
    }

    private void StopSFX(int id)
    {
        // SoundManager.Instance.StopSFX(musics[id]);
        Debug.Log("Stop SFX: " + id);
    }

    private void DisableAllDomesExcept(int id)
    {
        foreach (var dome in allVoicingObjects)
        {
            if (dome.GetComponent<VoicingObject1_6>().voicingObjectId == id) continue;

            // dome.raycastTarget = false;
        }
    }
}
