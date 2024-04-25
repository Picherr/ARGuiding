using System;
using UnityEngine;
using Vuforia;

/// <summary>
/// 游戏总管理器
/// 负责应用开启时的初始化工作
/// </summary>
public class GameMgr : SingletonWithMono<GameMgr>
{
    protected override void Awake()
    {
        base.Awake();

        //初始化面板
        UIManager.GetInstance().ShowPanel<MapPanel>("MapPanel", UI_Layer.Bot);
        UIManager.GetInstance().ShowPanel<GuidingPanel>("GuidingPanel", UI_Layer.Mid);
        UIManager.GetInstance().ShowPanel<InfoPanel>("InfoPanel", UI_Layer.Top);
        UIManager.GetInstance().ShowPanel<SystemPanel>("SystemPanel", UI_Layer.System);

        //场景中创建UICamera用于渲染LineRendererInMap
        ResMgr.GetInstance().LoadAsync<GameObject>("Prefabs/RouteCamera", (camera) =>
        {
            camera.name = "RouteCamera";
        });

        //打开程序后首次自动定位，顺便初始化GaoDeAPI
        GaoDeAPI.GetInstance().OnLocating();
        //this.TriggerEvent(EventName.LocatedTheFstTime);

        ARGroundPlane.GetInstance().planeFinder = GameObject.FindObjectOfType<PlaneFinderBehaviour>();
        ARGroundPlane.GetInstance().planeFinder.gameObject.SetActive(false);//使PlaneFinder先失活，等进入AR导航时再开启
    }
}
