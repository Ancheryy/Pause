using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 可被吸附的区域
public class AttachableZone1_3 : MonoBehaviour
{
    [SerializeField] private bool isAvaliable = true;
    [SerializeField] private Collider2D targetCld2D;
    
    [SerializeField] public GameObject blueDomeZone;
    [SerializeField] public GameObject glassDomeOpen;
    [SerializeField] public GameObject glassDomeOpenBG;
    [SerializeField] public GameObject note;

    private void Awake()
    {
        targetCld2D = blueDomeZone.GetComponent<Collider2D>();
    }

    /// <summary>
    /// 判断是否可吸附
    /// </summary>
    /// <param name="glassDome">判断的 GlassDome1_3 对象</param>
    /// <returns></returns>
    public bool IsAttachable(GlassDome1_3 glassDome)
    {
        Vector2 checkPos = (Vector2)glassDome.glassDomeClose.transform.position;
        if (isAvaliable && targetCld2D.OverlapPoint(checkPos))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 吸附到目标区域
    /// </summary>
    /// <param name="glassDome">要吸附的 GlassDome1_3 对象</param>
    public void SnapToTarget(GlassDome1_3 glassDome)
    {
        glassDome.glassDomeClose.transform.position = targetCld2D.bounds.center;
        isAvaliable = false;
        glassDome.glassDomeClose.GetComponent<Dragger>().enableDrag = false;
        glassDome.glassDomeClose.SetActive(false);
    }

    /// <summary>
    /// 打开钟罩并播放音乐
    /// </summary>
    /// <param name="glassDome">要打开的 GlassDome1_3 对象</param>
    public void OpenDomeAndPlay(GlassDome1_3 glassDome)
    {
        AnimSequence anim = AnimMgr.Instance.CreateSequence();
        anim.AddNode((() =>
        {
            Checkpoint1_3Gameplay.Instance.isPassed = true;
            glassDome.gameObject.SetActive(false);
            blueDomeZone.SetActive(false);
            glassDomeOpenBG.gameObject.SetActive(true);
            glassDomeOpen.gameObject.SetActive(true);
            glassDomeOpenBG.GetComponent<SpriteFade>().FadeIn(0.2f);
        })).AddWait(0.3f).AddNode(() =>
        {
            glassDomeOpenBG.GetComponent<SpriteFade>().FadeOut(0.2f);
            glassDomeOpen.GetComponent<SpriteFade>().FadeOut(0.2f);
        }).AddWait(0.3f).AddNode(() =>
        {
            note.gameObject.SetActive(true);
            glassDomeOpenBG.gameObject.SetActive(false);
            note.GetComponent<SpriteFade>().FadeIn(0.2f);
        });

        anim.Play();
    }
}