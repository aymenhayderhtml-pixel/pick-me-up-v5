using System.IO;
using UnityEditor;
using UnityEngine;

public static class CreateFacilityDefinitions
{
    private const string OutputFolder = "Assets/Resources/Facilities";

    [MenuItem("Tools/Pick Me Up/Create Facility Definitions")]
    public static void Execute()
    {
        EnsureFolder();

        CreateOrReplace("workshop", "Workshop", FacilityType.Workshop, "CRAFTING", "The room where scrap becomes purpose.", new Color(0.75f, 0.55f, 0.2f));
        CreateOrReplace("square", "Square", FacilityType.Square, "MORALE", "A gathering place where the Master’s voice can steady the roster.", new Color(0.55f, 0.45f, 0.25f));
        CreateOrReplace("dorms", "Dorms", FacilityType.Dorms, "RECOVERY", "Quiet rooms where the wounded recover their will.", new Color(0.35f, 0.5f, 0.65f));
        CreateOrReplace("crucible", "Crucible", FacilityType.Crucible, "SACRIFICE", "A dark chamber where choice becomes irreversible power.", new Color(0.65f, 0.2f, 0.25f));
        CreateOrReplace("training_hall", "Training Hall", FacilityType.TrainingHall, "GROWTH", "A hall of drills, trials, and hard-earned preparation.", new Color(0.25f, 0.6f, 0.75f));
        CreateOrReplace("flying_dock", "Flying Dock", FacilityType.FlyingDock, "EXPEDITION", "The launch point for sortie planning and deployment.", new Color(0.35f, 0.55f, 0.75f));
        CreateOrReplace("memorial_hall", "Memorial Hall", FacilityType.MemorialHall, "LEGACY", "The dead remain present here as Echoes.", new Color(0.45f, 0.35f, 0.55f));
        CreateOrReplace("tactical_station", "Tactical Station", FacilityType.TacticalStation, "COMMAND", "The Master reaches into battle from this room.", new Color(0.8f, 0.3f, 0.3f));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Pick Me Up: Facility definitions created or updated in Assets/Resources/Facilities.");
    }

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        if (!AssetDatabase.IsValidFolder(OutputFolder))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Facilities");
        }
    }

    private static void CreateOrReplace(string id, string displayName, FacilityType type, string role, string emotion, Color color)
    {
        string path = $"{OutputFolder}/{id}.asset";
        FacilityDefinition def = AssetDatabase.LoadAssetAtPath<FacilityDefinition>(path);
        if (def == null)
        {
            def = ScriptableObject.CreateInstance<FacilityDefinition>();
            AssetDatabase.CreateAsset(def, path);
        }

        def.FacilityId = id;
        def.DisplayName = displayName;
        def.Description = emotion;
        def.FacilityType = type;
        def.FacilityRole = role;
        def.FacilityEmotion = emotion;
        def.MaxLevel = 10;
        def.StartingLevel = 1;
        def.GoldCostPerLevel = BuildGoldCurve(type);
        def.GemCostPerLevel = BuildGemCurve(type);
        def.BenefitPerLevel = BuildBenefitCurve(type);
        def.UpgradeTimeBase = 60f;
        def.UpgradeTimeMultiplier = 1.25f;
        def.FacilityColor = color;
        EditorUtility.SetDirty(def);
    }

    private static int[] BuildGoldCurve(FacilityType type)
    {
        int baseCost = type == FacilityType.Crucible || type == FacilityType.MemorialHall ? 600 : 250;
        int[] values = new int[10];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = baseCost + (i * baseCost / 2);
        }

        return values;
    }

    private static int[] BuildGemCurve(FacilityType type)
    {
        int[] values = new int[10];
        int premiumStart = (type == FacilityType.FlyingDock || type == FacilityType.TacticalStation) ? 1 : 0;
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = i < 3 ? premiumStart : 0;
        }

        return values;
    }

    private static float[] BuildBenefitCurve(FacilityType type)
    {
        float[] values = new float[10];
        float start = type switch
        {
            FacilityType.Workshop => 10f,
            FacilityType.Square => 5f,
            FacilityType.Dorms => 8f,
            FacilityType.Crucible => 6f,
            FacilityType.TrainingHall => 12f,
            FacilityType.FlyingDock => 7f,
            FacilityType.MemorialHall => 4f,
            FacilityType.TacticalStation => 3f,
            _ => 5f
        };

        for (int i = 0; i < values.Length; i++)
        {
            values[i] = start + (i * start * 0.5f);
        }

        return values;
    }
}
