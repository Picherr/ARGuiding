using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ARGroundPlane : SingletonAutoMono<ARGroundPlane>
{
    public PlaneFinderBehaviour planeFinder;
    private bool isListenerAdded;
    private bool isCreatingModel;

    private void Awake()
    {
        EventCenter.GetInstance().AddEventListener(EventName.AlreadyCreatedModel, RemoveListener);//添加事件
    }

    public void AddListener()
    {
        if (planeFinder == null)
        {
            Debug.LogError("未找到 Vuforia PlaneFinderBehaviour，无法放置虚拟讲解员。");
            return;
        }

        if (isListenerAdded)
        {
            return;
        }

        planeFinder.OnInteractiveHitTest.AddListener(HandleInteractiveHitTest);//添加该事件
        isListenerAdded = true;
    }

    public void SetPlaneFinderActive(bool active)
    {
        if (planeFinder == null)
        {
            Debug.LogWarning("Plane Finder 尚未初始化。");
            return;
        }

        planeFinder.gameObject.SetActive(active);
        if (!active)
        {
            RemoveListener(this, EventArgs.Empty);
            isCreatingModel = false;
        }
    }

    private void HandleInteractiveHitTest(HitTestResult result)
    {
        if (isCreatingModel)
        {
            return;
        }

        isCreatingModel = true;
        RemoveListener(this, EventArgs.Empty);

        //异步加载创建虚拟导游
        ResMgr.GetInstance().LoadAsync<GameObject>("Prefabs/XiaoMing", (obj) =>
        {
            if (obj == null || planeFinder == null || !planeFinder.gameObject.activeInHierarchy)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
                isCreatingModel = false;
                return;
            }

            GameObject groundPlaneStage = GameObject.Find("Ground Plane Stage");
            if (groundPlaneStage == null)
            {
                Debug.LogError("未找到 Ground Plane Stage，无法放置虚拟讲解员。");
                Destroy(obj);
                isCreatingModel = false;
                return;
            }

            Debug.Log("创建小明");
            obj.transform.position = result.Position;
            obj.transform.rotation = Quaternion.identity;
            obj.transform.SetParent(groundPlaneStage.transform);//挂载
            this.TriggerEvent(EventName.AlreadyCreatedModel);
        });
    }

    public void RemoveListener(object sender, EventArgs e)
    {
        if (planeFinder == null || !isListenerAdded)
        {
            return;
        }

        planeFinder.OnInteractiveHitTest.RemoveListener(HandleInteractiveHitTest);//移除该事件
        isListenerAdded = false;
    }

    private void OnDestroy()
    {
        RemoveListener(this, EventArgs.Empty);
        EventCenter.GetInstance().RemoveEventListener(EventName.AlreadyCreatedModel, RemoveListener);//移除事件
    }
}
