using System;
using UnityEngine;

/// <summary>
/// Loads credentials from Assets/Resources/ARGuidingSecrets.json.
/// The concrete secrets file is intentionally ignored by Git.
/// </summary>
public static class AppSecrets
{
    private const string ResourceName = "ARGuidingSecrets";

    [Serializable]
    private class SecretsData
    {
        public string amapWebServiceKey;
    }

    private static bool hasLoaded;
    private static SecretsData data;

    public static bool TryGetAmapWebServiceKey(out string key, out string error)
    {
        Load();
        key = data == null ? string.Empty : data.amapWebServiceKey;
        if (string.IsNullOrWhiteSpace(key))
        {
            error = "未配置高德 Web 服务 Key，请根据 Config/ARGuidingSecrets.example.json 创建本地密钥文件。";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static void Load()
    {
        if (hasLoaded)
        {
            return;
        }

        hasLoaded = true;
        TextAsset asset = Resources.Load<TextAsset>(ResourceName);
        if (asset == null)
        {
            return;
        }

        try
        {
            data = JsonUtility.FromJson<SecretsData>(asset.text);
        }
        catch (Exception exception)
        {
            Debug.LogError("ARGuidingSecrets.json 格式无效：" + exception.Message);
        }
    }
}
