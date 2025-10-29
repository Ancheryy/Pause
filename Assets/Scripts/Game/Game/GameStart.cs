using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStart : MonoBehaviour
{
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private GameObject gameTips;
    [SerializeField] private Image background;
    
    public Canvas UICanvas => uiCanvas;
    
    private IDisposable _subscription1;

}
