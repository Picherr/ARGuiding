using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 便于触发事件的扩展类
/// </summary>
public static class EventTriggerExt
{
    /// <summary>
    /// 触发事件（无参数）
    /// </summary>
    /// <param name="sender">触发源</param>
    /// <param name="eventName">事件名</param>
    public static void TriggerEvent(this object sender, EventName eventName)
    {
        EventCenter.GetInstance().TriggerEvent(eventName, sender);
    }

    /// <summary>
    /// 触发事件（有参数）
    /// </summary>
    /// <param name="sender">触发源</param>
    /// <param name="eventName">事件名</param>
    /// <param name="args">事件参数</param>
    public static void TriggerEvent(this object sender, EventName eventName, EventArgs args)
    {
        EventCenter.GetInstance().TriggerEvent(eventName, sender, args);
    }
}

/// <summary>
/// 事件中心模块-单例模式对象
/// 1.观察者模式
/// 2.委托
/// 3.Dictionary
/// </summary>
public class EventCenter : BaseManager<EventCenter>
{
    private Dictionary<EventName, EventHandler> handlerDic = new Dictionary<EventName, EventHandler>();

    /// <summary>
    /// 添加一个事件的监听者
    /// </summary>
    /// <param name="eventName">事件名</param>
    /// <param name="handler">事件处理函数</param>
    public void AddEventListener(EventName eventName, EventHandler handler)
    {
        if (handlerDic.ContainsKey(eventName))
        {
            handlerDic[eventName] += handler;
        }
        else
        {
            handlerDic.Add(eventName, handler);
        }
    }

    /// <summary>
    /// 移除一个事件的监听者
    /// </summary>
    /// <param name="eventName">事件名</param>
    /// <param name="handler">事件处理函数</param>
    public void RemoveEventListener(EventName eventName, EventHandler handler)
    {
        if (handlerDic.ContainsKey(eventName))
        {
            handlerDic[eventName] -= handler;
        }
    }

    /// <summary>
    /// 触发事件（无参数）
    /// </summary>
    /// <param name="eventName">事件名</param>
    /// <param name="sender">触发源</param>
    public void TriggerEvent(EventName eventName, object sender)
    {
        if (handlerDic.ContainsKey(eventName))
        {
            handlerDic[eventName]?.Invoke(sender, EventArgs.Empty);
        }
    }

    /// <summary>
    /// 触发事件（有参数）
    /// </summary>
    /// <param name="eventName">事件名</param>
    /// <param name="sender">触发源</param>
    /// <param name="args">事件参数</param>
    public void TriggerEvent(EventName eventName, object sender, EventArgs args)
    {
        if (handlerDic.ContainsKey(eventName))
        {
            handlerDic[eventName]?.Invoke(sender, args);
        }
    }

    /// <summary>
    /// 清空所有事件
    /// </summary>
    public void Clear()
    {
        handlerDic.Clear();
    }
}

/*/// <summary>
/// 解决object装箱拆箱的问题
/// </summary>
public interface IEventInfo
{

}

public class EventInfo<T> : IEventInfo
{
    public UnityAction<T> actions;

    public EventInfo(UnityAction<T> action)
    {
        actions += action;
    }
}

public class EventInfo : IEventInfo
{
    public UnityAction actions;

    public EventInfo(UnityAction action)
    {
        actions += action;
    }
}

/// <summary>
/// 事件中心模块-单例模式对象
/// 1.Dictionary
/// 2.委托
/// 3.观察者设计模式
/// 4.泛型
/// </summary>
public class EventCenter : BaseManager<EventCenter>
{
    //key-事件的名字（比如：怪物死亡，玩家死亡，通关等）
    //value-对应监听这个事件对应的委托函数们
    //private Dictionary<string, UnityAction<object>> eventDic = new Dictionary<string, UnityAction<object>>();
    //用object当万能参数，有装箱拆箱，但游戏中参数传递多半是引用类，故消耗比较少
    //若订阅者含多个参数，则把参数封装成数组或结构或类，再传入
    private Dictionary<string, IEventInfo> eventDic = new Dictionary<string, IEventInfo>();

    /// <summary>
    /// 添加事件监听
    /// </summary>
    /// <param name="eventName">事件的名字</param>
    /// <param name="action">准备用来处理事件的委托函数</param>
    public void AddEventListener<T>(string eventName, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(eventName))// 是否存在该事件
        {
            (eventDic[eventName] as EventInfo<T>).actions += action;
        }
        else// 无则加入新事件
        {
            eventDic.Add(eventName, new EventInfo<T>(action));
        }
    }

    /// <summary>
    /// 添加事件监听-无参数
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="action"></param>
    public void AddEventListener(string eventName, UnityAction action)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo).actions += action;
            Debug.Log("EventCenter已有并成功添加事件：" + eventName);
        }
        else
        {
            eventDic.Add(eventName, new EventInfo(action));
            Debug.Log("EventCenter成功添加事件：" + eventName);
        }
    }

    /// <summary>
    /// 移除对应的事件监听
    /// 否则可能导致内存泄露
    /// </summary>
    /// <param name="eventName">事件的名字</param>
    /// <param name="action">对应之前添加的委托函数</param>
    public void RemoveEventListener<T>(string eventName, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo<T>).actions -= action;
        }
    }

    /// <summary>
    /// 移除对应的事件监听-无参数
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="action"></param>
    public void RemoveEventListener(string eventName, UnityAction action)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo).actions -= action;
            Debug.Log("EventCenter移除事件：" + eventName);
        }
    }

    /// <summary>
    /// 事件触发
    /// </summary>
    /// <param name="eventName">哪一个名字的事件触发了</param>
    /// <param name="obj"></param>
    public void EventTrigger<T>(string eventName, T info)
    {
        if (eventDic.ContainsKey(eventName))
        {
            if ((eventDic[eventName] as EventInfo<T>).actions != null)
                (eventDic[eventName] as EventInfo<T>).actions.Invoke(info);
            //eventDic[eventName]?.Invoke(obj);//?确保还有订阅者，等同于下列语句
            *//*if (eventDic[eventName] != null)
            {
                eventDic[eventName](obj);
            }*//*
        }
    }

    /// <summary>
    /// 事件触发-无参数
    /// </summary>
    /// <param name="eventName"></param>
    public void EventTrigger(string eventName)
    {
        Debug.Log("进入EventTrigger");
        if (eventDic.ContainsKey(eventName))
        {
            Debug.Log("EventTrigger进入事件触发：" + eventName);
            if ((eventDic[eventName] as EventInfo).actions != null)
            {
                (eventDic[eventName] as EventInfo).actions.Invoke();
                Debug.Log("EventTrigger事件触发：" + eventName);
            }
        }
    }

    /// <summary>
    /// 清空事件中心
    /// 主要用在场景切换时
    /// </summary>
    public void Clear()
    {
        eventDic.Clear();
    }
}*/
