using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Location
{
    //public static LatLng mLatLng = null;
    public static LatLng mLatLng = new LatLng(113.295128d, 23.139692d);

    public static IEnumerator SetMap(int x, int y, Image image, int zoom)
    {
        string _path = string.Format("http://webrd01.is.autonavi.com/appmaptile?x={0}&y={1}&z={2}&lang=zh_cn&size=1&scale=1&style=8", x, y, zoom);

        //string.Format("http://online1.map.bdimg.com/onlinelabel/?qt=tile&x={0}&y={1}&z=18", x, y);
        //string.Format("https://wprd02.is.autonavi.com/appmaptile?lang=zh_cn&size=1&style=7&x={0}&y={1}&z=16&scl=1&ltype=1", x, y);

        WWW www = new WWW(_path);
        while (!www.isDone)
        {
            yield return null;
        }
        image.sprite = Sprite.Create(www.texture, new Rect(0, 0, LocationMap.TileWidthAndHeigth, LocationMap.TileWidthAndHeigth), new Vector2(0.5f, 0.5f));//www.texture;SpriteRenderer.sprite.pivot
        //MonoMgr.GetInstance().StartCoroutine(PostSprite(_path, image));
        //yield break;
    }

    public static void LoadSpriteByte(byte[] path, Image image)
    {
        image.sprite = ChangeToSprite(ByteToTex2d(path));
    }

    public static Texture2D ByteToTex2d(byte[] bytes)
    {
        int w = 256;
        int h = 256;
        Texture2D tex = new Texture2D(w, h);
        tex.LoadImage(bytes);
        return tex;
    }

    private static Sprite ChangeToSprite(Texture2D tex)
    {
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        return sprite;
    }

    public static IEnumerator PostSprite(string url, Image image)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error:" + webRequest.error);
                Debug.Log(webRequest.responseCode);
                image.sprite = null;
            }
            else
            {
                LoadSpriteByte(webRequest.downloadHandler.data, image);
                webRequest.Dispose();
            }
        }
    }

    public static IEnumerator InitLocationPos()
    {
#if !UNITY_EDITOR
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("Location is not enabled");
            yield break;
        }
#endif
        Input.location.Start(1, 1);
        int maxWait = 30;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }
        if (maxWait < 1)
        {
            Debug.LogError("Location time out");
            yield break;
        }
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            yield break;
        }
        else
        {
            //�ߵ���Ƭ��ͼʹ�õ��Ǹߵ�����
            //mLatLng = new LatLng(double.Parse(GaoDeAPI.GetInstance().GetGDlongitude), double.Parse(GaoDeAPI.GetInstance().GetGDlatitude));
            //mLatLng = new LatLng(113.245600d, 23.070910d);
            mLatLng = new LatLng(113.295128d, 23.139692d);
            Input.location.Stop();
        }
    }

    /// <summary>
    /// ��tile(��Ƭ)����ϵת��ΪLatLngt(����)����ϵ��pixelX��pixelYΪͼƬƫ����������
    /// </summary>
    /// <param name="tileX"></param>
    /// <param name="tileY"></param>
    /// <param name="zoom"></param>
    /// <param name="pixelX"></param>
    /// <param name="pixelY"></param>
    /// <returns></returns>
    public static LatLng TileXYToLatLng(int tileX, int tileY, int zoom, int pixelX = 0, int pixelY = 0)
    {
        double size = Math.Pow(2, zoom);
        double pixelXToTileAddition = pixelX / LocationMap.TileWidthAndHeigth;
        double lng = (tileX + pixelXToTileAddition) / size * 360.0 - 180.0;

        double pixelYToTileAddition = pixelY / LocationMap.TileWidthAndHeigth;
        double lat = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * (tileY + pixelYToTileAddition) / size))) * 180.0 / Math.PI;
        return new LatLng(lng, lat);
    }

    /// <summary>
    /// ��LatLngt��������ϵת��Ϊtile��Ƭ����ϵ��pixelX��pixelYΪͼƬƫ����������
    /// </summary>
    /// <param name="latlng"></param>
    /// <param name="zoom"></param>
    /// <param name="tileX"></param>
    /// <param name="tileY"></param>
    /// <param name="pixelX"></param>
    /// <param name="pixelY"></param>
    public static TileInfo LatLngToTileXY(LatLng latlng, int zoom)
    {
        double size = Math.Pow(2, zoom);
        double x = ((latlng.Longitude + 180) / 360) * size;
        double lat_rad = latlng.Latitude * Math.PI / 180;
        double y = (1 - Math.Log(Math.Tan(lat_rad) + 1 / Math.Cos(lat_rad)) / Math.PI) / 2;
        y = y * size;

        int tileX = (int)x;
        int tileY = (int)y;
        return new TileInfo(tileX, tileY, (int)((x - tileX) * LocationMap.TileWidthAndHeigth), (int)((y - tileY) * LocationMap.TileWidthAndHeigth));
    }

    /// <summary>
    /// ��LatLngt��������ϵת��Ϊ��������ϵ
    /// </summary>
    /// <param name="latlng"></param>
    /// <param name="zoom"></param>
    /// <returns></returns>
    public static PixelXY LatLngToPixelXY(LatLng latlng, int zoom)
    {
        double size = Math.Pow(2, zoom);
        double x = ((latlng.Longitude + 180) / 360) * size;
        x = x * 256 % 256;
        double lat_rad = latlng.Latitude * Math.PI / 180;
        double y = (2 - Math.Log(Math.Tan(lat_rad) + 1 / Math.Cos(lat_rad)) / Math.PI) / 2;
        y = y * size * 256 % 256;

        float PixelX = (float)x;
        float PixelY = (float)y;
        return new PixelXY(PixelX, PixelY);
    }
}

public class LatLng
{
    public double Longitude;
    public double Latitude;
    public LatLng(double longitude, double latitude)
    {
        Longitude = longitude;
        Latitude = latitude;
    }
}

public class PixelXY
{
    public float X;
    public float Y;

    public PixelXY(float x, float y)
    {
        X = x;
        Y = y;
    }
}

/// <summary>
/// ����ת����-�ߵ¾�γ����ϵ��Unity��������ϵ��ת��
/// </summary>
public static class Conversion
{
    private const int TileSize = 256;//��Ƭ��ͼ��С
    private const int EarthRadius = 6378137;
    private const double InitialResolution = 2 * Math.PI * EarthRadius / TileSize;
    private const double OriginShift = 2 * Math.PI * EarthRadius / 2;

    //����������Ŀ��õ���
    //private static Vector2 bottomRightCoord = new Vector2(113.247890f, 23.068830f);//���ϽǾ�γ��
    //private static Vector2 topLeftCoord = new Vector2(113.242090f, 23.074180f);//�����Ǿ�γ��
    private static LatLng bottomRightCoord=new LatLng(113.297355d, 23.137078d);//���ϽǾ�γ��
    private static LatLng topLeftCoord=new LatLng(113.291528d, 23.142402d);//�����Ǿ�γ��

    //��������ֵ����1080*1080��MapPanel�еľ�γ�Ȳ�ֵ
    //private const double x_offset = 0.00580d;//����еľ��Ȳ�
    //private const double z_offset = 0.00535d;//����е�γ�Ȳ�
    private static double x_Offset;//����еľ��Ȳ�
    private static double z_Offset;//����е�γ�Ȳ�

    //�����ֵ��MapPanel�ĳ���/���
    private const int u_offset = 1080;

    private static Vector2 BottomRightPoint = new Vector2(1080, 920);//���Ͻ�����
    private static Vector2 TopLeftPoint = new Vector2(0, 2000);//����������

    public static LatLng BottomRightCoord
    {
        get { return bottomRightCoord; }
        set { bottomRightCoord = value; }
    }

    public static LatLng TopLeftCoord
    {
        get { return topLeftCoord; }
        set { topLeftCoord = value; }
    }

    public static double x_offset
    {
        get { return Math.Abs(TopLeftCoord.Longitude-BottomRightCoord.Longitude); }
        set { x_offset = value; }
    }

    public static double z_offset
    {
        get { return Math.Abs(TopLeftCoord.Latitude-BottomRightCoord.Latitude); }
        set { z_offset = value; }
    }

    /// <summary>
    /// �ɾ�γ�ȵõ������
    /// </summary>
    /// <param name="se"></param>
    /// <returns></returns>
    public static Vector3 GetWorldPoint(Vector2 se)
    {
        double tempX = se.x - TopLeftCoord.Longitude;
        double tempZ = se.y - BottomRightCoord.Latitude;
        double _tempX = tempX * u_offset / x_offset + TopLeftPoint.x;//����X��
        double _tempZ = tempZ * u_offset / z_offset + BottomRightPoint.y;//����Z��
        //��ȡ�õ���������
        return new Vector3((float)_tempX, 0, (float)_tempZ);
    }

    /// <summary>
    /// ��λ�õ�õ���γ��
    /// </summary>
    /// <param name="curPoint"></param>
    /// <returns></returns>
    public static Vector3 GetLatLon(Vector3 curPoint)
    {
        double _x_offset = (curPoint.x - BottomRightPoint.x) * x_offset / u_offset;
        double _z_offset = (curPoint.z - TopLeftPoint.y) * z_offset / u_offset;
        double resultX = _x_offset + BottomRightCoord.Longitude;
        double resultZ = _z_offset + TopLeftCoord.Latitude;
        return new Vector2((float)resultX, (float)resultZ);
    }

    /// <summary>
    /// ��������λ�õľ��룬��������ľ��룬��λ����
    /// �ù�ʽ��GOOGLE�ṩ�����С��0.2��
    /// </summary>
    /// <param name="lat1">���γ��</param>
    /// <param name="lng1">��㾭��</param>
    /// <param name="lat2">�յ�γ��</param>
    /// <param name="lng2">�յ㾭��</param>
    /// <returns></returns>
    public static float GetDistance(float lat1, float lng1, float lat2, float lng2)
    {
        float radLat1 = Rad(lat1);
        float radLng1 = Rad(lng1);
        float radLat2 = Rad(lat2);
        float radLng2 = Rad(lng2);
        float a = radLat1 - radLat2;
        float b = radLng1 - radLng2;
        float result = 2 * Mathf.Asin(Mathf.Sqrt(Mathf.Pow(Mathf.Sin(a / 2), 2) +
            Mathf.Cos(radLat1) * Mathf.Cos(radLat2) * Mathf.Pow(Mathf.Sin(b / 2), 2))) * EarthRadius;
        Debug.Log(result);
        return result;
    }

    private static float Rad(float d)
    {
        return d * Mathf.PI / 180;
    }
}