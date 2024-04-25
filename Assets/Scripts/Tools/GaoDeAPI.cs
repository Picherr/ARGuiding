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
    private const string key = "7b8f9443eed7ff041d9ad7d9dd9e87a2";//高德地图Web服务API类型key

    private string longitude;//unity坐标经度
    private string latitude;//unity坐标纬度
    private string GDlongitude;//高德坐标经度
    private string GDlatitude;//高德坐标纬度

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

    private bool isFstTime = true;//是否是第一次定位，即应用初始化

    private bool isGuiding = false;//是否正在导航

    private bool isLocating = false;//是否此次操作为定位

    private int i = 0;

    public string GetLongitude { get { return longitude; } }
    public string GetLatitude { get { return latitude; } }
    public string GetGDlongitude { get { return GDlongitude; } }
    public string GetGDlatitude { get { return GDlatitude; } }

    public bool IsLocating { set { isLocating = value; } }

    private void Awake()
    {
        EventCenter.GetInstance().AddEventListener(EventName.StartGuidingDirection, StartGuidingDirection);//添加事件
        EventCenter.GetInstance().AddEventListener(EventName.EndGuidingDirection, EndGuidingDirection);//添加事件
    }

    private void Start()
    {
        //场景中创建LineRendererInMap
        ResMgr.GetInstance().LoadAsync<GameObject>("Prefabs/RouteInMap", (obj) =>
        {
            lineRendererInMap = obj.GetComponent<LineRenderer>();
        });
        //场景中创建LineRendererInWorld
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
    /// 定位
    /// </summary>
    public void OnLocating()
    {
        // 允许定位
        if (StartGPS())
        {
            StartCoroutine(GPS());
        }
    }

    /// <summary>
    /// 给外部提供的路径规划函数
    /// 1.获取当前位置坐标-利用GPS和坐标转换函数
    /// 2.获取规划信息-利用路径规划函数
    /// 3.更新GuidePanel
    /// </summary>
    public void OnDirection()
    {
        Debug.Log("刷新导航" + i++);
        //1.获取当前位置坐标
        OnLocating();
        //2.获取规划信息
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
    /// 搜索按钮
    /// </summary>
    private void OnSearching()
    {
        StartCoroutine(Inputtips(
            "https://restapi.amap.com/v3/assistant/inputtips?output=json&city=020&keywords=" + search.text +
            "&location=" + GDlongitude + "," + GDlatitude + "&citylimit=true&datatype=poi&key=" + key));
    }

    /// <summary>
    /// 手机请求获取GPS定位权限
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
        //gps.text = "开始获取GPS信息";
        Debug.Log("开始获取GPS信息");

        // 检查位置服务是否可用
        if (!Input.location.isEnabledByUser)
        {
            //gps.text = "位置服务不可用";
            //Debug.Log("位置服务不可用");
            yield break;
        }

        // 查询位置前先开启位置服务
        //gps.text = "启动位置服务";

        Input.location.Start();
        Debug.Log("启动位置服务");

        // 等待服务初始化
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            //gps.text = Input.location.status.ToString() + ">>>" + maxWait.ToString();
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // 服务初始化超时
        if (maxWait < 1)
        {
            //gps.text = "服务初始化超时";
            Debug.Log("服务初始化超时");
            yield break;
        }

        // 连接失败
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            //gps.text = "无法确定设备位置";
            Debug.Log("无法确定设备位置");
            yield break;
        }
        else
        {
            /*gps.text = "Location:rn" + "\n" +
                "纬度：" + Input.location.lastData.latitude + "\n" +
                "经度：" + Input.location.lastData.longitude + "\n" +
                "海拔：" + Input.location.lastData.altitude + "\n" +
                "水平精度：" + Input.location.lastData.horizontalAccuracy + "\n" +
                "垂直精度：" + Input.location.lastData.verticalAccuracy + "\n" +
                "时间戳：" + Input.location.lastData.timestamp;*/

            longitude = Input.location.lastData.longitude.ToString();//GPS经度
            latitude = Input.location.lastData.latitude.ToString();//GPS纬度

            StartCoroutine(Convert(
                "https://restapi.amap.com/v3/assistant/coordinate/convert?locations=" + longitude + "," + latitude +
                "&coordsys=gps&output=json&key=" + key));
        }
        // 停止服务，如果没必要继续更新位置
        Input.location.Stop();
    }

    /// <summary>
    /// 坐标转换：将非高德坐标转换为高德坐标
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
                //高德经纬度坐标小数点后不得超过6位
                //此处更新高德位置坐标
                GDlongitude = String.Format("{0:0.000000}", float.Parse(GDlongitude));//高德经度
                GDlatitude = String.Format("{0:0.000000}", float.Parse(GDlatitude));//高德纬度
                //Debug.Log("GPS经度:" + longitude + "GPS纬度:" + latitude);
                //Debug.Log("高德经度:" + GDlongitude + "高德纬度:" + GDlatitude);

                /*if (!isGuiding)//如果不是正在导航而触发坐标转换，即是定位
                {
                    this.TriggerEvent(EventName.ShowNotification, new ShowNotificationArgs//触发ShowNotification事件
                    {
                        message =
                    "已定位！\nGPS经度：" + longitude + "\nGPS纬度：" + latitude + "\n高德经度：" + GDlongitude + "\n高德纬度：" + GDlatitude,
                        isBtnOn = false,
                        autoOff = true
                    });
                }*/

                if (isLocating)
                {
                    this.TriggerEvent(EventName.ShowNotification, new ShowNotificationArgs//触发ShowNotification事件
                    {
                        message =
                    "已定位！\nGPS经度：" + longitude + "\nGPS纬度：" + latitude + "\n高德经度：" + GDlongitude + "\n高德纬度：" + GDlatitude,
                        isBtnOn = false,
                        autoOff = true
                    });
                    isLocating = false;
                }

                /*if (isFstTime)//如果是刚打开应用，初始化瓦片地图，之后不再更新
                {
                    this.TriggerEvent(EventName.LocatedTheFstTime);//触发LocatedTheFstTime事件
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
    /// 逆地理编码
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
    /// 路径规划
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
                //定位与终点距离小于20米，判定为到达目的地
                if (Conversion.GetDistance(Info.DesCoord(InfoPanel.desIndex).y, Info.DesCoord(InfoPanel.desIndex).x,
                    float.Parse(GDlatitude), float.Parse(GDlongitude)) < 20)
                {
                    this.TriggerEvent(EventName.ShowNotification, new ShowNotificationArgs//触发ShowNotification事件
                    {
                        message = "已到达\n" + Info.DesInfo(InfoPanel.desIndex),
                        isBtnOn = true,//开启确认按钮
                        autoOff = false//信息框手动关闭
                    });
                    this.TriggerEvent(EventName.EndGuidingDirection);//触发事件，不再进行路径规划
                    yield break;//后面的代码不再执行
                }
                JsonData jd = JsonMapper.ToObject(webRequest.downloadHandler.text);
                //Debug.Log(jd["infocode"].ToString());
                //只需要获取路径规划的第一条数据即可（因为位置是不断刷新的，后面的数据用不到）
                this.TriggerEvent(EventName.UpdateGuidingInfo, new UpdateGuidingInfoArgs//触发事件UpdateGuidingInfo
                {
                    guidingText = jd["route"]["paths"][0]["steps"][0]["instruction"].ToString(),//获取步行指示
                    desName = Info.DesInfo(InfoPanel.desIndex),//获取目的地名称
                    disMiles = jd["route"]["paths"][0]["distance"].ToString()//获取剩余距离
                });

                //polyline在json中的格式：
                //"polyline":"116.46658,39.995686;116.46694,39.995686;116.46694,39.995686;116.467665,39.995686"

                //List<Vector3> waypoints = new List<Vector3>();
                List<LatLng> waypoints = new List<LatLng>();
                Vector2 point;
                for (int i = 0; i < jd["route"]["paths"][0]["steps"].Count; i++)//处理每一step中的polyline
                {
                    string[] polyline = jd["route"]["paths"][0]["steps"][i]["polyline"].ToString().Split(';');//将每一对坐标分开

                    for (int j = i == 0 ? 0 : 1; j < polyline.Length; j++)//处理每一polyline中的每个点
                    {
                        string[] points = polyline[j].Split(',');//将每个点的经纬坐标分开
                        double lng = double.Parse(points[0]);
                        double lat = double.Parse(points[1]);
                        point = new Vector2((float)lng, (float)lat);//打包成LatLng对象
                        waypoints.Add(Conversion.GetWorldPoint(point));//通过Conversion类将经纬度转换为世界坐标
                    }
                }
                DrawRouteInMap(waypoints);//绘制路径
                yield break;
            }
        }
    }

    /// <summary>
    /// 输入提示
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
    /// 在Unity世界中绘制导航线
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
    /// 在真实世界中绘制导航线
    /// </summary>
    /// <param name="waypoints"></param>
    public void DrawRouteInWorld(List<Vector3> waypoints)
    {
        lineRendererInWorld.positionCount = waypoints.Count >= 2 ? 2 : waypoints.Count;//只需要画出前两个点，即大致方向的线
        float distance;
        distance = Conversion.GetDistance(waypoints[0].y, waypoints[0].x, waypoints[1].y, waypoints[1].x);//计算两点之间的距离
        Vector3 v = Vector3.Normalize(waypoints[1] - waypoints[0]);//计算两点之间的单位向量
        v = v * distance;
        lineRendererInWorld.SetPosition(1, v + new Vector3(0, 0.01f, 0));
    }

    private void OnDestroy()
    {
        EventCenter.GetInstance().RemoveEventListener(EventName.StartGuidingDirection, StartGuidingDirection);//移除事件
        EventCenter.GetInstance().RemoveEventListener(EventName.EndGuidingDirection, EndGuidingDirection);//移除事件
    }
}