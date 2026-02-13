# 技能编辑器一键导出按钮

## 问题陈述

在技能编辑器 (`SkillEditorWindow`) 的工具栏上添加一个"一键导出"按钮，实现：
1. 将当前技能导出到 Luban Excel (`#SkillGraph.xlsx`)
2. 运行 Luban 生成 C# 代码

## 背景和动机

- 当前导出流程需要手动操作多个步骤
- 策划/开发需要频繁在编辑器和 Excel 之间同步数据
- 一键操作可以大幅提升工作效率
- 减少人为操作失误

## 技术考量

### 现有代码结构

**SkillEditorWindow.cs**:
- 工具栏在 `CreateToolbar()` 方法中创建
- 已有按钮：保存、全览
- 使用 UIElements 构建 UI

**现有工具**:
- `SkillAssetToLubanExporter.cs` - 导出到 Excel
- `LubanToSkillAssetImporter.cs` - 从 Excel 导入

**Luban 生成脚本**:
- `Luban/MiniTemplate/gen.bat` - 生成 C# 代码

### 实现方案

在工具栏添加"导出到 Luban"按钮，点击后：
1. 调用 `SkillAssetToLubanExporter` 导出当前技能到 Excel
2. 执行 `gen.bat` 生成 Luban C# 代码
3. 刷新 AssetDatabase

## 实现任务

- [x] 在 `CreateToolbar()` 中添加"导出到 Luban"按钮
- [x] 实现导出当前技能到 Excel 的方法
- [x] 实现调用 gen.bat 的方法
- [x] 添加进度提示和结果反馈
- [x] 处理错误情况

## 成功指标

1. 点击按钮后，当前技能数据正确写入 `#SkillGraph.xlsx`
2. Luban 代码生成成功
3. 操作完成后有清晰的反馈提示
4. 错误情况有友好的错误信息

## 依赖和风险

### 依赖
- `SkillAssetToLubanExporter.cs` 的导出逻辑
- `Luban/MiniTemplate/gen.bat` 脚本
- MiniExcel 库

### 风险
- gen.bat 执行可能失败（路径问题、Luban 配置问题）
- Excel 文件可能被其他程序占用
- 大量数据导出可能耗时较长

## 文件修改

- `Assets/SkillEditor/Editor/SkillEditorWindow.cs` - 添加按钮和导出逻辑
