# ARGuiding

ARGuiding 是面向广州黄花岗公园的 Unity Android AR 导览原型。应用提供五个固定景点的介绍、GPS 定位、高德步行路线、二维地图导航，以及到达后的 AR 虚拟讲解或视频介绍。

## 开发环境

- Unity：`2021.3.9f1c1`
- 目标平台：Android
- AR：Vuforia Engine `10.22.5`
- UI：Unity UGUI、TextMesh Pro `3.0.6`
- 主场景：`Assets/Scenes/Main_Scene.unity`

Unity 安装时需要同时安装 Android Build Support、Android SDK & NDK Tools 和 OpenJDK。尽量使用项目记录的精确 Unity 版本打开，避免未经验证的场景和资源升级。

## 获取项目

Vuforia 本地包通过 Git LFS 管理。首次克隆后执行：

```powershell
git lfs install
git lfs pull
git lfs fsck
```

确认 `Packages/com.ptc.vuforia.engine-10.22.5.tgz` 不是 LFS 指针文本，并且 `git lfs fsck` 没有报错。

Unity 项目中必须提交 `Assets`、所有 `.meta` 文件、`Packages` 和 `ProjectSettings`。不要提交 `Library`、`Temp`、`Logs`、`UserSettings`、APK 或 AAB。

## 首次恢复检查

1. 使用 Unity Hub 安装指定编辑器和 Android 模块。
2. 打开仓库根目录，等待 Package Manager 和资源导入完成。
3. 在 Console 中清除旧日志，再确认没有编译错误、`Missing Script` 或资源导入错误。
4. 打开 `Assets/Scenes/Main_Scene.unity`。
5. 检查 Build Settings 中只有 `Main_Scene` 被启用。
6. 在编辑器中进入 Play Mode，确认 UI、地图和测试导航能够启动。
7. 构建 Development APK，在真实 Android 设备上完成下方冒烟测试。

如果 Unity 自动修改了场景、预制体或 ProjectSettings，不要直接全部提交；先确认这些变化是否来自版本升级或平台差异。

## 真机冒烟测试

- 首次启动能够正确申请相机和精确定位权限。
- 拒绝权限、关闭系统定位或断网时，应用能够给出提示且不会崩溃。
- 地图瓦片能够加载，当前位置位于黄花岗公园附近。
- 五个景点都能打开介绍并开始导航。
- 导航文字、目的地和剩余距离能够更新。
- 停止导航后，二维和 AR 路线都被清除。
- 距离目的地小于 20 米时进入到达流程。
- 非“黄花文化馆”景点能够检测地面、放置“小明”并播放对应音频。
- “黄花文化馆”能够播放、暂停、拖动和重播介绍视频。
- 在 2D 与 AR 模式之间多次切换不会重复创建模型、监听器或路线对象。

## 敏感配置

当前历史版本曾在源码和 Vuforia 配置中保存客户端密钥。使用项目前应在对应服务控制台轮换或重新生成密钥，并限制可用应用、签名、接口和调用额度。已经进入 Git 历史的密钥不能通过普通删除恢复为秘密。

不要在 Issue、日志、截图或提交信息中粘贴完整密钥。

## 目录概览

- `Assets/Scripts`：导航、地图、AR、UI 和通用管理器。
- `Assets/Resources/UI`：运行时加载的 UI 预制体。
- `Assets/Resources/Prefabs`：路线、地图瓦片、相机和虚拟讲解员。
- `Assets/ArtRes`：图片、字体、音频、视频、动画和模型。
- `Packages`：Unity 依赖清单和 Vuforia 本地包。
- `ProjectSettings`：Unity 编辑器、Android 和构建配置。

## 当前限制

这是恢复中的原型项目。地图中心和部分编辑器导航数据仍使用固定坐标，定位与路线请求时序、网络异常处理、AR 方向计算及发布配置仍需继续修复。完成这些修复和真机验收前，不应作为正式导航产品发布。
