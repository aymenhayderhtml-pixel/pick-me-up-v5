#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CreateDefaultDungeons
{
    [MenuItem("Tools/Pick Me Up/Create Default Dungeons")]
    public static void Execute()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/Dungeons");
        EnsureFolder("Assets/Resources/Items");

        EnemyDataSO goblin = CreateEnemy("goblin_basic", "Goblin", 60, 12, 6);
        EnemyWaveSO basicWave = CreateWave("wave_basic", goblin, 3);

        ItemDefinition potion = CreateItem("item_minor_heal", "Minor Heal Potion", "heal_potion", ItemType.HealPotion, ItemRarity.Common, 20, 0, 0, 0, 0, 10, 0, 0, 0, 5);
        ItemDefinition iron = CreateMaterialItem("item_iron_ore", "Iron Ore", "iron_ore", ItemRarity.Common, 8, 0);
        ItemDefinition shard = CreateMaterialItem("item_attribute_shard", "Attribute Shard", "attribute_shard", ItemRarity.Uncommon, 0, 0);

        DungeonLootTableSO basicLoot = CreateLootTable("loot_basic",
            rolls: 3,
            bonusDropChance: 0.05f,
            entries: new[]
            {
                new DungeonLootTableSO.LootEntry { ItemId = potion.ItemId, FallbackName = potion.DisplayName, MinQuantity = 1, MaxQuantity = 2, Weight = 5, DropChance = 0.55f, Guaranteed = false },
                new DungeonLootTableSO.LootEntry { ItemId = iron.ItemId, FallbackName = iron.DisplayName, MinQuantity = 1, MaxQuantity = 3, Weight = 6, DropChance = 0.65f, Guaranteed = false },
                new DungeonLootTableSO.LootEntry { ItemId = shard.ItemId, FallbackName = shard.DisplayName, MinQuantity = 1, MaxQuantity = 1, Weight = 1, DropChance = 0.10f, Guaranteed = false }
            });

        CreateDungeon("isralta_mine", "Isralta Mine", "A blazing iron mine deep beneath Taoni.", 20, 100, 50, basicWave, basicLoot);
        CreateDungeon("kendert_forest", "Kendert Forest", "Ancient woodland riddled with goblin trails.", 18, 90, 45, basicWave, basicLoot);
        CreateDungeon("singmirel_plateau", "Singmirel Plateau", "A highland plain cut by cold wind and ruin.", 22, 85, 42, basicWave, basicLoot);
        CreateDungeon("main_tower", "Main Tower", "The 100-floor tower at the heart of the story.", 30, 150, 120, basicWave, basicLoot);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreateDefaultDungeons] Default dungeon assets created.");
    }

    private static EnemyDataSO CreateEnemy(string id, string displayName, int hp, int atk, int def)
    {
        string path = "Assets/Resources/Dungeons/" + id + ".asset";
        EnemyDataSO existing = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path);
        EnemyDataSO asset = existing != null ? existing : ScriptableObject.CreateInstance<EnemyDataSO>();

        SerializedObject so = new SerializedObject(asset);
        so.FindProperty("id").stringValue = id;
        so.FindProperty("displayName").stringValue = displayName;
        so.FindProperty("baseHp").intValue = hp;
        so.FindProperty("baseAttack").intValue = atk;
        so.FindProperty("baseDefense").intValue = def;
        so.ApplyModifiedProperties();

        if (existing == null)
        {
            AssetDatabase.CreateAsset(asset, path);
        }

        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static EnemyWaveSO CreateWave(string id, EnemyDataSO enemy, int quantity)
    {
        string path = "Assets/Resources/Dungeons/" + id + ".asset";
        EnemyWaveSO existing = AssetDatabase.LoadAssetAtPath<EnemyWaveSO>(path);
        EnemyWaveSO asset = existing != null ? existing : ScriptableObject.CreateInstance<EnemyWaveSO>();

        if (existing == null)
        {
            AssetDatabase.CreateAsset(asset, path);
        }

        SerializedObject so = new SerializedObject(asset);
        SerializedProperty spawns = so.FindProperty("spawns");
        spawns.ClearArray();
        spawns.InsertArrayElementAtIndex(0);
        SerializedProperty entry = spawns.GetArrayElementAtIndex(0);
        entry.FindPropertyRelative("enemyData").objectReferenceValue = enemy;
        entry.FindPropertyRelative("quantity").intValue = quantity;
        entry.FindPropertyRelative("spawnDelay").floatValue = 0f;
        entry.FindPropertyRelative("weight").intValue = 1;
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static void CreateDungeon(string id, string displayName, string description, int staminaCost, int gold, int exp, EnemyWaveSO wave, DungeonLootTableSO lootTable)
    {
        string path = "Assets/Resources/Dungeons/" + id + ".asset";
        DungeonDataSO existing = AssetDatabase.LoadAssetAtPath<DungeonDataSO>(path);
        DungeonDataSO asset = existing != null ? existing : ScriptableObject.CreateInstance<DungeonDataSO>();

        if (existing == null)
        {
            AssetDatabase.CreateAsset(asset, path);
        }

        SerializedObject so = new SerializedObject(asset);
        so.FindProperty("id").stringValue = id;
        so.FindProperty("displayName").stringValue = displayName;
        so.FindProperty("description").stringValue = description;
        so.FindProperty("staminaCost").intValue = staminaCost;
        so.FindProperty("minPlayerLevel").intValue = 1;
        so.FindProperty("baseGoldReward").intValue = gold;
        so.FindProperty("baseExpReward").intValue = exp;
        so.FindProperty("typedLootTable").objectReferenceValue = lootTable;
        SerializedProperty waves = so.FindProperty("waves");
        waves.ClearArray();
        waves.InsertArrayElementAtIndex(0);
        waves.GetArrayElementAtIndex(0).objectReferenceValue = wave;
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(asset);
    }

    private static ItemDefinition CreateItem(string id, string displayName, string description, ItemType type, ItemRarity rarity, int sellValue, int hp, int atk, int def, int spd, float critRate, float critDmg, int heal, int morale, int _maxStack)
    {
        string path = "Assets/Resources/Items/" + id + ".asset";
        ItemDefinition existing = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
        ItemDefinition asset = existing != null ? existing : ScriptableObject.CreateInstance<ItemDefinition>();

        if (existing == null)
        {
            AssetDatabase.CreateAsset(asset, path);
        }

        SerializedObject so = new SerializedObject(asset);
        so.FindProperty("ItemId").stringValue = id;
        so.FindProperty("DisplayName").stringValue = displayName;
        so.FindProperty("Description").stringValue = description;
        so.FindProperty("ItemType").enumValueIndex = (int)type;
        so.FindProperty("Rarity").enumValueIndex = (int)rarity;
        so.FindProperty("SellValue").intValue = sellValue;
        so.FindProperty("BaseHealth").intValue = hp;
        so.FindProperty("BaseAttack").intValue = atk;
        so.FindProperty("BaseDefense").intValue = def;
        so.FindProperty("BaseSpeed").intValue = spd;
        so.FindProperty("BaseCritRate").floatValue = critRate;
        so.FindProperty("BaseCritDamage").floatValue = critDmg;
        so.FindProperty("HealAmount").intValue = heal;
        so.FindProperty("MoraleRestoreAmount").intValue = morale;
        so.FindProperty("MaxStackSize").intValue = 99;
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static ItemDefinition CreateMaterialItem(string id, string displayName, string description, ItemRarity rarity, int sellValue, int _ignored)
    {
        return CreateItem(id, displayName, description, ItemType.Material, rarity, sellValue, 0, 0, 0, 0, 0f, 0f, 0, 0, 99);
    }

    private static DungeonLootTableSO CreateLootTable(string id, int rolls, float bonusDropChance, DungeonLootTableSO.LootEntry[] entries)
    {
        string path = "Assets/Resources/Dungeons/" + id + ".asset";
        DungeonLootTableSO existing = AssetDatabase.LoadAssetAtPath<DungeonLootTableSO>(path);
        DungeonLootTableSO asset = existing != null ? existing : ScriptableObject.CreateInstance<DungeonLootTableSO>();

        if (existing == null)
        {
            AssetDatabase.CreateAsset(asset, path);
        }

        SerializedObject so = new SerializedObject(asset);
        so.FindProperty("rolls").intValue = rolls;
        so.FindProperty("bonusDropChance").floatValue = bonusDropChance;
        SerializedProperty list = so.FindProperty("entries");
        list.ClearArray();
        for (int i = 0; i < entries.Length; i++)
        {
            list.InsertArrayElementAtIndex(i);
            SerializedProperty entry = list.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("ItemId").stringValue = entries[i].ItemId;
            entry.FindPropertyRelative("FallbackName").stringValue = entries[i].FallbackName;
            entry.FindPropertyRelative("MinQuantity").intValue = entries[i].MinQuantity;
            entry.FindPropertyRelative("MaxQuantity").intValue = entries[i].MaxQuantity;
            entry.FindPropertyRelative("Weight").intValue = entries[i].Weight;
            entry.FindPropertyRelative("DropChance").floatValue = entries[i].DropChance;
            entry.FindPropertyRelative("Guaranteed").boolValue = entries[i].Guaranteed;
        }
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        string name = System.IO.Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(parent, name);
    }

    [MenuItem("Tools/Pick Me Up/Create Default Enemy Prefab")]
    public static void CreateEnemyPrefab()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/EnemyPrefabs");

        string texturePath = "Assets/Resources/EnemyPrefabs/DefaultEnemySprite.png";

        // Generate a white 32×32 texture for the enemy placeholder
        Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        System.IO.File.WriteAllBytes(texturePath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(texturePath);
        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32;
            importer.SaveAndReimport();
        }

        Sprite defaultSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);

        GameObject tempObj = new GameObject("DefaultEnemy");
        SpriteRenderer sr = tempObj.AddComponent<SpriteRenderer>();
        sr.sprite = defaultSprite;
        sr.sortingOrder = 1;
        BoxCollider2D col = tempObj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        tempObj.AddComponent<EnemyEntity>();

        string prefabPath = "Assets/Resources/EnemyPrefabs/DefaultEnemy.prefab";
        PrefabUtility.SaveAsPrefabAsset(tempObj, prefabPath);
        Object.DestroyImmediate(tempObj);

        Debug.Log("[CreateDefaultDungeons] Default enemy prefab created at " + prefabPath);
    }
}
#endif
