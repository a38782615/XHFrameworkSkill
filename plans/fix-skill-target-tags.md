# 修复技能搜索目标标签配置

## 问题陈述

英雄技能攻击敌人不掉血。原因是大部分技能的 `SearchTargetTaskNodeData` 配置了错误的搜索目标标签 `unitType.boss`，但敌人（Monster）实际拥有的标签是 `unitType.monster`。

## 背景和动机

- `Monster.cs` 中敌人初始化时添加的标签是 `unitType.monster`
- 技能的搜索目标节点配置了 `unitType.boss` 标签
- 标签不匹配导致 `IsValidTarget()` 返回 false，搜索不到任何目标
- 伤害节点因为没有目标而不执行

## 技术分析

**搜索目标验证逻辑** (`SearchTargetTaskSpec.cs`):
```csharp
private bool IsValidTarget(AbilitySystemComponent target)
{
    // ...
    if (!nodeData.searchTargetTags.IsEmpty && !target.HasAnyTags(nodeData.searchTargetTags)) 
        return false;  // 标签不匹配，返回 false
    // ...
}
```

**受影响的技能文件**:
| 技能 | 当前配置 | 应改为 |
|------|---------|--------|
| Sweep.asset | unitType.boss | unitType.monster |
| God.asset | unitType.boss | unitType.monster |
| Wan.asset | unitType.boss | unitType.monster |
| RuFood.asset | unitType.boss | unitType.monster |
| FireCircle.asset | 需检查 | unitType.monster |
| ThreeFire.asset | 需检查 | unitType.monster |

**已正确配置的技能**:
- Wind.asset - unitType.monster ✓
- Blood.asset - unitType.monster ✓

## 实现任务

- [x] 修改 Sweep.asset 的 searchTargetTags 为 unitType.monster
- [x] 修改 God.asset 的 searchTargetTags 为 unitType.monster
- [x] 修改 Wan.asset 的 searchTargetTags 为 unitType.monster
- [x] 修改 RuFood.asset 的 searchTargetTags 为 unitType.monster
- [x] 检查并修改其他技能文件（Blood、Wind 已正确配置）
- [x] 验证修复效果

## 成功指标

1. 所有技能的 searchTargetTags 配置为 unitType.monster
2. 英雄技能攻击敌人时，敌人正确掉血
3. 伤害飘字正常显示

## 依赖和风险

### 依赖
- 敌人必须正确添加 unitType.monster 标签（已确认）

### 风险
- 如果有 Boss 类型敌人需要单独处理，可能需要额外配置
- 修改 asset 文件需要在 Unity 中刷新
