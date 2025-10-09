using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class Checkpoint
{
    // 关卡 全局唯一标识
    public int ID { get; }
    // 关卡名称
    public string Name { get; }
    // 关卡淡入淡出的遮罩
    // public Image UIMask;
    // 下一关卡名称
    public string NextCheckpointName { get; }
    
    // 策略配置
    public IEnterStrategy EnterStrategy;
    public IExitStrategy ExitStrategy;
    
    // 加载订阅
    protected IDisposable Subscription1;
    // 通关订阅
    protected IDisposable Subscription2;

    // 设置策略的方法
    protected void SetEnterStrategy(IEnterStrategy strategy) => EnterStrategy = strategy;
    protected void SetExitStrategy(IExitStrategy strategy) => ExitStrategy = strategy;

    // 对关卡内的资源（如：音效）加载方式
    // 加载资源行为
    private UnityAction _loadResourcesAction;
    // 释放资源行为
    private UnityAction _exitCheckpointAction;
    
    // 资源加载/释放方式设置
    protected void SetLoadResourcesAction(UnityAction action) => _loadResourcesAction = action;
    protected void SetExitCheckpointAction(UnityAction action) => _exitCheckpointAction = action;

    private UnityAction _onEnterComplete;
    protected void SetOnEnterCompleteAction(UnityAction action) => _onEnterComplete = action;

    protected Checkpoint(int id, string name, string nextCheckpointName)
    {
        this.ID = id;
        this.Name = name;
        this.NextCheckpointName = nextCheckpointName;
    }

    public virtual void Begin()
    {
        // 1. 添加订阅方法，加载策略
        SubscribePassEvent();
        LoadStrategy();
        
        // 2. 初始化关卡（包含 各游戏物体状态初始化 + SFX 加载 等）
        InitializeGameplay();
        
        // 3. 注册关卡结束事件（关卡结束后会调用）
        EventCenter.Subscribe<SceneMgr.ExitCompleteEvent>((evt) =>
        {
            // 1. 检查注册关卡的事件是否匹配（就是说 Publish 事件中的关卡参数 需要与 该关卡真正需要调用的方法 匹配，防止其他关卡 Begin 方法同时被调用）
            if (evt.TriggerCheckpoint.ID != this.ID)
            {
                return;
            }

            // 2. 释放资源（主要是音效等）
            _exitCheckpointAction?.Invoke();
            // 3. 暂停音效（可选）
            // SoundManager.Instance.StopAll();
            // 4. 取消事件订阅
            Subscription1?.Dispose();
            Subscription2?.Dispose();
            Subscription1 = null;
            Subscription2 = null;
            // 5. 开启下一关卡
            StartNextCheckpoint();
        });
        
        // 4. 通过 UIMgr 播放入场动效
        SceneMgr.Instance.EnterCheckpoint(this);
        
    }

    protected virtual void End()
    {
        // 1. 通过UIManager播放出场动效
        SceneMgr.Instance.ExitLevel(this);
    }
    
    private void GetUIMask()
    {
        
    }

    
    
    // 订阅加载关卡场景的事件 
    protected abstract void SubscribeLoadEvent();
    // 加载 Enter 和 Exit 策略
    protected abstract void LoadStrategy();
    // 订阅通关事件
    protected abstract void SubscribePassEvent();
    // 初始化关卡（包含 各游戏物体状态初始化 + SFX 加载 等）
    protected abstract void InitializeGameplay();
    // 
    protected abstract void StartNextCheckpoint();
}

// 泛型基类，自动处理各个关卡的事件订阅
// 每个关卡加载顺序说明：
/* 1. （上一关卡）StartNextCheckpoint() 或 在第一关读取进度并直接加载 -- SceneMgr.Instance.LoadScene<T4>(nextCheckpoint.Name)  =>  EventCenter.Publish 下一关卡的加载事件，如：Checkpoint1_1.LoadCheckpoint1_1Event
 * 2. （当前关卡）BeginAfterLoad(T1 evt) -- base.Begin() -- InitializeGameplay()（这里的初始化关卡逻辑可以选择在各个关卡的 Gameplay 类中直接通过 Awake() 调用） -- SceneMgr.Instance.EnterCheckpoint(this) 展示入场效果（如：淡入）  =>  EventCenter.Publish 场景加载结束事件 SceneMgr.EnterSceneCompleteEvent
 * 3. 当前关卡游戏逻辑结束  =>  Event.Publish 关卡通关事件，如：Checkpoint1_1.PassCheckpoint1_1Event
 * 4. Check(T2 evt) -- base.End() -- SceneMgr.Instance.ExitLevel(this) 展示退场效果（如：淡出）  =>  EventCenter.Publish 结束退场事件 SceneMgr.ExitCompleteEvent
 * 5. （base.Begin() 中）匿名函数 -- StartNextCheckpoint()（判断是否是最后一关，如果是，退出；如果不是，继续开启下一关） -- SceneMgr.Instance.LoadScene<T4>(nextCheckpoint.Name)  =>  开启循环
 */
public abstract class Checkpoint<T1, T2, T3, T4> : Checkpoint where T1 : EventCenter.IEvent where T2 : EventCenter.IEvent where T3 : EventCenter.IEvent, new() where T4 : EventCenter.IEvent, new()
{
    // 只要实例化，就一定会调用订阅方法
    protected Checkpoint(int id, string name, string nextCheckpointName) : base(id, name, nextCheckpointName)
    {
        SubscribeLoadEvent();
        // SubscribePassEvent();
        // InitializeGameplay();
    }
    
    // 为子类定制化包装 Begin() 方法
    private void BeginAfterLoad(T1 evt)
    {
        base.Begin();
        
    }
    // 订阅关卡加载结束事件
    protected sealed override void SubscribeLoadEvent()
    {
        Subscription1 = EventCenter.Subscribe<T1>(BeginAfterLoad);
    }

    // 检查并开启下一关
    private void Check(T2 evt)
    {
        if (true)
        {
            End();
        }
    }
    // 订阅通关事件
    protected sealed override void SubscribePassEvent()
    {
        Subscription2 = EventCenter.Subscribe<T2>(Check);
    }
    
    // 发布游戏初始化事件
    protected sealed override void InitializeGameplay()
    {
        EventCenter.Publish<T3>(new T3());
    }
    
    
    // 转到下一关
    protected sealed override  void StartNextCheckpoint()
    {
        if (typeof(T4) != typeof(EndOfCheckpointEvent))
        {
            Checkpoint nextCheckpoint = FlowController.GetCheckpoint(NextCheckpointName);
            SceneMgr.Instance.LoadScene<T4>(nextCheckpoint.Name);
        }
        else
        {
            // 添加退出章节逻辑（或加载下一章节的逻辑）
            
        }
    }
    
}

public class Checkpoint1_1 : Checkpoint<Checkpoint1_1.LoadCheckpoint1_1Event, Checkpoint1_1.PassCheckpoint1_1Event, Checkpoint1_1.InitCheckpoint1_1Event, Checkpoint1_2.LoadCheckpoint1_2Event>
{
    public Checkpoint1_1(int id = 101, string name = "Checkpoint1_1", string nextCheckpointName = "Checkpoint1_2") : base(id, name, nextCheckpointName)
    {
        
    }

    protected override void LoadStrategy()
    {
        SetEnterStrategy(new StraightEnter());
        SetExitStrategy(new DefaultFadeExit());
    }
    
    
    
    // 加载场景事件（订阅方法应该有：关卡启动方法）
    public class LoadCheckpoint1_1Event : EventCenter.IEvent { }
    // 通关事件
    public class PassCheckpoint1_1Event : EventCenter.IEvent { }
    // 初始化关卡事件
    public class InitCheckpoint1_1Event : EventCenter.IEvent { }
}

public class Checkpoint1_2 : Checkpoint<Checkpoint1_2.LoadCheckpoint1_2Event, Checkpoint1_2.PassCheckpoint1_2Event, Checkpoint1_2.InitCheckpoint1_2Event, Checkpoint1_3.LoadCheckpoint1_3Event>
{
    public Checkpoint1_2(int id = 102, string name = "Checkpoint1_2", string nextCheckpointName = "Checkpoint1_3") : base(id, name, nextCheckpointName)
    {
        
    }

    protected override void LoadStrategy()
    {
        SetEnterStrategy(new DefaultFadeEnter());
        SetExitStrategy(new DefaultFadeExit());
    }
    
    
    
    // 加载场景事件（订阅方法应该有：关卡启动方法）
    public class LoadCheckpoint1_2Event : EventCenter.IEvent { }
    // 通关事件
    public class PassCheckpoint1_2Event : EventCenter.IEvent { }
    // 初始化关卡事件
    public class InitCheckpoint1_2Event : EventCenter.IEvent { }
}

public class Checkpoint1_3 : Checkpoint<Checkpoint1_3.LoadCheckpoint1_3Event, Checkpoint1_3.PassCheckpoint1_3Event, Checkpoint1_3.InitCheckpoint1_3Event, Checkpoint1_4.LoadCheckpoint1_4Event>
{
    public Checkpoint1_3(int id = 103, string name = "Checkpoint1_3", string nextCheckpointName = "Checkpoint1_4") : base(id, name, nextCheckpointName)
    {
        
    }

    protected override void LoadStrategy()
    {
        SetEnterStrategy(new DefaultFadeEnter());
        SetExitStrategy(new DefaultFadeExit());
    }
    
    
    
    // 加载场景事件（订阅方法应该有：关卡启动方法）
    public class LoadCheckpoint1_3Event : EventCenter.IEvent { }
    // 通关事件
    public class PassCheckpoint1_3Event : EventCenter.IEvent { }
    // 初始化关卡事件
    public class InitCheckpoint1_3Event : EventCenter.IEvent { }
}

public class Checkpoint1_4 : Checkpoint<Checkpoint1_4.LoadCheckpoint1_4Event, Checkpoint1_4.PassCheckpoint1_4Event, Checkpoint1_4.InitCheckpoint1_4Event, Checkpoint1_5.LoadCheckpoint1_5Event>
{
    public Checkpoint1_4(int id = 104, string name = "Checkpoint1_4", string nextCheckpointName = "Checkpoint1_5") : base(id, name, nextCheckpointName)
    {
        
    }

    protected override void LoadStrategy()
    {
        throw new System.NotImplementedException();
    }
    
    
    
    // 加载场景事件（订阅方法应该有：关卡启动方法）
    public class LoadCheckpoint1_4Event : EventCenter.IEvent { }
    // 通关事件
    public class PassCheckpoint1_4Event : EventCenter.IEvent { }
    // 初始化关卡事件
    public class InitCheckpoint1_4Event : EventCenter.IEvent { }
}

public class Checkpoint1_5 : Checkpoint<Checkpoint1_5.LoadCheckpoint1_5Event, Checkpoint1_5.PassCheckpoint1_5Event, Checkpoint1_5.InitCheckpoint1_5Event, Checkpoint1_6.LoadCheckpoint1_6Event>
{
    public Checkpoint1_5(int id = 105, string name = "Checkpoint1_5", string nextCheckpointName = "Checkpoint1_6") : base(id, name, nextCheckpointName)
    {
        
    }

    protected override void LoadStrategy()
    {
        throw new System.NotImplementedException();
    }
    
    
    
    // 加载场景事件（订阅方法应该有：关卡启动方法）
    public class LoadCheckpoint1_5Event : EventCenter.IEvent { }
    // 通关事件
    public class PassCheckpoint1_5Event : EventCenter.IEvent { }
    // 初始化关卡事件
    public class InitCheckpoint1_5Event : EventCenter.IEvent { }
}

public class Checkpoint1_6 : Checkpoint<Checkpoint1_6.LoadCheckpoint1_6Event, Checkpoint1_6.PassCheckpoint1_6Event, Checkpoint1_6.InitCheckpoint1_6Event, Checkpoint1_7.LoadCheckpoint1_7Event>
{
    public Checkpoint1_6(int id = 106, string name = "Checkpoint1_6", string nextCheckpointName = "Checkpoint1_7") : base(id, name, nextCheckpointName)
    {
        
    }

    protected override void LoadStrategy()
    {
        throw new System.NotImplementedException();
    }
    
    
    
    // 加载场景事件（订阅方法应该有：关卡启动方法）
    public class LoadCheckpoint1_6Event : EventCenter.IEvent { }
    // 通关事件
    public class PassCheckpoint1_6Event : EventCenter.IEvent { }
    // 初始化关卡事件
    public class InitCheckpoint1_6Event : EventCenter.IEvent { }
}

public class Checkpoint1_7 : Checkpoint<Checkpoint1_7.LoadCheckpoint1_7Event, Checkpoint1_7.PassCheckpoint1_7Event, Checkpoint1_7.InitCheckpoint1_7Event, EndOfCheckpointEvent>
{
    public Checkpoint1_7(int id = 107, string name = "Checkpoint1_7", string nextCheckpointName = "None") : base(id, name, nextCheckpointName)
    {
        
    }

    protected override void LoadStrategy()
    {
        throw new System.NotImplementedException();
    }
    
    
    
    // 加载场景事件（订阅方法应该有：关卡启动方法）
    public class LoadCheckpoint1_7Event : EventCenter.IEvent { }
    // 通关事件
    public class PassCheckpoint1_7Event : EventCenter.IEvent { }
    // 初始化关卡事件
    public class InitCheckpoint1_7Event : EventCenter.IEvent { }
}






public class EndOfCheckpointEvent : EventCenter.IEvent { }
