using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
using System.Collections.Generic;

//转换到AR模式时的状态
public enum ModeToAR_Type
{
    Nothing,//未选择目的地，即未开始导航
    StartGuiding,//已经开始导航
    Arrived,//到达目的地
}

public class SystemPanel : BasePanel
{
    [SerializeField]
    private GameObject Message;//信息框根物体
    [SerializeField]
    private GameObject Video;//视频根物体

    [SerializeField]
    private TextMeshProUGUI message;//提示信息

    [SerializeField]
    private GameObject btnConfirm;//提示信息框确认按钮
    [SerializeField]
    private GameObject btnShowPause;//RawImage充当暂停按钮唤醒
    [SerializeField]
    private GameObject btnStop;//停止导航按钮
    [SerializeField]
    private GameObject btnRoute;//是否AR导航路径按钮
    [SerializeField]
    private GameObject btnPlay;//视频播放按钮
    [SerializeField]
    private GameObject btnPause;//视频暂停按钮
    [SerializeField]
    private GameObject btnAgain;//视频重播按钮

    [SerializeField]
    private Slider videoSlider;//视频进度条
    [SerializeField]
    private VideoPlayer videoPlayer;//视频播放器组件
    
    [SerializeField]
    private List<Sprite> btnRouteSprite;//AR导航路径按钮图片样式

    private GameObject lineRendererInWorld;//AR导航线

    private bool isAR = false;//是否处于AR导航模式
    private bool isBtnStopOn = false;//停止导航按钮是否显示
    private bool isARRouteOn = true;//AR路径是否显示
    private bool isBtnAgainDown = false;//重播按钮是否已经按下

    private ModeToAR_Type modeType = ModeToAR_Type.Nothing;//转换到AR模式时的状态

    protected override void Awake()
    {
        base.Awake();
        EventCenter.GetInstance().AddEventListener(EventName.ShowNotification, ShowNotification);//添加事件
        EventCenter.GetInstance().AddEventListener(EventName.StartGuidingDirection, StartGuidingDirection);//添加事件
        EventCenter.GetInstance().AddEventListener(EventName.EndGuidingDirection, EndGuidingDirection);//添加事件
        EventCenter.GetInstance().AddEventListener(EventName.VideoIntroduction, VideoIntroduction);//添加事件
        EventCenter.GetInstance().AddEventListener(EventName.ChangeModeToARGuidingType, ChangeModeToARGuidingType);//添加事件

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
        videoSlider.value = (float)videoPlayer.time / (float)videoPlayer.clip.length;//播放进度
        if (videoPlayer.frame == (long)(videoPlayer.frameCount - 1))//播放完毕
        {
            if (!isBtnAgainDown)//第一次播完或者重播键未按下
            {
                Debug.Log("开启按钮");
                btnAgain.SetActive(true);//弹出重播按钮
                btnShowPause.GetComponent<Button>().onClick.RemoveListener(ShowPause);//播放完时移除RawImage事件监听
                MonoMgr.GetInstance().RemoveUpdateListener(systemUpdate);
            }
        }
    }

    protected override void OnClick(string btnName)
    {
        base.OnClick(btnName);
        switch (btnName)
        {
            case "btnConfirm"://信息框确认按钮
                isAR = !isAR;
                ModeChange(isAR);
                CloseDebug();
                break;
            case "btnLocating"://定位
                GaoDeAPI.GetInstance().IsLocating = true;
                GaoDeAPI.GetInstance().OnLocating();
                break;
            case "btnChangeMode"://转换导航模式
                isAR = !isAR;
                ModeChange(isAR);
                break;
            case "btnStop"://停止导航
                this.TriggerEvent(EventName.EndGuidingDirection);
                break;
            case "btnRoute"://是否显示AR导航路径
                if (lineRendererInWorld)//如果存在
                {
                    isARRouteOn = !isARRouteOn;
                    lineRendererInWorld.SetActive(isARRouteOn);//使其失活
                    if (isARRouteOn)//此时AR路径有显示
                    {
                        btnRoute.GetComponent<Image>().sprite = btnRouteSprite[0];//改变按钮图片样式
                    }
                    else
                    {
                        btnRoute.GetComponent<Image>().sprite = btnRouteSprite[1];//改变按钮图片样式
                    }
                }
                break;
            case "btnPlay"://播放视频
                btnPlay.SetActive(false);
                videoPlayer.Play();
                btnShowPause.GetComponent<Button>().onClick.AddListener(ShowPause);//播放时添加RawImage事件监听
                MonoMgr.GetInstance().AddUpdateListener(systemUpdate);
                break;
            case "btnPause"://暂停视频
                btnPause.SetActive(false);
                videoPlayer.Pause();
                btnPlay.SetActive(true);
                btnShowPause.GetComponent<Button>().onClick.RemoveListener(ShowPause);//暂停时移除RawImage事件监听
                break;
            case "btnAgain"://重播视频
                btnAgain.SetActive(false);
                videoPlayer.time = 0;
                videoPlayer.Play();
                isBtnAgainDown = true;
                MonoMgr.GetInstance().AddUpdateListener(systemUpdate);
                btnShowPause.GetComponent<Button>().onClick.AddListener(ShowPause);//播放时添加RawImage事件监听
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
        if (isBtnStopOn)//已经开启按钮，直接返回
            return;
        btnStop.SetActive(true);
        isBtnStopOn = true;
    }

    private void EndGuidingDirection(object sender, EventArgs e)
    {
        btnStop.SetActive(false);
        isBtnStopOn = false;
        modeType = ModeToAR_Type.Nothing;
        if (isAR)//如果此时为AR导航模式，把AR导航显示按钮也隐藏
        {
            if (lineRendererInWorld)//如果存在
            {
                //lineRendererInWorld.SetActive(false);//使其失活
                Destroy(lineRendererInWorld);//销毁
            }
            btnRoute.SetActive(false);
        }
        GaoDeAPI.GetInstance().LineRendererInMap.positionCount = 0;//路径点数置为0
        this.TriggerEvent(EventName.UpdateGuidingInfo, new UpdateGuidingInfoArgs
        {
            desName = "",
            disMiles="",
            guidingText=""
        });
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
        message.text = s;
        Message.SetActive(true);
        btnConfirm.SetActive(isBtnOn);
        if (autoOff)//如果该条消息需要自动关闭，则执行Invoke
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
        Video.SetActive(true);//打开视频
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
        if (isar)//开启AR导航模式
        {
            UIManager.GetInstance().HidePanel("MapPanel");
            UIManager.GetInstance().HidePanel("InfoPanel");

            if (InfoPanel.desIndex != 4)//未选择黄花文化馆目的地时，才有虚拟导游
            {
                ARGroundPlane.GetInstance().planeFinder.gameObject.SetActive(true);//开启平面检测
                ARGroundPlane.GetInstance().AddListener();//添加检测平面事件
            }
            else
            {
                Video.SetActive(true); //选择黄花文化馆目的地时，是视频介绍
            }

            GaoDeAPI.GetInstance().IsARGuiding = true;

            //加载AR导航线预制体
            ResMgr.GetInstance().LoadAsync<GameObject>("Prefabs/RouteInWorld", (obj) =>
            {
                obj.transform.rotation = Quaternion.identity;
                lineRendererInWorld = obj;
                GaoDeAPI.GetInstance().LineRendererInWorld = obj.GetComponent<LineRenderer>();
                lineRendererInWorld.SetActive(isARRouteOn);
            });

            switch (modeType)
            {
                case ModeToAR_Type.Nothing://未开始导航，不显示停止导航按钮和AR导航线按钮
                case ModeToAR_Type.Arrived://到达目的地，不显示停止导航按钮和AR导航线按钮
                    btnStop.SetActive(false);
                    btnRoute.SetActive(false);
                    isBtnStopOn = false;
                    break;
                case ModeToAR_Type.StartGuiding://已经开始导航，显示停止导航按钮和AR导航线按钮
                    btnStop.SetActive(true);
                    btnRoute.SetActive(true);
                    isBtnStopOn = true;
                    break;
            }
        }
        else//关闭AR导航模式
        {
            UIManager.GetInstance().ShowPanel<MapPanel>("MapPanel", UI_Layer.Bot);
            UIManager.GetInstance().ShowPanel<InfoPanel>("InfoPanel", UI_Layer.Top);

            ARGroundPlane.GetInstance().planeFinder.gameObject.SetActive(false);//关闭平面检测
            this.TriggerEvent(EventName.ChangeModeTo2DGuiding);//触发事件

            GaoDeAPI.GetInstance().IsARGuiding = false;

            Destroy(lineRendererInWorld);

            if (Video.activeSelf)//如果视频窗口打开
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
        EventCenter.GetInstance().RemoveEventListener(EventName.ShowNotification, ShowNotification);//移除事件
        EventCenter.GetInstance().RemoveEventListener(EventName.StartGuidingDirection, StartGuidingDirection);//移除事件
        EventCenter.GetInstance().RemoveEventListener(EventName.EndGuidingDirection, EndGuidingDirection);//移除事件
        EventCenter.GetInstance().RemoveEventListener(EventName.VideoIntroduction, VideoIntroduction);//移除事件
        EventCenter.GetInstance().RemoveEventListener(EventName.ChangeModeToARGuidingType, ChangeModeToARGuidingType);//移除事件

        //MonoMgr.GetInstance().RemoveUpdateListener(systemUpdate);
    }
}
