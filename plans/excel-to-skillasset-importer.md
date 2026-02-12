# Excel 导入 SkillAsset 工具

## 问题陈述

需要创建一个 Unity 编辑器工具，将 `#SkillGraph.xlsx` 中的技能数据还原为 `SkillGraphData` ScriptableObject 资源文件。这是 `SkillAssetToLubanExporter` 的逆向操作，实现 Excel ↔ SkillAsset 的双向转换。

## 背景和动机

- 策划可以在 Excel 中批量编辑技能数据
- 需要将 Excel 修改同步回 Unity 的 SkillAsset
- 实现数据的双向流动：Unity → Excel → Unity
- 支持技能数据的版本控制和批量导入

## 技术分析

### 数据结构

**Excel 表结构 (#SkillGraph.xlsx)**:
| 列 | 字段 | 类型 | 说明 |
|----|------|------|------|
| B | Id | int | 主键ID（每个技能第一行） |
| C | SkillId | string | 技能标识（如 Skill_Wan） |
| D | Name | string | 技能名称（用作文件名） |
| E | Description | string | 技能描述 |
| F | nodeType | int | 节点类型枚举值 |
| G | content | string | 节点 JSON 数据 |
| H | ConnectionsJson | string | 连接 JSON 数据 |

**SkillGraphData 结构**:
```csharp
public class SkillGraphData : ScriptableObject
{
    public string SkillId;
    [SerializeReference]
    public List<NodeData> nodes = new List<NodeData>();
    public List<ConnectionData> connections = new List<ConnectionData>();
}
```

### 节点类型映射 (NodeType → Type)

```csharp
NodeType.Ability (0) → AbilityNodeData
NodeType.DamageEffect (1) → DamageEffectNodeData
NodeType.HealEffect (2) → HealEffectNodeData
NodeType.CostEffect (3) → CostEffectNodeData
NodeType.ModifyAttributeEffect (4) → ModifyAttributeEffectNodeData
NodeType.ProjectileEffect (5) → ProjectileEffectNodeData
NodeType.PlacementEffect (6) → PlacementEffectNodeData
NodeType.CooldownEffect (7) → CooldownEffectNodeData
NodeType.BuffEffect (8) → BuffEffectNodeData
NodeType.GenericEffect (9) → GenericEffectNodeData
NodeType.DisplaceEffect (10) → DisplaceEffectNodeData
NodeType.ParticleCue (11) → ParticleCueNodeData
NodeType.SoundCue (12) → SoundCueNodeData
NodeType.FloatingTextCue (14) → FloatingTextCueNodeData
NodeType.SearchTargetTask (15) → SearchTargetTaskNodeData
NodeType.EndAbilityTask (16) → EndAbilityTaskNodeData
NodeType.Animation (20) → AnimationNodeData
NodeType.AttributeCompareCondition (21) → AttributeCompareConditionNodeData
```

### 数据解析流程

1. 读取 Excel 数据（从第5行开始）
2. 按 Id 分组，每个 Id 对应一个技能
3. 解析每行的 nodeType + content → NodeData
4. 解析每行的 ConnectionsJson → ConnectionData
5. 创建或更新 SkillGraphData ScriptableObject

## 实现阶段

### 阶段 1：核心解析逻辑
- [x] 创建 `LubanToSkillAssetImporter.cs` 文件
- [x] 实现 Excel 数据读取（使用 MiniExcel）
- [x] 实现技能数据分组逻辑
- [x] 实现 NodeData 多态反序列化

### 阶段 2：ScriptableObject 创建/更新
- [x] 实现 SkillGraphData 创建逻辑
- [x] 实现已存在资源的更新逻辑
- [x] 处理 [SerializeReference] 的正确序列化

### 阶段 3：编辑器界面
- [x] 创建 EditorWindow 界面
- [x] 显示待导入的技能列表
- [x] 支持选择性导入
- [x] 显示导入进度和结果

### 阶段 4：错误处理和验证
- [x] JSON 解析错误处理
- [x] 节点类型验证
- [x] 导入结果报告

## 考虑过的替代方案

### 方案 A：直接使用 MiniExcel 读取
- 优点：与导出工具一致，代码复用
- 缺点：需要处理 MiniExcel 的数据格式

### 方案 B：使用 OpenXML 直接读取
- 优点：更精确控制
- 缺点：代码复杂度高

### 方案 C：通过 Luban 生成的代码读取
- 优点：类型安全
- 缺点：需要先运行 Luban 生成

**选择方案 A**：与导出工具保持一致，使用 MiniExcel。

## 验收标准

1. 能够读取 `#SkillGraph.xlsx` 中的所有技能数据
2. 正确解析所有节点类型（18+ 种）
3. 正确解析连接数据
4. 创建/更新 SkillGraphData ScriptableObject
5. 保持与原有 SkillAsset 的数据一致性
6. 提供清晰的导入结果反馈

## 依赖和风险

### 依赖
- MiniExcel 库
- SkillEditor.Data 命名空间
- NodeData 及其子类定义

### 风险
- JSON 格式变化可能导致解析失败
- 节点类型新增需要同步更新映射表
- [SerializeReference] 序列化可能有兼容性问题

## 风险缓解策略

1. 使用 try-catch 包装解析逻辑，单个节点失败不影响整体
2. 提供详细的错误日志
3. 导入前备份现有资源（可选）
4. 支持预览模式，先显示将要导入的内容

## 文件结构

```
Assets/SkillEditor/Editor/Tools/
├── SkillAssetToLubanExporter.cs  (已有 - 导出)
├── LubanToSkillAssetImporter.cs  (新建 - 导入)
└── SkillAssetRenamer.cs          (已有 - 重命名)
```

## 时间估计

- 阶段 1：核心解析逻辑 - 30 分钟
- 阶段 2：ScriptableObject 创建 - 20 分钟
- 阶段 3：编辑器界面 - 20 分钟
- 阶段 4：错误处理 - 10 分钟

**总计：约 80 分钟**
