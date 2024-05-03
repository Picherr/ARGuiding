using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : BasePanel
{
    //public GameObject Content;//ScrollView�е�Content
    public GameObject item;//InfoTabԤ����
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
    private List<GameObject> scrollViewItems;//�����б�

#if UNITY_EDITOR
    public static int desIndex = 1;//Ŀ�ĵ�ָʾ
#else
    public static int desIndex = -1;//Ŀ�ĵ�ָʾ
#endif

    private int tempDesIndex;

    protected override void Awake()
    {
        //��Content�����InfoTab
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
            case "��������":
                info.SetActive(true);
                info.GetComponentInChildren<TextMeshProUGUI>().text = Info.DesIntro(1).ToString();
                tempDesIndex = 1;
                break;
            case "��ʮ����ʿ֮Ĺ":
                info.SetActive(true);
                info.GetComponentInChildren<TextMeshProUGUI>().text = Info.DesIntro(2).ToString();
                tempDesIndex = 2;
                break;
            case "����ԪĹ":
                info.SetActive(true);
                info.GetComponentInChildren<TextMeshProUGUI>().text = Info.DesIntro(3).ToString();
                tempDesIndex = 3;
                break;
            case "�ƻ��Ļ���":
                info.SetActive(true);
                info.GetComponentInChildren<TextMeshProUGUI>().text = Info.DesIntro(4).ToString();
                tempDesIndex = 4;
                break;
            case "����":
                info.SetActive(true);
                info.GetComponentInChildren<TextMeshProUGUI>().text = Info.DesIntro(5).ToString();
                tempDesIndex = 5;
                break;
        }
    }

    /// <summary>
    /// ���ذ�ť
    /// </summary>
    private void OnBack()
    {
        info.SetActive(false);
    }

    /// <summary>
    /// ǰ����ť->·���滮
    /// </summary>
    private void OnGo()
    {
        desIndex = tempDesIndex;
        //��ֹͣ���е�����·���滮���ٽ����µ�·���滮
        this.TriggerEvent(EventName.EndGuidingDirection);//�����¼�
        this.TriggerEvent(EventName.StartGuidingDirection);//�����¼�
        this.TriggerEvent(EventName.ChangeModeToARGuidingType, new ChangeModeToARGuidingType
        {
            modeType = ModeToAR_Type.StartGuiding
        });
        /*this.TriggerEvent(EventName.ShowNotification, new ShowNotificationArgs//����ShowNotification�¼�
        {
            message = "�ѵ���\n" + Info.DesInfo(InfoPanel.desIndex),
            isBtnOn = true,//����ȷ�ϰ�ť
            autoOff = false//��Ϣ���ֶ��ر�
        });
        this.TriggerEvent(EventName.UpdateGuidingInfo, new UpdateGuidingInfoArgs//�����¼�UpdateGuidingInfo
        {
            guidingText = "�򱱲���1�׵���Ŀ�ĵ�",//��ȡ����ָʾ
            desName = Info.DesInfo(InfoPanel.desIndex),//��ȡĿ�ĵ�����
            disMiles = "1"//��ȡʣ�����
        });*/
    }

    /// <summary>
    /// ��������ɸѡitem
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
            if (!child.name.Contains(s))//��������Ʋ��������������
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
    /// ��Content�����InfoTab
    /// </summary>
    /// <param name="index"></param>
    /*private void AddItem(int index)
    {
        string picName = Info.GetInstance().DesInfo(index);//��ȡ��Ӧ�ĵ���
        ResMgr.GetInstance().LoadAsync<GameObject>("Prefabs/InfoTab", (IT) =>//�첽����Ԥ����
        {
            IT.transform.Find("Pic").GetComponent<Image>().sprite = ResMgr.GetInstance().Load<Sprite>("InfoPic/" + picName);
            IT.transform.Find("Place").GetComponent<TextMeshProUGUI>().text = picName.ToString();
            IT.gameObject.name = picName;
            IT.GetComponent<Transform>().SetParent(Content.GetComponent<Transform>(), false);
            IT.transform.localPosition = new Vector3(itemLocalPos.x, itemLocalPos.y - 5 * itemHeight, 0);
            //messages.Add(IT);

            if (contentSize.y <= 5 * itemHeight)//������Χ������Content�ĸ߶�
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

        for (int i = index; i < messages.Count; i++)//�Ƴ����б�����ÿһ���ǰ�ƶ�
        {
            messages[i].transform.localPosition += new Vector3(0, itemHeight, 0);
        }

        if (contentSize.y <= messages.Count * itemHeight)//�������ݵĸ߶�
        {
            Content.GetComponent<RectTransform>().sizeDelta = new Vector2(contentSize.x, messages.Count * itemHeight);
        }
        else
        {
            Content.GetComponent<RectTransform>().sizeDelta = contentSize;
        }
    }*/
}
