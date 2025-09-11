using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

// 管理整局游戏的 Canvas：
// gameCanvas：游戏画布，用于显示游戏 UI
// uiCanvas：UI 画布，用于显示游戏界面（或菜单界面）的选项 UI
public class CanvasMgr : MonoSingleton<CanvasMgr>
{
    [SerializeField]
    private Canvas gameCanvas;
    [SerializeField]
    public Canvas uiCanvas;

    public Canvas GameCanvas => gameCanvas;
    public Canvas UICanvas => uiCanvas;
    
    private CanvasMgr() { }

    protected override void Awake()
    {
        base.Awake();

        DOTween.Init();
    }
}
