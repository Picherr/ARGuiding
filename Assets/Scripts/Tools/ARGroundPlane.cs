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
        EventCenter.GetInstance().AddEventListener(EventName.AlreadyCreatedModel, RemoveListener);//����¼�
    }

    public void AddListener()
    {
        planeFinder.OnInteractiveHitTest.AddListener(HandleInteractiveHitTest);//��Ӹ��¼�
    }

    private void HandleInteractiveHitTest(HitTestResult result)
    {
        //�첽���ش������⵼��
        ResMgr.GetInstance().LoadAsync<GameObject>("Prefabs/XiaoMing", (obj) =>
        {
            Debug.Log("����С��");
            obj.transform.position = result.Position;
            obj.transform.rotation = Quaternion.identity;
            obj.transform.SetParent(GameObject.Find("Ground Plane Stage").transform);//����
            this.TriggerEvent(EventName.AlreadyCreatedModel);
        });
    }

    public void RemoveListener(object sender, EventArgs e)
    {
        planeFinder.OnInteractiveHitTest.RemoveListener(HandleInteractiveHitTest);//�Ƴ����¼�
    }

    private void OnDestroy()
    {
        EventCenter.GetInstance().RemoveEventListener(EventName.AlreadyCreatedModel, RemoveListener);//�Ƴ��¼�
    }
}
