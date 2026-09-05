# ARGuiding 开发指南

本文件适用于整个仓库，供自动化开发工具和新加入的开发者快速建立项目上下文。开始修改前先阅读本文件；涉及发布、目录调整或历史资产时，再阅读文末列出的专项文档。

## 项目定位

ARGuiding 是面向广州黄花岗公园的 Unity Android AR 导览原型。用户可以浏览五个固定景点、获取 GPS 位置、请求高德步行路线、在二维瓦片地图上查看路线，并在到达后使用 AR 虚拟讲解员或视频介绍。

- Unity：`2021.3.9f1c1`，应尽量使用精确版本打开。
- 目标平台：Android；当前 Player Settings 使用 IL2CPP、ARM64、minSdk 26、targetSdk 33。
- AR：Vuforia Engine `10.22.5`，本地 UPM 包位于 `Packages/com.ptc.vuforia.engine-10.22.5.tgz`。
- UI：UGUI 与 TextMesh Pro `3.0.6`。
- JSON：`Assets/LJR/Plugins/LitJson.dll`。
- 正式入口：`Assets/ARGuiding/Scenes/Main_Scene.unity`，Build Settings 只应启用此场景。
- 当前仍是恢复中的原型；地图、定位、路线和 AR 方向必须以 Android 真机结果为准。

## 首次开始工作

1. 在仓库根目录检查 `git status --short`，不要覆盖已有的用户改动。
2. 拉取并校验 Git LFS：

   ```powershell
   git lfs install
   git lfs pull
   git lfs fsck
   ```

3. 确认 Vuforia `.tgz` 是约 140 MB 的实际包，而不是 LFS 指针文本。
4. 用 Unity Hub 安装 `2021.3.9f1c1`，同时安装 Android Build Support、Android SDK & NDK Tools、OpenJDK。
5. 用 Unity 打开仓库根目录，等待 Package Manager 和资源导入完成；清空 Console 后检查编译错误、导入错误和 `Missing Script`。
6. 根据 `Config/ARGuidingSecrets.example.json` 创建本地文件 `Assets/ARGuiding/Resources/ARGuidingSecrets.json`，填写受限的高德 Web 服务 Key。该文件及其 `.meta` 已被忽略，禁止提交。

## 仓库结构与所有权

```text
Assets/ARGuiding/
├─ Runtime/
│  ├─ Application/        启动和主流程编排
│  ├─ Core/               事件、配置、资源加载、生命周期和单例基础设施
│  ├─ Features/
│  │  ├─ AR/              Vuforia 平面放置、指南针、陀螺仪、讲解模型
│  │  ├─ Attractions/     五个景点的名称、介绍和坐标
│  │  ├─ Map/             瓦片下载、缓存、坐标换算、平移缩放与标记
│  │  └─ Navigation/      高德请求、响应解析、距离和方向计算
│  └─ UI/                 面板基类、UI 管理器和四个业务面板
├─ Editor/                发布校验和 Android 构建菜单
├─ Tests/Editor/          NUnit Edit Mode 测试
├─ Scenes/                当前发布场景
├─ Resources/             通过字符串动态加载的 UI 与业务 Prefab
└─ Content/               图片、字体、音频、视频、动画和模型
```

其他目录：

- `Assets/Legacy`：旧场景、试验代码和 Prefab；未进入当前构建入口，但仍会被 Unity 导入，其中脚本仍可能参与编译。保留依据见 `Docs/LegacyInventory.md`。
- `Assets/BuildReport`、`Assets/LJR`、`Assets/Plugins`、`Assets/TextMesh Pro`：第三方或工具代码。除非任务明确要求，不要格式化、重构或批量调整这些目录。
- `Assets/Editor/Migration`：Vuforia 迁移代码，保持原路径。
- `Assets/Resources/VuforiaConfiguration.asset`：Vuforia 要求的固定配置位置，不要移动。
- `QCAR`：历史 Vuforia 数据，当前用途未完全确认，不要删除。
- `Packages`：Unity 依赖和 Vuforia 本地包。
- `ProjectSettings`：Unity、Android 和构建配置。
- `Config`：仅放可提交的配置说明和示例，不放真实凭据。

自有代码目前没有 Assembly Definition，也没有命名空间，继续编译到 Unity 默认程序集。引入命名空间或 `.asmdef` 会影响场景、Prefab、Editor 代码和测试，应作为单独迁移处理，不要顺手进行。

## 启动与主流程

主场景只直接引用自有脚本 `GameMgr`，大量对象在运行时创建：

1. `GameMgr.Awake` 通过 `UIManager` 加载 `MapPanel`、`GuidingPanel`、`InfoPanel`、`SystemPanel`。
2. `UIManager` 先同步创建 `Resources/UI/Canvas` 与 `Resources/UI/EventSystem`，再把面板异步挂到 `Bot`、`Mid`、`Top`、`System` 四层。
3. `GameMgr` 创建 `Prefabs/RouteCamera`，调用 `GaoDeAPI.OnLocating()`，并把场景里的 Vuforia `PlaneFinderBehaviour` 交给 `ARGroundPlane`。
4. `GaoDeAPI` 创建二维路线 `Prefabs/RouteInMap`；编辑器模式固定使用黄花岗公园测试坐标，设备模式申请精确定位权限并将 GPS 坐标转换为高德坐标。
5. 用户在 `InfoPanel` 选择景点并点击前往后，先发送结束导航事件清理旧状态，再发送开始导航事件。`GaoDeAPI` 每 10 秒刷新一次高德步行路线。
6. 路线响应由 `AmapResponseParser` 解析，`GaoDeAPI` 更新导航文字，在二维地图绘制完整 polyline；处于 AR 模式时，用下一段有效路线点计算设备相对方向。
7. 距离目的地小于 20 米时停止导航并进入到达状态。景点 4“黄花文化馆”展示视频；其他景点启用 Vuforia 平面检测，放置 `XiaoMing` 并按景点索引播放讲解音频。

关键状态是进程内静态/单例状态，没有持久化：

- `InfoPanel.desIndex`：当前目的地；编辑器默认 `1`，设备构建默认 `-1`。
- `Location.mLatLng`：最新高德坐标；定位完成前使用公园中心兜底。
- `GaoDeAPI`：定位、导航和路线 LineRenderer 状态。
- `SystemPanel`：2D/AR、停止按钮、AR 路线显示和视频状态。

## 关键代码索引

- `Runtime/Application/GameMgr.cs`：唯一业务启动入口。
- `Runtime/Core/Events/CustomEventArgs.cs`、`EventCenter.cs`：跨模块通信协议。订阅通常在 `Awake`，必须在 `OnDestroy` 对称取消。
- `Runtime/Core/Configuration/AppSecrets.cs`：从 `Resources/ARGuidingSecrets.json` 延迟读取高德 Key；首次读取后会缓存结果。
- `Runtime/Core/Managers/ResMgr.cs`、`MonoMgr.cs`：`Resources` 实例化和非 Mono 类的协程/Update 桥接。
- `Runtime/UI/BasePanel.cs`：按子对象名称缓存控件，并把所有 `Button` 自动转发到 `OnClick(objName)`。
- `Runtime/UI/UIManager.cs`：动态面板及 UI 分层。`HidePanel` 会销毁面板，而不是简单失活。
- `Runtime/UI/InfoPanel.cs`：景点选择、搜索、发起导航；当前列表与五个按钮由 Prefab 序列化维护。
- `Runtime/UI/SystemPanel.cs`：通知、停止导航、2D/AR 切换、AR 路线开关和视频控制。
- `Runtime/Features/Attractions/Info.cs`：五个景点的单一静态数据源。
- `Runtime/Features/Navigation/GaoDeAPI.cs`：权限、GPS、高德坐标转换、步行路线刷新、到达判断和路线绘制。
- `Runtime/Features/Navigation/AmapResponseParser.cs`：可独立测试的高德响应解析。
- `Runtime/Features/Navigation/NavigationDefaults.cs`：公园中心、20 米到达阈值和 10 秒刷新间隔。
- `Runtime/Features/Navigation/NavigationMath.cs`：bearing 与 AR 本地方向计算。
- `Runtime/Features/Map/Location.cs`：Web Mercator 换算、瓦片请求和最多 128 张的 LRU 缓存、经纬度与路线坐标换算。
- `Runtime/Features/Map/LocationMap.cs`：18 级、7×7、256 像素瓦片网格，瓦片回收，当前位置和五个景点标记。
- `Runtime/Features/Map/MapPosMgr.cs`：单指平移、双指/滚轮缩放、回中，并同步正交 `RouteCamera`。
- `Runtime/Features/AR/ARGroundPlane.cs`：Plane Finder 监听和讲解员单次放置。
- `Runtime/Features/AR/ModelMgr.cs`：讲解动画触发音频，切回 2D 时销毁模型。
- `Editor/ReleaseReadinessValidator.cs`：非 Development Android 构建前的阻断校验。
- `Editor/AndroidBuildCommands.cs`：Development 预览 APK 与签名 Release APK 构建入口。

## 隐式接口与高风险耦合

### Resources 路径

以下字符串是运行时接口；移动或改名时必须同步代码和所有序列化引用：

- `UI/Canvas`、`UI/EventSystem`
- `UI/MapPanel`、`UI/GuidingPanel`、`UI/InfoPanel`、`UI/SystemPanel`
- `Prefabs/RouteCamera`、`Prefabs/RouteInMap`、`Prefabs/RouteInWorld`、`Prefabs/XiaoMing`
- `ARGuidingSecrets`

`Resources` 资产应位于 `Assets/ARGuiding/Resources`。Vuforia 配置是例外，固定在 `Assets/Resources`。

### Prefab 层级与名称

`BasePanel` 使用 GameObject 名称找控件和分发点击事件。诸如 `btnStop`、`btnChangeMode`、`GuidingText`、`DesName`、`DisMiles` 等名称相当于代码 API。重命名 UI 节点前先搜索 `GetControl<...>("...")` 和 `OnClick` 的字符串分支。

`Canvas` 必须保留 `Bot/Mid/Top/System` 子层。场景必须保留 Vuforia `PlaneFinderBehaviour` 和名为 `Ground Plane Stage` 的对象。`RouteCamera` 的对象名也被 `MapPosMgr` 查找。

### 事件协议

主要事件流：

- `StartGuidingDirection` / `EndGuidingDirection`：`GaoDeAPI` 控制定时路线请求，`SystemPanel` 控制按钮和清理路线。
- `UpdateGuidingInfo`：`GuidingPanel` 更新导航提示、目的地和距离。
- `ShowNotification`：`SystemPanel` 显示自动或手动关闭的提示。
- `ChangeModeToARGuidingType`：同步未导航、导航中、已到达三种 AR UI 状态。
- `ChangeModeTo2DGuiding`：销毁已放置讲解模型。
- `LocationUpdated`：更新地图覆盖物；显式定位时还会重置地图平移并重新居中。
- `AlreadyCreatedModel`：移除 Plane Finder 的交互命中监听，避免重复创建。

`HaveArrivedDestination` 和 `VideoIntroduction` 当前枚举/监听存在，但主流程没有有效触发者。修改时先搜索实际生产者和消费者，不要仅凭枚举名假设它们在用。

### 地图与路线坐标

地图瓦片使用 Web Mercator；高德路线和景点数据使用经度、纬度顺序。`Vector2` 中约定 `x=经度`、`y=纬度`，AR 路线临时 `Vector3` 中使用 `x=经度`、`z=纬度`。不要在未补测试的情况下交换参数顺序。

二维路线由世界空间 LineRenderer 和正交 `RouteCamera` 渲染到 UI，地图平移/缩放后必须调用同步逻辑。`Conversion.ConfigureMapBounds` 会在地图重建时根据当前 1080×1080 视口更新换算边界；调整面板尺寸、相机或 LineRenderer 时应联合验证。

### 景点数量假设

项目多处硬编码 `1..5`，且景点 4有特殊视频分支。新增、删除或重排景点至少要联合修改并验证：

- `Info.cs` 的名称、介绍、坐标；
- `InfoPanel.prefab` 的按钮、搜索列表和 `InfoPanel.OnClick`；
- `LocationMap` 的标记循环；
- `XiaoMing.prefab` 上 `ModelMgr.audioClip` 的索引；
- `SystemPanel` 对景点 4的特殊处理；
- Edit Mode 测试与真机冒烟清单。

## 编码约定

- 保持与附近代码一致的 C# 风格和 Unity 生命周期结构；不要为了顺手“现代化”而大范围改名或格式化旧代码。
- 新的纯计算、解析和状态转换尽量写成不依赖场景的普通类/静态方法，并补 Edit Mode 测试。
- 新增跨模块事件时，在 `EventName` 定义事件，必要时添加专用 `EventArgs`；所有监听必须成对添加/移除。
- 异步资源和网络回调必须处理对象已销毁、模式已切换、请求已过期的情况，避免重复对象或旧响应覆盖新状态。
- 网络请求要设置合理超时，区分传输失败、业务失败和解析失败，并通过 `ShowNotification` 给出用户可理解的提示；日志不得包含完整 Key。
- 对经纬度和 API 参数使用 `CultureInfo.InvariantCulture`，避免设备区域设置改变小数点格式。
- 编辑器中的公园中心坐标是测试替身，不代表设备定位链路已通过。
- 修改 `[SerializeField]` 字段、组件类名、Prefab 层级、场景对象或动画状态名时，必须在 Unity 中检查序列化引用和 `Missing Script`。
- 移动 Unity 资产必须连同 `.meta` 一起移动并保留 GUID。优先在 Unity Editor 内移动；不要删除 `.meta` 后让 Unity 重新生成。

## 构建与验证

所有命令从仓库根目录运行。将下面的 `$unity` 替换为本机 Unity 2021.3.9f1c1 可执行文件绝对路径。

### Edit Mode 测试

推荐先在 Unity 中打开 `Window > General > Test Runner > EditMode`，运行全部测试。命令行等价方式：

```powershell
$unity = '<Unity.exe 的绝对路径>'
& $unity -batchmode -nographics -projectPath (Get-Location).Path -runTests -testPlatform EditMode -testResults 'Temp/EditModeResults.xml' -logFile -
```

现有测试覆盖：

- 高德坐标转换及步行路线响应的成功、空响应、无效 JSON、业务错误和不完整数据；
- Web Mercator 瓦片/全局像素/地图偏移换算；
- 地图边界到路线视口的换算；
- 方位角和跨北向最短转角。

网络、权限、Vuforia、视频、音频、触摸和场景序列化不能由这些测试充分覆盖。

### Development APK

Unity 菜单：`Tools > ARGuiding > Build Map Preview APK`。

```powershell
$unity = '<Unity.exe 的绝对路径>'
& $unity -batchmode -quit -projectPath (Get-Location).Path -executeMethod AndroidBuildCommands.BuildMapPreview -logFile -
```

输出为 `Builds/Development/ARGuiding-map-preview.apk`。构建期间包名临时追加 `.preview`，且不使用自定义 keystore；`finally` 会恢复编辑器设置。`Builds` 和 APK 已被 Git 忽略。

### 发布校验与签名 APK

先运行 `Tools > ARGuiding > Validate Release Readiness`。非 Development Android 构建会自动执行同一校验，并阻止以下情况：场景配置错误、默认包名或原型版本号、未配置正式签名、缺少高德 Key、缺少 Vuforia 配置。

签名构建菜单：`Tools > ARGuiding > Build Signed Release APK`。可在 Player Settings 配置签名，也可仅在本地/CI 设置：

- `ARGUIDING_KEYSTORE_PATH`
- `ARGUIDING_KEYALIAS_NAME`
- `ARGUIDING_KEYSTORE_PASSWORD`
- `ARGUIDING_KEYALIAS_PASSWORD`

输出为 `Builds/Release/ARGuiding-<bundleVersion>.apk`。不得提交 keystore、JKS、密码、APK 或 AAB。

### 变更后的最小验证

- 纯解析/数学逻辑：运行全部 Edit Mode 测试。
- UI/Prefab/场景：等待重新导入和编译，检查 Console、`Missing Script`，进入 Play Mode 走相关流程。
- 地图/定位/导航/AR/媒体/权限：除 Edit Mode 测试外，必须生成 Development APK 并在 Android 真机验证相关项。
- 发布配置：运行发布校验；只有计划发布时才生成签名包。
- 目录或依赖调整：在全新克隆或干净副本中完成 LFS 拉取、Unity 导入、测试和 Android 构建恢复验证。

完整真机清单见 `README.md` 和 `Docs/ReleaseChecklist.md`。不要把“编辑器 Play Mode 正常”写成“真机功能已验证”。

## 安全、删除与版本控制

- 禁止批量或递归删除文件和目录，不得使用 `del /s`、`rd /s`、`rmdir /s`、`Remove-Item -Recurse`、`rm -rf`。
- 需要删除时，只能一次删除一个已经核对过的明确文件路径；Unity 资产还要单独处理对应 `.meta`。如果任务需要批量删除，停止操作并让用户手动处理。
- `Assets/Legacy` 的清单不构成删除授权。任何清理必须先满足 `Docs/LegacyInventory.md` 的复核条件，使用独立提交，并逐文件处理。
- 开始和结束时检查 `git status --short` 与 `git diff --check`；保留并避开与任务无关的已有改动。
- 不提交 `Library`、`Temp`、`Logs`、`UserSettings`、`Builds`、生成的 IDE 文件或本地凭据。
- Vuforia `.tgz` 由 Git LFS 管理；不要改写 `.gitattributes` 或把大包直接转为普通 Git blob。
- 不要在源码、Issue、日志、截图、测试数据或提交信息中写入完整的高德 Key、Vuforia License、签名密码或其他秘密。
- 历史版本中的密钥已经暴露；删除当前文件不能恢复其保密性，必须在服务控制台轮换并设置应用、签名、接口和额度限制。

## 提交修改要求

提交信息必须以实际 diff 和实际验证结果为依据。所有普通提交固定使用 Conventional Commits，标题采用 `type: 中文概括式标题` 或 `type(scope): 中文概括式标题`。历史提交未统一使用类型前缀，不影响此规则；新提交不得省略 `type`。

### 提交前检查

1. 使用 `git status --short` 确认本次修改范围，并识别用户原有或与当前任务无关的改动。
2. 使用 `git diff --stat` 了解整体规模，再阅读 `git diff -- <相关文件>`；存在暂存内容时，同时检查 `git diff --cached`。
3. 确认每个提交只表达一个内聚目的。代码、文档、资源或配置若彼此无关，应拆成独立提交，不要用一个宽泛标题掩盖多项任务。
4. Unity 资产改名、移动或新增时，确认对应 `.meta` 与资产一起纳入；不要误提交 `Library`、`Temp`、`Logs`、`Builds`、IDE 文件、本地密钥或构建产物。
5. 按本文件“变更后的最小验证”运行与风险相称的检查。提交信息只能记录实际执行并得到结果的验证。
6. 提交前再次运行 `git diff --check`，并复核即将提交的 staged diff。

### 标题格式

- 固定使用 `type: 中文概括式标题`；仅当 scope 能稳定、准确地帮助定位影响范围时，使用 `type(scope): 中文概括式标题`。
- `type` 和 `scope` 使用小写英文，冒号后保留一个空格；标题正文使用简体中文、单行、概括式表达。
- 可用类型：
  - `feat`：新增或扩展用户可见功能；
  - `fix`：修复错误行为、崩溃或异常状态；
  - `style`：只改变 UI 视觉、布局、间距、颜色等，不改变业务行为；
  - `refactor`：内部结构调整，预期不改变外部行为；
  - `perf`：有依据的性能优化；
  - `test`：新增或调整测试；
  - `docs`：文档、注释或使用说明；
  - `build`：构建配置、依赖、打包或工具链；
  - `ci`：CI/CD 工作流；
  - `chore`：不属于上述类别的日常维护；
  - `security`：凭据、权限、密钥或漏洞加固。
- scope 是可选项，优先使用稳定的模块名，如 `map`、`navigation`、`ar`、`ui`、`android`；不要为了形式完整而强行添加。
- 标题应简洁明确，通常不超过 72 个字符，不以句号结尾。
- 使用 `perf(map): 优化地图交互与瓦片加载`、`docs: 补充 Android 发布检查清单` 这类改动导向的表达；不要只写 `chore: 更新代码`、`fix: 修改问题` 等含糊标题。
- 不要把文件名当成标题主体，除非该文件本身就是交付物；优先描述行为、能力或结构变化。
- 不要在标题中声称未经验证的修复效果、兼容性、发布状态或性能收益。
- 确有破坏性变更时，在类型或 scope 后添加 `!`，并在正文或 footer 中用 `BREAKING CHANGE:` 说明迁移影响；不得为了强调普通改动滥用该标记。

### 正文格式

小型且内聚的修改可以只写标题。复杂修改在标题后空一行补充正文，说明未来维护者真正需要知道的内容：

- 修改背景、原问题或本次调整的动机；
- 关键行为或结构变化，以及必要的实现理由；
- 用户可见影响、兼容性、数据/迁移影响、风险与仍存在的限制；
- 实际执行的测试或检查及其结果。

正文优先按逻辑分段；多个独立要点可使用项目符号或编号。不要逐文件罗列改动，也不要重复标题。

只有确实执行过测试或检查时，才能写“已通过……验证”，并准确描述实际命令或检查项。没有运行测试时，提交信息中直接省略验证陈述，不写“未测试”；如果是在对话中提供待复制的提交文本，应在代码块外单独提醒未运行测试。

### 真实性与敏感信息

- 不得编造测试结果、Issue 编号、评审者、部署状态、兼容性结论、性能数据或迁移结果。
- 不在提交信息中包含本机绝对路径、用户目录、临时目录或工具缓存路径；仓库/API 契约本身要求的路径除外。
- 不写入高德 Key、Vuforia License、keystore/JKS 密码、完整服务响应或其他秘密。
- 提交正文只保留有助于理解“为什么改、改了什么、有什么影响”的实现信息，避免无助于维护的内部细节。

### Git 操作权限

生成提交文本不等于授权执行 Git 写操作。除非用户明确要求，不要运行 `git add`、`git commit`、`git commit --amend`、rebase、push 或创建 PR。用户明确要求提交时，也必须先展示或核对实际 diff，并基于上述规则生成提交信息。

推荐格式：

```text
type(scope): 中文概括式标题

说明修改背景和主要调整，重点解释原因与行为变化。

说明用户可见影响、兼容性、风险或限制；没有相关影响时可省略。

已通过 <实际执行的测试或检查> 验证。
```

## 常见任务落点

- 修改景点文案/坐标：从 `Info.cs` 开始，同时检查五景点硬编码和 Prefab 展示。
- 修改路线解析：优先改 `AmapResponseParser.cs` 并先补测试，再改 `GaoDeAPI.cs` 的编排。
- 修改到达距离或刷新频率：改 `NavigationDefaults.cs`，验证到达提示、停止导航和网络请求频率。
- 修改地图投影或交互：联合检查 `Location.cs`、`LocationMap.cs`、`MapPosMgr.cs`、路线相机和相关测试。
- 修改 AR 指向：检查 `NavigationMath.cs`、`GaoDeAPI.DrawRouteInWorld`、指南针真北数据和真机方向。
- 修改 UI：在对应 `Resources/UI/*.prefab` 与面板脚本中同步处理，保留控件名约定。
- 修改启动或对象生命周期：检查动态加载竞态、事件退订、`DontDestroyOnLoad` 对象和重复进入 2D/AR。
- 修改 Android 发布：检查 `ProjectSettings`、`AndroidBuildCommands.cs`、`ReleaseReadinessValidator.cs` 和发布清单。

## 进一步阅读

- `README.md`：环境恢复、项目概览和真机冒烟测试。
- `Docs/ProjectStructure.md`：目录职责、固定路径和后续结构演进边界。
- `Docs/ReleaseChecklist.md`：凭据、Android 配置、自动校验和发布前真机检查。
- `Docs/LegacyInventory.md`：历史内容的证据、保留原因和逐项清理条件。
- `Config/README.md`：本地高德配置方法。
