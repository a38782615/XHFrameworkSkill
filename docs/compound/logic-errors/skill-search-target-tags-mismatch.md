# 技能搜索目标标签配置错误导致无法造成伤害

## 问题症状

英雄使用技能攻击敌人时，敌人不掉血，伤害飘字也不显示。技能动画正常播放，但没有任何伤害效果。

## 根因分析

技能系统使用 `SearchTargetTaskNodeData` 节点搜索攻击目标，该节点有一个 `searchTargetTags` 配置项，用于过滤符合条件的目标。

**问题代码** (`SearchTargetTaskSpec.cs`):
```csharp
private bool IsValidTarget(AbilitySystemComponent target)
{
    // ...
    if (!nodeData.searchTargetTags.IsEmpty && !target.HasAnyTags(nodeData.searchTargetTags)) 
        return false;  // 标签不匹配，返回 false
    // ...
}
```

**配置错误**：
- 技能配置的搜索标签：`unitType.boss`
- 敌人实际拥有的标签：`unitType.monster`

**敌人标签初始化** (`Monster.cs`):
```csharp
void Start()
{
    ownerASC.OwnedTags.AddTag(new GameplayTag("unitType.monster"));
    // ...
}
```

由于标签不匹配，`IsValidTarget()` 返回 false，导致：
1. 搜索不到任何目标
2. 伤害节点的 `targetType: ParentInput` 获取不到目标
3. 伤害效果不执行

## 解决方案

将技能 asset 文件中的 `searchTargetTags` 从 `unitType.boss` 改为 `unitType.monster`：

```yaml
# 修改前
searchTargetTags:
  _tags:
  - _name: unitType.boss
    _hashCode: -2067207703
    _shortName: boss

# 修改后
searchTargetTags:
  _tags:
  - _name: unitType.monster
    _hashCode: -2021655864
    _shortName: monster
```

## 涉及文件

修改的技能文件：
- `Assets/Unity/Resources/ScriptObject/SkillAsset/Sweep.asset`
- `Assets/Unity/Resources/ScriptObject/SkillAsset/God.asset`
- `Assets/Unity/Resources/ScriptObject/SkillAsset/Wan.asset`
- `Assets/Unity/Resources/ScriptObject/SkillAsset/RuFood.asset`

已正确配置的文件：
- `Assets/Unity/Resources/ScriptObject/SkillAsset/Wind.asset`
- `Assets/Unity/Resources/ScriptObject/SkillAsset/Blood.asset`

相关代码文件：
- `Assets/SkillEditor/Runtime/Task/SearchTargetTaskSpec.cs`
- `Assets/SkillEditor/Runtime/Demo/Battle/Monster.cs`

## 预防策略

1. **统一标签命名规范**：
   - 普通敌人使用 `unitType.monster`
   - Boss 敌人使用 `unitType.boss`
   - 在技能编辑器中提供标签选择下拉框，避免手动输入错误

2. **添加调试日志**：
   在 `SearchTargetTaskSpec.IsValidTarget()` 中添加调试日志，方便排查目标搜索问题：
   ```csharp
   if (!nodeData.searchTargetTags.IsEmpty && !target.HasAnyTags(nodeData.searchTargetTags))
   {
       Debug.Log($"[SearchTarget] 目标 {target.Id} 标签不匹配，需要: {nodeData.searchTargetTags}, 实际: {target.OwnedTags}");
       return false;
   }
   ```

3. **技能模板**：
   创建技能模板时，默认使用 `unitType.monster` 作为搜索目标标签

4. **单元测试**：
   添加测试用例验证技能能正确搜索到目标

## 标签

`技能系统` `GAS` `标签系统` `配置错误` `SearchTargetTask` `Unity`

## 参考

- 问题日期: 2026-02-13
- 相关计划: `plans/fix-skill-target-tags.md`
