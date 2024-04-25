using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// �����ṩ���ⲿ���֡�����¼��ķ���
/// �����ṩ���ⲿ���Э�̵ķ���
/// </summary>
public class MonoMgr : BaseManager<MonoMgr>
{
    public MonoController controller;

    public MonoMgr()
    {
        //��֤MonoController�����Ψһ��
        GameObject obj = new GameObject("MonoController");
        controller = obj.AddComponent<MonoController>();
    }

    /// <summary>
    /// ���ⲿ�ṩ�����֡�����¼��ĺ���
    /// </summary>
    /// <param name="fun"></param>
    public void AddUpdateListener(UnityAction fun)
    {
        controller.AddUpdateListener(fun);
    }

    /// <summary>
    /// ���ⲿ�ṩ�������Ƴ�֡�����¼��ĺ���
    /// </summary>
    /// <param name="fun"></param>
    public void RemoveUpdateListener(UnityAction fun)
    {
        controller.RemoveUpdateListener(fun);
    }

    public Coroutine StartCoroutine(IEnumerator routine)
    {
        return controller.StartCoroutine(routine);
    }

    //��string�����ĺ���ֻ�ܿ���controller�еĺ���Э��
    public Coroutine StartCoroutine(string methodName, [DefaultValue("null")] object value)
    {
        return controller.StartCoroutine(methodName, value);
    }

    public Coroutine StartCoroutine(string methodName)
    {
        return controller.StartCoroutine(methodName);
    }

    public void StopCoroutine(string methodName)
    {
        controller.StopCoroutine(methodName);
    }
}
