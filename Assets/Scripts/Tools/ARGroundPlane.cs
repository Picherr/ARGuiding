using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ARGroundPlane : SingletonAutoMono<ARGroundPlane>
{
    public PlaneFinderBehaviour planeFinder;

    private void Awake()
    {
        EventCenter.GetInstance().AddEventListener(EventName.AlreadyCreatedModel, RemoveListener);//添加事件
    }

    public void AddListener()
    {
        planeFinder.OnInteractiveHitTest.AddListener(HandleInteractiveHitTest);//添加该事件
    }

    private void HandleInteractiveHitTest(HitTestResult result)
    {
        //异步加载创建虚拟导游
        ResMgr.GetInstance().LoadAsync<GameObject>("Prefabs/XiaoMing", (obj) =>
        {
            Debug.Log("创建小明");
            obj.transform.position = result.Position;
            obj.transform.rotation = Quaternion.identity;
            obj.transform.SetParent(GameObject.Find("Ground Plane Stage").transform);//挂载
            this.TriggerEvent(EventName.AlreadyCreatedModel);
        });
    }

    public void RemoveListener(object sender, EventArgs e)
    {
        planeFinder.OnInteractiveHitTest.RemoveListener(HandleInteractiveHitTest);//移除该事件
    }

    private void OnDestroy()
    {
        EventCenter.GetInstance().RemoveEventListener(EventName.AlreadyCreatedModel, RemoveListener);//移除事件
    }
}
