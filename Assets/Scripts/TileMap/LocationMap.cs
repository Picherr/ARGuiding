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

    //最好是正方形，容易计算，目前没处理 非正方形
    //瓦片行数
    public readonly int TileRow = 7;
    //瓦片列数
    public readonly int TileColumn = 7;

    private void Awake()
    {
        Debug.Log("生成LocationMap");
        TileScale = TileMap.transform.localScale.x;
        //EventCenter.GetInstance().AddEventListener(EventName.LocatedTheFstTime, LocatedTheFstTime);//添加LocatedTheFstTime事件，当应用刚打开时初始化瓦片地图，之后不再更新
    }

    private void Start()
    {
#if UNITY_EDITOR
        //StartCoroutine(InitTileInfo());
#endif
        StartCoroutine(InitTileInfo());
    }

    private void LocatedTheFstTime(object sender, EventArgs e)
    {
        Debug.Log("进入LocatedTheFstTime");
        StartCoroutine(InitTileInfo());
        Debug.Log("完成LocatedTheFstTime");
    }

    private IEnumerator InitTileInfo()
    {
        //yield return Location.InitLocationPos();

        if (Location.mLatLng != null)
        {
            LatLng latLng = Location.mLatLng;
#if UNITY_EDITOR
            //测试数据
            //latLng = new LatLng(double.Parse(GaoDeAPI.GetInstance().GetLongitude),
            //    double.Parse(GaoDeAPI.GetInstance().GetLatitude));
            //latLng = new LatLng(113.245600d, 23.070910d);
            latLng = new LatLng(113.295128d, 23.139692d);
#endif
            if (latLng == null) yield break;

            m_centerTileInfo = Location.LatLngToTileXY(latLng, TileZoom);

            InitAllTile();
        }
    }

    private void InitAllTile()
    {
        //求的 左上角的 x 和 y的 瓦片值
        int x = m_centerTileInfo.TileX - (TileColumn - 1) / 2;
        int y = m_centerTileInfo.TileY - (TileRow - 1) / 2;
        //获取地图面板东南角和西北角的经纬度，用于地图路径的绘制
        //LatLng latlng;
        //Conversion.BottomRightCoord = Location.TileXYToLatLng(x + 6, y + 6, TileZoom, 156, 156);//东南角经纬度
        //Conversion.BottomRightCoord = new Vector2((float)latlng.Longitude, (float)latlng.Latitude);
        //Conversion.TopLeftCoord = Location.TileXYToLatLng(x + 1, y + 1, TileZoom, 100, 100);//西北角经纬度
        //Conversion.TopLeftCoord = new Vector2((float)latlng.Longitude, (float)latlng.Latitude);
        //Debug.Log("东南角经度为：" + Conversion.BottomRightCoord.Longitude+"纬度为："+Conversion.BottomRightCoord.Latitude);
        //Debug.Log("西北角经度为：" + Conversion.TopLeftCoord.Longitude + "纬度为：" + Conversion.TopLeftCoord.Latitude);
        //求得经纬度的差值
        //Conversion.x_Offset = Math.Abs(Conversion.BottomRightCoord.Longitude - Conversion.TopLeftCoord.Longitude);
        //Conversion.z_Offset = Math.Abs(Conversion.TopLeftCoord.Latitude - Conversion.BottomRightCoord.Latitude);
        //Debug.Log("经度差值为：" + Conversion.x_offset);
        //Debug.Log("纬度差值为：" + Conversion.z_offset);
        //左上角Image 图片的坐标
        Vector3 sour = new Vector3(0 - TileWidthAndHeigth * (TileColumn - 1) / 2 * TileScale, 0 + TileWidthAndHeigth * (TileRow - 1) / 2 * TileScale, 0);
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
                    info.TileX = y;

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
                    info.TileX = y;

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
                    info.TileX = y;

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
                    info.TileX = y;

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

        //每次修改 释放资源
        Resources.UnloadUnusedAssets();
    }

    private void OnDestroy()
    {
        Debug.Log("销毁LocationMap");
        //EventCenter.GetInstance().RemoveEventListener(EventName.LocatedTheFstTime, LocatedTheFstTime);//移除事件
    }
}