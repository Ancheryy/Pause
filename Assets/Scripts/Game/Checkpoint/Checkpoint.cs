using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Checkpoint
{
    // 关卡 全局唯一标识
    public int ID { get; }
    
    // 关卡名称
    public string Name { get; }
    
    // 下一关卡名称
    public string NextCheckpointName { get; }

    // 通关订阅
    protected IDisposable _subscription;
    // 关卡预制体
    protected GameObject UIGo;
    
    
    // 策略配置
    private IEnterStrategy _enterStrategy;
    private IExitStrategy _exitStrategy;

    // 设置策略的方法
    protected void SetEnterStrategy(IEnterStrategy strategy) => _enterStrategy = strategy;
    protected void SetExitStrategy(IExitStrategy strategy) => _exitStrategy = strategy;

    // 对关卡内的资源（如：音效）加载方式
    // 加载资源行为
    private UnityAction _loadResourcesAction;
    // 释放资源行为
    private UnityAction _unloadResourcesAction;
    
    // 资源加载/释放方式设置
    protected void SetLoadResourcesAction(UnityAction action) => _loadResourcesAction = action;
    protected void SetUnloadResourcesAction(UnityAction action) => _unloadResourcesAction = action;

    protected Checkpoint(int id, string name, string nextCheckpointName)
    {
        this.ID = id;
        this.Name = name;
        this.NextCheckpointName = nextCheckpointName;
    }

    public virtual void Begin()
    {
        // 1. 添加订阅方法，加载策略，获取 UIGo
        SubscribePassEvent();
        LoadStrategy();
        GetUIPrefab();
        
        // 2. 通过 UIMgr 播放入场动效
        UIMgr.Instance.EnterCheckpoint(
            this.UIGo, 
            _enterStrategy,
            // 3. 实例化 UIGo，且 gameplay 中的变量通过属性主动获取游戏物体引用 
            InitializeGameplay
            );
        
    }

    public virtual void End()
    {
        // 1. 通过UIManager播放出场动效
        UIMgr.Instance.ExitLevel(
            _exitStrategy,
            () => {
                // 2. 释放资源（主要是音效等）
                _unloadResourcesAction?.Invoke();
                // 3. 暂停音效（可选）
                // SoundManager.Instance.StopAll();
                // 4. 取消事件订阅
                _subscription?.Dispose();
                _subscription = null;
                // 5. 开启下一关卡
                StartNextCheckpoint();
            }
        );
    }

    // 调用 AB 包加载
    protected void GetUIPrefab()
    {
        AddressableMgr.LoadAssetAsync<GameObject>(Name, (obj) =>
        {
            UIGo = obj.Result;
        });
    }
    
    // 转到下一关（或在最后一关覆写特殊处理逻辑）
    protected virtual void StartNextCheckpoint()
    {
        Checkpoint nextCheckpoint = CheckpointMgr.Instance.GetCheckpoint(NextCheckpointName);
        nextCheckpoint?.Begin();
    }

    // 加载 Enter 和 Exit 策略
    protected abstract void LoadStrategy();
    // 订阅通关事件
    protected abstract void SubscribePassEvent();
    // 初始化关卡（包含 各游戏物体状态初始化 + SFX 加载 等）
    protected abstract void InitializeGameplay();
}

public class Checkpoint1_1 : Checkpoint
{
    public Checkpoint1_1(int id, string name, string nextCheckpointName) : base(id, name, nextCheckpointName)
    {
        
    }

    public override void Begin()
    {
        throw new System.NotImplementedException();
    }

    public override void End()
    {
        throw new System.NotImplementedException();
    }

    protected override void LoadStrategy()
    {
        throw new System.NotImplementedException();
    }

    protected override void SubscribePassEvent()
    {
        throw new System.NotImplementedException();
    }

    protected override void InitializeGameplay()
    {
        throw new NotImplementedException();
    }
}
