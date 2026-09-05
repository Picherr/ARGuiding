# 项目结构

ARGuiding 的自有代码和资源统一放在 `Assets/ARGuiding`。目录只表达所有权和职责；现有类型仍处于全局命名空间，并继续编译到 Unity 的 `Assembly-CSharp`。

## 业务目录

```text
Assets/ARGuiding/
├─ Runtime/
│  ├─ Application/       应用启动与流程编排
│  ├─ Core/              事件、配置、生命周期、资源加载和单例基础设施
│  ├─ Features/
│  │  ├─ AR/             平面检测、方向传感器和讲解模型
│  │  ├─ Attractions/    景点静态信息
│  │  ├─ Map/            地图瓦片、坐标换算和地图交互
│  │  └─ Navigation/     高德接口、响应解析和导航计算
│  └─ UI/                UI 基类、管理器和业务面板
├─ Editor/               ARGuiding 自有编辑器与构建工具
├─ Tests/Editor/          Edit Mode 测试
├─ Scenes/               当前发布场景
├─ Resources/            Resources.Load 使用的业务 Prefab、UI 和本地配置
└─ Content/              图片、字体、音频、视频、动画和模型
```

`Assets/Legacy` 保存未进入当前构建流程、仍需复核的旧场景、示例代码和试验 Prefab。它们仍位于 `Assets` 下，因此继续参与 Unity 导入和脚本编译。

## 第三方和兼容目录

- `Assets/BuildReport`：第三方构建报告工具。工具内部依赖固定目录，本轮保持原位。
- `Assets/LJR`：LitJson Ruler 资源。导航代码依赖其中的 `Plugins/LitJson.dll`。
- `Assets/Plugins/IngameDebugConsole`：运行时调试控制台，当前主场景仍有引用。
- `Assets/TextMesh Pro`：Unity TextMesh Pro 资源，保持标准目录。
- `Assets/Editor/Migration`：Vuforia 导入迁移代码，保持原路径以避免改变其路径约定。
- `Assets/Resources/VuforiaConfiguration.asset`：Vuforia 固定位置的运行时配置；移动后 Vuforia 会在该路径重新生成第二份配置，因此保持原位。
- `QCAR`：历史 Vuforia 数据；用途尚未由项目内引用确认，本轮保持原位。

## 路径约定

- 正式入口为 `Assets/ARGuiding/Scenes/Main_Scene.unity`，Build Settings 只启用该场景。
- 本地高德配置放在 `Assets/ARGuiding/Resources/ARGuidingSecrets.json`，该文件及 `.meta` 必须保持 Git ignored。
- Vuforia 配置固定放在 `Assets/Resources/VuforiaConfiguration.asset`，发布校验器按该路径检查。
- 移动 `Resources` 下的内容时必须保持逻辑加载路径，例如 `UI/SystemPanel` 和 `Prefabs/RouteInMap`。
- 移动 Unity 资产时必须同时保留对应 `.meta`，不得重新生成已有资产 GUID。

## 后续结构演进

命名空间和 Assembly Definition 会改变现有代码或程序集身份，因此不属于本次兼容性整理。后续若引入，应单独实施并完成场景、Prefab、反射调用和 Editor Tests 验证。
