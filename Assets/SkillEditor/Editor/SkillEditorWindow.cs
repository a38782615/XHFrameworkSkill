using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using SkillEditor.Data;
using SkillEditor.Editor.Tools;
using Debug = UnityEngine.Debug;
namespace SkillEditor.Editor
{
    public class SkillEditorWindow : EditorWindow
    {
        private SkillGraphView graphView;
        private NodeInspectorView inspectorView;
        private SkillAssetTreeView treeView;
        private IMGUIContainer treeViewContainer;
        private SkillGraphData currentGraphData;
        private string currentFilePath;
        private Button saveButton;
        private Button overviewButton;
        private Button exportButton;
        private VisualElement centerPanel;

        [MenuItem("Tools/Skill Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<SkillEditorWindow>();
            window.titleContent = new GUIContent("技能编辑器");
            window.minSize = new Vector2(1000, 600);
        }

        private void OnEnable()
        {
            ConstructUI();
            rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

            // 打开编辑器时自动执行全览
            EditorApplication.delayCall += FrameAllNodes;
        }

        private void OnDisable()
        {
            rootVisualElement.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            rootVisualElement.Clear();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.ctrlKey && evt.keyCode == KeyCode.S)
            {
                SaveCurrentGraph();
                evt.StopPropagation();
            }
        }

        private void ConstructUI()
        {
            rootVisualElement.Clear();

            var toolbar = CreateToolbar();
            rootVisualElement.Add(toolbar);

            var mainContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1
                }
            };

            var leftPanel = CreateLeftPanel();
            var centerPanel = CreateCenterPanel();
            var rightPanel = CreateRightPanel();

            mainContainer.Add(leftPanel);
            mainContainer.Add(centerPanel);
            mainContainer.Add(rightPanel);

            rootVisualElement.Add(mainContainer);
        }

        private VisualElement CreateToolbar()
        {
            var toolbar = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    height = 30,
                    backgroundColor = new Color(0.2f, 0.2f, 0.2f),
                    paddingLeft = 5,
                    paddingRight = 5,
                    paddingTop = 3,
                    paddingBottom = 3,
                    borderBottomWidth = 1,
                    borderBottomColor = new Color(0.1f, 0.1f, 0.1f),
                    alignItems = Align.Center
                }
            };

            saveButton = new Button(SaveCurrentGraph)
            {
                text = "保存 (Ctrl+S)",
                style =
                {
                    width = 120,
                    height = 24,
                    borderTopLeftRadius = 5,
                    borderTopRightRadius = 5,
                    borderBottomLeftRadius = 5,
                    borderBottomRightRadius = 5
                }
            };
            saveButton.SetEnabled(false);

            toolbar.Add(saveButton);

            // 添加全览按钮
            overviewButton = new Button(FrameAllNodes)
            {
                text = "全览",
                style =
                {
                    width = 80,
                    height = 24,
                    marginLeft = 10,
                    borderTopLeftRadius = 5,
                    borderTopRightRadius = 5,
                    borderBottomLeftRadius = 5,
                    borderBottomRightRadius = 5
                }
            };
            overviewButton.SetEnabled(false);

            // 保存按钮启用时，全览按钮也启用
            saveButton.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                overviewButton.SetEnabled(evt.newValue);
            });

            toolbar.Add(overviewButton);

            // 添加导出到 Luban 按钮
            exportButton = new Button(ExportToLuban)
            {
                text = "导出到 Luban",
                style =
                {
                    width = 100,
                    height = 24,
                    marginLeft = 10,
                    borderTopLeftRadius = 5,
                    borderTopRightRadius = 5,
                    borderBottomLeftRadius = 5,
                    borderBottomRightRadius = 5,
                    backgroundColor = new Color(0.2f, 0.5f, 0.2f)
                }
            };
            exportButton.SetEnabled(false);
            toolbar.Add(exportButton);

            return toolbar;
        }

        private void FrameAllNodes()
        {
            if (graphView == null || graphView.nodes.Count() == 0)
            {
                Debug.LogWarning("没有节点可以全览");
                return;
            }

            // 计算所有节点的边界
            var nodesList = graphView.nodes.ToList();
            if (nodesList.Count == 0) return;

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (var node in nodesList)
            {
                var pos = node.GetPosition();
                minX = Mathf.Min(minX, pos.x);
                minY = Mathf.Min(minY, pos.y);
                maxX = Mathf.Max(maxX, pos.x + pos.width);
                maxY = Mathf.Max(maxY, pos.y + pos.height);
            }

            // 计算中心点和所需的缩放
            float centerX = (minX + maxX) / 2f;
            float centerY = (minY + maxY) / 2f;
            float width = maxX - minX;
            float height = maxY - minY;

            // 添加边距
            const float padding = 100f;
            width += padding * 2;
            height += padding * 2;

            // 获取GraphView的可见区域大小
            var graphViewSize = graphView.layout.size;
            if (graphViewSize.x <= 0 || graphViewSize.y <= 0) return;

            // 计算缩放比例，确保所有节点都可见
            float scaleX = graphViewSize.x / width;
            float scaleY = graphViewSize.y / height;
            float scale = Mathf.Min(scaleX, scaleY, 1f); // 不超过100%缩放

            // 应用缩放
            graphView.contentViewContainer.style.scale = new Scale(new Vector2(scale, scale));

            // 计算偏移量使中心点居中
            float offsetX = graphViewSize.x / 2f - centerX * scale;
            float offsetY = graphViewSize.y / 2f - centerY * scale;

            graphView.contentViewContainer.style.translate = new Translate(offsetX, offsetY);
        }

        private void SaveCurrentGraph()
        {
            if (currentGraphData == null || string.IsNullOrEmpty(currentFilePath) || graphView == null)
            {
                Debug.LogWarning("没有打开的技能文件可以保存");
                return;
            }

            graphView.SaveGraph(currentGraphData);
            EditorUtility.SetDirty(currentGraphData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"已保存: {currentFilePath}");
        }

        private VisualElement CreateLeftPanel()
        {
            var panel = new VisualElement
            {
                style =
                {
                    width = 250,
                    backgroundColor = new Color(56f/255f, 56f/255f, 56f/255f),
                    borderRightWidth = 1,
                    borderRightColor = new Color(0.1f, 0.1f, 0.1f),
                    borderTopLeftRadius = 8,
                    borderTopRightRadius = 8,
                    borderBottomLeftRadius = 8,
                    borderBottomRightRadius = 8,
                    marginLeft = 5,
                    marginTop = 5,
                    marginBottom = 5,
                    flexDirection = FlexDirection.Column
                }
            };

            var header = new Label("技能资源")
            {
                style =
                {
                    fontSize = 14,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    paddingTop = 5,
                    paddingBottom = 5,
                    paddingLeft = 10,
                    backgroundColor = new Color(0.25f, 0.25f, 0.25f),
                    borderTopLeftRadius = 8,
                    borderTopRightRadius = 8
                }
            };
            panel.Add(header);

            treeView = new SkillAssetTreeView(new TreeViewState());
            treeView.OnFileSelected += LoadGraphFromPath;

            // 创建滚动容器
            var scrollContainer = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    flexGrow = 1,
                    backgroundColor = new Color(56f/255f, 56f/255f, 56f/255f)
                }
            };

            treeViewContainer = new IMGUIContainer(() =>
            {
                if (treeView != null)
                {
                    var rect = treeViewContainer.contentRect;

                    // Set background color for TreeView
                    var bgColor = new Color(56f/255f, 56f/255f, 56f/255f);
                    EditorGUI.DrawRect(new Rect(0, 0, rect.width, rect.height), bgColor);

                    treeView.OnGUI(new Rect(0, 0, rect.width, rect.height));
                }
            })
            {
                style = {
                    minHeight = 400,
                    backgroundColor = new Color(56f/255f, 56f/255f, 56f/255f)
                }
            };

            scrollContainer.Add(treeViewContainer);
            panel.Add(scrollContainer);
            return panel;
        }

        private VisualElement CreateCenterPanel()
        {
            centerPanel = new VisualElement
            {
                style = {
                    flexGrow = 1,
                    marginTop = 5,
                    marginBottom = 5,
                    marginLeft = 5,
                    marginRight = 5
                }
            };

            graphView = new SkillGraphView
            {
                name = "Skill Graph",
                style = {
                    flexGrow = 1,
                    borderTopLeftRadius = 8,
                    borderTopRightRadius = 8,
                    borderBottomLeftRadius = 8,
                    borderBottomRightRadius = 8
                }
            };

            graphView.OnNodeSelected += OnNodeSelected;
            graphView.SetNodeSelectionCallback();

            centerPanel.Add(graphView);
            return centerPanel;
        }

        private VisualElement CreateRightPanel()
        {
            inspectorView = new NodeInspectorView();
            inspectorView.style.borderTopLeftRadius = 8;
            inspectorView.style.borderTopRightRadius = 8;
            inspectorView.style.borderBottomLeftRadius = 8;
            inspectorView.style.borderBottomRightRadius = 8;
            inspectorView.style.marginRight = 5;
            inspectorView.style.marginTop = 5;
            inspectorView.style.marginBottom = 5;
            return inspectorView;
        }

        private void OnNodeSelected(SkillNodeBase node)
        {
            if (node != null)
            {
                // 选择了节点，显示节点属性
                inspectorView.UpdateSelection(node);
            }
            else
            {
                // 取消选择节点（点击空白区域），恢复显示技能 SO 属性
                inspectorView.ShowSkillAssetInfo();
            }
        }

        private void LoadGraphFromPath(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path) || !path.EndsWith(".asset"))
            {
                graphView.ClearGraph();
                currentFilePath = null;
                currentGraphData = null;
                inspectorView.SetGraphContext(null, null, null);
                UpdateUIState(false);
                return;
            }

            currentFilePath = path;
            currentGraphData = AssetDatabase.LoadAssetAtPath<SkillGraphData>(path);

            if (currentGraphData != null)
            {
                graphView.ClearGraph();
                graphView.LoadGraph(currentGraphData);
                inspectorView.SetGraphContext(graphView, currentGraphData, currentFilePath);
                UpdateUIState(true);

                // 加载完成后自动执行全览
                EditorApplication.delayCall += FrameAllNodes;
            }
            else
            {
                UpdateUIState(false);
            }
        }

        private void UpdateUIState(bool isFileSelected)
        {
            saveButton.SetEnabled(isFileSelected);
            overviewButton.SetEnabled(isFileSelected);
            exportButton.SetEnabled(isFileSelected);
            graphView.SetEnabled(isFileSelected);
        }

        /// <summary>
        /// 导出当前技能到 Luban Excel 并生成代码
        /// </summary>
        private void ExportToLuban()
        {
            if (currentGraphData == null)
            {
                EditorUtility.DisplayDialog("导出失败", "请先选择一个技能", "确定");
                return;
            }

            // 先保存当前技能
            SaveCurrentGraph();

            try
            {
                // 1. 导出所有技能到 Excel
                EditorUtility.DisplayProgressBar("导出到 Luban", "正在导出技能到 Excel...", 0.3f);
                
                var guids = AssetDatabase.FindAssets("t:SkillGraphData", new[] { "Assets/Unity/Resources/ScriptObject/SkillAsset" });
                var skills = new List<SkillGraphData>();
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<SkillGraphData>(path);
                    if (asset != null)
                        skills.Add(asset);
                }

                if (skills.Count == 0)
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("导出失败", "未找到任何技能资源", "确定");
                    return;
                }

                SkillAssetToLubanExporter.ExportToExcel(skills);
                Debug.Log($"[SkillEditor] 已导出 {skills.Count} 个技能到 Excel");

                // 2. 运行 Luban 生成代码
                EditorUtility.DisplayProgressBar("导出到 Luban", "正在生成 Luban 代码...", 0.6f);
                
                var genBatPath = Path.Combine(Application.dataPath, "..", "Luban", "MiniTemplate", "gen.bat");
                if (!File.Exists(genBatPath))
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("导出失败", $"找不到 gen.bat: {genBatPath}", "确定");
                    return;
                }

                var workingDir = Path.GetDirectoryName(genBatPath);
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c gen.bat",
                        WorkingDirectory = workingDir,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                EditorUtility.DisplayProgressBar("导出到 Luban", "正在刷新资源...", 0.9f);
                AssetDatabase.Refresh();

                EditorUtility.ClearProgressBar();

                if (process.ExitCode == 0)
                {
                    EditorUtility.DisplayDialog("导出成功", 
                        $"已导出 {skills.Count} 个技能到 Luban Excel\nLuban 代码生成成功", "确定");
                    Debug.Log($"[SkillEditor] Luban 生成成功:\n{output}");
                }
                else
                {
                    EditorUtility.DisplayDialog("导出警告", 
                        $"技能已导出到 Excel，但 Luban 生成可能有问题\n\n错误: {error}", "确定");
                    Debug.LogWarning($"[SkillEditor] Luban 生成警告:\n{error}\n{output}");
                }
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("导出失败", $"导出过程中发生错误:\n{e.Message}", "确定");
                Debug.LogError($"[SkillEditor] 导出失败: {e}");
            }
        }
    }
}
