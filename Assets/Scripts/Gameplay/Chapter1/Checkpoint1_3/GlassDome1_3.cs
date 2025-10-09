using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GlassDome1_3 : MonoBehaviour
{
    public int glassDomeId;
    public bool isClickable = true;
    public GameObject glassDomeOpen;
    public Collider2D cld2D;
    
    private Vector3 originalPos;

    private void Awake()
    {
        originalPos = transform.position;
    }
    
    
    public void OpenGlassDome()
    {
        this.gameObject.SetActive(false);
        glassDomeOpen.gameObject.SetActive(true);
    }

    public void CloseGlassDome()
    {
        this.gameObject.SetActive(true);
        glassDomeOpen.gameObject.SetActive(false);
    }

}