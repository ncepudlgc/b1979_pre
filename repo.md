# Bombing Run

## Repo简介

Bombing Run 是一个 3D 街机风格的飞机射击和轰炸游戏，使用 Unity 引擎开发。玩家控制一架向前飞行的飞机，与敌方飞机（bogies）和地面目标（turrets 和 bunkers）进行空中战斗。游戏具有直观的控制系统，包括瞄准、射击、轰炸、加速和制动等功能。

### 主要功能

1. **菜单系统**
   - 主菜单场景：包含开始游戏按钮，可以从菜单场景启动游戏
   - 暂停菜单：在游戏过程中可以暂停游戏，显示包含 Resume、Restart、Quit 按钮的面板
   - 完整的场景导航和工作流

2. **游戏玩法**
   - 动态敌人 AI，具有多种攻击模式
   - 地面和空中战斗场景
   - 可收集的健康和弹药道具
   - 可破坏的地面目标
   - 预测性炸弹瞄准系统
   - 具有跟踪行为的敌方炮塔
   - 健康和弹药管理
   - 速度加速和制动系统，共享资源池
   - 特殊磁力 Bogies，可以吸收子弹并将其作为强力弹丸发射回来

### 技术栈

- **引擎**: Unity (Universal Render Pipeline)
- **编程语言**: C#
- **UI 系统**: TextMesh Pro
- **输入系统**: Unity 新输入系统 (InputSystem_Actions)
- **平台**: 移动设备和 PC

### 项目结构

```
Assets/
├── Scenes/
│   ├── Menu.unity          # 主菜单场景 (Build Index 1)
│   └── GameScene.unity     # 游戏场景 (Build Index 0)
├── MainMenu.cs             # 主菜单控制器
├── GameMenu.cs             # 暂停菜单控制器
├── GameManager.cs           # 游戏状态管理、敌人生成、计分、暂停/恢复
├── PlaneController.cs       # 玩家飞机控制
├── Turret.cs                # 地面炮塔控制
├── Bogie.cs                 # 敌方飞机控制
├── MagnetBogie.cs           # 磁力敌方飞机
├── Bunker.cs                # 地面掩体
├── Bullet.cs                # 子弹行为
├── Bomb.cs                  # 炸弹物理和爆炸效果
├── HealthPickup.cs          # 健康道具
└── AmmoPickup.cs            # 弹药道具
```

### 菜单架构

- **Menu Scene (Build Index 1)**:
  - MenuManager: 空游戏对象，附加 `MainMenu.cs`
  - StartButton: UI 按钮，分配给 `MainMenu.cs` 中的 `startGameButton`

- **Game Scene (Build Index 0)**:
  - MenuButton: UI 按钮，点击时暂停游戏并显示暂停菜单
  - GameMenu: 暂停菜单脚本，附加到 GameManager 游戏对象
  - Menu Panel: 包含 Resume、Restart、Quit 按钮的 UI 面板

### 设计特点

1. **暂停系统**
   - 使用 `Time.timeScale` 暂停/恢复游戏时间
   - 暂停时隐藏游戏逻辑更新
   - 菜单面板显示/隐藏控制

2. **场景管理**
   - 使用 Unity SceneManager 进行场景切换
   - 主菜单到游戏场景的导航
   - 游戏场景到主菜单的返回

3. **按钮交互**
   - 所有按钮通过 Unity Button 组件连接
   - 使用 onClick 事件处理程序
   - 完整的错误处理和日志记录

## 题目Prompt

I added a Menu Scene with a start button that should let me start the game from the menu scene. Theres a new menu button in the game view and a panel with resume, restart, and quit buttons. The panel is just sitting in the game view now so we need logic to hide it but pause the game and make it appear when I click the menu button. Then also wire up all the buttons in the pause menu correctly. See the documentation for more and update the readme to reflect the updated codebase.

## PR链接

https://github.com/ncepudlgc/b1979_pre/pull/1
