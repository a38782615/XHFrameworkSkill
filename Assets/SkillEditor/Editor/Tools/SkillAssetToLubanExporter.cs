using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniExcelLibs;
using SkillEditor.Data;
using UnityEditor;
using UnityEngine;

namespace SkillEditor.Editor.Tools
{
    /// <summary>
    /// SkillAsset 导出到 Luban Excel 工具
    /// 读取现有表头，清除数据区，写入新数据
    /// </summary>
    public class SkillAssetToLubanExporter : EditorWindow
    {
        private const string SKILL_ASSET_PATH = "Assets/Unity/Resources/ScriptObject/SkillAsset";
        private const string EXCEL_PATH = "Luban/MiniTemplate/Datas/#SkillGraph.xlsx";
        private const int HEADER_ROWS = 4; // 表头占4行
        
        private Vector2 _scrollPosition;
        private List<SkillGraphData> _skillAssets = new List<SkillGraphData>();
        private List<bool> _selectedAssets = new List<bool>();
        private bool _selectAll = true;
        
        [MenuItem("SkillEditor/导出技能到 Luban Excel")]
        public static void ShowWindow()
        {
            var window = GetWindow<SkillAssetToLubanExporter>("技能导出工具");
            window.minSize = new Vector2(400, 500);
            window.LoadSkillAssets();
        }

        [MenuItem("SkillEditor/快速导出所有技能到 Excel")]
        public static void QuickExportAll()
        {
            var guids = AssetDatabase.FindAssets("t:SkillGraphData", new[] { SKILL_ASSET_PATH });
            var skills = new List<SkillGraphData>();
            
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<SkillGraphData>(path);
                if (asset != null)
                    skills.Add(asset);
            }
            
            if (skills.Count > 0)
            {
                ExportToExcel(skills);
                EditorUtility.DisplayDialog("导出成功", $"已导出 {skills.Count} 个技能到 Excel", "确定");
            }
            else
            {
                Debug.LogWarning("[SkillExporter] 未找到任何技能资源");
            }
        }

        private void LoadSkillAssets()
        {
            _skillAssets.Clear();
            _selectedAssets.Clear();
            
            var guids = AssetDatabase.FindAssets("t:SkillGraphData", new[] { SKILL_ASSET_PATH });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<SkillGraphData>(path);
                if (asset != null)
                {
                    _skillAssets.Add(asset);
                    _selectedAssets.Add(true);
                }
            }
            
            Debug.Log($"[SkillExporter] 找到 {_skillAssets.Count} 个技能资源");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("技能资源导出工具", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("将 SkillAsset 导出到 Luban Excel 表 (#SkillGraph.xlsx)\n从第5行开始写入数据，保持表头不变", MessageType.Info);
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("刷新技能列表", GUILayout.Height(25)))
                LoadSkillAssets();
            
            EditorGUILayout.Space(10);
            
            EditorGUI.BeginChangeCheck();
            _selectAll = EditorGUILayout.Toggle("全选", _selectAll);
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < _selectedAssets.Count; i++)
                    _selectedAssets[i] = _selectAll;
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"技能列表 ({_skillAssets.Count} 个)", EditorStyles.boldLabel);
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(300));
            for (int i = 0; i < _skillAssets.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _selectedAssets[i] = EditorGUILayout.Toggle(_selectedAssets[i], GUILayout.Width(20));
                EditorGUILayout.ObjectField(_skillAssets[i], typeof(SkillGraphData), false);
                EditorGUILayout.LabelField($"ID: {_skillAssets[i].SkillId}", GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space(10);
            
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("导出选中的技能到 Excel", GUILayout.Height(40)))
                ExportSelectedSkills();
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.Space(5);
            
            var outputPath = Path.Combine(Application.dataPath, "..", EXCEL_PATH);
            EditorGUILayout.LabelField("输出路径:", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(outputPath, EditorStyles.textField, GUILayout.Height(20));
            
            if (GUILayout.Button("打开 Excel 文件"))
            {
                if (File.Exists(outputPath))
                    System.Diagnostics.Process.Start(outputPath);
            }
        }

        private void ExportSelectedSkills()
        {
            var selectedSkills = new List<SkillGraphData>();
            for (int i = 0; i < _skillAssets.Count; i++)
            {
                if (_selectedAssets[i])
                    selectedSkills.Add(_skillAssets[i]);
            }
            
            if (selectedSkills.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "请至少选择一个技能", "确定");
                return;
            }
            
            ExportToExcel(selectedSkills);
            EditorUtility.DisplayDialog("导出成功", $"已导出 {selectedSkills.Count} 个技能到 Excel", "确定");
        }

        /// <summary>
        /// 导出技能到 Excel
        /// 策略：读取现有表头 + 生成新数据行 → 整体写入
        /// </summary>
        private static void ExportToExcel(List<SkillGraphData> skills)
        {
            try
            {
                var excelPath = Path.Combine(Application.dataPath, "..", EXCEL_PATH);
                
                // 1. 读取现有表头（前4行）
                var headerRows = new List<object[]>();
                using (var stream = File.OpenRead(excelPath))
                {
                    var allRows = MiniExcel.Query(stream, useHeaderRow: false).ToList();
                    for (int i = 0; i < HEADER_ROWS && i < allRows.Count; i++)
                    {
                        var row = allRows[i] as IDictionary<string, object>;
                        if (row != null)
                        {
                            // 转换为 object[] (A-H 共8列)
                            var rowData = new object[8];
                            for (int col = 0; col < 8; col++)
                            {
                                var key = GetColumnName(col);
                                rowData[col] = row.ContainsKey(key) ? row[key] : null;
                            }
                            headerRows.Add(rowData);
                        }
                    }
                }
                
                // 2. 生成数据行
                var dataRows = new List<object[]>();
                int skillId = 1;
                
                foreach (var skill in skills)
                {
                    int nodeCount = skill.nodes?.Count ?? 0;
                    int connCount = skill.connections?.Count ?? 0;
                    int maxRows = Math.Max(Math.Max(nodeCount, connCount), 1);
                    
                    for (int i = 0; i < maxRows; i++)
                    {
                        var row = new object[8];
                        
                        // A列: 空
                        row[0] = null;
                        
                        // 第一行写基础信息
                        if (i == 0)
                        {
                            row[1] = skillId;                    // B: Id
                            row[2] = skill.SkillId ?? "";        // C: SkillId
                            row[3] = skill.name ?? "";           // D: Name
                            row[4] = "";                         // E: Description
                        }
                        
                        // F: nodeType, G: content
                        if (i < nodeCount && skill.nodes[i] != null)
                        {
                            var node = skill.nodes[i];
                            row[5] = (int)node.nodeType;
                            row[6] = JsonUtility.ToJson(node);
                        }
                        
                        // H: ConnectionsJson
                        if (i < connCount && skill.connections[i] != null)
                        {
                            row[7] = JsonUtility.ToJson(skill.connections[i]);
                        }
                        
                        dataRows.Add(row);
                    }
                    
                    skillId++;
                }
                
                // 3. 合并表头和数据，使用 DataTable 写入
                var allData = new List<object[]>();
                allData.AddRange(headerRows);
                allData.AddRange(dataRows);
                
                // 4. 转换为 IEnumerable<Dictionary> 格式写入
                var dictRows = allData.Select(row => new Dictionary<string, object>
                {
                    { "A", row[0] },
                    { "B", row[1] },
                    { "C", row[2] },
                    { "D", row[3] },
                    { "E", row[4] },
                    { "F", row[5] },
                    { "G", row[6] },
                    { "H", row[7] }
                }).ToList();
                
                // 5. 写入 Excel（覆盖整个文件）
                MiniExcel.SaveAs(excelPath, dictRows, printHeader: false, sheetName: "Sheet1", overwriteFile: true);
                
                Debug.Log($"[SkillExporter] 导出成功！共 {skills.Count} 个技能，{dataRows.Count} 行数据");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SkillExporter] 导出失败: {e}");
                throw;
            }
        }
        
        private static string GetColumnName(int index)
        {
            return ((char)('A' + index)).ToString();
        }
    }
}
