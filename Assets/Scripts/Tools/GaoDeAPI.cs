using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;
using UnityEngine.UI;
using LitJson;

public enum Quadrant
{
    //ע�⣺�����޼���������ָ����Ϊ��׼
    First,//0��~90��
    Second,//90��~180��
    Third,//180��~270��
    Fourth,//270��~360��
    Null
};

public class GaoDeAPI : SingletonAutoMono<GaoDeAPI>
{
    private const string key = "7b8f9443eed7ff041d9ad7d9dd9e87a2";//�ߵµ�ͼWeb����API����key

    private string longitude;//unity���꾭��
    private string latitude;//unity����γ��
    private string GDlongitude = "113.295082";//�ߵ����꾭��
    private string GDlatitude = "23.138099";//�ߵ�����γ��

    private Text searchinfo;
    private InputField search;
    private Button Locating;
    private Button Searching;
    private Transform content;
    private Transform tip;

    [SerializeField]
    private LineRenderer lineRendererInMap;
    [SerializeField]
    private LineRenderer lineRendererInWorld;

    private bool isGuiding = false;//�Ƿ����ڵ���
    private bool isARGuiding = false;//�Ƿ�����AR����
    private bool isLocating = false;//�Ƿ�˴β���Ϊ��λ

    public string GetLongitude { get { return longitude; } }
    public string GetLatitude { get { return latitude; } }
    public string GetGDlongitude { get { return GDlongitude; } }
    public string GetGDlatitude { get { return GDlatitude; } }
    public bool IsARGuiding { set { isARGuiding = value; } }
    public bool IsLocating { set { isLocating = value; } }
    public LineRenderer LineRendererInMap { get { return lineRendererInMap; } }
    public LineRenderer LineRendererInWorld { set { lineRendererInWorld = value; } }

    private void Awake()
    {
        EventCenter.GetInstance().AddEventListener(EventName.StartGuidingDirection, StartGuidingDirection);//����¼�
        EventCenter.GetInstance().AddEventListener(EventName.EndGuidingDirection, EndGuidingDirection);//����¼�
    }

    private void Start()
    {
        //�����д���LineRendererInMap
        ResMgr.GetInstance().LoadAsync<GameObject>("Prefabs/RouteInMap", (obj) =>
        {
            lineRendererInMap = obj.GetComponent<LineRenderer>();
        });
        //�����д���LineRendererInWorld
        /*ResMgr.GetInstance().LoadAsync<GameObject>("Prefabs/RouteInWorld", (obj) =>
        {
            lineRendererInWorld = obj.GetComponent<LineRenderer>();
        });*/
    }

    private void StartGuidingDirection(object sender, EventArgs e)
    {
        isGuiding = true;
        InvokeRepeating("OnDirection", 0, 3f);
    }

    private void EndGuidingDirection(object sender, EventArgs e)
    {
        isGuiding = false;
        CancelInvoke("OnDirection");
    }

    /// <summary>
    /// ��λ
    /// </summary>
    public void OnLocating()
    {
        // ����λ
        if (StartGPS())
        {
            StartCoroutine(GPS());
        }
    }

    /// <summary>
    /// ���ⲿ�ṩ��·���滮����
    /// 1.��ȡ��ǰλ������-����GPS������ת������
    /// 2.��ȡ�滮��Ϣ-����·���滮����
    /// 3.����GuidePanel
    /// </summary>
    public void OnDirection()
    {
        //1.��ȡ��ǰλ������
        OnLocating();
        //2.��ȡ�滮��Ϣ
#if UNITY_EDITOR
        StartCoroutine(Direction(
            "https://restapi.amap.com/v5/direction/walking?origin=113.295082,23.138099&destination=" +
            Info.DesCoord(InfoPanel.desIndex).x.ToString() + "," + Info.DesCoord(InfoPanel.desIndex).y.ToString() +
            "&show_fields=polyline&key=" + key));
#else
        StartCoroutine(Direction(
            "https://restapi.amap.com/v5/direction/walking?origin=" + GDlongitude + "," + GDlatitude + "&destination=" +
            Info.DesCoord(InfoPanel.desIndex).x.ToString() + "," + Info.DesCoord(InfoPanel.desIndex).y.ToString() +
            "&show_fields=polyline&key=" + key));
#endif
    }

    /// <summary>
    /// ������ť
    /// </summary>
    private void OnSearching()
    {
        StartCoroutine(Inputtips(
            "https://restapi.amap.com/v3/assistant/inputtips?output=json&city=020&keywords=" + search.text +
            "&location=" + GDlongitude + "," + GDlatitude + "&citylimit=true&datatype=poi&key=" + key));
    }

    /// <summary>
    /// �ֻ������ȡGPS��λȨ��
    /// </summary>
    /// <returns></returns>
    private bool StartGPS()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
        return true;
    }

    private IEnumerator GPS()
    {
        //gps.text = "��ʼ��ȡGPS��Ϣ";
        Debug.Log("��ʼ��ȡGPS��Ϣ");

        // ���λ�÷����Ƿ����
        if (!Input.location.isEnabledByUser)
        {
            //gps.text = "λ�÷��񲻿���";
            //Debug.Log("λ�÷��񲻿���");
            yield break;
        }

        // ��ѯλ��ǰ�ȿ���λ�÷���
        //gps.text = "����λ�÷���";

        Input.location.Start();
        Debug.Log("����λ�÷���");

        // �ȴ������ʼ��
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            //gps.text = Input.location.status.ToString() + ">>>" + maxWait.ToString();
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // �����ʼ����ʱ
        if (maxWait < 1)
        {
            //gps.text = "�����ʼ����ʱ";
            Debug.Log("�����ʼ����ʱ");
            yield break;
        }

        // ����ʧ��
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            //gps.text = "�޷�ȷ���豸λ��";
            Debug.Log("�޷�ȷ���豸λ��");
            yield break;
        }
        else
        {
            /*gps.text = "Location:rn" + "\n" +
                "γ�ȣ�" + Input.location.lastData.latitude + "\n" +
                "���ȣ�" + Input.location.lastData.longitude + "\n" +
                "���Σ�" + Input.location.lastData.altitude + "\n" +
                "ˮƽ���ȣ�" + Input.location.lastData.horizontalAccuracy + "\n" +
                "��ֱ���ȣ�" + Input.location.lastData.verticalAccuracy + "\n" +
                "ʱ�����" + Input.location.lastData.timestamp;*/

            longitude = Input.location.lastData.longitude.ToString();//GPS����
            latitude = Input.location.lastData.latitude.ToString();//GPSγ��

            StartCoroutine(Convert(
                "https://restapi.amap.com/v3/assistant/coordinate/convert?locations=" + longitude + "," + latitude +
                "&coordsys=gps&output=json&key=" + key));
        }
        // ֹͣ�������û��Ҫ��������λ��
        Input.location.Stop();
    }

    /// <summary>
    /// ����ת�������Ǹߵ�����ת��Ϊ�ߵ�����
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private IEnumerator Convert(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            //Request and wait for the desired page
            yield return webRequest.SendWebRequest();

            string[] pages = url.Split('/');
            int page = pages.Length - 1;

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(webRequest.result.ToString());
            }
            else
            {
                JsonData jd = JsonMapper.ToObject(webRequest.downloadHandler.text);
                int index = jd["locations"].ToString().IndexOf(",");
                GDlongitude = jd["locations"].ToString().Substring(0, index);
                GDlatitude = jd["locations"].ToString().Substring(index + 1);
                //�ߵ¾�γ������С����󲻵ó���6λ
                //�˴����¸ߵ�λ������
                GDlongitude = String.Format("{0:0.000000}", float.Parse(GDlongitude));//�ߵ¾���
                GDlatitude = String.Format("{0:0.000000}", float.Parse(GDlatitude));//�ߵ�γ��
                //Debug.Log("GPS����:" + longitude + "GPSγ��:" + latitude);
                //Debug.Log("�ߵ¾���:" + GDlongitude + "�ߵ�γ��:" + GDlatitude);

                /*if (!isGuiding)//����������ڵ�������������ת�������Ƕ�λ
                {
                    this.TriggerEvent(EventName.ShowNotification, new ShowNotificationArgs//����ShowNotification�¼�
                    {
                        message =
                    "�Ѷ�λ��\nGPS���ȣ�" + longitude + "\nGPSγ�ȣ�" + latitude + "\n�ߵ¾��ȣ�" + GDlongitude + "\n�ߵ�γ�ȣ�" + GDlatitude,
                        isBtnOn = false,
                        autoOff = true
                    });
                }*/

                if (isLocating)
                {
                    this.TriggerEvent(EventName.ShowNotification, new ShowNotificationArgs//����ShowNotification�¼�
                    {
                        message =
                    "�Ѷ�λ��\nGPS���ȣ�" + longitude + "\nGPSγ�ȣ�" + latitude + "\n�ߵ¾��ȣ�" + GDlongitude + "\n�ߵ�γ�ȣ�" + GDlatitude,
                        isBtnOn = false,
                        autoOff = true
                    });
                    isLocating = false;
                }

                /*if (isFstTime)//����Ǹմ�Ӧ�ã���ʼ����Ƭ��ͼ��֮���ٸ���
                {
                    this.TriggerEvent(EventName.LocatedTheFstTime);//����LocatedTheFstTime�¼�
                    isFstTime = false;
                }*/
                yield break;

                /*yield return StartCoroutine(Regeo(
                "https://restapi.amap.com/v3/geocode/regeo?output=json&location=" + GDlongitude + "," + GDlatitude +
                "&key=" + key));*/
            }
        }
    }

    /// <summary>
    /// ��������
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private IEnumerator Regeo(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page
            yield return webRequest.SendWebRequest();

            string[] pages = url.Split('/');
            int page = pages.Length - 1;

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(webRequest.result.ToString());
            }
            else
            {
                JsonData jd = JsonMapper.ToObject(webRequest.downloadHandler.text);
                //AddressText.text = jd["regeocode"]["formatted_address"].ToString();
                Debug.Log(jd["regeocode"]["formatted_address"].ToString());
                yield break;
            }
        }
    }

    /// <summary>
    /// ·���滮
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private IEnumerator Direction(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page
            yield return webRequest.SendWebRequest();

            string[] pages = url.Split('/');
            int page = pages.Length - 1;

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(webRequest.result.ToString());
            }
            else
            {
                //��λ���յ����С��20�ף��ж�Ϊ����Ŀ�ĵ�
                if (Conversion.GetDistance(Info.DesCoord(InfoPanel.desIndex).y, Info.DesCoord(InfoPanel.desIndex).x,
                    float.Parse(GDlatitude), float.Parse(GDlongitude)) < 20)
                {
                    this.TriggerEvent(EventName.ShowNotification, new ShowNotificationArgs//����ShowNotification�¼�
                    {
                        message = "�ѵ���\n" + Info.DesInfo(InfoPanel.desIndex),
                        isBtnOn = true,//����ȷ�ϰ�ť
                        autoOff = false//��Ϣ���ֶ��ر�
                    });
                    this.TriggerEvent(EventName.EndGuidingDirection);//�����¼���ֹͣ·���滮
                    this.TriggerEvent(EventName.ChangeModeToARGuidingType, new ChangeModeToARGuidingType
                    {
                        modeType = ModeToAR_Type.Arrived
                    });
                    yield break;//����Ĵ��벻��ִ��
                }

                JsonData jd = JsonMapper.ToObject(webRequest.downloadHandler.text);
                //Debug.Log(jd["infocode"].ToString());
                //ֻ��Ҫ��ȡ·���滮�ĵ�һ�����ݼ��ɣ���Ϊλ���ǲ���ˢ�µģ�����������ò�����
                this.TriggerEvent(EventName.UpdateGuidingInfo, new UpdateGuidingInfoArgs//�����¼�UpdateGuidingInfo
                {
                    guidingText = jd["route"]["paths"][0]["steps"][0]["instruction"].ToString(),//��ȡ����ָʾ
                    desName = Info.DesInfo(InfoPanel.desIndex),//��ȡĿ�ĵ�����
                    disMiles = jd["route"]["paths"][0]["distance"].ToString()//��ȡʣ�����
                });

                //polyline��json�еĸ�ʽ��
                //"polyline":"116.46658,39.995686;116.46694,39.995686;116.46694,39.995686;116.467665,39.995686"

                List<Vector3> waypoints = new List<Vector3>();//��ά��ͼ������б�
                Vector2 point;
                Vector3 pnt;
                for (int i = 0; i < jd["route"]["paths"][0]["steps"].Count; i++)//����ÿһstep�е�polyline
                {
                    string[] polyline = jd["route"]["paths"][0]["steps"][i]["polyline"].ToString().Split(';');//��ÿһ������ֿ�

                    for (int j = i == 0 ? 0 : 1; j < polyline.Length; j++)//����ÿһpolyline�е�ÿ����
                    {
                        string[] points = polyline[j].Split(',');//��ÿ����ľ�γ����ֿ�
                        float lng = float.Parse(points[0]);//����
                        float lat = float.Parse(points[1]);//γ��
                        point = new Vector2(lng, lat);
                        waypoints.Add(Conversion.GetWorldPoint(point));//ͨ��Conversion�ཫ��γ��ת��Ϊ��������
                        if (i == 0 && j == 0)//����һ��������AR����·���Ļ���
                        {
                            pnt = new Vector3(lng, 0, lat);
                            if (isARGuiding)
                            {
                                DrawRouteInWorld(pnt);//����AR����·��
                            }
                        }
                    }
                }
                DrawRouteInMap(waypoints);//���ƶ�ά��ͼ·��
                yield break;
            }
        }
    }

    /// <summary>
    /// ������ʾ
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private IEnumerator Inputtips(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page
            yield return webRequest.SendWebRequest();

            string[] pages = url.Split('/');
            int page = pages.Length - 1;

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(webRequest.result.ToString());
            }
            else
            {
                JsonData jd = JsonMapper.ToObject(webRequest.downloadHandler.text);
                searchinfo.text = jd["status"].ToString() + "\n" +
                    jd["info"].ToString() + "\n" +
                    jd["infocode"].ToString() + "\n" +
                    jd["count"].ToString();
                //content.GetComponent<RectTransform>().sizeDelta = new Vector2(1040, jd["tips"].Count * 150);
                for (int i = 0; i < jd["tips"].Count; i++)
                {
                    Transform obj = Instantiate(tip);
                    obj.transform.SetParent(content);
                    Text text;
                    text = obj.transform.GetChild(0).GetComponent<Text>();
                    text.text = jd["tips"][i]["name"].ToString();
                    text = obj.transform.GetChild(1).GetComponent<Text>();
                    text.text = jd["tips"][i]["address"].ToString();
                }
                yield break;
            }
        }
    }

    /// <summary>
    /// ��Unity�����л��Ƶ�����
    /// </summary>
    /// <param name="waypoints"></param>
    public void DrawRouteInMap(List<Vector3> waypoints)
    {
        lineRendererInMap.positionCount = waypoints.Count;

        for (int i = 0; i < lineRendererInMap.positionCount; i++)
        {
            lineRendererInMap.SetPosition(i, new Vector3(waypoints[i].x, 5f, waypoints[i].z));
        }
    }

    /// <summary>
    /// ����ʵ�����л��Ƶ�����
    /// </summary>
    /// <param name="waypoints"></param>
    public void DrawRouteInWorld(Vector3 pnt)
    {
        lineRendererInWorld.positionCount = 2;
        lineRendererInWorld.SetPosition(0, new Vector3(0, 0, 0));

        //һ������£�Ӧ�ÿ���ʱ���ֻ����������LineRenderer�ĳ��������Ϳ��ܳ��ֵ����򱱡�AR·�����ϵ�����������Ҫ����v.z��ָ����ĽǶȽ��е���
        float angleA = Input.compass.trueHeading;//�ֻ�����ĳ���Ƕ�
        float angleB;
        Vector2 vCam = new Vector2(Mathf.Sin(angleA / Mathf.Rad2Deg), Mathf.Cos(angleA / Mathf.Rad2Deg));//�ֻ��������Ķ�ά����
        int quadrantCam = GetQuadrant(vCam);//����ֻ�������������
        Vector2 vNor = new Vector2(pnt.x - float.Parse(GDlongitude), pnt.z - float.Parse(GDlatitude));//��ǰλ�ú���һ����Ķ�ά����
        int quadrantNor = GetQuadrant(vNor);//��ù滮·��������������
        angleB = Vector2.Angle(vCam, vNor);//��������֮��ļн�
        /*Debug.Log("angleA:" + angleA);
        Debug.Log("vCam:" + vCam.ToString() + "������Ϊ��" + quadrantCam);
        Debug.Log("vNor:" + vNor.ToString() + "������Ϊ��" + quadrantNor);
        Debug.Log("angleB:" + angleB);*/
        int diff = Mathf.Abs(quadrantCam - quadrantNor);//���޲�ֵ
        //�������н�angleB���жϲ��˵ģ�����Ҫ��vCam��vNor������ά�������з�������
        if (diff == 0)//�����ͬһ���ޣ����跭ת
        {
            lineRendererInWorld.SetPosition(1, new Vector3(Mathf.Tan(angleB), 1, 5));
            //Debug.Log("ͬһ���ޣ����跭ת");
        }
        else//�������ͬһ���ޣ�������������
        {
            if ((diff == 1 || diff == 3) && angleB <= 60)//�����������ڣ��Ҽнǲ�����60�ȣ����跭ת
            {
                lineRendererInWorld.SetPosition(1, new Vector3(Mathf.Tan(angleB), 1, 5));
                //Debug.Log("�������ڣ��Ҽнǲ�����90�ȣ����跭ת");
            }
            else if ((diff == 1 || diff == 3) && angleB > 60)//�����������ڣ����нǳ���60�ȣ���ת
            {
                lineRendererInWorld.SetPosition(1, new Vector3(Mathf.Tan(angleB), 1, -5));
                //Debug.Log("�������ڣ����нǳ���90�ȣ���ת");
            }
            else//�������޶Խǣ���ת
            {
                lineRendererInWorld.SetPosition(1, new Vector3(Mathf.Tan(angleB), 1, -5));
                //Debug.Log("���޶Խǣ���ת");
            }
        }
    }

    /// <summary>
    /// ������ת��Ϊ��������
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    private int GetQuadrant(Vector2 vec)
    {
        if (vec.x > 0 && vec.y > 0)
        {
            return 1;
        }
        else if (vec.x > 0 && vec.y < 0)
        {
            return 2;
        }
        else if (vec.x < 0 && vec.y < 0)
        {
            return 3;
        }
        else if (vec.x < 0 && vec.y > 0)
        {
            return 4;
        }
        return 0;
    }

    private void OnDestroy()
    {
        EventCenter.GetInstance().RemoveEventListener(EventName.StartGuidingDirection, StartGuidingDirection);//�Ƴ��¼�
        EventCenter.GetInstance().RemoveEventListener(EventName.EndGuidingDirection, EndGuidingDirection);//�Ƴ��¼�
    }
}