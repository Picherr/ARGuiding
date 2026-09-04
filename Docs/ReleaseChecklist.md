# Android 发布检查清单

本项目在恢复与真机验收完成前只能生成 Development 构建，不应直接发布。

## 凭据

- 在高德控制台轮换已经进入 Git 历史的 Key，并限制接口、应用和调用额度。
- 根据 `Config/ARGuidingSecrets.example.json` 创建本地 `Assets/Resources/ARGuidingSecrets.json`。
- 在 Vuforia 控制台轮换历史 License，并确认新 License 的应用限制。
- 不要提交高德配置、keystore、JKS、口令或完整服务响应。

## Android 配置

- 将包名从 `com.DefaultCompany.ARGuiding` 改为团队确认的正式标识。
- 设置版本名称并递增 version code。
- 根据计划支持的设备确定 minSdk；当前值 33 会排除较旧 Android 设备。
- 根据目标应用商店的当前政策确定 targetSdk，并验证 Unity、Gradle 与 Vuforia 的兼容性。
- 配置正式 keystore；密钥文件与口令只能保存在受控的本地或 CI 密钥存储中。

## 自动检查

在 Unity 中执行：

```text
Tools > ARGuiding > Validate Release Readiness
```

非 Development Android 构建也会自动执行相同检查。存在以下问题时将阻止构建：

- Build Settings 没有且仅有 `Assets/Scenes/Main_Scene.unity`。
- 包名或版本名称仍为原型默认值。
- 未配置正式签名。
- 缺少本地高德 Key 或 Vuforia 配置。

## Development APK 冒烟测试

- 首次启动正确申请相机和精确定位权限。
- 拒绝权限、关闭定位和断网不会导致崩溃，并显示可理解的提示。
- 地图以实际位置为中心，瓦片和当前位置标记正常加载。
- 五个景点均可打开介绍、开始导航并更新指令与剩余距离。
- 停止导航会清除二维及 AR 路线。
- 距离目的地小于 20 米时进入到达流程。
- 四个讲解景点可检测地面、放置“小明”并播放音频。
- 黄花文化馆视频可播放、暂停、拖动和重播。
- 重复切换 2D/AR 不会重复创建模型、事件监听或路线对象。

记录测试设备、Android 版本、APK commit、执行日期、失败项和相关日志。所有失败项关闭后，再生成非 Development 构建。
