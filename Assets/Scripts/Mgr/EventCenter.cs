using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ���ڴ����¼�����չ��
/// </summary>
public static class EventTriggerExt
{
    /// <summary>
    /// �����¼����޲�����
    /// </summary>
    /// <param name="sender">����Դ</param>
    /// <param name="eventName">�¼���</param>
    public static void TriggerEvent(this object sender, EventName eventName)
    {
        EventCenter.GetInstance().TriggerEvent(eventName, sender);
    }

    /// <summary>
    /// �����¼����в�����
    /// </summary>
    /// <param name="sender">����Դ</param>
    /// <param name="eventName">�¼���</param>
    /// <param name="args">�¼�����</param>
    public static void TriggerEvent(this object sender, EventName eventName, EventArgs args)
    {
        EventCenter.GetInstance().TriggerEvent(eventName, sender, args);
    }
}

/// <summary>
/// �¼�����ģ��-����ģʽ����
/// 1.�۲���ģʽ
/// 2.ί��
/// 3.Dictionary
/// </summary>
public class EventCenter : BaseManager<EventCenter>
{
    private Dictionary<EventName, EventHandler> handlerDic = new Dictionary<EventName, EventHandler>();

    /// <summary>
    /// ���һ���¼��ļ�����
    /// </summary>
    /// <param name="eventName">�¼���</param>
    /// <param name="handler">�¼�������</param>
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
    /// �Ƴ�һ���¼��ļ�����
    /// </summary>
    /// <param name="eventName">�¼���</param>
    /// <param name="handler">�¼�������</param>
    public void RemoveEventListener(EventName eventName, EventHandler handler)
    {
        if (handlerDic.ContainsKey(eventName))
        {
            handlerDic[eventName] -= handler;
        }
    }

    /// <summary>
    /// �����¼����޲�����
    /// </summary>
    /// <param name="eventName">�¼���</param>
    /// <param name="sender">����Դ</param>
    public void TriggerEvent(EventName eventName, object sender)
    {
        if (handlerDic.ContainsKey(eventName))
        {
            handlerDic[eventName]?.Invoke(sender, EventArgs.Empty);
        }
    }

    /// <summary>
    /// �����¼����в�����
    /// </summary>
    /// <param name="eventName">�¼���</param>
    /// <param name="sender">����Դ</param>
    /// <param name="args">�¼�����</param>
    public void TriggerEvent(EventName eventName, object sender, EventArgs args)
    {
        if (handlerDic.ContainsKey(eventName))
        {
            handlerDic[eventName]?.Invoke(sender, args);
        }
    }

    /// <summary>
    /// ��������¼�
    /// </summary>
    public void Clear()
    {
        handlerDic.Clear();
    }
}

/*/// <summary>
/// ���objectװ����������
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
/// �¼�����ģ��-����ģʽ����
/// 1.Dictionary
/// 2.ί��
/// 3.�۲������ģʽ
/// 4.����
/// </summary>
public class EventCenter : BaseManager<EventCenter>
{
    //key-�¼������֣����磺�������������������ͨ�صȣ�
    //value-��Ӧ��������¼���Ӧ��ί�к�����
    //private Dictionary<string, UnityAction<object>> eventDic = new Dictionary<string, UnityAction<object>>();
    //��object�����ܲ�������װ����䣬����Ϸ�в������ݶ���������࣬�����ıȽ���
    //�������ߺ������������Ѳ�����װ�������ṹ���࣬�ٴ���
    private Dictionary<string, IEventInfo> eventDic = new Dictionary<string, IEventInfo>();

    /// <summary>
    /// ����¼�����
    /// </summary>
    /// <param name="eventName">�¼�������</param>
    /// <param name="action">׼�����������¼���ί�к���</param>
    public void AddEventListener<T>(string eventName, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(eventName))// �Ƿ���ڸ��¼�
        {
            (eventDic[eventName] as EventInfo<T>).actions += action;
        }
        else// ����������¼�
        {
            eventDic.Add(eventName, new EventInfo<T>(action));
        }
    }

    /// <summary>
    /// ����¼�����-�޲���
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="action"></param>
    public void AddEventListener(string eventName, UnityAction action)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo).actions += action;
            Debug.Log("EventCenter���в��ɹ�����¼���" + eventName);
        }
        else
        {
            eventDic.Add(eventName, new EventInfo(action));
            Debug.Log("EventCenter�ɹ�����¼���" + eventName);
        }
    }

    /// <summary>
    /// �Ƴ���Ӧ���¼�����
    /// ������ܵ����ڴ�й¶
    /// </summary>
    /// <param name="eventName">�¼�������</param>
    /// <param name="action">��Ӧ֮ǰ��ӵ�ί�к���</param>
    public void RemoveEventListener<T>(string eventName, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo<T>).actions -= action;
        }
    }

    /// <summary>
    /// �Ƴ���Ӧ���¼�����-�޲���
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="action"></param>
    public void RemoveEventListener(string eventName, UnityAction action)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo).actions -= action;
            Debug.Log("EventCenter�Ƴ��¼���" + eventName);
        }
    }

    /// <summary>
    /// �¼�����
    /// </summary>
    /// <param name="eventName">��һ�����ֵ��¼�������</param>
    /// <param name="obj"></param>
    public void EventTrigger<T>(string eventName, T info)
    {
        if (eventDic.ContainsKey(eventName))
        {
            if ((eventDic[eventName] as EventInfo<T>).actions != null)
                (eventDic[eventName] as EventInfo<T>).actions.Invoke(info);
            //eventDic[eventName]?.Invoke(obj);//?ȷ�����ж����ߣ���ͬ���������
            *//*if (eventDic[eventName] != null)
            {
                eventDic[eventName](obj);
            }*//*
        }
    }

    /// <summary>
    /// �¼�����-�޲���
    /// </summary>
    /// <param name="eventName"></param>
    public void EventTrigger(string eventName)
    {
        Debug.Log("����EventTrigger");
        if (eventDic.ContainsKey(eventName))
        {
            Debug.Log("EventTrigger�����¼�������" + eventName);
            if ((eventDic[eventName] as EventInfo).actions != null)
            {
                (eventDic[eventName] as EventInfo).actions.Invoke();
                Debug.Log("EventTrigger�¼�������" + eventName);
            }
        }
    }

    /// <summary>
    /// ����¼�����
    /// ��Ҫ���ڳ����л�ʱ
    /// </summary>
    public void Clear()
    {
        eventDic.Clear();
    }
}*/
