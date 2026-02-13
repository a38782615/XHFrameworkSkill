using Luban;
using SkillEditor.Runtime;
using UnityEngine;

public class LubanManager
{
    private static LubanManager _instance;
    public static LubanManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new LubanManager();
                _instance.Load();
            }
            return _instance;
        }
    }

    public cfg.Tables Tables { get; private set; }

    /// <summary>
    /// 是否已加载技能图数据到 SkillDataCenter
    /// </summary>
    public bool SkillGraphLoaded { get; private set; }

    private void Load()
    {
        Tables = new cfg.Tables(name =>
        {
            var textAsset = Resources.Load<TextAsset>($"Luban/{name}");
            return new ByteBuf(textAsset.bytes);
        });
        LoadSkillGraphData();
    }

    /// <summary>
    /// 加载所有技能图数据到 SkillDataCenter
    /// </summary>
    public void LoadSkillGraphData()
    {
        if (SkillGraphLoaded)
            return;

        if (Tables?.TbSkillGraph != null)
        {
            SkillDataConverter.ConvertAndRegisterAll(Tables.TbSkillGraph);
            SkillGraphLoaded = true;
            Debug.Log($"[LubanManager] Loaded {Tables.TbSkillGraph.DataList.Count} skill graphs to SkillDataCenter");
        }
    }
}
