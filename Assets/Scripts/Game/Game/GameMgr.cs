using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameMgr : MonoSingleton<GameMgr>
{
    // （当前）关卡控制器
    private CheckpointController _checkpointController;
    
    private IDisposable _subscription1;
    
    protected override void Awake()
    {
        base.Awake();
        // 判断对象是否被销毁，若被销毁则不执行后续代码
        if (this.IsDestroyed()) return;

        // GameMgr 为继承了 MonoBehaviour 的懒汉式单例类，只适合用于初始化游戏进程中 逻辑层 的相关数据
        // 现在这段逻辑只会在游戏启动时执行一次
        Debug.Log("GameMgr Awake()");
        // if (_subscription1 == null)
        // {
        //     _subscription1 = EventCenter.Subscribe<SceneMgr.EndLoadSceneEvent>(GetCheckpointController);
        // }
        // else
        // {
        //     _subscription1.Dispose();
        //     _subscription1 = EventCenter.Subscribe<SceneMgr.EndLoadSceneEvent>(GetCheckpointController);
        //     Debug.Log("GameMgr.GetCheckpointController() 方法当前更新订阅了 SceneMgr.EndLoadSceneEvent 事件");
        // }

    }

    private void GetCheckpointController(SceneMgr.EndLoadSceneEvent evt)
    {
        _checkpointController = FindObjectOfType<CheckpointController>();
        if(_checkpointController == null)
        {
            Debug.LogWarning("场景中没有找到 CheckpointController 组件");
            return;
        }
        _checkpointController.CreateCheckpoint();
    }
    
    // 订阅 SceneMgr.EndLoadSceneEvent 事件
    public void SubscribeEndLoadSceneEvent()
    {
        if (_subscription1 == null)
        {
            _subscription1 = EventCenter.Subscribe<SceneMgr.EndLoadSceneEvent>(GetCheckpointController);
        }
        else
        {
            _subscription1.Dispose();
            _subscription1 = EventCenter.Subscribe<SceneMgr.EndLoadSceneEvent>(GetCheckpointController);
            Debug.Log("GameMgr.GetCheckpointController() 方法当前更新订阅了 SceneMgr.EndLoadSceneEvent 事件");
        }
    }

}
