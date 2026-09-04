using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class ReleaseReadinessValidator : IPreprocessBuildWithReport
{
    private const string MainScenePath = "Assets/Scenes/Main_Scene.unity";
    private const string DefaultApplicationIdentifier = "com.DefaultCompany.ARGuiding";

    public int callbackOrder
    {
        get { return 0; }
    }

    [MenuItem("Tools/ARGuiding/Validate Release Readiness")]
    public static void ValidateFromMenu()
    {
        Validate(true);
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.Android ||
            (report.summary.options & BuildOptions.Development) != 0)
        {
            return;
        }

        if (!Validate(true))
        {
            throw new BuildFailedException("ARGuiding 发布前校验失败，请先修复 Console 中列出的问题。");
        }
    }

    public static bool Validate(bool logResult)
    {
        List<string> errors = new List<string>();
        List<string> warnings = new List<string>();

        ValidateScenes(errors);
        ValidateAndroidSettings(errors, warnings);
        ValidateCredentials(errors, warnings);

        if (logResult)
        {
            foreach (string error in errors)
            {
                Debug.LogError("[发布校验] " + error);
            }

            foreach (string warning in warnings)
            {
                Debug.LogWarning("[发布校验] " + warning);
            }

            if (errors.Count == 0)
            {
                Debug.Log("ARGuiding 发布前校验通过，共有 " + warnings.Count + " 项警告需要确认。");
            }
        }

        return errors.Count == 0;
    }

    private static void ValidateScenes(List<string> errors)
    {
        string[] enabledScenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (enabledScenes.Length != 1 || enabledScenes[0] != MainScenePath)
        {
            errors.Add("Build Settings 必须只启用 " + MainScenePath + "。");
        }
    }

    private static void ValidateAndroidSettings(List<string> errors, List<string> warnings)
    {
        string applicationIdentifier = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
        if (string.IsNullOrWhiteSpace(applicationIdentifier) ||
            applicationIdentifier == DefaultApplicationIdentifier)
        {
            errors.Add("Android 包名仍是默认值，请设置正式 application identifier。");
        }

        if (string.IsNullOrWhiteSpace(PlayerSettings.bundleVersion) || PlayerSettings.bundleVersion == "0.1")
        {
            errors.Add("版本名称仍是原型默认值 0.1，请设置正式版本号。");
        }

        if (PlayerSettings.Android.bundleVersionCode <= 1)
        {
            warnings.Add("Android version code 仍为 1，发布新版本前必须递增。");
        }

        if (!PlayerSettings.Android.useCustomKeystore)
        {
            errors.Add("尚未启用自定义 Android keystore，不能生成正式签名包。");
        }

        int minimumSdk = (int)PlayerSettings.Android.minSdkVersion;
        int targetSdk = (int)PlayerSettings.Android.targetSdkVersion;
        if (minimumSdk > targetSdk && targetSdk > 0)
        {
            errors.Add("Android minSdk 不能高于 targetSdk。");
        }

        if (minimumSdk >= 33)
        {
            warnings.Add("minSdk 为 " + minimumSdk + "，设备兼容范围较窄，请确认这是产品决策。");
        }

        if (targetSdk == 33)
        {
            warnings.Add("targetSdk 为 33，提交应用商店前请按目标渠道的当前政策复核。");
        }
    }

    private static void ValidateCredentials(List<string> errors, List<string> warnings)
    {
        string key;
        string keyError;
        if (!AppSecrets.TryGetAmapWebServiceKey(out key, out keyError))
        {
            errors.Add(keyError);
        }

        Object vuforiaConfiguration = AssetDatabase.LoadMainAssetAtPath(
            "Assets/Resources/VuforiaConfiguration.asset");
        if (vuforiaConfiguration == null)
        {
            errors.Add("缺少 VuforiaConfiguration.asset。");
        }
        else
        {
            warnings.Add("请确认 Vuforia License 已在控制台轮换，并受正确的应用限制保护。");
        }
    }
}
