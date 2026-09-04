using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MapPanel : BasePanel
{
    private Image image;

    //public LineRenderer lineRenderInMap;//二维地图上的导航线

    protected override void Start()
    {
        base.Start();
        image = GetControl<Image>("map");
    }

    public void OnMap()
    {
        string key;
        string keyError;
        if (!AppSecrets.TryGetAmapWebServiceKey(out key, out keyError))
        {
            this.TriggerEvent(EventName.ShowNotification, new ShowNotificationArgs
            {
                message = keyError,
                isBtnOn = false,
                autoOff = true
            });
            return;
        }

        LatLng center = Location.mLatLng ?? NavigationDefaults.CreateParkCenter();
        string coordinate = center.Longitude.ToString(CultureInfo.InvariantCulture) + "," +
                            center.Latitude.ToString(CultureInfo.InvariantCulture);
        StartCoroutine(PostSprite("https://restapi.amap.com/v3/staticmap?zoom=15&size=400*400&location=" +
            coordinate + "&markers=mid,0xFF0000,A:" + coordinate +
            "&key=" + UnityWebRequest.EscapeURL(key)));
    }

    public void LoadSpriteByte(byte[] path)
    {
        image.sprite = ChangeToSprite(ByteToTex2d(path));
    }

    public static Texture2D ByteToTex2d(byte[] bytes)
    {
        int w = 400;
        int h = 400;
        Texture2D tex = new Texture2D(w, h);
        tex.LoadImage(bytes);
        return tex;
    }

    private Sprite ChangeToSprite(Texture2D tex)
    {
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        return sprite;
    }

    public IEnumerator PostSprite(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
        {
            webRequest.timeout = 15;
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("静态地图加载失败：" + webRequest.responseCode + " " + webRequest.error);
                this.TriggerEvent(EventName.ShowNotification, new ShowNotificationArgs
                {
                    message = "地图加载失败，请检查网络后重试。",
                    isBtnOn = false,
                    autoOff = true
                });
                if (image != null)
                {
                    image.sprite = null;
                }
            }
            else
            {
                if (image == null)
                {
                    Debug.LogWarning("未找到静态地图 Image 控件。");
                    yield break;
                }

                LoadSpriteByte(webRequest.downloadHandler.data);
            }
        }
    }
}
