# 技能编辑器：点击技能显示 SO Inspector 信息

## 问题陈述

当前技能编辑器的右侧 Inspector 面板只在选中图形节点时显示节点属性。用户希望在点击左侧技能列表中的技能文件时，能够在右侧 Inspector 区域显示该技能 ScriptableObject（SkillGraphData）本身的属性信息（如 SkillId 等）。

## 背景与动机

- 当前流程：点击左侧技能 → 加载图形 → 必须点击节点才能看到属性
- 期望流程：点击左侧技能 → 立即在右侧显示技能 SO 的基本信息
- 这样用户可以快速查看和编辑技能的元数据（如 SkillId），无需额外操作

## 技术考量

### 现有架构分析

1. **SkillEditorWindow.cs**
   - `LoadGraphFromPath()` 方法处理技能文件选择
   - 调用 `inspectorView.SetGraphContext()` 设置上下文
   - 需要在此处触发显示 SO 信息

2. **NodeInspectorView.cs**
   - 当前只有 `UpdateSelection(SkillNodeBase node)` 方法
   - 需要新增方法来显示 SkillGraphData 的属性
   - 使用 Unity 的 `PropertyField` 或自定义 UI 绑定 SO 字段

3. **SkillGraphData.cs**
   - 包含 `SkillId` 字段和节点/连接数据
   - 作为 ScriptableObject，可以使用 SerializedObject 进行绑定

### 实现方案

在 `NodeInspectorView` 中新增 `ShowSkillAssetInfo(SkillGraphData data)` 方法：
- 当选择技能文件时调用，显示 SO 属性
- 当选择节点时，切换回节点属性显示
- 使用 `SerializedObject` + `PropertyField` 实现数据绑定和自动保存

## 验收标准

- [x] 点击左侧技能文件时，右侧 Inspector 显示 SkillGraphData 的属性（SkillId 等）
- [x] 修改 Inspector 中的属性值后，能正确保存到 SO
- [x] 点击图形中的节点时，Inspector 切换为显示节点属性
- [x] 取消选择节点后，Inspector 恢复显示技能 SO 属性
- [x] UI 样式与现有 Inspector 保持一致

## 依赖与风险

### 依赖
- Unity Editor UIElements API
- SerializedObject/SerializedProperty 数据绑定

### 风险
- 低风险：改动范围小，仅涉及 Inspector 显示逻辑
- 需要处理好节点选择和 SO 显示之间的切换逻辑

## 实现任务

- [x] 1. 在 `NodeInspectorView` 中添加 `ShowSkillAssetInfo(SkillGraphData data)` 方法
- [x] 2. 修改 `SkillEditorWindow.LoadGraphFromPath()` 在加载技能后调用显示 SO 信息
- [x] 3. 修改 `SkillGraphView` 添加取消选择节点的回调
- [x] 4. 在取消选择节点时恢复显示技能 SO 信息
- [x] 5. 测试验证功能正常
