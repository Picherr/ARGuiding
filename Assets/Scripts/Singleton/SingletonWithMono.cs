using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonWithMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T GetInstance()
    {
        //�̳���Mono�Ľű�������ֱ��new��ֻ���϶��������ϻ���ͨ��AddComponent��Ӹýű�
        return instance;
    }

    protected virtual void Awake()//protected virtual���������д����Awake
    {
        instance = this as T;
    }

    protected virtual void Start()
    {

    }

    protected virtual void OnDestroy()
    {
        if (instance != null)
        {
            instance = null;
        }
    }

    public static bool IsInitialized // �ṩһ���ⲿȷ���Ѿ�����instance�ķ���
    {
        get { return instance != null; }
    }
}
