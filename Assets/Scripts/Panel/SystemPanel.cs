using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
using System.Collections.Generic;

//ת����ARģʽʱ��״̬
public enum ModeToAR_Type
{
    Nothing,//δѡ��Ŀ�ĵأ���δ��ʼ����
    StartGuiding,//�Ѿ���ʼ����
    Arrived,//����Ŀ�ĵ�
}

public class SystemPanel : BasePanel
{
    [SerializeField]
    private GameObject Message;//��Ϣ�������
    [SerializeField]
    private GameObject Video;//��Ƶ������

    [SerializeField]
    private TextMeshProUGUI message;//��ʾ��Ϣ

    [SerializeField]
    private GameObject btnConfirm;//��ʾ��Ϣ��ȷ�ϰ�ť
    [SerializeField]
    private GameObject btnShowPause;//RawImage�䵱��ͣ��ť����
    [SerializeField]
    private GameObject btnStop;//ֹͣ������ť
    [SerializeField]
    private GameObject btnRoute;//�Ƿ�AR����·����ť
    [SerializeField]
    private GameObject btnPlay;//��Ƶ���Ű�ť
    [SerializeField]
    private GameObject btnPause;//��Ƶ��ͣ��ť
    [SerializeField]
    private GameObject btnAgain;//��Ƶ�ز���ť

    [SerializeField]
    private Slider videoSlider;//��Ƶ������
    [SerializeField]
    private VideoPlayer videoPlayer;//��Ƶ���������
    
    [SerializeField]
    private List<Sprite> btnRouteSprite;//AR����·����ťͼƬ��ʽ

    private GameObject lineRendererInWorld;//AR������

    private bool isAR = false;//�Ƿ���AR����ģʽ
    private bool isBtnStopOn = false;//ֹͣ������ť�Ƿ���ʾ
    private bool isARRouteOn = true;//AR·���Ƿ���ʾ
    private bool isBtnAgainDown = false;//�ز���ť�Ƿ��Ѿ�����

    private ModeToAR_Type modeType = ModeToAR_Type.Nothing;//ת����ARģʽʱ��״̬

    protected override void Awake()
    {
        base.Awake();
        EventCenter.GetInstance().AddEventListener(EventName.ShowNotification, ShowNotification);//����¼�
        EventCenter.GetInstance().AddEventListener(EventName.StartGuidingDirection, StartGuidingDirection);//����¼�
        EventCenter.GetInstance().AddEventListener(EventName.EndGuidingDirection, EndGuidingDirection);//����¼�
        EventCenter.GetInstance().AddEventListener(EventName.VideoIntroduction, VideoIntroduction);//����¼�
        EventCenter.GetInstance().AddEventListener(EventName.ChangeModeToARGuidingType, ChangeModeToARGuidingType);//����¼�

        //MonoMgr.GetInstance().AddUpdateListener(systemUpdate);
    }
    protected override void Start()
    {
        base.Start();
        btnConfirm = GetControl<Button>("btnConfirm").gameObject;
        btnShowPause = GetControl<Button>("video").gameObject;
        btnStop = GetControl<Button>("btnStop").gameObject;
        btnRoute = GetControl<Button>("btnRoute").gameObject;
        btnPlay = GetControl<Button>("btnPlay").gameObject;
        btnPause = GetControl<Button>("btnPause").gameObject;
        btnAgain = GetControl<Button>("btnAgain").gameObject;

        videoSlider = GetControl<Slider>("videoSlider");
        videoSlider.onValueChanged.AddListener(SliderEvent);

        Message.SetActive(false);
        Video.SetActive(false);

        btnStop.SetActive(false);
        btnRoute.SetActive(false);
        btnPause.SetActive(false);
        btnAgain.SetActive(false);
    }

    private void systemUpdate()
    {
        videoSlider.value = (float)videoPlayer.time / (float)videoPlayer.clip.length;//���Ž���
        if (videoPlayer.frame == (long)(videoPlayer.frameCount - 1))//�������
        {
            if (!isBtnAgainDown)//��һ�β�������ز���δ����
            {
                Debug.Log("������ť");
                btnAgain.SetActive(true);//�����ز���ť
                btnShowPause.GetComponent<Button>().onClick.RemoveListener(ShowPause);//������ʱ�Ƴ�RawImage�¼�����
                MonoMgr.GetInstance().RemoveUpdateListener(systemUpdate);
            }
        }
    }

    protected override void OnClick(string btnName)
    {
        base.OnClick(btnName);
        switch (btnName)
        {
            case "btnConfirm"://��Ϣ��ȷ�ϰ�ť
                isAR = !isAR;
                ModeChange(isAR);
                CloseDebug();
                break;
            case "btnLocating"://��λ
                GaoDeAPI.GetInstance().IsLocating = true;
                GaoDeAPI.GetInstance().OnLocating();
                break;
            case "btnChangeMode"://ת������ģʽ
                isAR = !isAR;
                ModeChange(isAR);
                break;
            case "btnStop"://ֹͣ����
                this.TriggerEvent(EventName.EndGuidingDirection);
                break;
            case "btnRoute"://�Ƿ���ʾAR����·��
                if (lineRendererInWorld)//�������
                {
                    isARRouteOn = !isARRouteOn;
                    lineRendererInWorld.SetActive(isARRouteOn);//ʹ��ʧ��
                    if (isARRouteOn)//��ʱAR·������ʾ
                    {
                        btnRoute.GetComponent<Image>().sprite = btnRouteSprite[0];//�ı䰴ťͼƬ��ʽ
                    }
                    else
                    {
                        btnRoute.GetComponent<Image>().sprite = btnRouteSprite[1];//�ı䰴ťͼƬ��ʽ
                    }
                }
                break;
            case "btnPlay"://������Ƶ
                btnPlay.SetActive(false);
                videoPlayer.Play();
                btnShowPause.GetComponent<Button>().onClick.AddListener(ShowPause);//����ʱ���RawImage�¼�����
                MonoMgr.GetInstance().AddUpdateListener(systemUpdate);
                break;
            case "btnPause"://��ͣ��Ƶ
                btnPause.SetActive(false);
                videoPlayer.Pause();
                btnPlay.SetActive(true);
                btnShowPause.GetComponent<Button>().onClick.RemoveListener(ShowPause);//��ͣʱ�Ƴ�RawImage�¼�����
                break;
            case "btnAgain"://�ز���Ƶ
                btnAgain.SetActive(false);
                videoPlayer.time = 0;
                videoPlayer.Play();
                isBtnAgainDown = true;
                MonoMgr.GetInstance().AddUpdateListener(systemUpdate);
                btnShowPause.GetComponent<Button>().onClick.AddListener(ShowPause);//����ʱ���RawImage�¼�����
                Invoke("SetBtnAgainDownFalse", 1f);
                break;
        }
    }

    private void SetBtnAgainDownFalse()
    {
        isBtnAgainDown = false;
    }

    private void ShowPause()
    {
        btnPause.SetActive(true);
        Invoke("DelayClosePause", 2f);
    }

    private void DelayClosePause()
    {
        btnPause.SetActive(false);
    }

    private void SliderEvent(float value)
    {
        videoPlayer.frame = long.Parse((value * videoPlayer.frameCount).ToString("0."));
    }

    private void StartGuidingDirection(object sender, EventArgs e)
    {
        if (isBtnStopOn)//�Ѿ�������ť��ֱ�ӷ���
            return;
        btnStop.SetActive(true);
        isBtnStopOn = true;
    }

    private void EndGuidingDirection(object sender, EventArgs e)
    {
        btnStop.SetActive(false);
        isBtnStopOn = false;
        modeType = ModeToAR_Type.Nothing;
        if (isAR)//�����ʱΪAR����ģʽ����AR������ʾ��ťҲ����
        {
            if (lineRendererInWorld)//�������
            {
                //lineRendererInWorld.SetActive(false);//ʹ��ʧ��
                Destroy(lineRendererInWorld);//����
            }
            btnRoute.SetActive(false);
        }
        GaoDeAPI.GetInstance().LineRendererInMap.positionCount = 0;//·��������Ϊ0
        this.TriggerEvent(EventName.UpdateGuidingInfo, new UpdateGuidingInfoArgs
        {
            desName = "",
            disMiles="",
            guidingText=""
        });
    }

    /// <summary>
    /// ��ʾ֪ͨ
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ShowNotification(object sender, EventArgs e)
    {
        var data = e as ShowNotificationArgs;
        if (data != null)
        {
            StartCoroutine(Notification(data.message, data.isBtnOn, data.autoOff));
        }
    }

    private IEnumerator Notification(string s, bool isBtnOn, bool autoOff)
    {
        message.text = s;
        Message.SetActive(true);
        btnConfirm.SetActive(isBtnOn);
        if (autoOff)//���������Ϣ��Ҫ�Զ��رգ���ִ��Invoke
        {
            Invoke("CloseDebug", 2);
        }
        yield break;
    }

    private void CloseDebug()
    {
        Message.SetActive(false);
    }

    private void VideoIntroduction(object sender, EventArgs e)
    {
        Video.SetActive(true);//����Ƶ
    }

    private void ChangeModeToARGuidingType(object sender,EventArgs e)
    {
        var data = e as ChangeModeToARGuidingType;
        if (data!=null)
        {
            modeType = data.modeType;
        }
    }

    private void ModeChange(bool isar)
    {
        if (isar)//����AR����ģʽ
        {
            UIManager.GetInstance().HidePanel("MapPanel");
            UIManager.GetInstance().HidePanel("InfoPanel");

            if (InfoPanel.desIndex != 4)//δѡ��ƻ��Ļ���Ŀ�ĵ�ʱ���������⵼��
            {
                ARGroundPlane.GetInstance().planeFinder.gameObject.SetActive(true);//����ƽ����
                ARGroundPlane.GetInstance().AddListener();//��Ӽ��ƽ���¼�
            }
            else
            {
                Video.SetActive(true); //ѡ��ƻ��Ļ���Ŀ�ĵ�ʱ������Ƶ����
            }

            GaoDeAPI.GetInstance().IsARGuiding = true;

            //����AR������Ԥ����
            ResMgr.GetInstance().LoadAsync<GameObject>("Prefabs/RouteInWorld", (obj) =>
            {
                obj.transform.rotation = Quaternion.identity;
                lineRendererInWorld = obj;
                GaoDeAPI.GetInstance().LineRendererInWorld = obj.GetComponent<LineRenderer>();
                lineRendererInWorld.SetActive(isARRouteOn);
            });

            switch (modeType)
            {
                case ModeToAR_Type.Nothing://δ��ʼ����������ʾֹͣ������ť��AR�����߰�ť
                case ModeToAR_Type.Arrived://����Ŀ�ĵأ�����ʾֹͣ������ť��AR�����߰�ť
                    btnStop.SetActive(false);
                    btnRoute.SetActive(false);
                    isBtnStopOn = false;
                    break;
                case ModeToAR_Type.StartGuiding://�Ѿ���ʼ��������ʾֹͣ������ť��AR�����߰�ť
                    btnStop.SetActive(true);
                    btnRoute.SetActive(true);
                    isBtnStopOn = true;
                    break;
            }
        }
        else//�ر�AR����ģʽ
        {
            UIManager.GetInstance().ShowPanel<MapPanel>("MapPanel", UI_Layer.Bot);
            UIManager.GetInstance().ShowPanel<InfoPanel>("InfoPanel", UI_Layer.Top);

            ARGroundPlane.GetInstance().planeFinder.gameObject.SetActive(false);//�ر�ƽ����
            this.TriggerEvent(EventName.ChangeModeTo2DGuiding);//�����¼�

            GaoDeAPI.GetInstance().IsARGuiding = false;

            Destroy(lineRendererInWorld);

            if (Video.activeSelf)//�����Ƶ���ڴ�
            {
                videoPlayer.Stop();
                btnPlay.SetActive(true);
                btnShowPause.GetComponent<Button>().onClick.RemoveListener(ShowPause);
                Video.SetActive(false);
            }
            btnRoute.SetActive(false);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventCenter.GetInstance().RemoveEventListener(EventName.ShowNotification, ShowNotification);//�Ƴ��¼�
        EventCenter.GetInstance().RemoveEventListener(EventName.StartGuidingDirection, StartGuidingDirection);//�Ƴ��¼�
        EventCenter.GetInstance().RemoveEventListener(EventName.EndGuidingDirection, EndGuidingDirection);//�Ƴ��¼�
        EventCenter.GetInstance().RemoveEventListener(EventName.VideoIntroduction, VideoIntroduction);//�Ƴ��¼�
        EventCenter.GetInstance().RemoveEventListener(EventName.ChangeModeToARGuidingType, ChangeModeToARGuidingType);//�Ƴ��¼�

        //MonoMgr.GetInstance().RemoveUpdateListener(systemUpdate);
    }
}
