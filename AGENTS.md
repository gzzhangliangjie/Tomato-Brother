# AGENTS.md — Tomato-Brother 项目说明（面向 AI 编码助手）

本项目是一个用于学习 Unity 开发的 2D 练习项目，玩法模仿《土豆兄弟》（Brotato）：波次割草 + 回合间商店买装备。修改代码前请先读完本文档。

## 环境与工具链

- **Unity 版本**：6000.0.30f1（升级过版本，旧版本编辑器无法打开；编辑器安装路径 `D:\Unity\6000.0.30f1\`）
- **架构**：传统 GameObject + MonoBehaviour 面向对象，**没有使用 ECS/DOTS**，请勿引入 Entities 相关包
- **UI**：uGUI；相机：Cinemachine 2.x；数据：ScriptableObject 数据驱动
- **目标平台**：Windows x64（StandaloneWindows64）

## 目录结构

```
Assets/
  Scenes/        Start.unity（入口）→ Select.unity（选人）→ Fight.unity（战斗）
  Scripts/
    Pool/        对象池系统（本项目最核心的基础设施，见下文"对象池规则"）
    Player/      PlayerController、Weapon/（Melee|Ranged|Projectile）、Loot/
    Enemy/       BaseEnemy 基类 + Melee/、Ranged/、Bird、Slime 等
    Effect/      HurtText（伤害飘字）、Hit、Burst 特效
    System/      AudioManager（单例，AudioSource 池）、MapManager（刷怪/波次）、
                 SceneLoader、Menu、GameOver、Setting
    UI/          BaseUI 及各界面，Shop/（回合商店）
    ScriptObj/   数据资产定义：Character、Item、Weapon、Bag、GameStatus、PlayerStatus、ShopProduct
    Util/        LayerUtil（图层常量）
  Editor/        CommandLineBuild.cs（命令行打包）、AtlasCreator.cs（Sprite Atlas 批量生成）
  Atlases/       Sprite Atlas 图集（按 Sprites 子目录分 6 集，由 AtlasCreator 生成）
  Prefabs/ Resources/ Sprites/ Anim/ Audio/ UI/ Inventory/ Map/ Fonts/ Materials/
```

## 代码规范

- 注释用中文；标识符、命名空间用英文
- 命名空间与目录结构一致（如 `Pool`、`Player.Weapon.Projectile`、`UI.Shop`）
- ⚠️ 已知历史遗留：`Scripts/System/` 下的类用了 `namespace System`，与 .NET 的 `System` 冲突。**不要在新代码里模仿，也不要扩大使用范围**；在该命名空间内写代码时注意 `using System.xxx` 可能解析异常
- 序列化字段用 `[SerializeField] private`，避免 public 字段
- 组件引用（Rigidbody2D、SpriteRenderer 等）在 `Awake`/`Start` 缓存，**禁止在 Update/FixedUpdate 中调用 GetComponent / GameObject.Find / Camera.main**
- 风格示例参考 `Assets/Scripts/Pool/BulletPool.cs`、`Assets/Scripts/System/AudioManager.cs`

## 对象池规则（重要）

高频生成/销毁的实体**必须走对象池**，不允许在战斗循环里直接 Instantiate/Destroy：

- 基类 `BasePool<T>`（`Scripts/Pool/BasePool.cs`），封装 Unity 官方 `UnityEngine.Pool.ObjectPool<T>`，默认 100 个、上限 500
- 现有池：`BulletPool`（子弹）、`EnemyPool`（敌人）、`GoldPool`（金币）、`HitPool`（受击）、`BurstPool`（爆炸）、`TextPool`（伤害飘字），由 `PoolController` 统一挂载
- **池化对象只会创建一次，`Awake` 只执行一次**：对象的重置逻辑要写在 `OnEnable` 或池的 Get/Release 回调里，不能依赖 Awake/Start
- 一次性、低频物体（场景 UI、敌人生成动画等）可以直接 Instantiate/Destroy

## 构建与验证

命令行打包 Windows 版（需项目未被 Unity 编辑器占用，脚本编译约 40 秒）：

```bash
"D:/Unity/6000.0.30f1/Editor/Unity.exe" -batchmode -quit -nographics \
  -projectPath . -executeMethod AtlasCreator.CreateAtlasesAndBuild -logFile unity_build.log
```

- 上面的一体化命令会先刷新 Sprite Atlas（`Tools → Create Sprite Atlases` 的命令行版，幂等）再打包；只打包不建图集用 `-executeMethod CommandLineBuild.BuildWin64`
- 产物：`Builds/Windows/TomatoBrother.exe`；日志尾部出现 `BUILD OK` 即成功
- 本项目暂无自动化测试，改完逻辑只能靠编辑器里从 `Start` 场景跑一遍冒烟验证
- 已知无害告警：`DashBoy._isDashing` 未使用（CS0414）；`BaseWeapon.cs:98`、`LightingShiv.cs:44` 的 `OverlapCircleNonAlloc` 过时提示（替换方案见性能文档待办 P3）

## 双向提交（重要）

项目同时托管在 GitHub 和 Gitee，改完代码**必须用根目录脚本提交**，保证两边同步：

```bash
./commit.sh "提交信息"
```

- 脚本会 `git add -A` 全部改动 → commit → 依次推送 `origin/main`（GitHub fork）与 `gitee/main`（Gitee 镜像），任一远端失败会自动重试一次并在最后汇总提示
- 远程布局：`origin` = GitHub（gzzhangliangjie/Tomato-Brother），`upstream` = 原作者仓库（lancer1126/Tomato-Brother，只读），`gitee` = Gitee 镜像（gzzhangliangjie/Tomato-Brother）
- 三个远程全部走 SSH（`git@github.com:...` / `git@gitee.com:...`），使用 `~/.ssh/id_ed25519` 密钥认证，不会弹验证框；Gitee 可直连，**GitHub 依赖本机代理（127.0.0.1:4780，透明代理开启时 SSH 直连即通）**，若推送超时先确认代理软件已开启
- **禁止**只推单个远端或用裸 `git push`，否则两边会失去同步；若 `commit.sh` 推送失败，修复网络后重跑 `git push origin main && git push gitee main`

## Unity 资产注意事项

- 新增资产（脚本/贴图/预制体等）会同时生成同名 `.meta` 文件，**必须和资产一起提交**，否则他人打开工程会丢引用
- 不要提交 `Library/`、`Temp/`、`Logs/`、`Builds/`、`UserSettings/`（已在 .gitignore 中）
- 修改 `Packages/manifest.json` 或 `ProjectSettings/` 属于工程级变更，提交前提醒用户确认

## 性能约定

改动战斗热路径（子弹、刷怪、特效、飘字）前，先阅读 [Docs/性能优化.md](Docs/性能优化.md)。核心红线：不破坏现有对象池机制、不在热路径引入每帧 GC 分配（如 `new WaitForSeconds`、字符串拼接、闭包）。
