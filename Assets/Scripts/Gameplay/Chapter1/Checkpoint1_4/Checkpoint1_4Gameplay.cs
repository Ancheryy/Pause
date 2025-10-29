using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Checkpoint1_4Gameplay : PrefabSingleton<Checkpoint1_4Gameplay>
{
    [SerializeField] private bool isDebug;

    [Header("ParentGameObject")]
    [SerializeField] private GameObject checkpointGameObject;
    
    [Header("Checkpoint1_4——UI")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private Image uiMask;
    [SerializeField] private Canvas gameCanvas;
    [SerializeField] private TMP_Text part1Hint1;
    
    [Header("Checkpoint1_4——GameObject")] 
    [SerializeField] private GameObject background;
    
    [Header("Part1")] 
    [SerializeField] private GameObject part1;
    [SerializeField] private GameObject emptyDomeClose;
    [SerializeField] private GameObject emptyDomeOpen;
    [SerializeField] private Collider2D targetCollider2D;
    [SerializeField] private List<InteractableItem1_4> interactableItems; 
    [SerializeField] private List<GameObject> interactableItemHints; 
    
    [Header("Part2")] 
    [SerializeField] private GameObject part2;
    [SerializeField] private GameObject BG_cool;
    [SerializeField] private GameObject BG_hot;
    [SerializeField] private GameObject girlCool;
    [SerializeField] private GameObject girlHot;
    [SerializeField] private GameObject thermometerHot;
    [SerializeField] private GameObject thermometerCool;

    public bool isPassed = false;
    private List<IDisposable> _subscriptions;
    // 可交互物件（8个）
    private List<InteractableItem> items;
    // 标志当前是否有音效播放
    private bool isPlayingSFX = false;
    // 当前播放的音乐名称
    private string currentSFXName;
    // 当前正在播放SFX的协程
    private Coroutine corCurrentSFX;
    
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
            EventCenter.Publish(new Checkpoint1_4.LoadCheckpoint1_4Event());
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
        
        // Checkpoint1_4 UI 初始化
        uiCanvas.gameObject.SetActive(true);
        gameCanvas.gameObject.SetActive(true);
        part1Hint1.gameObject.SetActive(false);

        // Checkpoint1_4 GameObject 初始化
        background.gameObject.SetActive(true);
        
        // Part1 初始化
        part1.SetActive(false);
        emptyDomeClose.SetActive(true);
        emptyDomeOpen.SetActive(false);
        targetCollider2D.gameObject.SetActive(true);
        for(int i = 0; i < interactableItems.Count; ++i)
        {
            interactableItems[i].gameObject.SetActive(true);
            interactableItemHints[i].SetActive(false);
        }
        
        // Part2 初始化
        part2.SetActive(false);
        
        // 其他初始化
        isPassed = false;
        _subscriptions = new List<IDisposable>();
        _subscriptions.Add(EventCenter.Subscribe<SceneMgr.EnterStrategyCompleteEvent>(ShowRestaurant));
        _subscriptions.Add(EventCenter.Subscribe<ChildDragger.StartDraggingEvent>(AfterItemPointDown));
        _subscriptions.Add(EventCenter.Subscribe<ChildDragger.StopDraggingEvent>(AfterItemPointUp));

        items = new List<InteractableItem>();
        items.Add(new InteractableItem(1, "ice-cubes-in-water-104820"));
        items.Add(new InteractableItem(2, "cooking-in-cooking-pot-129386"));
        items.Add(new InteractableItem(3, "juice-drink-214466"));
        items.Add(new InteractableItem(4, "typing-on-laptop-keyboard-308455"));
        items.Add(new InteractableItem(5, "tasting-the-soup-43610"));
        items.Add(new InteractableItem(6, "tasting-the-soup-43610"));
        items.Add(new InteractableItem(7, "tasting-the-soup-43610"));
        items.Add(new InteractableItem(8, "tasting-the-soup-43610"));
    }
    
    // -------- Part 1 方法集 --------
    // 显现餐厅
    private void ShowRestaurant(SceneMgr.EnterStrategyCompleteEvent evt)
    {
        if (evt.TriggerCheckpoint != null && evt.TriggerCheckpoint.ID != 104)
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
            part1.SetActive(true);
            part1Hint1.gameObject.SetActive(true);
            uiMask.gameObject.SetActive(true);
            uiMask.GetComponent<UIFade>().FadeOut(0.8f);
        });

        anim.Play();
    }

    private void PlayItemSFX(int id)
    {
        for (int i = 0; i < items.Count; ++i)
        {
            if (items[i].id == id)
            {
                if (isPlayingSFX)
                {
                    StopCurrentSFX();
                }

                corCurrentSFX = MonoMgr.StartGlobalCoroutine(DoPlaySFX("Audio/Chapter1/1_4_rainVoice/" + items[i].SFXName));
                Debug.Log($"播放物件id为 {i + 1} 的音乐");
            }
        }

        IEnumerator DoPlaySFX(string SFXName)
        {
            // SoundManager.Instance.PlaySFX(SFXName, false);
            isPlayingSFX = true;
            currentSFXName = SFXName;

            yield return new WaitForSeconds(8.0f);
            // SoundManager.Instance.StopSFX(SFXName);
            isPlayingSFX = false;
            currentSFXName = null;
        }
    }

    private void StopCurrentSFX()
    {
        // SoundManager.Instance.StopSFX(currentSFXName);
        MonoMgr.StopGlobalCoroutine(corCurrentSFX);
        isPlayingSFX = false;
        currentSFXName = null;
    }

    private void AfterItemPointDown(ChildDragger.StartDraggingEvent evt)
    {
        InteractableItem1_4 item = evt.DraggedObject.GetComponent<InteractableItem1_4>();
        
        item.StopHint();
        item.OnBeginDrag();
    }

    private void AfterItemPointUp(ChildDragger.StopDraggingEvent evt)
    {
        InteractableItem1_4 item = evt.DraggedObject.GetComponent<InteractableItem1_4>();

        AnimSequence anim = AnimMgr.Instance.CreateSequence();
        anim.AddNode(() =>
            {
                isPassed = true;
                item.gameObject.SetActive(false);
                emptyDomeClose.GetComponent<SpriteFade>().FadeOut(0.3f);

                PlayItemSFX(item.ItemID);
            })
            .AddWait(0.4f)
            .AddNode(() =>
            {
                emptyDomeClose.gameObject.SetActive(false);
                emptyDomeOpen.gameObject.SetActive(true);
                emptyDomeOpen.GetComponent<SpriteFade>().FadeIn(0.3f);
            })
            .AddWait(2.3f)
            .AddNode(() =>
            {
                uiMask.gameObject.SetActive(true);
                uiMask.GetComponent<UIFade>().FadeIn(0.8f);
            })
            .AddWait(1.3f)
            .AddNode(() =>
            {
                part1.SetActive(false);
                part1Hint1.gameObject.SetActive(false);
                part2.SetActive(true);
                BG_hot.SetActive(true);
                BG_cool.SetActive(false);
                girlHot.SetActive(true);
                girlCool.SetActive(false);
                thermometerHot.SetActive(true);
                thermometerCool.SetActive(false);
                uiMask.GetComponent<UIFade>().FadeOut(0.8f);
            })
            .AddWait(3.3f, "展示降温前")
            .AddNode(() =>
            {
                uiMask.GetComponent<UIFade>().FadeIn(0.5f);
            })
            .AddWait(1.3f)
            .AddNode(() =>
            {
                BG_hot.SetActive(false);
                BG_cool.SetActive(true);
                girlHot.SetActive(false);
                girlCool.SetActive(true);
                thermometerHot.SetActive(false);
                thermometerCool.SetActive(true);
                uiMask.GetComponent<UIFade>().FadeOut(0.5f);
            })
            .AddWait(3.8f, "展示降温后")
            .AddNode(() =>
            {
                EventCenter.Publish(new Checkpoint1_4.PassCheckpoint1_4Event());
            });
        
        if (item.NeedAttachToZone(targetCollider2D))
        {
            if (item.ItemID == 3)
            {
                // 首先设置拖动物体的拖动目标区域（ChildDragger 脚本会自动处理 是吸附 还是返回）
                item.GetComponent<ChildDragger>().SetAttachableZone(targetCollider2D);
                // 接着播放对应动效
                anim.Play();
            }
        }
        else
        {
            PlayItemSFX(item.ItemID);
        }

        item.OnEndDrag();
    }
    
    

    // -------- Part 2 方法集 --------
    // 包含可拖动物件的属性
    public struct InteractableItem
    {
        // 物件识别号码
        public int id;
        // 对应音效名称
        public string SFXName;

        public InteractableItem(int id, string SFXName)
        {
            this.id = id;
            this.SFXName = SFXName;
        }
    }

    private void GirlCoolDown()
    {
        // MonoMgr.StartGlobalCoroutine(DoGirlCoolDown());

        // IEnumerator DoGirlCoolDown()
        // {
            // backHalf.GetComponent<UIFade>().FadeOut(0.8f);
            //
            // yield return new WaitForSeconds(1.0f);
            // backHalf.SetActive(false);
            // girlHot.gameObject.SetActive(true);
            // girlHot.GetComponent<UIFade>().FadeIn(0.8f);
            //
            // yield return new WaitForSeconds(1.8f);
            // girlHot.GetComponent<UIFade>().FadeOut(0.8f);
            //
            // yield return new WaitForSeconds(1.0f);
            // girlHot.gameObject.SetActive(false);
            // girlCool.gameObject.SetActive(true);
            // girlCool.GetComponent<UIFade>().FadeIn(0.8f);
            //
            // yield return new WaitForSeconds(2.3f);
            // EventCenter.Publish(new Checkpoint1_4.PassCheckpoint1_4Event());
        // }
    }
    
    
}
