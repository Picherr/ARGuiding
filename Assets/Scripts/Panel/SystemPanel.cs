using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;

public class SystemPanel : BasePanel
{
    [SerializeField]
    private TextMeshProUGUI debug;
    [SerializeField]
    private Button btnConfirm;
    [SerializeField]
    private Button btnShowPause;
    [SerializeField]
    private GameObject btnStop;
    [SerializeField]
    private GameObject btnPlay;
    [SerializeField]
    private GameObject btnPause;
    [SerializeField]
    private GameObject btnAgain;
    [SerializeField]
    private Slider videoSlider;
    [SerializeField]
    private VideoPlayer videoPlayer;
    [SerializeField]
    private GameObject Video;


    private bool isBtnPauseOn = false;

    private bool isAR = false;

    protected override void Awake()
    {
        base.Awake();
        EventCenter.GetInstance().AddEventListener(EventName.ShowNotification, ShowNotification);//����¼�
        EventCenter.GetInstance().AddEventListener(EventName.StartGuidingDirection, StartGuidingDirection);//����¼�
        EventCenter.GetInstance().AddEventListener(EventName.EndGuidingDirection, EndGuidingDirection);//����¼�
    }
    protected override void Start()
    {
        base.Start();
        btnConfirm.onClick.AddListener(OnConfirm);
        btnShowPause = GetControl<Button>("video");
        btnShowPause.onClick.AddListener(ShowPause);
        btnStop = GetControl<Button>("btnStop").gameObject;//��ʼ����
        btnStop.SetActive(false);
        btnPlay = GetControl<Button>("btnPlay").gameObject;
        btnPause = GetControl<Button>("btnPause").gameObject;
        btnPause.SetActive(false);
        btnAgain = GetControl<Button>("btnAgain").gameObject;
        btnAgain.SetActive(false);
        videoSlider = GetControl<Slider>("videoSlider");
        videoSlider.onValueChanged.AddListener(SliderEvent);

        Video.SetActive(false);
    }

    protected override void Update()
    {
        base.Update();
        videoSlider.value = (float)videoPlayer.time / (float)videoPlayer.clip.length;//���Ž���
        if (videoPlayer.frame == (long)(videoPlayer.frameCount - 1))//�������
        {
            btnAgain.SetActive(true);
        }
    }

    protected override void OnClick(string btnName)
    {
        base.OnClick(btnName);
        switch (btnName)
        {
            case "btnLocating"://��λ
                GaoDeAPI.GetInstance().IsLocating = true;
                GaoDeAPI.GetInstance().OnLocating();
                break;
            case "btnChangeMode"://ת������ģʽ
                isAR = !isAR;
                ModeChange(isAR);
                break;
            case "btnStop"://��ͣ����
                this.TriggerEvent(EventName.EndGuidingDirection);
                break;
            case "btnPlay"://������Ƶ
                btnPlay.SetActive(false);
                videoPlayer.Play();
                break;
            case "btnPause"://��ͣ��Ƶ
                btnPause.SetActive(false);
                videoPlayer.Pause();
                break;
            case "btnAgain"://�ز���Ƶ
                btnAgain.SetActive(false);
                videoPlayer.time = 0;
                videoPlayer.Play();
                break;
        }
    }

    private void OnConfirm()
    {
        isAR = !isAR;
        ModeChange(isAR);
        CloseDebug();
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
        if (isBtnPauseOn)//�Ѿ�������ť��ֱ�ӷ���
            return;
        btnPause.SetActive(true);
        isBtnPauseOn = true;
    }

    private void EndGuidingDirection(object sender, EventArgs e)
    {
        btnPause.SetActive(false);
        isBtnPauseOn = false;
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
        debug.text = s;
        debug.transform.parent.gameObject.SetActive(true);//����debug
        btnConfirm.gameObject.SetActive(isBtnOn);
        if (autoOff)//���������Ϣ��Ҫ�Զ��رգ���ִ��Invoke
        {
            this.Invoke("CloseDebug", 2);
        }
        yield break;
    }

    private void CloseDebug()
    {
        debug.transform.parent.gameObject.SetActive(false);
    }

    private void ModeChange(bool isar)
    {
        if (isar)//����AR����ģʽ
        {
            UIManager.GetInstance().HidePanel("MapPanel");
            UIManager.GetInstance().HidePanel("InfoPanel");

            ARGroundPlane.GetInstance().planeFinder.gameObject.SetActive(true);//����ƽ����
            ARGroundPlane.GetInstance().AddListener();//��Ӽ��ƽ���¼�
        }
        else//�ر�AR����ģʽ
        {
            UIManager.GetInstance().ShowPanel<MapPanel>("MapPanel", UI_Layer.Bot);
            UIManager.GetInstance().ShowPanel<InfoPanel>("InfoPanel", UI_Layer.Top);

            //this.TriggerEvent(EventName.LocatedTheFstTime);//�����¼�

            ARGroundPlane.GetInstance().planeFinder.gameObject.SetActive(false);//�ر�ƽ����
            this.TriggerEvent(EventName.ChangeModeTo2DGuiding);//�����¼�
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventCenter.GetInstance().RemoveEventListener(EventName.ShowNotification, ShowNotification);//�Ƴ��¼�
        EventCenter.GetInstance().RemoveEventListener(EventName.StartGuidingDirection, StartGuidingDirection);//�Ƴ��¼�
        EventCenter.GetInstance().RemoveEventListener(EventName.EndGuidingDirection, EndGuidingDirection);//�Ƴ��¼�
    }
}
