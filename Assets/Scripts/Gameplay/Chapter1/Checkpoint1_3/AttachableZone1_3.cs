using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 可被吸附的区域
public class AttachableZone1_3 : MonoBehaviour
{
    public bool isAvaliable = true;
    [SerializeField] private Collider2D cld2D;
    public Image glassDomeOpen;
    public Image glassDomeOpenBG;
    public Image note;

    private void Awake()
    {
        cld2D = GetComponent<Collider2D>();
    }

    /// <summary>
    /// 判断是否可吸附
    /// </summary>
    /// <param name="glassDome">判断的 GlassDome1_3 对象</param>
    /// <returns></returns>
    public bool IsAttachable(GlassDome1_3 glassDome)
    {
        Vector2 checkPos = (Vector2)glassDome.transform.position;
        if (isAvaliable && cld2D.OverlapPoint(checkPos))
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
        glassDome.transform.position = cld2D.bounds.center;
        isAvaliable = false;
    }

}