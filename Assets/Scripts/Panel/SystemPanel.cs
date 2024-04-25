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
        EventCenter.GetInstance().AddEventListener(EventName.ShowNotification, ShowNotification);//添加事件
        EventCenter.GetInstance().AddEventListener(EventName.StartGuidingDirection, StartGuidingDirection);//添加事件
        EventCenter.GetInstance().AddEventListener(EventName.EndGuidingDirection, EndGuidingDirection);//添加事件
    }
    protected override void Start()
    {
        base.Start();
        btnConfirm.onClick.AddListener(OnConfirm);
        btnShowPause = GetControl<Button>("video");
        btnShowPause.onClick.AddListener(ShowPause);
        btnStop = GetControl<Button>("btnStop").gameObject;//初始隐藏
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
        videoSlider.value = (float)videoPlayer.time / (float)videoPlayer.clip.length;//播放进度
        if (videoPlayer.frame == (long)(videoPlayer.frameCount - 1))//播放完毕
        {
            btnAgain.SetActive(true);
        }
    }

    protected override void OnClick(string btnName)
    {
        base.OnClick(btnName);
        switch (btnName)
        {
            case "btnLocating"://定位
                GaoDeAPI.GetInstance().IsLocating = true;
                GaoDeAPI.GetInstance().OnLocating();
                break;
            case "btnChangeMode"://转换导航模式
                isAR = !isAR;
                ModeChange(isAR);
                break;
            case "btnStop"://暂停导航
                this.TriggerEvent(EventName.EndGuidingDirection);
                break;
            case "btnPlay"://播放视频
                btnPlay.SetActive(false);
                videoPlayer.Play();
                break;
            case "btnPause"://暂停视频
                btnPause.SetActive(false);
                videoPlayer.Pause();
                break;
            case "btnAgain"://重播视频
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
        if (isBtnPauseOn)//已经开启按钮，直接返回
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
    /// 显示通知
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
        debug.transform.parent.gameObject.SetActive(true);//激活debug
        btnConfirm.gameObject.SetActive(isBtnOn);
        if (autoOff)//如果该条消息需要自动关闭，则执行Invoke
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
        if (isar)//开启AR导航模式
        {
            UIManager.GetInstance().HidePanel("MapPanel");
            UIManager.GetInstance().HidePanel("InfoPanel");

            ARGroundPlane.GetInstance().planeFinder.gameObject.SetActive(true);//开启平面检测
            ARGroundPlane.GetInstance().AddListener();//添加检测平面事件
        }
        else//关闭AR导航模式
        {
            UIManager.GetInstance().ShowPanel<MapPanel>("MapPanel", UI_Layer.Bot);
            UIManager.GetInstance().ShowPanel<InfoPanel>("InfoPanel", UI_Layer.Top);

            //this.TriggerEvent(EventName.LocatedTheFstTime);//触发事件

            ARGroundPlane.GetInstance().planeFinder.gameObject.SetActive(false);//关闭平面检测
            this.TriggerEvent(EventName.ChangeModeTo2DGuiding);//触发事件
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventCenter.GetInstance().RemoveEventListener(EventName.ShowNotification, ShowNotification);//移除事件
        EventCenter.GetInstance().RemoveEventListener(EventName.StartGuidingDirection, StartGuidingDirection);//移除事件
        EventCenter.GetInstance().RemoveEventListener(EventName.EndGuidingDirection, EndGuidingDirection);//移除事件
    }
}
