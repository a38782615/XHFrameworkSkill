# 技能激活阻止标签（activationBlockedTags）不生效

## 问题症状

- 技能在播放时，连续按键可以重复触发技能
- `activationBlockedTags` 配置了 CD 标签但没有阻止技能激活
- 调试日志显示 `Owner.HasAnyTags(Tags.ActivationBlockedTags)` 返回 `False`
- CD 效果确实执行了，`IsRunning: True`

## 根因分析

**标签名称不一致**：

| 配置位置 | 标签名 |
|---------|--------|
| Ability 节点的 `activationBlockedTags` | `CD.ThreeFire` |
| Cooldown 节点的 `grantedTags` | `CD.三火球`（中文） |

CD 效果执行时将 `grantedTags` 添加到了 `Owner.OwnedTags`，但由于标签名不匹配，`HasAnyTags` 检查失败。

## 解决方案

统一标签名称，确保 Ability 节点的 `activationBlockedTags` 和 Cooldown 节点的 `grantedTags` 使用相同的标签。

**修复步骤**：
1. 打开技能编辑器
2. 选择对应技能（如 ThreeFire）
3. 找到 Cooldown 节点
4. 将 `grantedTags` 修改为与 `activationBlockedTags` 一致的标签名（如 `CD.ThreeFire`）

## 涉及文件

- `Assets/Unity/Resources/ScriptObject/SkillAsset/*.asset` - 技能配置文件
- `Assets/SkillEditor/Runtime/Ability/GameplayAbilitySpec.cs` - `CanActivate()` 方法检查 `activationBlockedTags`
- `Assets/SkillEditor/Runtime/Effect/CooldownEffectSpec.cs` - `Execute()` 方法添加 `grantedTags`
- `Assets/SkillEditor/Runtime/Effect/GameplayEffectSpec.cs` - 基类将 `Tags.GrantedTags` 添加到 `Target.OwnedTags`

## 预防策略

1. **添加验证功能**：在技能编辑器中检查 `activationBlockedTags` 和关联的 Cooldown 节点的 `grantedTags` 是否一致
2. **自动同步**：当设置 `activationBlockedTags` 时，自动将相同标签设置到 Cooldown 节点的 `grantedTags`
3. **使用标签选择器**：避免手动输入标签名，减少拼写错误
4. **统一命名规范**：建议使用英文标签名保持一致性

## 调试技巧

在 `GameplayAbilitySpec.CanActivate()` 中添加日志：

```csharp
UnityEngine.Debug.Log($"[Ability] {SkillId} 检查ActivationBlockedTags: [{string.Join(", ", Tags.ActivationBlockedTags.Tags)}], Owner拥有的标签: [{string.Join(", ", Owner.OwnedTags.Tags)}], HasAnyTags结果: {Owner.HasAnyTags(Tags.ActivationBlockedTags)}");
```

在 `CooldownEffectSpec.Execute()` 中添加日志：

```csharp
UnityEngine.Debug.Log($"[CooldownEffect] 执行CD效果, SkillId: {SkillId}, GrantedTags: [{string.Join(", ", Tags.GrantedTags.Tags)}]");
```

## 标签

`skill-editor` `cooldown` `gameplay-tags` `activation-blocked-tags` `tag-mismatch`

## 日期

2026-02-13
