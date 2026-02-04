# b1979_pre

## Repo简介

Bombing Run 是一个 3D 街机风格的飞机射击和轰炸游戏，玩家控制一架向前飞行的飞机，与敌方飞机（bogies）和地面目标（炮塔和掩体）进行空中战斗。

**主要功能：**
- 3D 飞机控制和导航
- 空中和地面战斗场景
- 动态敌人 AI，具有多种攻击模式
- 可收集的健康和弹药道具
- 可破坏的地面目标
- 预测性炸弹瞄准系统
- 速度提升和制动系统，共享资源池
- 特殊的磁力 Bogies，可以吸收子弹并作为强力弹丸发射回来
- 完整的菜单系统（主菜单和暂停菜单）

**技术栈：**
- Unity Engine
- C#
- Unity Input System
- TextMesh Pro
- Unity UI System

**项目结构：**
- `Assets/Scenes/` - 游戏场景
  - `Menu.unity` - 主菜单场景（Build Index 1）
  - `GameScene.unity` - 游戏场景（Build Index 0）
- `Assets/MainMenu.cs` - 主菜单功能，处理场景导航
- `Assets/GameMenu.cs` - 暂停菜单管理（Resume, Restart, Quit 按钮）
- `Assets/GameManager.cs` - 游戏状态管理、敌人生成、计分、游戏进度、暂停/恢复功能
- `Assets/PlaneController.cs` - 玩家飞机移动、射击、轰炸、加速/制动机制、相机控制
- `Assets/Turret.cs` - 地面炮塔控制
- `Assets/Bogie.cs` - 敌方飞机控制，具有动态飞行模式
- `Assets/Bunker.cs` - 可破坏的地面掩体
- `Assets/Bullet.cs` - 弹丸行为和伤害
- `Assets/Bomb.cs` - 炸弹物理和爆炸效果
- `Assets/HealthPickup.cs & AmmoPickup.cs` - 可收集道具管理

**核心组件：**
- GameManager - 游戏状态、敌人生成、计分系统、暂停/恢复
- PlaneController - 玩家控制、射击、轰炸、速度管理
- MainMenu - 主菜单场景导航
- GameMenu - 暂停菜单功能

**敌人类型：**
- Turrets - 地面敌人，跟踪并向玩家开火
- Bogies - 标准敌方飞机，遵循各种飞行模式
- Bunkers - 静止的地面目标，可被摧毁获得分数
- Magnet Bogies - 特殊敌人，可以吸引和吸收玩家子弹，然后作为组合弹丸发射回来

## 题目Prompt

I added a Menu Scene with a start button that should let me start the game from the menu scene. Theres a new menu button in the game view and a panel with resume, restart, and quit buttons. The panel is just sitting in the game view now so we need logic to hide it but pause the game and make it appear when I click the menu button. Then also wire up all the buttons in the pause menu correctly. See the documentation for more and update the readme to reflect the updated codebase.

## PR链接

https://github.com/ncepudlgc/b1979_pre/pull/3
