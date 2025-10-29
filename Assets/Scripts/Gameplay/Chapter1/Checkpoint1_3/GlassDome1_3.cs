using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GlassDome1_3 : MonoBehaviour
{
    [SerializeField] public int glassDomeId;
    [SerializeField] public bool isClickable = true;
    [SerializeField] public GameObject glassDomeClose;
    [SerializeField] public GameObject glassDomeOpen;
    [SerializeField] public Collider2D cld2D;
    
    private Vector3 _originalPos;

    private void Awake()
    {
        _originalPos = glassDomeClose.transform.position;
    }
    
    
    public void OpenGlassDome()
    {
        glassDomeClose.gameObject.SetActive(false);
        glassDomeOpen.gameObject.SetActive(true);
    }

    public void CloseGlassDome()
    {
        glassDomeClose.gameObject.SetActive(true);
        glassDomeOpen.gameObject.SetActive(false);
    }
    
    public void ResetPosition()
    {
        glassDomeClose.transform.position = _originalPos;
    }

}