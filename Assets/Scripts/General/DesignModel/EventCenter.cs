using System;
using System.Collections.Generic;
using UnityEngine;

public class EventCenter
{
    private static readonly Dictionary<Type, List<ISubscription>> Subscriptions = new Dictionary<Type, List<ISubscription>>();
    private static readonly List<ISubscription> PendingSubscribes = new List<ISubscription>();
    private static readonly List<ISubscription> PendingUnsubscribes = new List<ISubscription>();

    private static bool _isPublishing = false;
    
    /// <summary>
    /// 订阅某事件，并注册其处理方法
    /// </summary>
    /// <param name="action">处理方法</param>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <returns>该订阅的销毁</returns>
    public static IDisposable Subscribe<TEvent>(Action<TEvent> action) where TEvent : IEvent
    {
        var subscription = new Subscription<TEvent>(action);

        if (_isPublishing)
        {
            PendingSubscribes.Add(subscription);
        }
        else
        {
            AddSubscription(subscription);
        }
        
        return subscription;
    }

    /// <summary>
    /// 发布事件
    /// </summary>
    /// <param name="evt">事件</param>
    /// <typeparam name="TEvent">事件类型</typeparam>
    public static void Publish<TEvent>(TEvent evt) where TEvent : IEvent
    {
        var eventType = typeof(TEvent);
        if (!Subscriptions.TryGetValue(eventType, out var subscriptions)) 
            return;

        _isPublishing = true;
        try
        {
            foreach (var subscription in subscriptions.ToArray())
            {
                if (subscription is Subscription<TEvent> typedSubscription && !subscription.IsDisposed)
                {
                    try
                    {
                        typedSubscription.Invoke(evt);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"EventBus: Error handling event {eventType.Name}: {ex}");
                    }
                }
            }
        }
        finally
        {
            _isPublishing = false;
            ProcessPendingOperations();
        }
    }
    
    // 统一处理待处理的订阅/取消订阅操作
    private static void ProcessPendingOperations()
    {
        // 先处理取消订阅
        foreach (var subscription in PendingUnsubscribes)
        {
            RemoveSubscription(subscription);
        }
        PendingUnsubscribes.Clear();

        // 再处理新订阅
        foreach (var subscription in PendingSubscribes)
        {
            AddSubscription(subscription);
        }
        PendingSubscribes.Clear();
    }


    // 添加订阅
    private static void AddSubscription(ISubscription subscription)
    {
        var type = subscription.EventType;
        if (!Subscriptions.ContainsKey(type))
        {
            Subscriptions[type] = new List<ISubscription>();
        }
        Subscriptions[type].Add(subscription);
    }

    // 取消订阅
    private static void RemoveSubscription(ISubscription subscription)
    {
        var type = subscription.EventType;
        if (!Subscriptions.TryGetValue(type, out var subscriptions))
            return;
        
        subscriptions.Remove(subscription);
        if (subscriptions.Count == 0)
        {
            Subscriptions.Remove(type);
        }
    }
    
    
    // 取消订阅逻辑
    private static void ScheduleUnsubscribe(ISubscription subscription)
    {
        if (_isPublishing)
        {
            PendingUnsubscribes.Add(subscription);
        }
        else
        {
            RemoveSubscription(subscription);
        }
    }

    public static void Clear()
    {
        Subscriptions.Clear();
        PendingSubscribes.Clear();
        PendingUnsubscribes.Clear();
    }
    
    #region 内部接口和类
    
    public interface IEvent { }

    // 封装事件类型和销毁状态
    private interface ISubscription
    {
        Type EventType { get; }
        bool IsDisposed { get; }
    }

    private class Subscription<TEvent> : ISubscription, IDisposable where TEvent : IEvent
    {
        private Action<TEvent> _action;
        private bool _isDisposed;
        
        public Type EventType => typeof(TEvent);
        public bool IsDisposed => _isDisposed;
        
        public Subscription(Action<TEvent> action)
        {
            _action = action;
        }

        public void Invoke(TEvent evt)
        {
            _action?.Invoke(evt);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                _action = null;
                _isDisposed = true;
                ScheduleUnsubscribe(this);
            }
        }
    }
    
    #endregion
}
