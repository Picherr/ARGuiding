# Legacy 内容清单

本清单只记录隔离结果，不授权删除。所有内容在完成 Unity 编译、Edit Mode 测试、主流程验证并经人工确认前必须保留。

## 已隔离内容

| 内容 | 当前证据 | 保留原因 | 后续清理条件 |
| --- | --- | --- | --- |
| `Assets/Legacy/Scenes/Main.unity` | 未列入 Build Settings | 历史完整场景，包含大量序列化引用 | 确认不再用于演示或资产恢复 |
| `Assets/Legacy/Scenes/New Scene.unity` | 未列入 Build Settings；引用 `test1.cs` | 视频交互试验场景 | 主场景视频流程真机验证通过 |
| `Assets/Legacy/Scenes/New Scene 1.unity` | 未列入 Build Settings | Vuforia/调试试验场景 | 主场景 AR 与调试控制台验证通过 |
| `Assets/Legacy/Scenes/New Scene 2.unity` | 未列入 Build Settings | 历史试验场景 | 确认没有独立演示用途 |
| `Assets/Legacy/Runtime/Test.cs` | 场景和 Prefab 中未发现脚本 GUID 引用 | 音频和生命周期试验代码 | Unity 编译及音频讲解验证通过 |
| `Assets/Legacy/Runtime/test1.cs` | 仅由旧 `New Scene.unity` 引用 | 视频播放试验代码 | 对应旧场景一并获准清理 |
| `Assets/Legacy/Runtime/Framework/InputMgr.cs` | 项目代码中没有消费者 | 通用输入管理模板 | 确认没有外部脚本调用其公开 API |
| `Assets/Legacy/Runtime/Framework/MusicMgr.cs` | 项目代码中没有消费者 | 通用音频管理模板 | 确认讲解音频不依赖该管理器 |
| `Assets/Legacy/Runtime/Framework/PoolMgr.cs` | 项目代码中没有消费者 | 通用对象池模板 | 确认运行时没有动态或反射调用 |
| `Assets/Legacy/Runtime/Framework/ScenesMgr.cs` | 项目代码中没有消费者 | 通用场景管理模板 | 确认应用仅使用当前单场景流程 |
| `Assets/Legacy/Runtime/Framework/Singleton.cs` | 项目代码中没有派生或调用 | 未使用的普通 C# 单例模板 | Unity 编译和代码搜索再次确认无引用 |
| `Assets/Legacy/Content/Prefabs/Directional Light.prefab` | 正式场景和 Prefab 中未发现 GUID 引用 | 历史场景对象模板 | 主场景光照与 AR 显示验证通过 |
| `Assets/Legacy/Content/Prefabs/Main Camera.prefab` | 正式场景和 Prefab 中未发现 GUID 引用 | 历史相机模板 | 主场景相机与 AR 流程验证通过 |
| `Assets/Legacy/Content/Prefabs/TestPanel.prefab`、`Inputtip.prefab` | 正式场景和 Prefab 中未发现入口引用；两者仅互相关联 | UI 输入试验 Prefab | 确认没有独立演示用途 |

## 保留原位的待复核内容

- `Assets/LJR`：`LitJson.dll` 是当前运行时依赖；其 Editor、Demo 和代码生成内容可在替换或单独保留 DLL 后复核。
- `Assets/BuildReport`：业务代码未直接调用，但它是可独立使用的编辑器工具，并含 `Assets/BuildReport` 固定路径。
- `Assets/Editor/Migration`：当前 Vuforia 已由本地 UPM 包提供，迁移脚本是否仍需保留应在全新克隆恢复测试后决定。
- `QCAR`：项目内未发现文本引用，在确认 Vuforia 运行时和构建产物不依赖前不处理。

## 未使用资产静态审计

审计以正式主场景、全部业务 `Resources` 资产和 Vuforia 配置为入口，递归分析序列化 GUID 依赖。结果用于缩小人工复核范围，不能单独作为删除依据；运行时按名称、反射或原生插件加载的内容可能不会出现在依赖闭包中。

| 候选 | 体积 | 证据 | 建议 |
| --- | ---: | --- | --- |
| `Content/Fonts/HWZS.TTF` | 11.57 MB | 与正在使用的 `STZHONGS.TTF` 字节完全相同；没有序列化或代码引用 | 高优先级，确认字体回退后逐文件删除 |
| `Content/Model/Talking.fbx`、`Walking.fbx`、`Waving Gesture.fbx` | 约 21.04 MB | 未进入正式资产依赖闭包；当前动画控制器引用独立 `.anim` 文件 | 在模型和全部动画状态目视验证后清理 |
| `Content/Model/Standing Idle.fbm` 中四张贴图及两个 Materials | 约 5.64 MB | 只形成内部材质—贴图子图，没有正式入口引用 | 检查讲解模型材质后整组复核 |
| `Content/Animation/standing.anim` | 0.88 MB | 动画控制器使用 `standing_short.anim`，未引用该文件 | 播放待机动画后复核 |
| Logo、字符集文本、三个未使用方向箭头、fillet_style UI、`icon_bg.png`、`Message.png` | 约 0.37 MB | 未进入正式依赖闭包 | 截图比对 UI 后复核 |
| `Assets/LJR` 中除 `Plugins/LitJson.dll` 外的生成器、Demo 和辅助代码 | 约 6.69 MB | 应用代码只直接依赖 LitJson DLL；Editor 子目录占约 6.62 MB | 先复制项目做全新导入和 Android 构建，再决定是否仅保留 DLL 与许可说明 |
| `Assets/BuildReport` | 2.40 MB | 不参与业务运行，但提供独立编辑器工具且依赖固定目录 | 由团队确认是否继续使用，不能按“无运行时引用”直接删除 |

本次扫描共发现 23 个未进入正式依赖闭包的业务内容文件，合计约 39.5 MB。最大收益来自重复字体和三个未引用的 FBX 源文件。

## 清理规则

后续清理应使用独立提交，并逐个列出和删除已确认的文件及其 `.meta`。禁止批量删除或递归删除目录；每次清理后重新执行 Unity 导入、测试和主流程冒烟验证。
