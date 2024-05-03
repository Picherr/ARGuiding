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
    StartGuidingDirection,//��ʼ����·���滮
    EndGuidingDirection,//�����ȡ��·���滮
    UpdateGuidingInfo,//���µ�����Ϣ
    AlreadyCreatedModel,//�������⵼��
    HaveArrivedDestination,//����Ŀ�ĵ�
    ChangeModeTo2DGuiding,//ת����2Dƽ�浼��ģʽ
    VideoIntroduction,//�򿪽�����Ƶ
    ChangeModeToARGuidingType//�л�ת����AR����ģʽʱ��״̬
}

public class ShowNotificationArgs : EventArgs
{
    public string message;
    public bool isBtnOn;//�Ƿ���ʾ��ť
    public bool autoOff = true;//�Զ��ر�
}

public class UpdateGuidingInfoArgs : EventArgs
{
    public string guidingText;
    public string desName;
    public string disMiles;
}

public class HaveArrivedDestinationArgs : EventArgs
{
    public int desIndex;//Ŀ�ĵش���
    public string notice;//ָʾ�Ѿ��������Ϣ
}

public class ChangeModeToARGuidingType : EventArgs
{
    public ModeToAR_Type modeType;
}