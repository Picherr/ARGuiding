using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 事件名类
/// </summary>
public enum EventName
{
    ShowNotification,//显示通知
    LocatedTheFstTime,//应用开启初始化定位后
    StartGuidingDirection,//开始进行路径规划
    EndGuidingDirection,//变更或取消路径规划
    UpdateGuidingInfo,//更新导航信息
    AlreadyCreatedModel,//创建虚拟导游
    HaveArrivedDestination,//到达目的地
    ChangeModeTo2DGuiding,//转换回2D平面导航模式
}

public class ShowNotificationArgs : EventArgs
{
    public string message;
    public bool isBtnOn;//是否显示按钮
    public bool autoOff = true;//自动关闭
}

public class LocatedTheFstTimeArgs: EventArgs
{

}

public class UpdateGuidingInfoArgs : EventArgs
{
    public string guidingText;
    public string desName;
    public string disMiles;
}

public class AlreadyCreatedModelArgs : EventArgs
{
    
}

public class ChangeModeToARArgs : EventArgs
{

}

public class HaveArrivedDestinationArgs : EventArgs
{
    public int desIndex;//目的地代数
    public string notice;//指示已经到达的消息
}

public class ChangeModeTo2DGuidingArgs : EventArgs
{
    
}