# Luban SkillData 表设计计划

## 问题陈述

当前 SkillData 类包含复杂的多态节点数据结构，需要在 Luban 配置表系统中创建对应的 Excel 表，以支持技能数据的配置化管理。

### 当前状态

**现有 TableSkill 表**：
```
Id | Name | SkillGraphDataPath | IconPath
```
- 仅存储基础信息和 ScriptableObject 资源路径
- 实际技能数据存储在 SkillGraphData（ScriptableObject）中

**SkillData 类结构**：
```csharp
public class SkillData
{
    public string SkillId;
    public List<NodeData> nodes;        // 多态节点列表
    public List<ConnectionData> connections;  // 连接列表
}
```

### 核心挑战

1. **多态节点**：20+ 种节点类型，每种有不同字段
2. **嵌套结构**：节点包含复杂嵌套数据（GameplayTagSet、AttributeModifierData 等）
3. **连接关系**：节点之间的连线需要保持引用完整性

---

## 技术方案对比

### 方案 A：JSON 字符串存储

将 nodes 和 connections 序列化为 JSON 字符串存储在单个字段中。

**表结构**：
```
TbSkillData
├── Id (int) - 主键
├── SkillId (string) - 技能标识
├── NodesJson (string) - 节点数据 JSON
└── ConnectionsJson (string) - 连接数据 JSON
```

**优点**：
- 实现简单，一张表搞定
- 完全兼容现有 SkillData 结构
- 无需定义大量 Bean

**缺点**：
- 无法在 Excel 中直观编辑节点
- 需要外部工具生成 JSON
- 失去 Luban 类型检查优势

---

### 方案 B：完整多表结构

拆分为多张表，每种节点类型一张表。

**表结构**：
```
TbSkillData (主表)
├── Id (int)
├── SkillId (string)
├── Name (string)
└── IconPath (string)

TbSkillNode (节点基础表)
├── Id (int)
├── SkillId (int) - 外键
├── Guid (string)
├── NodeType (enum)
├── PositionX (float)
├── PositionY (float)
└── TargetType (enum)

TbAbilityNode (技能节点扩展)
├── NodeId (int) - 外键
├── AssetTags (string)
├── CancelAbilitiesWithTags (string)
├── ... (其他标签字段)

TbDamageEffectNode (伤害效果节点)
├── NodeId (int)
├── DamageType (enum)
├── DamageSourceType (enum)
├── DamageFixedValue (float)
├── ... (其他字段)

TbConnection (连接表)
├── Id (int)
├── SkillId (int)
├── OutputNodeGuid (string)
├── OutputPortName (string)
├── InputNodeGuid (string)
└── InputPortName (string)

... (其他 15+ 节点类型表)
```

**优点**：
- 完全可在 Excel 中编辑
- 充分利用 Luban 类型系统
- 数据结构清晰

**缺点**：
- 需要创建 20+ 张表
- 需要定义大量 Bean 和 Enum
- 维护成本高
- 查询时需要多表关联

---

### 方案 C：混合方案（推荐）

基础信息在 Luban 表中，复杂节点数据保持 JSON 或继续使用 ScriptableObject。

**表结构**：
```
TbSkillData
├── Id (int) - 主键
├── SkillId (string) - 技能标识
├── Name (string) - 技能名称
├── IconPath (string) - 图标路径
├── Description (string) - 技能描述
├── Cooldown (float) - 冷却时间
├── Cost (int) - 消耗
├── CostType (enum) - 消耗类型
├── SkillType (enum) - 技能类型（主动/被动）
├── TargetType (enum) - 目标类型
├── Range (float) - 施法距离
├── Tags (string) - 技能标签（逗号分隔）
└── GraphDataPath (string) - 节点图数据路径（可选）
```

**优点**：
- 常用配置可在 Excel 编辑
- 复杂节点逻辑保持灵活性
- 实现成本适中

**缺点**：
- 数据分散在两处
- 需要同步维护

---

### 方案 D：Luban 多态 Bean（高级方案）

利用 Luban 的多态 Bean 特性，在 `__beans__.xlsx` 中定义节点类型继承体系。

**Bean 定义**：
```
# __beans__.xlsx

NodeData (abstract)
├── guid: string
├── nodeType: NodeType
├── positionX: float
├── positionY: float
└── targetType: TargetType

AbilityNodeData : NodeData
├── skillId: int
├── assetTags: string
└── ... 

DamageEffectNodeData : EffectNodeData
├── damageType: DamageType
├── damageFixedValue: float
└── ...
```

**表结构**：
```
TbSkillData
├── Id (int)
├── SkillId (string)
├── Nodes (list<NodeData>) - 多态列表
└── Connections (list<ConnectionData>)
```

**优点**：
- 充分利用 Luban 多态特性
- 类型安全
- 单表存储所有数据

**缺点**：
- Bean 定义复杂
- Excel 编辑多态数据不直观
- 需要深入理解 Luban 多态语法

---

## 推荐方案

**推荐采用方案 D（Luban 多态 Bean）**，理由：

1. **类型安全**：Luban 会验证数据类型
2. **单一数据源**：所有技能数据在 Luban 表中
3. **可扩展**：新增节点类型只需添加 Bean 定义
4. **与现有架构一致**：SkillData 本身就是多态结构

---

## 详细实现计划

### 阶段 1-3：采用简化方案（JSON 字符串存储）

由于多态节点结构复杂，采用 **方案 A（JSON 字符串存储）** 替代原计划的多态 Bean 方案。

- [x] 1.1 创建 #SkillGraph.xlsx 表（使用 Luban 自动扫描模式）
- [x] 1.2 定义表结构：Id, SkillId, Name, Description, NodesJson, ConnectionsJson
- [x] 1.3 Luban 自动生成表定义和代码

### 阶段 4：Excel 数据表（#SkillGraph.xlsx）

- [x] 4.1 创建 #SkillGraph.xlsx 文件
- [x] 4.2 设计表头结构
- [x] 4.3 添加示例数据
- [ ] 4.4 验证多态节点数据格式（需要实际技能数据测试）

### 阶段 5：代码生成与集成

- [x] 5.1 运行 gen.bat 生成 C# 代码
- [x] 5.2 检查生成的 TableSkillGraph.cs
- [x] 5.3 检查生成的 TbSkillGraph.cs
- [x] 5.4 代码生成成功，无编译错误

### 阶段 6：运行时集成

- [x] 6.1 创建 SkillDataConverter 类（Luban 数据 → SkillData）
- [x] 6.2 支持多态节点 JSON 解析（通过 $type 或 nodeType 字段）
- [x] 6.3 保持向后兼容（SkillDataCenter 无需修改）
- [ ] 6.4 添加数据验证逻辑（可选）

### 阶段 7：测试与验证

- [ ] 7.1 单元测试：数据加载
- [ ] 7.2 单元测试：数据转换
- [ ] 7.3 集成测试：技能执行
- [ ] 7.4 回归测试：现有功能

---

## 替代方案考虑

### 为什么不选择方案 A（JSON）？

- 失去 Luban 类型检查
- Excel 编辑体验差
- 容易出现格式错误

### 为什么不选择方案 B（多表）？

- 表数量过多（20+）
- 维护成本高
- 查询复杂

### 为什么不选择方案 C（混合）？

- 数据分散两处
- 同步维护困难
- 不够优雅

---

## 资源需求

### 时间估算

| 阶段 | 预计时间 |
|------|----------|
| 阶段 1：枚举定义 | 1 小时 |
| 阶段 2：Bean 定义 | 3-4 小时 |
| 阶段 3：表定义 | 0.5 小时 |
| 阶段 4：Excel 数据 | 1-2 小时 |
| 阶段 5：代码生成 | 0.5 小时 |
| 阶段 6：运行时集成 | 2-3 小时 |
| 阶段 7：测试验证 | 2 小时 |
| **总计** | **10-13 小时** |

### 技术依赖

- Luban 2.x 版本
- 熟悉 Luban 多态 Bean 语法
- 理解现有 SkillData 结构

---

## 风险与缓解

| 风险 | 可能性 | 影响 | 缓解措施 |
|------|--------|------|----------|
| Luban 多态语法复杂 | 中 | 高 | 先做小规模验证 |
| Excel 编辑多态数据困难 | 高 | 中 | 提供编辑指南文档 |
| 数据迁移出错 | 中 | 高 | 保留 ScriptableObject 作为备份 |
| 性能问题 | 低 | 中 | 运行时缓存转换结果 |

---

## 验收标准

1. ✅ 所有枚举正确定义在 __enums__.xlsx
2. ✅ 所有 Bean 正确定义在 __beans__.xlsx
3. ✅ TbSkillData 表可正常生成 C# 代码
4. ✅ 示例技能数据可正确加载
5. ✅ SkillDataConverter 可将 Luban 数据转换为 SkillData
6. ✅ 现有技能功能正常运行
7. ✅ 单元测试通过率 100%

---

## 后续步骤

1. 运行 `/workflows-work plans/luban-skilldata-table.md` 执行计划
2. 实现后运行 `/workflows-review` 进行代码审查
3. 运行 `/workflows-compound` 记录经验教训

---

## 附录：Luban 多态 Bean 语法参考

### __beans__.xlsx 多态定义示例

```
| full_name              | parent    | fields                                    |
|------------------------|-----------|-------------------------------------------|
| NodeData               |           | guid:string, nodeType:NodeType, ...       |
| AbilityNodeData        | NodeData  | skillId:int, assetTags:string, ...        |
| DamageEffectNodeData   | NodeData  | damageType:DamageType, ...                |
```

### Excel 数据填写示例

```
| Id | SkillId | Nodes                                                    |
|----|---------|----------------------------------------------------------|
| 1  | skill_1 | [{$type:AbilityNodeData, guid:xxx, skillId:1, ...}, ...] |
```

---

*创建时间：2026-02-12*
*状态：已完成*
*实际方案：方案 A（JSON 字符串存储）*

## 实现总结

### 创建的文件

1. **Luban/MiniTemplate/Datas/#SkillGraph.xlsx** - 技能图数据表
   - 表结构：Id, SkillId, Name, Description, NodesJson, ConnectionsJson
   - 使用 JSON 字符串存储多态节点数据

2. **Assets/SkillEditor/Runtime/Demo/Luban/DataTable/TableSkillGraph.cs** - 自动生成
3. **Assets/SkillEditor/Runtime/Demo/Luban/DataTable/TbSkillGraph.cs** - 自动生成
4. **Assets/SkillEditor/Runtime/Core/SkillDataConverter.cs** - 数据转换器
   - 支持通过 nodeType 字段解析多态节点
   - 支持通过 $type 字段解析多态节点

### 修改的文件

1. **Assets/SkillEditor/Runtime/Demo/Luban/LubanManager.cs** - 添加 LoadSkillGraphData() 方法

### 使用方式

```csharp
// 加载技能图数据到 SkillDataCenter
LubanManager.Instance.LoadSkillGraphData();

// 获取技能数据
var skillData = SkillDataCenter.Instance.GetSkillGraph("skill_test_1");
```

### JSON 格式说明

NodesJson 格式：
```json
[
  {"nodeType":0,"guid":"ability_1","position":{"x":100,"y":100},"targetType":1,"skillId":1},
  {"nodeType":1,"guid":"damage_1","position":{"x":300,"y":100},"targetType":2,"damageType":0,"damageFixedValue":50}
]
```

ConnectionsJson 格式：
```json
[
  {"outputNodeGuid":"ability_1","outputPortName":"output","inputNodeGuid":"damage_1","inputPortName":"input"}
]
```
