using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableItem1_4 : MonoBehaviour
{
    [SerializeField] private int itemID;
    [SerializeField] private GameObject itemHint;

    
    // 启动DoShowHint()方法的协程
    private Coroutine _corHint;
    private bool _needHint = true;
    private bool _isDragging = false;
    private bool _isStartCountTime;
    private float _startTime;
    
    private IDisposable _subscriptionPassCheckpoint;
    
    public int ItemID => itemID;

    private void Awake()
    {
        _subscriptionPassCheckpoint = EventCenter.Subscribe<Checkpoint1_4.PassCheckpoint1_4Event>(StopHint);
    }
    
    private void Start()
    {
        ShowHint();
    }

    private void Update()
    {
        // 8秒未动主动提示
        if(_isStartCountTime && !_isDragging && Time.time - _startTime >= 8.0f)
        {
            _needHint = true;
            ShowHint();
            _isStartCountTime = false;
        }
    }
    
    // 显示提示
    private void ShowHint()
    {
        itemHint.gameObject.SetActive(true);

        if(_corHint == null)
        {
            _corHint = MonoMgr.StartGlobalCoroutine(DoShowHint());
        }
        
        IEnumerator DoShowHint()
        {
            itemHint.gameObject.SetActive(false);
            yield return new WaitForSeconds(1.8f);

            itemHint.gameObject.SetActive(true);
            while (_needHint)
            {
                if (Checkpoint1_4Gameplay.Instance.isPassed) yield break;
                itemHint.GetComponent<SpriteFade>().FadeIn(0.5f);

                yield return new WaitForSeconds(0.5f);
                if (Checkpoint1_4Gameplay.Instance.isPassed) yield break;
                itemHint.GetComponent<SpriteFade>().FadeOut(0.5f);

                yield return new WaitForSeconds(0.5f);
                if (Checkpoint1_4Gameplay.Instance.isPassed) yield break;
            }

            itemHint.gameObject.SetActive(false);
        }
    }

    // 停止提示
    public void StopHint(Checkpoint1_4.PassCheckpoint1_4Event evt = null)
    {
        _needHint = false;
        itemHint.gameObject.SetActive(false);
        if(_corHint != null)
        {
            MonoMgr.StopGlobalCoroutine(_corHint);
            _corHint = null;
        }
    }
    
    public void OnBeginDrag()
    {
        _startTime = Time.time;
        _isDragging = true;
    }

    public void OnEndDrag()
    {
        _isDragging = false;
        _isStartCountTime = true;
    }

    /// <summary>
    /// 判断是否附着到指定区域
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool NeedAttachToZone(Collider2D target)
    {
        if (target.OverlapPoint(transform.position))
        {
            return true;
        }

        return false;
    }
    
}
