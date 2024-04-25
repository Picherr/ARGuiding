using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonWithMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T GetInstance()
    {
        //继承了Mono的脚本，不能直接new，只能拖动到对象上或者通过AddComponent添加该脚本
        return instance;
    }

    protected virtual void Awake()//protected virtual方便后续重写复用Awake
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

    public static bool IsInitialized // 提供一个外部确定已经生成instance的方法
    {
        get { return instance != null; }
    }
}
