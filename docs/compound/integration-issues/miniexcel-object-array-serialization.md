# MiniExcel 使用 object[] 导致序列化数组属性问题

## 问题症状

使用 MiniExcel 导出数据到 Excel 时，Excel 文件内容变成：

| A | B | C | D | E | F | G |
|---|---|---|---|---|---|---|
| LongLength | IsFixedSize | IsReadOnly | IsSynchronized | SyncRoot | Length | Rank |
| 8 | true | false | false | System.Object[] | 8 | 1 |

而不是预期的数据内容。原有表头被完全覆盖。

## 根因分析

MiniExcel 的 `SaveAs` 方法在处理数据源时，会通过反射获取对象的属性作为列名。

当使用 `List<object[]>` 作为数据源时：
- `object[]` 是 `System.Array` 类型
- Array 类型的属性包括：LongLength, IsFixedSize, IsReadOnly, IsSynchronized, SyncRoot, Length, Rank
- MiniExcel 将这些属性名作为列头，属性值作为单元格内容

## 解决方案

### 错误用法

```csharp
// ❌ 错误 - 会序列化数组的属性而不是内容
var rows = new List<object[]>();
rows.Add(new object[] { "##var", "Id", "Name" });
rows.Add(new object[] { "##type", "int", "string" });
MiniExcel.SaveAs(path, rows, printHeader: false);
```

### 正确用法

```csharp
// ✅ 正确 - 使用 Dictionary<string, object>
var rows = new List<Dictionary<string, object>>();
rows.Add(new Dictionary<string, object>
{
    { "A", "##var" },
    { "B", "Id" },
    { "C", "Name" }
});
rows.Add(new Dictionary<string, object>
{
    { "A", "##type" },
    { "B", "int" },
    { "C", "string" }
});
MiniExcel.SaveAs(path, rows, printHeader: false, sheetName: "Sheet1", overwriteFile: true);
```

### 保留表头的完整方案

```csharp
// 1. 读取现有表头
var headerRows = new List<object[]>();
using (var stream = File.OpenRead(excelPath))
{
    var allRows = MiniExcel.Query(stream, useHeaderRow: false).ToList();
    for (int i = 0; i < 4 && i < allRows.Count; i++) // 假设表头4行
    {
        var row = allRows[i] as IDictionary<string, object>;
        if (row != null)
        {
            var rowData = new object[8]; // 8列
            for (int col = 0; col < 8; col++)
            {
                var key = ((char)('A' + col)).ToString();
                rowData[col] = row.ContainsKey(key) ? row[key] : null;
            }
            headerRows.Add(rowData);
        }
    }
}

// 2. 生成数据行
var dataRows = new List<object[]>();
// ... 添加数据

// 3. 合并并转换为 Dictionary 格式
var allData = new List<object[]>();
allData.AddRange(headerRows);
allData.AddRange(dataRows);

var dictRows = allData.Select(row => new Dictionary<string, object>
{
    { "A", row[0] },
    { "B", row[1] },
    // ...
}).ToList();

// 4. 写入
MiniExcel.SaveAs(excelPath, dictRows, printHeader: false, sheetName: "Sheet1", overwriteFile: true);
```

## 涉及文件

- `Assets/SkillEditor/Editor/Tools/SkillAssetToLubanExporter.cs`
- `Luban/MiniTemplate/Datas/#SkillGraph.xlsx`

## 预防策略

1. **永远不要使用 `List<object[]>` 作为 MiniExcel 数据源**
2. 使用 `List<Dictionary<string, object>>` 或强类型类
3. 设置 `printHeader: false` 避免自动生成列头
4. 如需保留表头，先读取再合并后整体写入
5. 写入前备份原文件

## 标签

`MiniExcel` `Unity` `Excel` `序列化` `Luban` `C#`

## 参考

- MiniExcel GitHub: https://github.com/mini-software/MiniExcel
- 问题日期: 2026-02-12
