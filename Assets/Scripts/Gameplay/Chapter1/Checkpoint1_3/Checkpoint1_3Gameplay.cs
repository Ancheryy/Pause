using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
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
    
    [Header("Part1")] 
    [SerializeField] private GameObject part1;
    [SerializeField] private AttachableZone1_3 targetZone;
    [SerializeField] private List<GlassDome1_3> domes;
    [SerializeField] private List<GameObject> domesClose;
    [SerializeField] private List<GameObject> domesOpen;
    
    [Header("Part2")] 
    [SerializeField] private GameObject part2;
    [SerializeField] private GameObject girlWithFullDrink;
    [SerializeField] private GameObject girlWithEmptyGlass;
    [SerializeField] private GameObject fullDrink;
    [SerializeField] private GameObject emptyGlass;
    [SerializeField] private Animator ice;
    [SerializeField] private Animator hint;
    [SerializeField] private GameObject halfWindowAndGlass;
    [SerializeField] private GameObject fullWindowAndRain;

    public bool isPassed = false;
    private GameObject _lastDome;
    private List<IDisposable> _subscriptions;
    public Dictionary<int, string> Musics;
    
    // 唯一钟罩播放协程
    private Coroutine _coroutine1;
    // 冰块融化播放协程
    private Coroutine _coroutine2;
    // 是否需要提示长按冰块
    private bool _needHint = true;
    // 是否正在按压冰块
    private bool _isPressingIce = false;
    // 动画持续时间
    public float duration = 0f;
    // 是否触发融化阶段
    private bool[] isTriggers = new bool[4];
    
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
            EventCenter.Publish(new Checkpoint1_3.LoadCheckpoint1_3Event());
        }
    }

    void Update()
    {
        if (_isPressingIce)
        {
            duration += Time.deltaTime;
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
        checkpointGameObject.SetActive(true);
        background.gameObject.SetActive(true);
        
        // Part1 初始化
        part1.SetActive(false);
        targetZone.gameObject.SetActive(true);
        targetZone.blueDomeZone.gameObject.SetActive(true);
        targetZone.glassDomeOpen.gameObject.SetActive(false);
        targetZone.glassDomeOpenBG.gameObject.SetActive(false);
        targetZone.note.gameObject.SetActive(false);
        for (int i = 0; i < 4; i++)
        {
            domes[i].gameObject.SetActive(true);
            domes[i].glassDomeClose.gameObject.SetActive(true);
            domes[i].glassDomeOpen.gameObject.SetActive(false);
        }
        
        // Part2 初始化
        part2.SetActive(false);
        girlWithFullDrink.SetActive(true);
        girlWithEmptyGlass.SetActive(false);
        fullDrink.SetActive(true);
        emptyGlass.SetActive(false);
        ice.gameObject.SetActive(false);
        hint.gameObject.SetActive(false);
        halfWindowAndGlass.SetActive(false);
        fullWindowAndRain.SetActive(false);
        
        // 其他初始化
        isPassed = false;
        _lastDome = null;
        _subscriptions = new List<IDisposable>();
        _subscriptions.Add(EventCenter.Subscribe<SceneMgr.EnterStrategyCompleteEvent>(ShowSummerTitle));
        _subscriptions.Add(EventCenter.Subscribe<Dragger.OnDragEndEvent>(AfterDrag));
        Musics = new Dictionary<int, string>();

    }

    // -------- Part 1 方法集 --------
    // 夏日标题缓缓显现
    private void ShowSummerTitle(SceneMgr.EnterStrategyCompleteEvent evt)
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
                // text1Image.GetComponent<UIFade>().FadeOut(0.8f);
                uiMask.gameObject.SetActive(true);
                uiMask.GetComponent<UIFade>().FadeIn(0.8f);
            })
            .AddWait(1.0f)
            .AddNode(() =>
            {
                text1Image.gameObject.SetActive(false);
                gameCanvas.gameObject.SetActive(false);
                part1.SetActive(true);
                uiMask.GetComponent<UIFade>().FadeOut(0.5f);
            });

        anim.Play();
    }
    
    // 集中判断，并选择执行的方法
    // 1.先判断当前游戏状态，即：isPassed
    //   如果为 false，执行一般的音乐播放 PlayCorrespondingMusic
    private void AfterDrag(Dragger.OnDragEndEvent evt)
    {
        // 1.获取当前操作的物体
        bool isFound = false;
        GlassDome1_3 dome = null;
        foreach (var d in domes)
        {
            if (evt.GameObject == d.glassDomeClose)
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
            targetZone.SnapToTarget(dome);
            MonoMgr.StartGlobalCoroutine(DelayToNextPart(dome));
        }
        else
        {
            // 不在目标区域上
            dome.ResetPosition();
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
            StopSFX(_lastDome.GetComponent<GlassDome1_3>().glassDomeId);
            if (_coroutine1 != null)
            {
                MonoMgr.StopGlobalCoroutine(_coroutine1);
            }
        }
            
        _coroutine1 = MonoMgr.StartGlobalCoroutine(OpenAndPlaySFX());
        
        IEnumerator OpenAndPlaySFX()
        {
            yield return new WaitForNextFrameUnit();
            if (isPassed) yield break;
            glassDome.OpenGlassDome();
            //PlaySFX(id);
            Debug.Log("播放：" + glassDome.name);
            _lastDome = glassDome.gameObject;

            yield return new WaitForSeconds(3.0f);
            glassDome.CloseGlassDome();
        }
        
    }

    private IEnumerator DelayToNextPart(GlassDome1_3 dome)
    {
        targetZone.OpenDomeAndPlay(dome);
        PlayMusic(dome.glassDomeId);
        Debug.Log("播放选择的音乐：" + dome.name);
        
        yield return new WaitForSeconds(4.0f);
        DrinkJuice();

    }

    public void PlaySFX(int id)
    {
        // SoundManager.Instance.PlaySFX(musics[id], false);
    }

    private void PlayMusic(int id)
    {
        MonoMgr.StartGlobalCoroutine(DoStopMusicLater(id));

        IEnumerator DoStopMusicLater(int id)
        {
            //SoundManager.Instance.PlaySFX(musics[id], false);

            yield return new WaitForSeconds(5.8f);
            //SoundManager.Instance.PlayMusic(musics[id]);

            yield return new WaitForSeconds(7.8f);
            //SoundManager.Instance.StopMusic();
        }
    }
    
    private void StopSFX(int id)
    {
        // SoundManager.Instance.StopSFX(musics[id]);
    }
    
    // -------- Part 2 方法集 --------
    
    // 喝果汁
    private void DrinkJuice()
    {
        AnimSequence anim = AnimMgr.Instance.CreateSequence();
        anim.AddNode(() =>
            {
                uiMask.gameObject.SetActive(true);  // 随用随设，遮罩出现
                uiMask.GetComponent<UIFade>().FadeIn(0.8f);
            })
            .AddWait(1.3f)
            .AddNode(() =>
            {
                part1.SetActive(false);
                part2.SetActive(true);
                uiMask.GetComponent<UIFade>().FadeOut(0.8f);
            })
            .AddWait(3.0f)
            .AddNode(() =>
            {
                uiMask.gameObject.SetActive(false);
                girlWithFullDrink.GetComponent<SpriteFade>().FadeOut(0.8f);
                fullDrink.GetComponent<SpriteFade>().FadeOut(0.8f);
            })
            .AddWait(1.3f)
            .AddNode(() =>
            {
                girlWithFullDrink.SetActive(false);
                fullDrink.SetActive(false);
                girlWithEmptyGlass.SetActive(true);
                emptyGlass.SetActive(true);
                girlWithEmptyGlass.GetComponent<SpriteFade>().FadeIn(0.8f);
                emptyGlass.GetComponent<SpriteFade>().FadeIn(0.8f);
            })
            .AddWait(1.8f)
            .AddNode(() =>
            {
                emptyGlass.GetComponent<SpriteFade>().FadeOut(0.8f);
            })
            .AddWait(1.3f)
            .AddNode(() =>
            {
                emptyGlass.SetActive(false);
                ice.gameObject.SetActive(true);
                ice.GetComponent<SpriteFade>().FadeIn(0.8f);
            })
            .AddWait(1.0f)
            .AddNode(JudgePressing());
        
        anim.Play();

        IEnumerator JudgePressing()
        {
            if (_needHint)
            {
                hint.gameObject.SetActive(true);
                yield return null; // 等待一帧
                hint.Play("LongPress");
            }
        }
        
    }
    
    // 长按冰块
    public void LongPressIce()
    {
        _isPressingIce = true;
        _needHint = false;
        if (hint != null && hint.isActiveAndEnabled)
        {
            hint.StopPlayback();
            hint.Rebind(); // 重置所有参数和状态
            hint.Update(0); // 立即应用重置
        }
        hint.gameObject.SetActive(false);

        // 开始融化，播放融化动画
        ice.enabled = true;
        // ice.Play("TransferHeat");

        if (_coroutine2 == null)
        {
            _coroutine2 = MonoMgr.StartGlobalCoroutine(DoIceMelt());
        }
        
        IEnumerator DoIceMelt()
        {
            // 如果尚未执行TouchIce，则等待一帧
            if (!_isPressingIce)
                yield return null;

            while (_isPressingIce && duration <= 5f)
            {
                if(duration >= 2.5f && duration < 3.0f && !isTriggers[0])
                {
                    girlWithEmptyGlass.GetComponent<SpriteFade>().FadeOut(0.4f);
                    isTriggers[0] = true;
                }

                // 融化一半
                if (duration >= 3.0f && duration < 4.0f && !isTriggers[1])
                {
                    halfWindowAndGlass.gameObject.SetActive(true);
                    halfWindowAndGlass.GetComponent<SpriteFade>().FadeIn(0.4f);
                    isTriggers[1] = true;
                }

                if (duration >= 4.0f && duration < 4.5f && !isTriggers[2])
                {
                    halfWindowAndGlass.GetComponent<SpriteFade>().FadeOut(0.4f);
                    isTriggers[2] = true;
                }

                // 彻底融化
                if (duration >= 4.5f && !isTriggers[3])
                {
                    halfWindowAndGlass.gameObject.SetActive(false);
                    fullWindowAndRain.gameObject.SetActive(true);
                    fullWindowAndRain.GetComponent<SpriteFade>().FadeIn(0.4f);
                    isTriggers[3] = true;
                }

                yield return null;
            }

            if(duration >= 5.0f)
            {
                girlWithEmptyGlass.SetActive(false);

                ice.GetComponent<SpriteButton>().SetInteractable(false);

                // 延迟转到下一关
                yield return new WaitForSeconds(1.8f);
                EventCenter.Publish(new Checkpoint1_3.PassCheckpoint1_3Event());
            }
        }
    }

    public void StopLongPressIce()
    {
        _isPressingIce = false;
        // _needHint = true;
        ice.enabled = false;
    }
}
