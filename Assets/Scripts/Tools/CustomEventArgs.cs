using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �¼�����
/// </summary>
public enum EventName
{
    ShowNotification,//��ʾ֪ͨ
    LocatedTheFstTime,//Ӧ�ÿ�����ʼ����λ��
    StartGuidingDirection,//��ʼ����·���滮
    EndGuidingDirection,//�����ȡ��·���滮
    UpdateGuidingInfo,//���µ�����Ϣ
    AlreadyCreatedModel,//�������⵼��
    HaveArrivedDestination,//����Ŀ�ĵ�
    ChangeModeTo2DGuiding,//ת����2Dƽ�浼��ģʽ
}

public class ShowNotificationArgs : EventArgs
{
    public string message;
    public bool isBtnOn;//�Ƿ���ʾ��ť
    public bool autoOff = true;//�Զ��ر�
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
    public int desIndex;//Ŀ�ĵش���
    public string notice;//ָʾ�Ѿ��������Ϣ
}

public class ChangeModeTo2DGuidingArgs : EventArgs
{
    
}