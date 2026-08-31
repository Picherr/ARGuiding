using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;
using UnityEngine.UI;
using LitJson;
using System.Globalization;

public class GaoDeAPI : SingletonAutoMono<GaoDeAPI>
{
    private string longitude;//unity坐标经度
    private string latitude;//unity坐标纬度
    private string GDlongitude = "113.295082";//高德坐标经度
    private string GDlatitude = "23.138099";//高德坐标纬度

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

    private bool isGuiding = false;//是否正在导航
    private bool isARGuiding = false;//是否正在AR导航
    private bool isLocating = false;//是否此次操作为定位
    private bool isLocatingInProgress;
    private bool isDirectionRequestInProgress;
    private bool hasValidLocation;

    public string GetLongitude { get { return longitude; } }
    public string GetLatitude { get { return latitude; } }
    public string GetGDlongitude { get { return GDlongitude; } }
    public string GetGDlatitude { get { return GDlatitude; } }
    public bool IsARGuiding { set { isARGuiding = value; } }
    public bool IsLocating { set { isLocating = value; } }
    public bool HasValidLocation { get { return hasValidLocation; } }
    public LineRenderer LineRendererInMap { get { return lineRendererInMap; } }
    public LineRenderer LineRendererInWorld { set { lineRendererInWorld = value; } }

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
        if (!IsDestinationValid())
        {
            ShowMessage("请先选择目的地。");
            return;
        }

        isGuiding = true;
        InvokeRepeating("OnDirection", 0, 3f);
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
        if (!isLocatingInProgress)
        {
            StartCoroutine(Locate());
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
        if (!isDirectionRequestInProgress && IsDestinationValid())
        {
            StartCoroutine(RefreshDirection());
        }
    }

    private IEnumerator RefreshDirection()
    {
        isDirectionRequestInProgress = true;

        string key;
        string keyError;
        if (!AppSecrets.TryGetAmapWebServiceKey(out key, out keyError))
        {
            ShowMessage(keyError);
            isDirectionRequestInProgress = false;
            yield break;
        }

#if UNITY_EDITOR
        GDlongitude = "113.295082";
        GDlatitude = "23.138099";
        hasValidLocation = true;
#else
        if (isLocatingInProgress)
        {
            while (isLocatingInProgress)
            {
                yield return null;
            }
        }
        else
        {
            yield return Locate();
        }

        if (!hasValidLocation)
        {
            isDirectionRequestInProgress = false;
            yield break;
        }
#endif

        if (!isGuiding)
        {
            isDirectionRequestInProgress = false;
            yield break;
        }

        Vector2 destination = Info.DesCoord(InfoPanel.desIndex);
        string url = "https://restapi.amap.com/v5/direction/walking?origin=" + GDlongitude + "," + GDlatitude +
                     "&destination=" + destination.x.ToString(CultureInfo.InvariantCulture) + "," +
                     destination.y.ToString(CultureInfo.InvariantCulture) +
                     "&show_fields=polyline&key=" + UnityWebRequest.EscapeURL(key);
        yield return Direction(url);
        isDirectionRequestInProgress = false;
    }

    /// <summary>
    /// 搜索按钮
    /// </summary>
    private void OnSearching()
    {
        string key;
        string keyError;
        if (!AppSecrets.TryGetAmapWebServiceKey(out key, out keyError))
        {
            ShowMessage(keyError);
            return;
        }

        StartCoroutine(Inputtips(
            "https://restapi.amap.com/v3/assistant/inputtips?output=json&city=020&keywords=" +
            UnityWebRequest.EscapeURL(search.text) + "&location=" + GDlongitude + "," + GDlatitude +
            "&citylimit=true&datatype=poi&key=" + UnityWebRequest.EscapeURL(key)));
    }

    /// <summary>
    /// 手机请求获取GPS定位权限
    /// </summary>
    /// <returns></returns>
    private IEnumerator Locate()
    {
        isLocatingInProgress = true;

#if UNITY_EDITOR
        GDlongitude = "113.295082";
        GDlatitude = "23.138099";
        Location.mLatLng = new LatLng(113.295082d, 23.138099d);
        hasValidLocation = true;
        isLocatingInProgress = false;
        if (isLocating)
        {
            ShowMessage("编辑器模式使用黄花岗公园测试坐标。");
            isLocating = false;
        }
        yield break;
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            float permissionTimeout = 15f;
            while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation) && permissionTimeout > 0f)
            {
                permissionTimeout -= Time.unscaledDeltaTime;
                yield return null;
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                hasValidLocation = false;
                isLocatingInProgress = false;
                ShowMessage("未获得定位权限，无法进行导航。");
                yield break;
            }
        }
#endif

        yield return GPS();
        isLocatingInProgress = false;
        if (!hasValidLocation)
        {
            isLocating = false;
        }
    }

    private IEnumerator GPS()
    {
        Debug.Log("开始获取GPS信息");
        hasValidLocation = false;

        // 检查位置服务是否可用
        if (!Input.location.isEnabledByUser)
        {
            ShowMessage("位置服务不可用，请开启系统定位服务。");
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
            Debug.Log("服务初始化超时");
            Input.location.Stop();
            ShowMessage("定位初始化超时，请稍后重试。");
            yield break;
        }

        // 连接失败
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("无法确定设备位置");
            Input.location.Stop();
            ShowMessage("无法确定设备位置。");
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

            longitude = Input.location.lastData.longitude.ToString("0.000000", CultureInfo.InvariantCulture);//GPS经度
            latitude = Input.location.lastData.latitude.ToString("0.000000", CultureInfo.InvariantCulture);//GPS纬度

            string key;
            string keyError;
            if (!AppSecrets.TryGetAmapWebServiceKey(out key, out keyError))
            {
                Input.location.Stop();
                ShowMessage(keyError);
                yield break;
            }

            yield return Convert(
                "https://restapi.amap.com/v3/assistant/coordinate/convert?locations=" + longitude + "," + latitude +
                "&coordsys=gps&output=json&key=" + UnityWebRequest.EscapeURL(key));
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
            webRequest.timeout = 15;
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                ShowRequestError("坐标转换", webRequest);
                yield break;
            }

            try
            {
                ApplyConvertedLocation(webRequest.downloadHandler.text);
            }
            catch (Exception exception)
            {
                Debug.LogError("解析高德坐标转换结果失败：" + exception);
                ShowMessage("无法解析定位结果，请稍后重试。");
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
            webRequest.timeout = 15;
            yield return webRequest.SendWebRequest();

            if (!isGuiding)
            {
                yield break;
            }

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                ShowRequestError("步行路线规划", webRequest);
                yield break;
            }

            try
            {
                ProcessDirectionResponse(webRequest.downloadHandler.text);
            }
            catch (Exception exception)
            {
                Debug.LogError("解析高德步行路线失败：" + exception);
                ShowMessage("无法解析步行路线，请稍后重试。");
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
    public void DrawRouteInMap(List<Vector3> waypoints)
    {
        if (lineRendererInMap == null)
        {
            Debug.LogWarning("二维路线对象尚未加载完成。");
            return;
        }

        lineRendererInMap.positionCount = waypoints.Count;

        for (int i = 0; i < lineRendererInMap.positionCount; i++)
        {
            lineRendererInMap.SetPosition(i, new Vector3(waypoints[i].x, 5f, waypoints[i].z));
        }
    }

    /// <summary>
    /// 在真实世界中绘制导航线
    /// </summary>
    /// <param name="waypoints"></param>
    public void DrawRouteInWorld(Vector3 pnt)
    {
        if (lineRendererInWorld == null)
        {
            Debug.LogWarning("AR 路线对象尚未加载完成。");
            return;
        }

        double currentLongitude;
        double currentLatitude;
        if (!double.TryParse(GDlongitude, NumberStyles.Float, CultureInfo.InvariantCulture,
                out currentLongitude) ||
            !double.TryParse(GDlatitude, NumberStyles.Float, CultureInfo.InvariantCulture,
                out currentLatitude))
        {
            Debug.LogWarning("当前位置无法用于计算 AR 导航方向。");
            return;
        }

        float targetBearing = NavigationMath.CalculateBearing(currentLatitude, currentLongitude, pnt.z, pnt.x);
        lineRendererInWorld.positionCount = 2;
        lineRendererInWorld.SetPosition(0, Vector3.zero);
        lineRendererInWorld.SetPosition(1,
            NavigationMath.GetLocalDirection(Input.compass.trueHeading, targetBearing));
    }

    private static bool IsDestinationValid()
    {
        return InfoPanel.desIndex >= 1 && InfoPanel.desIndex <= 5;
    }

    private void ShowMessage(string message)
    {
        this.TriggerEvent(EventName.ShowNotification, new ShowNotificationArgs
        {
            message = message,
            isBtnOn = false,
            autoOff = true
        });
    }

    private void ShowRequestError(string operation, UnityWebRequest request)
    {
        Debug.LogError(operation + "失败：" + request.responseCode + " " + request.error);
        ShowMessage(operation + "失败，请检查网络后重试。");
    }

    private static string GetApiError(JsonData response)
    {
        if (response == null)
        {
            return string.Empty;
        }

        try
        {
            string info = response["info"].ToString();
            return string.IsNullOrEmpty(info) ? string.Empty : "（" + info + "）";
        }
        catch
        {
            return string.Empty;
        }
    }

    private void ProcessDirectionResponse(string responseText)
    {
        JsonData jd = JsonMapper.ToObject(responseText);
        if (jd["status"].ToString() != "1" || jd["route"] == null ||
            jd["route"]["paths"] == null || jd["route"]["paths"].Count == 0 ||
            jd["route"]["paths"][0]["steps"] == null || jd["route"]["paths"][0]["steps"].Count == 0)
        {
            ShowMessage("未获取到可用的步行路线。" + GetApiError(jd));
            return;
        }

        if (Conversion.GetDistance(Info.DesCoord(InfoPanel.desIndex).y, Info.DesCoord(InfoPanel.desIndex).x,
                float.Parse(GDlatitude, CultureInfo.InvariantCulture),
                float.Parse(GDlongitude, CultureInfo.InvariantCulture)) < 20)
        {
            this.TriggerEvent(EventName.ShowNotification, new ShowNotificationArgs
            {
                message = "已到达\n" + Info.DesInfo(InfoPanel.desIndex),
                isBtnOn = true,
                autoOff = false
            });
            this.TriggerEvent(EventName.EndGuidingDirection);
            this.TriggerEvent(EventName.ChangeModeToARGuidingType, new ChangeModeToARGuidingType
            {
                modeType = ModeToAR_Type.Arrived
            });
            return;
        }

        JsonData path = jd["route"]["paths"][0];
        this.TriggerEvent(EventName.UpdateGuidingInfo, new UpdateGuidingInfoArgs
        {
            guidingText = path["steps"][0]["instruction"].ToString(),
            desName = Info.DesInfo(InfoPanel.desIndex),
            disMiles = path["distance"].ToString()
        });

        List<Vector3> waypoints = new List<Vector3>();
        bool hasNextDirectionPoint = false;
        Vector3 nextDirectionPoint = Vector3.zero;
        float currentLatitude = float.Parse(GDlatitude, CultureInfo.InvariantCulture);
        float currentLongitude = float.Parse(GDlongitude, CultureInfo.InvariantCulture);
        for (int i = 0; i < path["steps"].Count; i++)
        {
            string[] polyline = path["steps"][i]["polyline"].ToString().Split(';');
            for (int j = i == 0 ? 0 : 1; j < polyline.Length; j++)
            {
                string[] points = polyline[j].Split(',');
                if (points.Length != 2)
                {
                    continue;
                }

                float lng;
                float lat;
                if (!float.TryParse(points[0], NumberStyles.Float, CultureInfo.InvariantCulture, out lng) ||
                    !float.TryParse(points[1], NumberStyles.Float, CultureInfo.InvariantCulture, out lat))
                {
                    continue;
                }

                waypoints.Add(Conversion.GetWorldPoint(new Vector2(lng, lat)));
                if (!hasNextDirectionPoint &&
                    Conversion.GetDistance(currentLatitude, currentLongitude, lat, lng) >= 2f)
                {
                    nextDirectionPoint = new Vector3(lng, 0, lat);
                    hasNextDirectionPoint = true;
                }
            }
        }

        DrawRouteInMap(waypoints);
        if (isARGuiding && hasNextDirectionPoint)
        {
            DrawRouteInWorld(nextDirectionPoint);
        }
    }

    private void ApplyConvertedLocation(string responseText)
    {
        JsonData jd = JsonMapper.ToObject(responseText);
        if (jd["status"].ToString() != "1" || jd["locations"] == null)
        {
            ShowMessage("高德坐标转换失败。" + GetApiError(jd));
            return;
        }

        string[] convertedLocation = jd["locations"].ToString().Split(',');
        if (convertedLocation.Length != 2)
        {
            ShowMessage("高德坐标转换返回了无效坐标。");
            return;
        }

        double convertedLongitude;
        double convertedLatitude;
        if (!double.TryParse(convertedLocation[0], NumberStyles.Float, CultureInfo.InvariantCulture,
                out convertedLongitude) ||
            !double.TryParse(convertedLocation[1], NumberStyles.Float, CultureInfo.InvariantCulture,
                out convertedLatitude))
        {
            ShowMessage("高德坐标转换返回了无法解析的坐标。");
            return;
        }

        GDlongitude = convertedLongitude.ToString("0.000000", CultureInfo.InvariantCulture);
        GDlatitude = convertedLatitude.ToString("0.000000", CultureInfo.InvariantCulture);
        Location.mLatLng = new LatLng(convertedLongitude, convertedLatitude);
        hasValidLocation = true;

        if (isLocating)
        {
            ShowMessage("已定位！\nGPS经度：" + longitude + "\nGPS纬度：" + latitude +
                        "\n高德经度：" + GDlongitude + "\n高德纬度：" + GDlatitude);
            isLocating = false;
        }
    }

    private void OnDestroy()
    {
        CancelInvoke("OnDirection");
        StopAllCoroutines();
        EventCenter.GetInstance().RemoveEventListener(EventName.StartGuidingDirection, StartGuidingDirection);//移除事件
        EventCenter.GetInstance().RemoveEventListener(EventName.EndGuidingDirection, EndGuidingDirection);//移除事件
    }
}
