using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 滑动方向枚举
/// </summary>
public enum Direction
{
    up,
    down,
    left,
    right
}

/// <summary>
/// 瓦片信息类
/// </summary>
public class TileInfo
{
    public int TileX { get; set; }
    public int TileY { get; set; }
    public int PixelX { get; set; }
    public int PixelY { get; set; }

    public TileInfo(int tileX, int tileY, int pixelX, int pixelY)
    {
        TileX = tileX;
        TileY = tileY;
        PixelX = pixelX;
        PixelY = pixelY;
    }
}

public class TileImageInfo
{
    public int TileX { get; set; }
    public int TileY { get; set; }
    public GameObject Go { get; set; }

    public TileImageInfo(int tileX, int tileY, GameObject go)
    {
        TileX = tileX;
        TileY = tileY;
        Go = go;
    }
}

/// <summary>
/// 此类中 有两个坐标系，
/// 一个是瓦片地图 的坐标
/// 一个是实例化prefab 把瓦片地图 负值的gameobject 坐标
/// </summary>
public class LocationMap : MonoBehaviour
{
    //为什么设置成256*256，因为高德返回的图就是256的，还有一种类型是512*512
    public static float TileWidthAndHeigth = 256;
    public int TileZoom = 18;

    //加载的Image 的缩放
    public static float TileScale = 1;

    [SerializeField]
    private GameObject TileMap;

    private List<TileImageInfo> TileMaps = null;

    //中心 瓦片信息类
    private TileInfo m_centerTileInfo = null;
    private LatLng currentMapCenter;
    private RectTransform overlayRoot;
    private RectTransform currentLocationMarker;

    //最好是正方形，容易计算，目前没处理 非正方形
    //瓦片行数
    public readonly int TileRow = 7;
    //瓦片列数
    public readonly int TileColumn = 7;

    private void Awake()
    {
        Debug.Log("生成LocationMap");
        TileScale = TileMap.transform.localScale.x;
        EventCenter.GetInstance().AddEventListener(EventName.LocationUpdated, OnLocationUpdated);
    }

    private void Start()
    {
        StartCoroutine(InitTileInfo());
#if !UNITY_EDITOR
        StartCoroutine(RecenterWhenLocationIsReady());
#endif
    }

    private void OnLocationUpdated(object sender, EventArgs e)
    {
        LocationUpdatedEventArgs data = e as LocationUpdatedEventArgs;
        if (data == null || data.location == null)
        {
            return;
        }

        if (data.recenterRequested)
        {
            RebuildMap(data.location, true);
        }
        else
        {
            UpdateMapOverlay();
        }
    }

    private IEnumerator InitTileInfo()
    {
        LatLng latLng = Location.mLatLng;
        if (latLng == null)
        {
            yield break;
        }

#if UNITY_EDITOR
        latLng = NavigationDefaults.CreateParkCenter();
#endif

        RebuildMap(latLng);
    }

    private IEnumerator RecenterWhenLocationIsReady()
    {
        float remaining = 25f;
        GaoDeAPI api = GaoDeAPI.GetInstance();
        while (!api.HasValidLocation && remaining > 0f)
        {
            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (api.HasValidLocation && Location.mLatLng != null)
        {
            RebuildMap(Location.mLatLng, true);
        }
    }

    public void RecenterOnCurrentLocation()
    {
        RebuildMap(Location.mLatLng ?? NavigationDefaults.CreateParkCenter(), true);
    }

    private void RebuildMap(LatLng center, bool force = false)
    {
        if (center == null)
        {
            return;
        }

        if (!force && currentMapCenter != null &&
            Math.Abs(currentMapCenter.Longitude - center.Longitude) < 0.000001d &&
            Math.Abs(currentMapCenter.Latitude - center.Latitude) < 0.000001d)
        {
            return;
        }

        if (TileMaps != null)
        {
            foreach (TileImageInfo tile in TileMaps)
            {
                if (tile != null && tile.Go != null)
                {
                    Destroy(tile.Go);
                }
            }
            TileMaps.Clear();
        }

        currentMapCenter = new LatLng(center.Longitude, center.Latitude);
        m_centerTileInfo = Location.LatLngToTileXY(currentMapCenter, TileZoom);
        InitAllTile();
    }

    private void InitAllTile()
    {
        //求的 左上角的 x 和 y的 瓦片值
        int x = m_centerTileInfo.TileX - (TileColumn - 1) / 2;
        int y = m_centerTileInfo.TileY - (TileRow - 1) / 2;
        double globalPixelX = m_centerTileInfo.TileX * TileWidthAndHeigth + m_centerTileInfo.PixelX;
        double globalPixelY = m_centerTileInfo.TileY * TileWidthAndHeigth + m_centerTileInfo.PixelY;
        const double viewportSize = 1080d;
        Conversion.ConfigureMapBounds(
            Location.GlobalPixelToLatLng(globalPixelX - viewportSize / 2d,
                globalPixelY - viewportSize / 2d, TileZoom),
            Location.GlobalPixelToLatLng(globalPixelX + viewportSize / 2d,
                globalPixelY + viewportSize / 2d, TileZoom));

        //左上角Image 图片的坐标
        Vector3 sour = new Vector3(
            -TileWidthAndHeigth * (TileColumn - 1) / 2 * TileScale +
            (TileWidthAndHeigth / 2 - m_centerTileInfo.PixelX) * TileScale,
            TileWidthAndHeigth * (TileRow - 1) / 2 * TileScale +
            (m_centerTileInfo.PixelY - TileWidthAndHeigth / 2) * TileScale,
            0);
        TileMaps = new List<TileImageInfo>(TileRow * TileColumn);
        TileImageInfo[] gameObjects = new TileImageInfo[TileRow * TileColumn];
        for (int i = 0; i < TileRow; i++)
        {
            for (int j = 0; j < TileColumn; j++)
            {
                GameObject g = Instantiate(TileMap, transform);
                g.transform.localPosition = sour + new Vector3(j * TileWidthAndHeigth * TileScale, 0, 0);

                //求取瓦片地图横向坐标
                int _x = x + j;
                
                //加载瓦片地图
                StartCoroutine(Location.SetMap(_x, y, g.GetComponent<Image>(), TileZoom));

                //瓦片Image 存入数组
                gameObjects[j + i * TileColumn] = new TileImageInfo(_x, y, g);
                g.SetActive(true);
            }
            //
            y += 1;
            //
            sour -= new Vector3(0, TileWidthAndHeigth * TileScale, 0);
        }
        TileMaps.AddRange(gameObjects);
        UpdateMapOverlay();
    }

    private void UpdateMapOverlay()
    {
        if (currentMapCenter == null)
        {
            return;
        }

        EnsureOverlayRoot();

        LatLng currentLocation = Location.mLatLng ?? currentMapCenter;
        currentLocationMarker.anchoredPosition =
            Location.LatLngToMapPixelOffset(currentLocation, currentMapCenter, TileZoom) * TileScale;
        Image currentImage = currentLocationMarker.GetComponent<Image>();
        currentImage.color = GaoDeAPI.GetInstance().HasValidLocation
            ? new Color(0.05f, 0.45f, 1f, 1f)
            : new Color(0.42f, 0.48f, 0.55f, 1f);

        for (int i = 1; i <= 5; i++)
        {
            string markerName = "ParkMarker" + i;
            RectTransform marker = overlayRoot.Find(markerName) as RectTransform;
            if (marker == null)
            {
                marker = CreateMarker(markerName, new Color(0.95f, 0.34f, 0.10f, 1f), 48f, i.ToString());
            }

            Vector2 coordinate = Info.DesCoord(i);
            LatLng point = new LatLng(coordinate.x, coordinate.y);
            marker.anchoredPosition = Location.LatLngToMapPixelOffset(point, currentMapCenter, TileZoom) * TileScale;
        }

        overlayRoot.SetAsLastSibling();
    }

    private void EnsureOverlayRoot()
    {
        if (overlayRoot != null)
        {
            return;
        }

        GameObject overlayObject = new GameObject("MapOverlay", typeof(RectTransform));
        overlayRoot = overlayObject.GetComponent<RectTransform>();
        overlayRoot.SetParent(transform, false);
        overlayRoot.anchorMin = new Vector2(0.5f, 0.5f);
        overlayRoot.anchorMax = new Vector2(0.5f, 0.5f);
        overlayRoot.pivot = new Vector2(0.5f, 0.5f);
        overlayRoot.anchoredPosition = Vector2.zero;
        overlayRoot.sizeDelta = Vector2.zero;

        currentLocationMarker = CreateMarker("CurrentLocationMarker", new Color(0.05f, 0.45f, 1f, 1f),
            42f, "GPS");
    }

    private RectTransform CreateMarker(string markerName, Color color, float size, string labelText)
    {
        GameObject markerObject = new GameObject(markerName, typeof(RectTransform), typeof(CanvasRenderer),
            typeof(Image), typeof(Outline));
        RectTransform marker = markerObject.GetComponent<RectTransform>();
        marker.SetParent(overlayRoot, false);
        marker.anchorMin = new Vector2(0.5f, 0.5f);
        marker.anchorMax = new Vector2(0.5f, 0.5f);
        marker.pivot = new Vector2(0.5f, 0.5f);
        marker.sizeDelta = new Vector2(size, size);

        Image image = markerObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;

        Outline outline = markerObject.GetComponent<Outline>();
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(3f, -3f);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.SetParent(marker, false);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        Text label = labelObject.GetComponent<Text>();
        label.text = labelText;
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.fontSize = labelText == "GPS" ? 12 : 24;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.raycastTarget = false;
        return marker;
    }

    /// <summary>
    /// 更新瓦片位置
    /// </summary>
    /// <param name="direction"></param>
    public void MapUpdate(Direction direction = Direction.up)
    {
        int x;
        int y;

        switch (direction)
        {
            case Direction.up:
                //下向上更新
                x = m_centerTileInfo.TileX - (TileColumn - 1) / 2;
                y = m_centerTileInfo.TileY - (TileRow - 1) / 2 - 1;
                for (int i = 0; i < TileColumn; i++)
                {
                    //把最后一排搬到第一排
                    TileImageInfo info = TileMaps[i + TileColumn * (TileRow - 1)];
                    info.Go.transform.localPosition += new Vector3(0, TileWidthAndHeigth * TileRow * TileScale, 0);
                    info.TileX = x;
                    info.TileY = y;

                    StartCoroutine(Location.SetMap(x, y, info.Go.GetComponent<Image>(), TileZoom));
                    x++;
                    //冒泡
                    for (int j = 1; j <= TileRow - 1; j++)
                    {
                        int index1 = i + TileColumn * (TileRow - j);
                        int index2 = i + TileColumn * (TileRow - j - 1);
                        TileImageInfo temp = TileMaps[index1];
                        TileMaps[index1] = TileMaps[index2];
                        TileMaps[index2] = temp;
                    }
                }
                m_centerTileInfo.TileY--;
                break;
            case Direction.down:
                x = m_centerTileInfo.TileX - (TileColumn - 1) / 2;
                y = m_centerTileInfo.TileY + (TileRow - 1) / 2 + 1;
                for (int i = 0; i < TileColumn; i++)
                {
                    //把第一排搬到最后一排
                    TileImageInfo info = TileMaps[i];
                    info.Go.transform.localPosition -= new Vector3(0, TileWidthAndHeigth * TileRow * TileScale, 0);
                    info.TileX = x;
                    info.TileY = y;

                    StartCoroutine(Location.SetMap(x, y, info.Go.GetComponent<Image>(), TileZoom));
                    x++;
                    //冒泡
                    for (int j = 0; j < TileRow - 1; j++)
                    {
                        int index1 = i + TileColumn * j;
                        int index2 = i + TileColumn * (j + 1);
                        TileImageInfo temp = TileMaps[index1];
                        TileMaps[index1] = TileMaps[index2];
                        TileMaps[index2] = temp;
                    }
                }
                m_centerTileInfo.TileY++;
                break;
            case Direction.left:
                x = m_centerTileInfo.TileX - (TileColumn - 1) / 2 - 1;
                y = m_centerTileInfo.TileY - (TileRow - 1) / 2;
                for (int i = 0; i < TileRow; i++)
                {
                    //把最右列移到最左列
                    TileImageInfo info = TileMaps[i * TileColumn + (TileColumn - 1)];
                    info.Go.transform.localPosition -= new Vector3(TileWidthAndHeigth * TileColumn * TileScale, 0, 0);
                    info.TileX = x;
                    info.TileY = y;

                    StartCoroutine(Location.SetMap(x, y, info.Go.GetComponent<Image>(), TileZoom));
                    y++;
                    //冒泡
                    for (int j = 1; j <= TileColumn - 1; j++)
                    {
                        int index1 = i * TileColumn + (TileColumn - j);
                        int index2 = i * TileColumn + (TileColumn - j - 1);
                        TileImageInfo temp = TileMaps[index1];
                        TileMaps[index1] = TileMaps[index2];
                        TileMaps[index2] = temp;
                    }
                }
                m_centerTileInfo.TileX--;
                break;
            case Direction.right:
                x = m_centerTileInfo.TileX + (TileColumn - 1) / 2 + 1;
                y = m_centerTileInfo.TileY - (TileRow - 1) / 2;
                for (int i = 0; i < TileRow; i++)
                {
                    //把最左列移到最右列
                    TileImageInfo info = TileMaps[i * TileColumn];
                    info.Go.transform.localPosition += new Vector3(TileWidthAndHeigth * TileColumn * TileScale, 0, 0);
                    info.TileX = x;
                    info.TileY = y;

                    StartCoroutine(Location.SetMap(x, y, info.Go.GetComponent<Image>(), TileZoom));
                    y++;
                    //冒泡
                    for (int j = 0; j < TileColumn - 1; j++)
                    {
                        int index1 = i * TileColumn + j;
                        int index2 = i * TileColumn + (j + 1);
                        TileImageInfo temp = TileMaps[index1];
                        TileMaps[index1] = TileMaps[index2];
                        TileMaps[index2] = temp;
                    }
                }
                m_centerTileInfo.TileX++;
                break;
        }

    }

    private void OnDestroy()
    {
        Debug.Log("销毁LocationMap");
        EventCenter.GetInstance().RemoveEventListener(EventName.LocationUpdated, OnLocationUpdated);
    }
}
