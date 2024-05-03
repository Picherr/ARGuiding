using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : BasePanel
{
    //public GameObject Content;//ScrollView中的Content
    public GameObject item;//InfoTab预制体
    //private Vector2 contentSize;
    //private Vector3 itemLocalPos;
    //private float itemHeight;

    [SerializeField]
    private GameObject info;
    [SerializeField]
    private TMP_InputField input;
    [SerializeField]
    private Button btnBack;
    [SerializeField]
    private Button btnGo;
    [SerializeField]
    private List<GameObject> scrollViewItems;//景点列表

#if UNITY_EDITOR
    public static int desIndex = 1;//目的地指示
#else
    public static int desIndex = -1;//目的地指示
#endif

    private int tempDesIndex;

    protected override void Awake()
    {
        //在Content中添加InfoTab
        /*contentSize = Content.GetComponent<RectTransform>().sizeDelta;
        itemLocalPos = item.transform.localPosition;
        itemHeight = item.GetComponent<RectTransform>().rect.height;
        for (int i = 1; i <= 5; i++)
        {
            AddItem(i);
        }*/
        base.Awake();
        input.onValueChanged.AddListener(SearchFilter);
        btnBack.onClick.AddListener(OnBack);
        btnGo.onClick.AddListener(OnGo);
    }

    protected override void Start()
    {
        
        //foreach(var child in GetControl<>)

        /*EventTrigger trigger = GetControl<Button>("btnStart").gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Drag;
        entry.callback.AddListener(Drag);
        trigger.triggers.Add(entry);*/
    }

    protected override void OnClick(string btnName)
    {
        base.OnClick(btnName);
        switch (btnName)
        {
            case "浩气长存":
                info.SetActive(true);
                info.GetComponentInChildren<TextMeshProUGUI>().text = Info.DesIntro(1).ToString();
                tempDesIndex = 1;
                break;
            case "七十二烈士之墓":
                info.SetActive(true);
                info.GetComponentInChildren<TextMeshProUGUI>().text = Info.DesIntro(2).ToString();
                tempDesIndex = 2;
                break;
            case "邓仲元墓":
                info.SetActive(true);
                info.GetComponentInChildren<TextMeshProUGUI>().text = Info.DesIntro(3).ToString();
                tempDesIndex = 3;
                break;
            case "黄花文化馆":
                info.SetActive(true);
                info.GetComponentInChildren<TextMeshProUGUI>().text = Info.DesIntro(4).ToString();
                tempDesIndex = 4;
                break;
            case "龙柱":
                info.SetActive(true);
                info.GetComponentInChildren<TextMeshProUGUI>().text = Info.DesIntro(5).ToString();
                tempDesIndex = 5;
                break;
        }
    }

    /// <summary>
    /// 返回按钮
    /// </summary>
    private void OnBack()
    {
        info.SetActive(false);
    }

    /// <summary>
    /// 前往按钮->路径规划
    /// </summary>
    private void OnGo()
    {
        desIndex = tempDesIndex;
        //先停止已有的所有路径规划，再进行新的路径规划
        this.TriggerEvent(EventName.EndGuidingDirection);//触发事件
        this.TriggerEvent(EventName.StartGuidingDirection);//触发事件
        this.TriggerEvent(EventName.ChangeModeToARGuidingType, new ChangeModeToARGuidingType
        {
            modeType = ModeToAR_Type.StartGuiding
        });
        /*this.TriggerEvent(EventName.ShowNotification, new ShowNotificationArgs//触发ShowNotification事件
        {
            message = "已到达\n" + Info.DesInfo(InfoPanel.desIndex),
            isBtnOn = true,//开启确认按钮
            autoOff = false//信息框手动关闭
        });
        this.TriggerEvent(EventName.UpdateGuidingInfo, new UpdateGuidingInfoArgs//触发事件UpdateGuidingInfo
        {
            guidingText = "向北步行1米到达目的地",//获取步行指示
            desName = Info.DesInfo(InfoPanel.desIndex),//获取目的地名称
            disMiles = "1"//获取剩余距离
        });*/
    }

    /// <summary>
    /// 搜索功能筛选item
    /// </summary>
    /// <param name="s"></param>
    private void SearchFilter(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            foreach(var child in scrollViewItems)
            {
                child.SetActive(true);
            }
        }
        foreach(var child in scrollViewItems)
        {
            if (!child.name.Contains(s))//景点的名称不包含输入的内容
            {
                child.SetActive(false);
            }
            else
            {
                child.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 在Content中添加InfoTab
    /// </summary>
    /// <param name="index"></param>
    /*private void AddItem(int index)
    {
        string picName = Info.GetInstance().DesInfo(index);//获取对应的地名
        ResMgr.GetInstance().LoadAsync<GameObject>("Prefabs/InfoTab", (IT) =>//异步加载预制体
        {
            IT.transform.Find("Pic").GetComponent<Image>().sprite = ResMgr.GetInstance().Load<Sprite>("InfoPic/" + picName);
            IT.transform.Find("Place").GetComponent<TextMeshProUGUI>().text = picName.ToString();
            IT.gameObject.name = picName;
            IT.GetComponent<Transform>().SetParent(Content.GetComponent<Transform>(), false);
            IT.transform.localPosition = new Vector3(itemLocalPos.x, itemLocalPos.y - 5 * itemHeight, 0);
            //messages.Add(IT);

            if (contentSize.y <= 5 * itemHeight)//超出范围，增加Content的高度
            {
                Content.GetComponent<RectTransform>().sizeDelta = new Vector2(contentSize.x, 5 * itemHeight + 120);
            }
        });
    }*/

    /*private void RemoveItem(GameObject t)
    {
        int index = messages.IndexOf(t);
        messages.Remove(t);
        Destroy(t);

        for (int i = index; i < messages.Count; i++)//移除的列表项后的每一项都向前移动
        {
            messages[i].transform.localPosition += new Vector3(0, itemHeight, 0);
        }

        if (contentSize.y <= messages.Count * itemHeight)//调整内容的高度
        {
            Content.GetComponent<RectTransform>().sizeDelta = new Vector2(contentSize.x, messages.Count * itemHeight);
        }
        else
        {
            Content.GetComponent<RectTransform>().sizeDelta = contentSize;
        }
    }*/
}
