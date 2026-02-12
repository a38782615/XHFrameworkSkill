# XHFrameworkSkill 项目架构分析

> 创建时间: 2026-02-12  
> 分类: architecture  
> 标签: `unity` `gas` `skill-system` `node-editor` `gameplay-ability-system` `buff-system`

---

## 项目概述

**XHFrameworkSkill** 是一个基于 **Unity GAS (Gameplay Ability System)** 模式的技能框架与可视化节点编辑器。

- **Unity 版本**: 2022.3.62f2c1
- **核心理念**: 一个技能 = 一个节点图，所有效果、子弹、Buff 都在同一图中配置
- **适用场景**: ARPG、MOBA 等需要复杂技能配置的游戏项目

---

## 核心架构

### GAS 核心系统

**路径**: `Assets/SkillEditor/Runtime/Core/`

| 组件 | 职责 |
|------|------|
| `AbilitySystemComponent` | GAS 核心组件，管理技能、效果、属性、标签 |
| `GASHost` | 全局单例驱动器，统一 Tick 更新所有 ASC |
| `SkillDataCenter` | 技能数据中心 |
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

| 文件 | 职责 |
|------|------|
| `SkillEditorWindow.cs` | 主编辑器窗口（菜单：Tools → Skill Editor） |
| `SkillAssetTreeView.cs` | 左侧技能资源树 |

**编辑器功能**:
- 节点图编辑器（基于 Unity GraphView）
- 节点属性面板（Inspector）
- Timeline 动画编辑器
- 快捷键支持（Ctrl+S 保存）

---

## 数据层结构

**路径**: `Assets/SkillEditor/Data/`

```
Data/
├── Base/          # 基础类：NodeData, SkillGraphData, NodeType 枚举
├── Ability/       # 技能节点数据 (AbilityNodeData)
├── Effect/        # 效果节点数据 (Damage, Heal, Buff, Projectile...)
├── Task/          # 任务节点数据 (Animation, SearchTarget, EndAbility)
├── Condition/     # 条件节点数据 (AttributeCompare)
├── Cue/           # 表现节点数据 (Particle, Sound, FloatingText)
├── Tags/          # 标签系统
└── Attribute/     # 属性系统
```

---

## Demo 示例

**路径**: `Assets/Demo/`

| 模块 | 内容 |
|------|------|
| `Battle/` | Unit、Player、Monster、UnitManager、AnimationComponent |
| `Luban/` | 配表系统（TbUnit、TbSkill） |
| `UI/` | UI 相关 |

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
| 节点类型定义 | `Assets/SkillEditor/Data/Base/NodeType.cs` |
| 技能图数据 | `Assets/SkillEditor/Data/Base/SkillGraphData.cs` |
| 单位基类 | `Assets/Demo/Battle/Unit.cs` |

---

## 参考资料

- [虚幻 GAS 中文文档](https://github.com/BillEliot/GASDocumentation_Chinese)
- QQ 交流群：621790749
