using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ��������ö��
/// </summary>
public enum Direction
{
    up,
    down,
    left,
    right
}

/// <summary>
/// ��Ƭ��Ϣ��
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
/// ������ ����������ϵ��
/// һ������Ƭ��ͼ ������
/// һ����ʵ����prefab ����Ƭ��ͼ ��ֵ��gameobject ����
/// </summary>
public class LocationMap : MonoBehaviour
{
    //Ϊʲô���ó�256*256����Ϊ�ߵ·��ص�ͼ����256�ģ�����һ��������512*512
    public static float TileWidthAndHeigth = 256;
    public int TileZoom = 18;

    //���ص�Image ������
    public static float TileScale = 1;

    [SerializeField]
    private GameObject TileMap;

    private List<TileImageInfo> TileMaps = null;

    //���� ��Ƭ��Ϣ��
    private TileInfo m_centerTileInfo = null;

    //����������Σ����׼��㣬Ŀǰû���� ��������
    //��Ƭ����
    public readonly int TileRow = 7;
    //��Ƭ����
    public readonly int TileColumn = 7;

    private void Awake()
    {
        Debug.Log("����LocationMap");
        TileScale = TileMap.transform.localScale.x;
        //EventCenter.GetInstance().AddEventListener(EventName.LocatedTheFstTime, LocatedTheFstTime);//���LocatedTheFstTime�¼�����Ӧ�øմ�ʱ��ʼ����Ƭ��ͼ��֮���ٸ���
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
        Debug.Log("����LocatedTheFstTime");
        StartCoroutine(InitTileInfo());
        Debug.Log("���LocatedTheFstTime");
    }

    private IEnumerator InitTileInfo()
    {
        //yield return Location.InitLocationPos();

        if (Location.mLatLng != null)
        {
            LatLng latLng = Location.mLatLng;
#if UNITY_EDITOR
            //��������
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
        //��� ���Ͻǵ� x �� y�� ��Ƭֵ
        int x = m_centerTileInfo.TileX - (TileColumn - 1) / 2;
        int y = m_centerTileInfo.TileY - (TileRow - 1) / 2;
        //��ȡ��ͼ��嶫�ϽǺ������ǵľ�γ�ȣ����ڵ�ͼ·���Ļ���
        //LatLng latlng;
        //Conversion.BottomRightCoord = Location.TileXYToLatLng(x + 6, y + 6, TileZoom, 156, 156);//���ϽǾ�γ��
        //Conversion.BottomRightCoord = new Vector2((float)latlng.Longitude, (float)latlng.Latitude);
        //Conversion.TopLeftCoord = Location.TileXYToLatLng(x + 1, y + 1, TileZoom, 100, 100);//�����Ǿ�γ��
        //Conversion.TopLeftCoord = new Vector2((float)latlng.Longitude, (float)latlng.Latitude);
        //Debug.Log("���ϽǾ���Ϊ��" + Conversion.BottomRightCoord.Longitude+"γ��Ϊ��"+Conversion.BottomRightCoord.Latitude);
        //Debug.Log("�����Ǿ���Ϊ��" + Conversion.TopLeftCoord.Longitude + "γ��Ϊ��" + Conversion.TopLeftCoord.Latitude);
        //��þ�γ�ȵĲ�ֵ
        //Conversion.x_Offset = Math.Abs(Conversion.BottomRightCoord.Longitude - Conversion.TopLeftCoord.Longitude);
        //Conversion.z_Offset = Math.Abs(Conversion.TopLeftCoord.Latitude - Conversion.BottomRightCoord.Latitude);
        //Debug.Log("���Ȳ�ֵΪ��" + Conversion.x_offset);
        //Debug.Log("γ�Ȳ�ֵΪ��" + Conversion.z_offset);
        //���Ͻ�Image ͼƬ������
        Vector3 sour = new Vector3(0 - TileWidthAndHeigth * (TileColumn - 1) / 2 * TileScale, 0 + TileWidthAndHeigth * (TileRow - 1) / 2 * TileScale, 0);
        TileMaps = new List<TileImageInfo>(TileRow * TileColumn);
        TileImageInfo[] gameObjects = new TileImageInfo[TileRow * TileColumn];
        for (int i = 0; i < TileRow; i++)
        {
            for (int j = 0; j < TileColumn; j++)
            {
                GameObject g = Instantiate(TileMap, transform);
                g.transform.localPosition = sour + new Vector3(j * TileWidthAndHeigth * TileScale, 0, 0);

                //��ȡ��Ƭ��ͼ��������
                int _x = x + j;
                
                //������Ƭ��ͼ
                StartCoroutine(Location.SetMap(_x, y, g.GetComponent<Image>(), TileZoom));

                //��ƬImage ��������
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
    /// ������Ƭλ��
    /// </summary>
    /// <param name="direction"></param>
    public void MapUpdate(Direction direction = Direction.up)
    {
        int x;
        int y;

        switch (direction)
        {
            case Direction.up:
                //�����ϸ���
                x = m_centerTileInfo.TileX - (TileColumn - 1) / 2;
                y = m_centerTileInfo.TileY - (TileRow - 1) / 2 - 1;
                for (int i = 0; i < TileColumn; i++)
                {
                    //�����һ�Űᵽ��һ��
                    TileImageInfo info = TileMaps[i + TileColumn * (TileRow - 1)];
                    info.Go.transform.localPosition += new Vector3(0, TileWidthAndHeigth * TileRow * TileScale, 0);
                    info.TileX = x;
                    info.TileX = y;

                    StartCoroutine(Location.SetMap(x, y, info.Go.GetComponent<Image>(), TileZoom));
                    x++;
                    //ð��
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
                    //�ѵ�һ�Űᵽ���һ��
                    TileImageInfo info = TileMaps[i];
                    info.Go.transform.localPosition -= new Vector3(0, TileWidthAndHeigth * TileRow * TileScale, 0);
                    info.TileX = x;
                    info.TileX = y;

                    StartCoroutine(Location.SetMap(x, y, info.Go.GetComponent<Image>(), TileZoom));
                    x++;
                    //ð��
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
                    //���������Ƶ�������
                    TileImageInfo info = TileMaps[i * TileColumn + (TileColumn - 1)];
                    info.Go.transform.localPosition -= new Vector3(TileWidthAndHeigth * TileColumn * TileScale, 0, 0);
                    info.TileX = x;
                    info.TileX = y;

                    StartCoroutine(Location.SetMap(x, y, info.Go.GetComponent<Image>(), TileZoom));
                    y++;
                    //ð��
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
                    //���������Ƶ�������
                    TileImageInfo info = TileMaps[i * TileColumn];
                    info.Go.transform.localPosition += new Vector3(TileWidthAndHeigth * TileColumn * TileScale, 0, 0);
                    info.TileX = x;
                    info.TileX = y;

                    StartCoroutine(Location.SetMap(x, y, info.Go.GetComponent<Image>(), TileZoom));
                    y++;
                    //ð��
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

        //ÿ���޸� �ͷ���Դ
        Resources.UnloadUnusedAssets();
    }

    private void OnDestroy()
    {
        Debug.Log("����LocationMap");
        //EventCenter.GetInstance().RemoveEventListener(EventName.LocatedTheFstTime, LocatedTheFstTime);//�Ƴ��¼�
    }
}