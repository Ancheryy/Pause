using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

public class AudioMgr : MonoSingleton<AudioMgr>
{
    [Header("音频配置")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private int maxSfxChannels = 10;
    private float _fadeInDuration = 0.5f;
    private float _fadeOutDuration = 1.0f;

    private Dictionary<string, AudioClip> _loadedClips = new Dictionary<string, AudioClip>();
    private List<AudioSource> _sfxPool = new List<AudioSource>();
    private Dictionary<string, List<AudioSource>> _playingInstances = new Dictionary<string, List<AudioSource>>();
    private Dictionary<string, bool> _clipsInUse = new Dictionary<string, bool>();

    // 在类中添加状态变量
    private bool _isMusicFadingOut = false;
    // private bool _isSFXFadingOut = false;

    protected override void Awake()
    {
        base.Awake();
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < maxSfxChannels; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
            _sfxPool.Add(source);
        }
    }

    private readonly HashSet<string> _loadingClips = new HashSet<string>();

    /// <summary>
    /// 动态加载音频（AddressableMgr）
    /// </summary>
    /// <param name="audioName">音频名称</param>
    /// <param name="playAfterLoad">加载过后是否立刻播放</param>
    /// <param name="isMusic">是否是音乐</param>
    public void LoadAudio(string audioName, bool playAfterLoad = false, bool isMusic = false)
    {
        if (_loadedClips.ContainsKey(audioName))
        {
            Debug.LogWarning($"音频已加载: {audioName}");
            return;
        }
        
        _loadingClips.Add(audioName); // 标记为正在加载
        AddressableMgr.LoadAssetAsync<AudioClip>(audioName, (obj) =>
        {
            _loadingClips.Remove(audioName); // 加载完成移除标记

            AudioClip clip = obj.Result;
            _loadedClips.Add(audioName, clip);

            if (isMusic)
            {
                musicSource.clip = clip;
                Debug.Log($"音乐预加载完成: {clip.name}");
            }
            else
            {
                Debug.Log($"音效预加载完成: {clip.name}");
            }
            
            // 判断是否默认播放
            if (playAfterLoad)
            {
                DoPlayMusic(obj.Result);
            }
        });
        
    }

    /// <summary>
    /// 播放音乐（带淡入效果）
    /// </summary>
    public void PlayMusic(string audioName, float volume = 1f, bool fadeIn = true, bool loop = true)
    {
        // 如果正在淡出，立即完成淡出并停止
        if (_isMusicFadingOut)
        {
            DOTween.Kill(musicSource); // 终止当前淡出音效
            musicSource.Stop();
            _isMusicFadingOut = false;
        }
        
        // 直接播放
        if (_loadedClips.TryGetValue(audioName, out AudioClip clip))
        {
            DoPlayMusic(clip);
        }
        // 加载，然后触发 playMusicAction 回调
        else
        {
            LoadAudio(audioName, true, true);
        }

    }

    // 实际播放音乐
    private void DoPlayMusic(AudioClip clip, float volume = 1f, bool fadeIn = true, bool loop = true)
    {
        musicSource.clip = clip;
        musicSource.loop = loop;

        if (fadeIn)
        {
            musicSource.volume = 0f; // 初始音量为0
            musicSource.Play();
            musicSource.DOFade(volume, _fadeInDuration).SetUpdate(true);
        }
        else
        {
            musicSource.volume = volume;
            musicSource.Play();
        }
    }

    /// <summary>
    /// 播放音效（带可选淡入）
    /// </summary>
    public void PlaySFX(string audioName, bool allowOverlap = true, bool fadeIn = true, float volume = 1f, float pitch = 1f)
    {
        
        // 直接播放
        if (_loadedClips.TryGetValue(audioName, out AudioClip clip))
        {
            DoPlaySFX(clip, audioName, allowOverlap, volume, fadeIn, pitch);
        }
        // 加载，然后触发 playMusicAction 回调
        else
        {
            LoadAudio(audioName, true, true);
        }

    }
    
    // 实际播放音效
    private void DoPlaySFX(AudioClip clip, string audioName, bool allowOverlap, float volume = 1f, bool fadeIn = true, float pitch = 1f)
    {
        if (!allowOverlap && _playingInstances.TryGetValue(audioName, out var sources))
        {
            foreach (var src in sources.ToArray())
            {
                if (src.isPlaying) {
                    src.DOFade(0f, _fadeInDuration)
                        .SetUpdate(true)
                        .OnComplete(() => {
                            src.Stop();
                            src.clip = null;
                        });
                }
            }
            _playingInstances.Remove(audioName);
        }
        
        AudioSource freeSource = _sfxPool.Find(s => !s.isPlaying);
        if (freeSource == null) return;

        freeSource.clip = clip;
        freeSource.pitch = pitch;

        if (fadeIn)
        {
            freeSource.volume = 0f;
            freeSource.Play();
            freeSource.DOFade(volume, _fadeInDuration).SetUpdate(true);
        }
        else
        {
            freeSource.volume = volume;
            freeSource.Play();
        }

        StartCoroutine(MarkClipInUse(audioName, clip.length));
        
        if (!_playingInstances.ContainsKey(audioName))
        {
            _playingInstances.Add(audioName, new List<AudioSource>());
        }
        _playingInstances[audioName].Add(freeSource);
        StartCoroutine(TrackPlayingInstance(audioName, freeSource, clip.length));
    }
    

    /// <summary>
    /// 停止音乐（确保淡出完成前不会被中断）
    /// </summary>
    public void StopMusic()
    {
        if (!musicSource.isPlaying || _isMusicFadingOut)
            return; // 正在淡出或未播放时忽略

        _isMusicFadingOut = true;
        float originalVolume = musicSource.volume;

        musicSource.DOFade(0f, _fadeOutDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                musicSource.Stop();
                musicSource.volume = originalVolume; // 恢复音量
                musicSource.clip = null;
                _isMusicFadingOut = false;
            });
    }

    /// <summary>
    /// 停止音效（淡出效果）
    /// </summary>
    public void StopSFX(string path)
    {
        if (!_loadedClips.TryGetValue(path, out AudioClip clip)) return;

        foreach (AudioSource source in _sfxPool)
        {
            if (source.isPlaying && source.clip == clip)
            {
                source.DOFade(0f, _fadeOutDuration)
                    .SetUpdate(true)
                    .OnComplete(() => {
                        source.Stop();
                        source.clip = null;
                    });
            }
        }
    }

    /// <summary>
    /// 停止所有音效（淡出）
    /// </summary>
    public void StopAllSFX()
    {
        foreach (AudioSource source in _sfxPool)
        {
            if (source.isPlaying)
            {
                source.DOFade(0f, _fadeOutDuration)
                    .SetUpdate(true)
                    .OnComplete(() => {
                        source.Stop();
                        source.clip = null;
                    });
            }
        }
    }

    /// <summary>
    /// 停止所有音频（音乐+音效，带淡出效果）
    /// </summary>
    public void StopAll()
    {
        StopMusic();
        StopAllSFX();
    }

    private IEnumerator TrackPlayingInstance(string path, AudioSource source, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (_playingInstances.TryGetValue(path, out var sources))
        {
            sources.Remove(source);
            if (sources.Count == 0)
            {
                _playingInstances.Remove(path);
            }
        }
    }

    /// <summary>
    /// 立刻释放音频资源
    /// </summary>
    /// <param name="audioName">音频名称</param>
    private void UnloadAudio(string audioName)
    {
        if (_loadedClips.TryGetValue(audioName, out AudioClip clip))
        {
            if (musicSource.clip == clip) musicSource.Stop();
            
            AddressableMgr.ReleaseAsset<AudioClip>(audioName);
            _loadedClips.Remove(audioName);
        }
    }

    // 标记为正在播放
    private IEnumerator MarkClipInUse(string path, float duration)
    {
        _clipsInUse[path] = true;
        yield return new WaitForSeconds(duration);
        _clipsInUse.Remove(path);
    }

    /// <summary>
    /// 保证延迟释放音频资源
    /// </summary>
    /// <param name="path"></param>
    public void SafeUnload(string path)
    {
        if (_clipsInUse.ContainsKey(path))
        {
            StartCoroutine(DelayedUnload(path));
            return;
        }
        UnloadAudio(path);
    }

    // 延迟释放
    private IEnumerator DelayedUnload(string path)
    {
        while (_clipsInUse.ContainsKey(path))
        {
            yield return null;
        }
        UnloadAudio(path);
    }

    // 音量控制
    public void SetMusicVolume(float volume) => musicSource.volume = volume;
    public void SetSFXVolume(float volume) => _sfxPool.ForEach(s => s.volume = volume);
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    // [SerializeField] private AudioSource bkMusic;
    //
    // public void LoadAudio(string audioName)
    // {
    //     
    //     AddressableMgr.LoadAssetAsync<AudioClip>(audioName, null);
    // }
    //
    // public void LoadAndPlayBKMusic(string music, bool loop = true, bool fade = true)
    // {
    //     // 如果没有挂载 AudioSource 则主动添加
    //     if (bkMusic == null)
    //     {
    //         GameObject obj = new GameObject
    //         {
    //             name = "BKMusic"
    //         };
    //         GameObject.DontDestroyOnLoad(obj);
    //         bkMusic = obj.AddComponent<AudioSource>();
    //     }
    //     
    //     AddressableMgr.LoadAssetAsync<AudioClip>(music, (obj) =>
    //     {
    //         bkMusic.volume = 0f;
    //         if (fade)
    //         {
    //             bkMusic.DOFade(1f, _fadeInDuration).SetEase(Ease.Linear).onComplete = () =>
    //             {
    //                 bkMusic.clip = obj.Result;
    //                 bkMusic.loop = loop;
    //                 bkMusic.Play();
    //             };
    //         }
    //         else
    //         {
    //             
    //         }
    //     });
    // }
    //
    // public void StopBKMusic()
    // {
    //     if (bkMusic == null)
    //         return;
    //     
    //     bkMusic.Stop();
    // }
    //
    // public void PauseBKMusic()
    // {
    //     if (bkMusic == null)
    //         return;
    //     
    //     bkMusic.Pause();
    // }
    //
    //
    // public class AudioData
    // {
    //     private AudioClip _audioClip;
    //     private string _audioClipName;
    //
    //     public void Play(bool loop, bool fade)
    //     {
    //         
    //     }
    // }
    
}
