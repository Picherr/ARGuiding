using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AndroidBuildCommands
{
    private const string PreviewOutput = "Builds/Development/ARGuiding-map-preview.apk";

    [MenuItem("Tools/ARGuiding/Build Map Preview APK")]
    public static void BuildMapPreview()
    {
        bool useCustomKeystore = PlayerSettings.Android.useCustomKeystore;
        string applicationIdentifier = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
        try
        {
            PlayerSettings.Android.useCustomKeystore = false;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, applicationIdentifier + ".preview");
            BuildAndroid(PreviewOutput, BuildOptions.Development | BuildOptions.AllowDebugging);
        }
        finally
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, applicationIdentifier);
            PlayerSettings.Android.useCustomKeystore = useCustomKeystore;
        }
    }

    [MenuItem("Tools/ARGuiding/Build Signed Release APK")]
    public static void BuildSignedRelease()
    {
        ApplySigningSettingsFromEnvironment();
        if (!PlayerSettings.Android.useCustomKeystore ||
            string.IsNullOrWhiteSpace(PlayerSettings.Android.keystoreName) ||
            string.IsNullOrWhiteSpace(PlayerSettings.Android.keyaliasName) ||
            string.IsNullOrEmpty(PlayerSettings.Android.keystorePass) ||
            string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass))
        {
            throw new InvalidOperationException(
                "签名配置不完整。请在 Unity Player Settings 中配置密钥库、别名和密码，或设置 " +
                "ARGUIDING_KEYSTORE_PATH、ARGUIDING_KEYALIAS_NAME、" +
                "ARGUIDING_KEYSTORE_PASSWORD 与 ARGUIDING_KEYALIAS_PASSWORD 环境变量。");
        }

        string output = "Builds/Release/ARGuiding-" + PlayerSettings.bundleVersion + ".apk";
        BuildAndroid(output, BuildOptions.None);
    }

    private static void ApplySigningSettingsFromEnvironment()
    {
        string keystorePath = Environment.GetEnvironmentVariable("ARGUIDING_KEYSTORE_PATH");
        string aliasName = Environment.GetEnvironmentVariable("ARGUIDING_KEYALIAS_NAME");
        string keystorePassword = Environment.GetEnvironmentVariable("ARGUIDING_KEYSTORE_PASSWORD");
        string aliasPassword = Environment.GetEnvironmentVariable("ARGUIDING_KEYALIAS_PASSWORD");
        if (!string.IsNullOrWhiteSpace(keystorePath))
        {
            PlayerSettings.Android.keystoreName = Path.GetFullPath(keystorePath.Trim());
            PlayerSettings.Android.useCustomKeystore = true;
        }
        if (!string.IsNullOrWhiteSpace(aliasName))
        {
            PlayerSettings.Android.keyaliasName = aliasName.Trim();
        }
        if (!string.IsNullOrEmpty(keystorePassword))
        {
            PlayerSettings.Android.keystorePass = keystorePassword;
        }
        if (!string.IsNullOrEmpty(aliasPassword))
        {
            PlayerSettings.Android.keyaliasPass = aliasPassword;
        }
    }

    private static void BuildAndroid(string relativeOutputPath, BuildOptions options)
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
        if (scenes.Length == 0)
        {
            throw new InvalidOperationException("Build Settings 中没有启用场景。");
        }

        string outputPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), relativeOutputPath));
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        EditorUserBuildSettings.buildAppBundle = false;

        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = options
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException("Android 构建失败：" + report.summary.result);
        }

        Debug.Log("Android APK 构建成功：" + outputPath + "，大小 " + report.summary.totalSize + " 字节。");
    }
}
