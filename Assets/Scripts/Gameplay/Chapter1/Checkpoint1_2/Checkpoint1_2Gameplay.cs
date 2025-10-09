using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Checkpoint1_2Gameplay : PrefabSingleton<Checkpoint1_2Gameplay>
{
    [SerializeField] private bool isDebug;
    
    [Header("ParentGameObject")]
    [SerializeField] private GameObject checkpointGameObject;
    
    [Header("Checkpoint1_2——UI")]
    [SerializeField] private Canvas gameCanvas;
    [SerializeField] private Image uiMask;

    [Header("Checkpoint1_2——GameObject")] 
    [Header("Part 1")] 
    [SerializeField] private GameObject part1GameObject;
    [SerializeField] private List<GlassDome1_2> fourDomes;
    [SerializeField] private List<GameObject> fourDomes_close;
    [SerializeField] private List<GameObject> fourDomes_open;
    
    [Header("Part 2")] 
    [SerializeField] private GameObject part2GameObject;
    [SerializeField] private GameObject part2Domes;
    [SerializeField] private GameObject part2Seasons;
    [SerializeField] private List<GlassDome1_2> glassDomes;
    [SerializeField] private List<GameObject> glassDomesBG_select;
    [SerializeField] private List<GameObject> glassDomesBG_correct;
    [SerializeField] private List<GameObject> glassDomesClose;
    [SerializeField] private List<GameObject> glassDomesOpen;
    [SerializeField] private List<Season1_2> seasons;

    private bool allDestroyed = false;
    private int count = 0;
    private GlassDome1_2 _currentGlassDome = null;
    private Season1_2 _currentSeason;
    private Coroutine _currentPlayingDome;
    // 标记已经匹配成功的玻璃罩
    private bool[] isMatched = { false, false, false, false };
    private bool[] isPlaying = { false, false, false, false };
    private bool[] isClicked = { false, false, false, false };
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
            EventCenter.Publish(new Checkpoint1_2.LoadCheckpoint1_2Event());
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
        Musics = null;
        glassDomes = null;
        seasons = null;
    }

    private void InitGameplay()
    {
        // Checkpoint1_2 UI 初始化
        gameCanvas.gameObject.SetActive(true);
        uiMask.gameObject.SetActive(false);
        uiMask.GetComponent<CanvasGroup>().alpha = 0;
        
        // part 1 初始化
        part1GameObject.SetActive(true);
        for (int i = 0; i < 4; i++)
        {
            fourDomes[i].gameObject.SetActive(true);
            fourDomes_close[i].SetActive(true);
            fourDomes_open[i].SetActive(false);
        }
        
        // part 2 初始化
        part2GameObject.SetActive(false);
        part2Domes.SetActive(true);
        part2Seasons.SetActive(true);
        for (int i = 0; i < 4; i++)
        {
            glassDomes[i].isClickable = true;
            glassDomesBG_select[i].SetActive(false);
            glassDomesBG_correct[i].SetActive(false);
            glassDomesClose[i].SetActive(true);
            glassDomesOpen[i].SetActive(false);
            seasons[i].isClickable = true;
        }

        // 基础变量初始化
        allDestroyed = false;
        count = 0;
        _currentGlassDome = null;
        _currentSeason = null;
        _currentPlayingDome = null;
        for (int i = 0; i < 4; i++)
        {
            isMatched[i] = false;
            isPlaying[i] = false;
            isClicked[i] = false;
        }
        _subscriptions = new List<IDisposable>();
        _subscriptions.Add(EventCenter.Subscribe<SceneMgr.EnterSceneCompleteEvent>(PlayFourDomeMusic));
        Musics = new Dictionary<int, string>();
    }

    // Part 1:
    // 4 个玻璃罩依次打开并播放音效
    // 需要对触发事件的关卡进行判断
    private void PlayFourDomeMusic(SceneMgr.EnterSceneCompleteEvent evt)
    {
        if (evt.TriggerCheckpoint != null && evt.TriggerCheckpoint.ID != 102)
        {
            return;
        }
        if (evt.TriggerCheckpoint == null && !isDebug)
        {
            return;
        }
        
        AnimSequence anim = AnimMgr.Instance.CreateSequence();
        IEnumerator fourDomesOpen()
        {
            for (int i = 0; i < fourDomes.Count; ++i)
            {
                fourDomes[i].gameObject.SetActive(true);
                fourDomes[i].GetComponent<GlassDome1_2>().Part1OpenGlassDome();
                // SoundManager.Instance.PlaySFX(musics[i + 1]);

                yield return new WaitForSeconds(2f);
                fourDomes[i].GetComponent<GlassDome1_2>().Part1CloseGlassDome();
                // SoundManager.Instance.StopSFX(musics[i + 1]);
            }
            uiMask.gameObject.SetActive(true);
            uiMask.GetComponent<UIFade>().FadeIn(0.5f);
        }

        anim.AddWait(1f)
            .AddNode(fourDomesOpen())
            .AddWait(1.2f)
            .AddNode(() =>
            {
                part1GameObject.SetActive(false);
                part2GameObject.SetActive(true);
                uiMask.GetComponent<UIFade>().FadeOut(0.5f);
            })
            .AddWait(1.2f)
            .AddNode(() =>
            {
                uiMask.gameObject.SetActive(false);
            });
        
        anim.Play();
    }
    
    
    // Part 2:
    // 点击玻璃罩
    public void OnClickGlassDome(int glassDomeId)
    {
        if (!glassDomes[glassDomeId - 1].isClickable)
            return;
        
        // 重置其他Dome播放状态
        foreach(var dome in glassDomes)
        {
            if(dome.glassDomeId != glassDomeId)
            {
                isPlaying[dome.glassDomeId - 1] = false;
            }
        }

        _currentGlassDome = glassDomes[glassDomeId - 1];
        MonoMgr.StartGlobalCoroutine(DoCloseOtherDomes(glassDomeId));
        if (_currentPlayingDome != null)
        {
            MonoMgr.StopGlobalCoroutine(_currentPlayingDome);
        }
        _currentPlayingDome = MonoMgr.StartGlobalCoroutine(OpenDomeAndPlayMusic(glassDomeId));
        
        IEnumerator DoCloseOtherDomes(int glassDomeId)
        {
            for(int i = 0; i < glassDomes.Count; ++i)
            {
                if (glassDomes[i].glassDomeId != glassDomeId && glassDomes[i].isClickable)
                {
                    glassDomes[i].GetComponent<GlassDome1_2>().Part2CloseGlassDome();
                    // SoundManager.Instance.StopMusic();
                }
            }
            yield return null;
        }

        IEnumerator OpenDomeAndPlayMusic(int glassDomeId)
        {
            if (!isPlaying[glassDomeId - 1])
            {
                isPlaying[glassDomeId - 1] = true;
                // SoundManager.Instance.StopSFX(Musics[_currentGlassDome.glassDomeId]);
                glassDomes[glassDomeId - 1].GetComponent<GlassDome1_2>().Part2OpenGlassDome(true);
                // SoundManager.Instance.PlayMusic(Musics[glassDomeId]);
                Debug.Log("播放：GlassDome " + glassDomeId);

                yield return null;
            }
        }
    }

    private void ResetIsClicked(int index)
    {
        isClicked[index] = false;
    }

    public void OnClickSeason(int correspondingSeasonId)
    {
        if (!seasons[correspondingSeasonId - 1].isClickable)
            return;
        // if (!startAlready) return;
        if (isClicked[correspondingSeasonId - 1])
        {
            return;
        }

        isClicked[correspondingSeasonId - 1] = true;
        _currentSeason = seasons[correspondingSeasonId - 1];
        if (_currentGlassDome == null || _currentGlassDome.correspondingSeasonId != _currentSeason.seasonId)
        {
            _currentSeason.SelectSeason(ResetIsClicked);
            return;
        }

        int currentGlassDomeId = _currentGlassDome.glassDomeId;
        if (_currentGlassDome.correspondingSeasonId == _currentSeason.seasonId)
        {
            ++count;
            // 标记当前正在播放的玻璃罩为 “已匹配”
            isMatched[currentGlassDomeId - 1] = true;
            // SoundManager.Instance.PlaySFX("Audio/Chapter1/1_1_correctHint/select_the_correct_prompt");
            // SoundManager.Instance.StopSFX(musics[currentGlassDomeId]);
            MonoMgr.StopGlobalCoroutine(_currentPlayingDome);
            // SoundManager.Instance.StopMusic();
            if (count == 4)
            {
                allDestroyed = true;
                MonoMgr.StartGlobalCoroutine(DelayPublish());
                return;
            }

            glassDomes[currentGlassDomeId - 1].isClickable = false;
            seasons[correspondingSeasonId - 1].isClickable = false;
            glassDomes[currentGlassDomeId - 1].TurnToGoldenBG();
            seasons[correspondingSeasonId - 1].TurnToGoldenSeason();

            _currentGlassDome = null;
            _currentSeason = null;

            IEnumerator DelayPublish()
            {
                yield return new WaitForSeconds(1.5f);
                EventCenter.Publish(new Checkpoint1_2.PassCheckpoint1_2Event());
            }
        }
    }
    
}
