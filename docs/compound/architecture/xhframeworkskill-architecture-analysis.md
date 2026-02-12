# XHFrameworkSkill 项目架构分析

> 创建时间: 2026-02-12  
> 更新时间: 2026-02-12  
> 分类: architecture  
> 标签: `unity` `gas` `skill-system` `node-editor` `gameplay-ability-system` `buff-system`

---

## 项目概述

**XHFrameworkSkill** 是一个基于 **Unity GAS (Gameplay Ability System)** 模式的技能框架与可视化节点编辑器。

- **Unity 版本**: 2022.3.62f2c1
- **核心理念**: 一个技能 = 一个节点图，所有效果、子弹、Buff 都在同一图中配置
- **适用场景**: ARPG、MOBA 等需要复杂技能配置的游戏项目

---

## 程序集结构 (Assembly Definition)

| 程序集 | 路径 | 职责 |
|--------|------|------|
| `Skill.Data` | `Assets/SkillEditor/Data/` | 数据定义层（节点数据、标签、属性等） |
| `Skill.Runtime` | `Assets/SkillEditor/Runtime/` | 运行时逻辑（GAS 核心、效果执行等） |
| `SkillEditor.Editor` | `Assets/SkillEditor/Editor/` | 编辑器工具（节点图编辑器） |

---

## 核心架构

### GAS 核心系统

**路径**: `Assets/SkillEditor/Runtime/Core/`

| 组件 | 职责 |
|------|------|
| `AbilitySystemComponent` | GAS 核心组件，管理技能、效果、属性、标签 |
| `GASHost` | 全局单例驱动器，统一 Tick 更新所有 ASC |
| `SkillDataCenter` | 技能数据中心（单例），管理技能图表数据、节点缓存、连接缓存 |
| `SpecExecutor` | 技能执行器 |
| `SpecExecutionContext` | 技能执行上下文 |
| `SpecFactory` | Spec 工厂，创建各类运行时实例 |

### 五大节点类型

| 类型 | 说明 | 示例 |
|------|------|------|
| **Ability (技能)** | 技能入口节点，配置 ID、标签、CD、消耗 | 横扫、火球术 |
| **Effect (效果)** | 属性/状态修改，分瞬时/持续/永久 | 伤害、治疗、Buff、投射物、放置物、位移 |
| **Task (任务)** | 执行特定行为 | 搜索目标、动画、结束技能 |
| **Condition (条件)** | 条件判断分支 | 属性比较 |
| **Cue (表现)** | 美术表现 | 粒子特效、音效、飘字 |

---

## 标签系统

**路径**: `Assets/SkillEditor/Data/Tags/`

| 文件 | 职责 |
|------|------|
| `GameplayTag.cs` | 单个标签定义 |
| `GameplayTagSet.cs` | 标签集合 |
| `GameplayTagContainer.cs` | 标签容器（运行时） |
| `GameplayTagLibrary.cs` | 标签库 |
| `GameplayTagsAsset.cs` | 标签资产（树形结构编辑器） |

**设计优势**: 用标签系统替代大量事件，更灵活、更易维护。

---

## 效果系统

**路径**: `Assets/SkillEditor/Runtime/Effect/`

| 效果类型 | 文件 | 说明 |
|----------|------|------|
| Buff | `BuffEffectSpec.cs` | Buff 效果 |
| 冷却 | `CooldownEffectSpec.cs` | CD 冷却 |
| 消耗 | `CostEffectSpec.cs` | 资源消耗 |
| 伤害 | `DamageEffectSpec.cs` | 伤害计算 |
| 治疗 | `HealEffectSpec.cs` | 治疗恢复 |
| 位移 | `DisplaceEffectSpec.cs` | 位移效果（击退/吸引） |
| 投射物 | `ProjectileEffectSpec.cs` | 投射物（火球等） |
| 放置物 | `PlacementEffectSpec.cs` | 放置物（陷阱等） |

**容器管理**:
- `GameplayEffectContainer.cs` - 效果容器
- `GameplayEffectSpec.cs` - 效果基类

---

## 编辑器系统

**路径**: `Assets/SkillEditor/Editor/`

```
Editor/
├── Base/
│   ├── GraphView/           # 节点图视图
│   │   ├── SkillGraphView.cs
│   │   ├── SkillGraphView.CopyPaste.cs
│   │   ├── SkillGraphView.NodeCreation.cs
│   │   └── SkillGraphView.Serialization.cs
│   ├── Inspector/           # 属性面板
│   ├── Node/                # 节点基类
│   │   ├── SkillNodeBase.cs
│   │   ├── NodeFactory.cs
│   │   ├── EffectNode.cs
│   │   ├── TaskNode.cs
│   │   ├── ConditionNode.cs
│   │   └── CueNode.cs
│   ├── SkillGraphData.cs    # 编辑器用技能图数据（ScriptableObject）
│   └── SkillEditorConstants.cs
├── SkillEditorWindow.cs     # 主编辑器窗口
├── SkillAssetTreeView.cs    # 技能资源树
└── [各节点类型编辑器]/       # Ability, Effect, Task, Condition, Cue, Tags, Attribute
```

**编辑器功能**:
- 节点图编辑器（基于 Unity GraphView）
- 节点属性面板（Inspector）
- Timeline 动画编辑器
- 快捷键支持（Ctrl+S 保存）
- 复制粘贴支持

---

## 数据层结构

### 编辑器数据 vs 运行时数据

| 类型 | 类名 | 路径 | 说明 |
|------|------|------|------|
| 编辑器 | `SkillGraphData` | `Editor/Base/` | ScriptableObject，用于编辑器序列化 |
| 运行时 | `SkillData` | `Data/Base/` | 纯数据类，用于运行时 |

两者结构相同，都包含 `SkillId`、`nodes`、`connections`。

### 数据目录结构

**路径**: `Assets/SkillEditor/Data/`

```
Data/
├── Base/          # 基础类：NodeData, SkillData, NodeType 枚举, SkillConstants
├── Ability/       # 技能节点数据 (AbilityNodeData)
├── Effect/        # 效果节点数据 (Damage, Heal, Buff, Projectile, Placement, Displace...)
├── Task/          # 任务节点数据 (Animation, SearchTarget, EndAbility)
├── Condition/     # 条件节点数据 (AttributeCompare)
├── Cue/           # 表现节点数据 (Particle, Sound, FloatingText)
├── Tags/          # 标签系统
├── Attribute/     # 属性系统
└── Skill.Data.asmdef
```

---

## Demo 示例

**路径**: `Assets/SkillEditor/Runtime/Demo/`（已移入 Runtime）

| 模块 | 内容 |
|------|------|
| `Battle/` | Unit、Player、Monster、UnitManager、AnimationComponent |
| `Luban/` | 配表系统（TbUnit、TbSkill） |
| `UI/` | UI 相关 |
| `Unitls/` | 工具类 |

---

## 第三方依赖

| 依赖 | 用途 |
|------|------|
| **Spine** | 骨骼动画 |
| **TextMesh Pro** | 文本渲染 |
| **Luban** | 配表工具 |

---

## 数值系统

支持四种数值配置方式：

| 类型 | 枚举值 | 说明 |
|------|--------|------|
| 具体值 | `FixedValue` | 固定数值 |
| 公式 | `Formula` | 支持技能等级等变量 |
| MMC | `ModifierMagnitudeCalculation` | 自定义计算逻辑 |
| 上下文 | `SetByCaller` | 从配置表获取 |

---

## 技术亮点

1. **一体化设计** - 一个技能对应一个节点图，避免多资产跳转
2. **标签驱动** - 用标签系统替代大量事件，更灵活
3. **高度可扩展** - 节点类型通过枚举管理，易于扩展
4. **可视化编辑** - 节点图 + Timeline 动画编辑器
5. **GAS 模式** - 借鉴虚幻引擎 GAS，成熟的技能系统架构

---

## 关键文件索引

| 文件 | 路径 |
|------|------|
| 技能编辑器入口 | `Assets/SkillEditor/Editor/SkillEditorWindow.cs` |
| ASC 核心组件 | `Assets/SkillEditor/Runtime/Core/AbilitySystemComponent.cs` |
| 全局驱动器 | `Assets/SkillEditor/Runtime/Core/GASHost.cs` |
| 技能数据中心 | `Assets/SkillEditor/Runtime/Core/SkillDataCenter.cs` |
| 节点类型定义 | `Assets/SkillEditor/Data/Base/NodeType.cs` |
| 运行时技能数据 | `Assets/SkillEditor/Data/Base/SkillData.cs` |
| 编辑器技能图数据 | `Assets/SkillEditor/Editor/Base/SkillGraphData.cs` |
| 节点基类 | `Assets/SkillEditor/Editor/Base/Node/SkillNodeBase.cs` |
| 节点工厂 | `Assets/SkillEditor/Editor/Base/Node/NodeFactory.cs` |

---

## 重构记录 (2026-02-12)

### 目录结构变化
1. `Assets/Demo/` → `Assets/SkillEditor/Runtime/Demo/`（Demo 移入 Runtime）
2. `SkillGraphData.cs` 移至 `Editor/Base/`（编辑器专用）
3. 新增 `SkillData.cs` 在 `Data/Base/`（运行时数据）

### 程序集重命名
- `SkillEditor.Data` → `Skill.Data`
- 新增 `Skill.Runtime`

### 数据分离
- 编辑器使用 `SkillGraphData`（ScriptableObject）
- 运行时使用 `SkillData`（纯数据类）
- `SkillDataCenter` 改为使用 `SkillData` 进行缓存管理

---

## 参考资料

- [虚幻 GAS 中文文档](https://github.com/BillEliot/GASDocumentation_Chinese)
- QQ 交流群：621790749
