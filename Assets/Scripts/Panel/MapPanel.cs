using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MapPanel : BasePanel
{
    private static string key = "7b8f9443eed7ff041d9ad7d9dd9e87a2";

    private Image image;

    //public LineRenderer lineRenderInMap;//二维地图上的导航线

    public void OnMap()
    {
        StartCoroutine(PostSprite("https://restapi.amap.com/v3/staticmap?zoom=15&size=400*400&markers=mid,0xFF0000,A:" +
            "113.245600,23.070910&key=" + key));
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
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error:" + webRequest.error);
                Debug.Log(webRequest.responseCode);
                image.sprite = null;
            }
            else
            {
                LoadSpriteByte(webRequest.downloadHandler.data);
                webRequest.Dispose();
            }
        }
    }
}
