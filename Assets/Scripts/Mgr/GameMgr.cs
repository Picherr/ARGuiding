using System;
using UnityEngine;
using Vuforia;

/// <summary>
/// ��Ϸ�ܹ�����
/// ����Ӧ�ÿ���ʱ�ĳ�ʼ������
/// </summary>
public class GameMgr : SingletonWithMono<GameMgr>
{
    protected override void Awake()
    {
        base.Awake();

        //��ʼ�����
        UIManager.GetInstance().ShowPanel<MapPanel>("MapPanel", UI_Layer.Bot);
        UIManager.GetInstance().ShowPanel<GuidingPanel>("GuidingPanel", UI_Layer.Mid);
        UIManager.GetInstance().ShowPanel<InfoPanel>("InfoPanel", UI_Layer.Top);
        UIManager.GetInstance().ShowPanel<SystemPanel>("SystemPanel", UI_Layer.System);

        //�����д���UICamera������ȾLineRendererInMap
        ResMgr.GetInstance().LoadAsync<GameObject>("Prefabs/RouteCamera", (camera) =>
        {
            camera.name = "RouteCamera";
        });

        //�򿪳�����״��Զ���λ��˳���ʼ��GaoDeAPI
        GaoDeAPI.GetInstance().OnLocating();
        //this.TriggerEvent(EventName.LocatedTheFstTime);

        ARGroundPlane.GetInstance().planeFinder = GameObject.FindObjectOfType<PlaneFinderBehaviour>();
        ARGroundPlane.GetInstance().planeFinder.gameObject.SetActive(false);//ʹPlaneFinder��ʧ��Ƚ���AR����ʱ�ٿ���
    }
}
