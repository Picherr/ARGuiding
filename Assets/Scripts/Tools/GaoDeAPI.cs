using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;
using UnityEngine.UI;
using LitJson;
using TMPro;

public class GaoDeAPI : SingletonAutoMono<GaoDeAPI>
{
    private const string key = "7b8f9443eed7ff041d9ad7d9dd9e87a2";//�ߵµ�ͼWeb����API����key

    private string longitude;//unity���꾭��
    private string latitude;//unity����γ��
    private string GDlongitude;//�ߵ����꾭��
    private string GDlatitude;//�ߵ�����γ��

    public Text searchinfo;
    public InputField search;
    public Button Locating;
    public Button Searching;

    public Transform content;
    public Transform tip;

    [SerializeField]
    private LineRenderer lineRendererInMap;
    [SerializeField]
    private LineRenderer lineRendererInWorld;

    private bool isFstTime = true;//�Ƿ��ǵ�һ�ζ�λ����Ӧ�ó�ʼ��

    private bool isGuiding = false;//�Ƿ����ڵ���

    private bool isLocating = false;//�Ƿ�˴β���Ϊ��λ

    private int i = 0;

    public string GetLongitude { get { return longitude; } }
    public string GetLatitude { get { return latitude; } }
    public string GetGDlongitude { get { return GDlongitude; } }
    public string GetGDlatitude { get { return GDlatitude; } }

    public bool IsLocating { set { isLocating = value; } }

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
        InvokeRepeating("OnDirection", 0, 5f);
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
        Debug.Log("ˢ�µ���" + i++);
        //1.��ȡ��ǰλ������
        OnLocating();
        //2.��ȡ�滮��Ϣ
#if UNITY_EDITOR
        StartCoroutine(Direction(
            "https://restapi.amap.com/v5/direction/walking?origin=113.295128,23.139692&destination=" +
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
                    this.TriggerEvent(EventName.EndGuidingDirection);//�����¼������ٽ���·���滮
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

                //List<Vector3> waypoints = new List<Vector3>();
                List<LatLng> waypoints = new List<LatLng>();
                Vector2 point;
                for (int i = 0; i < jd["route"]["paths"][0]["steps"].Count; i++)//����ÿһstep�е�polyline
                {
                    string[] polyline = jd["route"]["paths"][0]["steps"][i]["polyline"].ToString().Split(';');//��ÿһ������ֿ�

                    for (int j = i == 0 ? 0 : 1; j < polyline.Length; j++)//����ÿһpolyline�е�ÿ����
                    {
                        string[] points = polyline[j].Split(',');//��ÿ����ľ�γ����ֿ�
                        double lng = double.Parse(points[0]);
                        double lat = double.Parse(points[1]);
                        point = new Vector2((float)lng, (float)lat);//�����LatLng����
                        waypoints.Add(Conversion.GetWorldPoint(point));//ͨ��Conversion�ཫ��γ��ת��Ϊ��������
                    }
                }
                DrawRouteInMap(waypoints);//����·��
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
    public void DrawRouteInMap(List<LatLng> waypoints)
    {
        lineRendererInMap.positionCount = waypoints.Count;
        Debug.Log(lineRendererInMap.positionCount);

        for (int i = 0; i < lineRendererInMap.positionCount; i++)
        {
            lineRendererInMap.SetPosition(i, new Vector3((float)waypoints[i].Longitude, 5f, (float)waypoints[i].Latitude));
        }
    }

    /// <summary>
    /// ����ʵ�����л��Ƶ�����
    /// </summary>
    /// <param name="waypoints"></param>
    public void DrawRouteInWorld(List<Vector3> waypoints)
    {
        lineRendererInWorld.positionCount = waypoints.Count >= 2 ? 2 : waypoints.Count;//ֻ��Ҫ����ǰ�����㣬�����·������
        float distance;
        distance = Conversion.GetDistance(waypoints[0].y, waypoints[0].x, waypoints[1].y, waypoints[1].x);//��������֮��ľ���
        Vector3 v = Vector3.Normalize(waypoints[1] - waypoints[0]);//��������֮��ĵ�λ����
        v = v * distance;
        lineRendererInWorld.SetPosition(1, v + new Vector3(0, 0.01f, 0));
    }

    private void OnDestroy()
    {
        EventCenter.GetInstance().RemoveEventListener(EventName.StartGuidingDirection, StartGuidingDirection);//�Ƴ��¼�
        EventCenter.GetInstance().RemoveEventListener(EventName.EndGuidingDirection, EndGuidingDirection);//�Ƴ��¼�
    }
}