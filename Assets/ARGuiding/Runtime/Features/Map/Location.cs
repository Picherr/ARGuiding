using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Location
{
    private const int MaxCachedTiles = 128;
    private static readonly Dictionary<string, Sprite> TileCache = new Dictionary<string, Sprite>();
    private static readonly Dictionary<string, LinkedListNode<string>> TileCacheNodes =
        new Dictionary<string, LinkedListNode<string>>();
    private static readonly LinkedList<string> TileCacheLru = new LinkedList<string>();
    private static readonly Dictionary<int, string> ActiveTileRequests = new Dictionary<int, string>();
    private static readonly Dictionary<int, string> DisplayedTileKeys = new Dictionary<int, string>();
    private static readonly HashSet<string> LoggedTileFailures = new HashSet<string>();
    private static readonly Color TileLoadingColor = new Color(0.91f, 0.93f, 0.94f, 1f);
    private static readonly Color TileFailureColor = new Color(0.82f, 0.85f, 0.87f, 1f);

    // GPS/高德定位完成前使用的园区兜底中心点。
    public static LatLng mLatLng = NavigationDefaults.CreateParkCenter();

    public static IEnumerator SetMap(int x, int y, Image image, int zoom, Action<bool> onComplete = null)
    {
        if (image == null)
        {
            if (onComplete != null)
            {
                onComplete(false);
            }
            yield break;
        }

        string tileKey = zoom + "/" + x + "/" + y;
        int imageId = image.GetInstanceID();
        ActiveTileRequests[imageId] = tileKey;

        Sprite cachedSprite;
        if (TryGetCachedTile(tileKey, out cachedSprite))
        {
            ActiveTileRequests.Remove(imageId);
            ApplyTile(image, tileKey, cachedSprite);
            if (onComplete != null)
            {
                onComplete(true);
            }
            yield break;
        }

        // 清除复用 Image 上的旧瓦片，避免网络较慢时短暂显示错误区域。
        DisplayedTileKeys.Remove(imageId);
        image.sprite = null;
        image.color = TileLoadingColor;

        string path = string.Format(
            "https://webrd01.is.autonavi.com/appmaptile?x={0}&y={1}&z={2}&lang=zh_cn&size=1&scale=1&style=8",
            x, y, zoom);

        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(path, true))
        {
            webRequest.timeout = 15;
            yield return webRequest.SendWebRequest();

            string activeKey;
            if (!ActiveTileRequests.TryGetValue(imageId, out activeKey) || activeKey != tileKey)
            {
                if (onComplete != null)
                {
                    onComplete(false);
                }
                yield break;
            }

            ActiveTileRequests.Remove(imageId);
            if (image == null)
            {
                if (onComplete != null)
                {
                    onComplete(false);
                }
                yield break;
            }

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                image.sprite = null;
                image.color = TileFailureColor;
                if (LoggedTileFailures.Add(tileKey))
                {
                    Debug.LogWarning("地图瓦片加载失败：" + webRequest.responseCode + " " + webRequest.error);
                }
                if (onComplete != null)
                {
                    onComplete(false);
                }
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
            if (texture == null || image == null)
            {
                if (onComplete != null)
                {
                    onComplete(false);
                }
                yield break;
            }

            Sprite sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            CacheTile(tileKey, sprite);
            ApplyTile(image, tileKey, sprite);
            if (onComplete != null)
            {
                onComplete(true);
            }
        }
    }

    private static void ApplyTile(Image image, string tileKey, Sprite sprite)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = sprite;
        DisplayedTileKeys[image.GetInstanceID()] = tileKey;
        image.color = Color.white;
        image.canvasRenderer.SetAlpha(0f);
        image.CrossFadeAlpha(1f, 0.12f, true);
    }

    private static bool TryGetCachedTile(string tileKey, out Sprite sprite)
    {
        if (!TileCache.TryGetValue(tileKey, out sprite) || sprite == null)
        {
            return false;
        }

        LinkedListNode<string> node;
        if (TileCacheNodes.TryGetValue(tileKey, out node))
        {
            TileCacheLru.Remove(node);
            TileCacheLru.AddLast(node);
        }
        return true;
    }

    private static void CacheTile(string tileKey, Sprite sprite)
    {
        if (sprite == null || TileCache.ContainsKey(tileKey))
        {
            return;
        }

        TileCache[tileKey] = sprite;
        LinkedListNode<string> node = TileCacheLru.AddLast(tileKey);
        TileCacheNodes[tileKey] = node;

        TrimTileCache();
    }

    private static void TrimTileCache()
    {
        if (TileCache.Count <= MaxCachedTiles)
        {
            return;
        }

        HashSet<string> displayedTiles = new HashSet<string>(DisplayedTileKeys.Values);
        LinkedListNode<string> candidate = TileCacheLru.First;
        while (TileCache.Count > MaxCachedTiles && candidate != null)
        {
            LinkedListNode<string> next = candidate.Next;
            string tileKey = candidate.Value;
            if (!displayedTiles.Contains(tileKey))
            {
                Sprite sprite;
                TileCache.TryGetValue(tileKey, out sprite);
                TileCacheLru.Remove(candidate);
                TileCacheNodes.Remove(tileKey);
                TileCache.Remove(tileKey);

                if (sprite != null)
                {
                    Texture2D texture = sprite.texture;
                    UnityEngine.Object.Destroy(sprite);
                    if (texture != null)
                    {
                        UnityEngine.Object.Destroy(texture);
                    }
                }
            }

            candidate = next;
        }
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

    /// <summary>
    /// 将tile(瓦片)坐标系转换为LatLngt(地理)坐标系，pixelX，pixelY为图片偏移像素坐标
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
    /// 将LatLngt地理坐标系转换为tile瓦片坐标系，pixelX，pixelY为图片偏移像素坐标
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
    /// 将LatLngt地理坐标系转换为像素坐标系
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
        double y = (1 - Math.Log(Math.Tan(lat_rad) + 1 / Math.Cos(lat_rad)) / Math.PI) / 2;
        y = y * size * 256 % 256;

        float PixelX = (float)x;
        float PixelY = (float)y;
        return new PixelXY(PixelX, PixelY);
    }

    /// <summary>
    /// 计算某经纬度相对地图中心点的 UI 像素偏移。
    /// </summary>
    public static Vector2 LatLngToMapPixelOffset(LatLng point, LatLng center, int zoom)
    {
        if (point == null || center == null)
        {
            return Vector2.zero;
        }

        double mapSize = LocationMap.TileWidthAndHeigth * Math.Pow(2, zoom);
        double pointX = (point.Longitude + 180d) / 360d * mapSize;
        double centerX = (center.Longitude + 180d) / 360d * mapSize;
        double pointLatitudeRadians = point.Latitude * Math.PI / 180d;
        double centerLatitudeRadians = center.Latitude * Math.PI / 180d;
        double pointY = (1d - Math.Log(Math.Tan(pointLatitudeRadians) + 1d / Math.Cos(pointLatitudeRadians)) /
            Math.PI) / 2d * mapSize;
        double centerY = (1d - Math.Log(Math.Tan(centerLatitudeRadians) + 1d / Math.Cos(centerLatitudeRadians)) /
            Math.PI) / 2d * mapSize;

        return new Vector2((float)(pointX - centerX), (float)(centerY - pointY));
    }

    /// <summary>
    /// 将 Web Mercator 全局像素坐标转换为经纬度。
    /// </summary>
    public static LatLng GlobalPixelToLatLng(double pixelX, double pixelY, int zoom)
    {
        double mapSize = LocationMap.TileWidthAndHeigth * Math.Pow(2, zoom);
        double longitude = pixelX / mapSize * 360d - 180d;
        double mercatorY = Math.PI - 2d * Math.PI * pixelY / mapSize;
        double latitude = Math.Atan(Math.Sinh(mercatorY)) * 180d / Math.PI;
        return new LatLng(longitude, latitude);
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
/// 坐标转换类-高德经纬坐标系和Unity世界坐标系的转换
/// </summary>
public static class Conversion
{
    private const int TileSize = 256;//瓦片切图大小
    private const int EarthRadius = 6378137;
    private const double InitialResolution = 2 * Math.PI * EarthRadius / TileSize;
    private const double OriginShift = 2 * Math.PI * EarthRadius / 2;

    //这两个点是目测得到的
    //private static Vector2 bottomRightCoord = new Vector2(113.247890f, 23.068830f);//东南角经纬度
    //private static Vector2 topLeftCoord = new Vector2(113.242090f, 23.074180f);//西北角经纬度
    private static LatLng bottomRightCoord=new LatLng(113.297355d, 23.137078d);//东南角经纬度
    private static LatLng topLeftCoord=new LatLng(113.291528d, 23.142402d);//西北角经纬度

    //这两个差值是在1080*1080的MapPanel中的经纬度差值
    //private const double x_offset = 0.00580d;//面板中的经度差
    //private const double z_offset = 0.00535d;//面板中的纬度差
    private static double x_Offset;//面板中的经度差
    private static double z_Offset;//面板中的纬度差

    //这个差值是MapPanel的长度/宽度
    private const int u_offset = 1080;

    private static Vector2 BottomRightPoint = new Vector2(1080, 920);//东南角坐标
    private static Vector2 TopLeftPoint = new Vector2(0, 2000);//西北角坐标

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
    }

    public static double z_offset
    {
        get { return Math.Abs(TopLeftCoord.Latitude-BottomRightCoord.Latitude); }
    }

    public static void ConfigureMapBounds(LatLng topLeft, LatLng bottomRight)
    {
        TopLeftCoord = topLeft;
        BottomRightCoord = bottomRight;
    }

    /// <summary>
    /// 由经纬度得到坐标点
    /// </summary>
    /// <param name="se"></param>
    /// <returns></returns>
    public static Vector3 GetWorldPoint(Vector2 se)
    {
        double tempX = se.x - TopLeftCoord.Longitude;
        double tempZ = se.y - BottomRightCoord.Latitude;
        double _tempX = tempX * u_offset / x_offset + TopLeftPoint.x;//计算X轴
        double _tempZ = tempZ * u_offset / z_offset + BottomRightPoint.y;//计算Z轴
        //获取该点世界坐标
        return new Vector3((float)_tempX, 0, (float)_tempZ);
    }

    /// <summary>
    /// 由位置点得到经纬度
    /// </summary>
    /// <param name="curPoint"></param>
    /// <returns></returns>
    public static Vector3 GetLatLon(Vector3 curPoint)
    {
        double _x_offset = (curPoint.x - TopLeftPoint.x) * x_offset / u_offset;
        double _z_offset = (curPoint.z - BottomRightPoint.y) * z_offset / u_offset;
        double resultX = _x_offset + TopLeftCoord.Longitude;
        double resultZ = _z_offset + BottomRightCoord.Latitude;
        return new Vector2((float)resultX, (float)resultZ);
    }

    /// <summary>
    /// 计算两点位置的距离，返回两点的距离，单位：米
    /// 该公式由GOOGLE提供，误差小于0.2米
    /// </summary>
    /// <param name="lat1">起点纬度</param>
    /// <param name="lng1">起点经度</param>
    /// <param name="lat2">终点纬度</param>
    /// <param name="lng2">终点经度</param>
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
        return result;
    }

    private static float Rad(float d)
    {
        return d * Mathf.PI / 180;
    }
}
