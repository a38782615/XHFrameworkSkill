# 投射物反弹效果功能

## 问题陈述

需要为投射物效果添加反弹功能，使投射物到达目标后可以反弹到下一个目标，实现类似"链式闪电"或"弹射飞镖"的效果。

## 背景与动机

- 当前投射物只能飞向单一目标，到达后销毁
- 游戏中常见的弹射技能需要投射物在多个目标间反弹
- 反弹功能可以增加技能的策略性和视觉效果

## 技术考量

### 现有架构分析

1. **ProjectileEffectNodeData** - 投射物节点数据
   - 已有穿透功能（isPiercing, maxPierceCount）
   - 需要新增反弹相关配置

2. **ProjectileController** - 投射物控制器
   - 负责飞行逻辑和碰撞检测
   - 需要添加反弹逻辑：到达目标后搜索下一个目标

3. **ProjectileEffectSpec** - 投射物效果Spec
   - 管理投射物生命周期
   - 需要添加反弹事件端口

### 实现方案

#### 方案：在现有 ProjectileEffect 中添加反弹功能

**优点**：
- 复用现有代码，改动较小
- 配置集中在一个节点

**新增配置**：
- `isBouncing` - 是否启用反弹
- `maxBounceCount` - 最大反弹次数
- `bounceSearchRadius` - 反弹搜索半径
- `bounceTargetTags` - 反弹目标标签（可复用碰撞标签）
- `canBounceToSameTarget` - 是否可以反弹到已命中的目标

**新增端口**：
- `反弹时` - 每次反弹触发

## 验收标准

- [ ] 投射物节点新增反弹配置区域
- [ ] 投射物到达目标后，如果启用反弹，搜索下一个目标并飞向它
- [ ] 反弹次数达到上限后销毁投射物
- [ ] 每次反弹触发"反弹时"端口
- [ ] 反弹目标搜索支持标签过滤
- [ ] 可配置是否允许反弹到已命中的目标

## 依赖与风险

### 依赖
- 现有投射物系统（ProjectileController, ProjectileEffectSpec）
- 目标搜索逻辑（可参考 SearchTargetTaskSpec）

### 风险
- 中等风险：需要修改投射物控制器的核心逻辑
- 需要处理边界情况：无可反弹目标时的行为

## 实现任务

- [x] 1. 在 ProjectileEffectNodeData 中添加反弹配置字段
- [x] 2. 在 ProjectileController 中实现反弹逻辑
- [x] 3. 在 ProjectileEffectSpec 中添加反弹事件处理
- [x] 4. 在 ProjectileEffectNode 中添加"反弹时"输出端口
- [x] 5. 在 ProjectileEffectNodeInspector 中添加反弹配置 UI
- [x] 6. 更新 ProjectileInitData 传递反弹参数
- [x] 7. 测试验证功能
