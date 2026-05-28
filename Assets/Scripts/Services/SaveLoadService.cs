using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveLoadService : ISaveLoadService
{
    private const string SaveFileName = "pickmeup_save.json";

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public void Save(GameSaveData data)
    {
        GameSaveData safeData = Normalize(data);
        SaveFileData fileData = ToFileData(safeData);
        string json = JsonUtility.ToJson(fileData, true);
        File.WriteAllText(SaveFilePath, json);
    }

    public GameSaveData Load()
    {
        if (!File.Exists(SaveFilePath))
        {
            return CreateDefaultSave();
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            SaveFileData fileData = JsonUtility.FromJson<SaveFileData>(json);
            GameSaveData data = FromFileData(fileData);
            data = Normalize(data);
            return data;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SaveLoadService] Failed to load save, using defaults. {ex.Message}");
            return CreateDefaultSave();
        }
    }

    private GameSaveData CreateDefaultSave()
    {
        GameSaveData newSave = new GameSaveData
        {
            Gold = 100000,
            Gems = 10000,
            AttributeStones = 500,
            MemorialFragments = 0,
            Heroes = new List<HeroInstance>(),
            Pity = new PityData(),
            Items = new List<ItemInstance>(),
            DiscoveredHeroIds = new List<string>(),
            MemorialEchoHeroIds = new List<string>(),
            DungeonProgress = new Dictionary<string, int>(),
            FacilityLevels = new Dictionary<string, int>(),
            FacilityUpgradeQueue = new List<FacilityUpgradeProgress>(),
            Stamina = 100,
            LastStaminaRegenTime = DateTime.UtcNow.Ticks
        };

        Save(newSave);
        return newSave;
    }

    private GameSaveData Normalize(GameSaveData data)
    {
        if (data == null)
        {
            data = new GameSaveData();
        }

        if (data.Heroes == null)
        {
            data.Heroes = new List<HeroInstance>();
        }

        if (data.Pity == null)
        {
            data.Pity = new PityData();
        }

        if (data.Items == null)
        {
            data.Items = new List<ItemInstance>();
        }

        if (data.DiscoveredHeroIds == null)
        {
            data.DiscoveredHeroIds = new List<string>();
        }

        if (data.MemorialEchoHeroIds == null)
        {
            data.MemorialEchoHeroIds = new List<string>();
        }

        if (data.DungeonProgress == null)
        {
            data.DungeonProgress = new Dictionary<string, int>();
        }

        if (data.FacilityLevels == null)
        {
            data.FacilityLevels = new Dictionary<string, int>();
        }

        if (data.FacilityUpgradeQueue == null)
        {
            data.FacilityUpgradeQueue = new List<FacilityUpgradeProgress>();
        }

        if (data.Stamina <= 0)
        {
            data.Stamina = 100;
        }

        if (data.MemorialFragments < 0)
        {
            data.MemorialFragments = 0;
        }

        return data;
    }

    private SaveFileData ToFileData(GameSaveData data)
    {
        return new SaveFileData
        {
            Gold = data.Gold,
            Gems = data.Gems,
            AttributeStones = data.AttributeStones,
            MemorialFragments = data.MemorialFragments,
            Heroes = data.Heroes ?? new List<HeroInstance>(),
            Pity = data.Pity ?? new PityData(),
            Items = data.Items ?? new List<ItemInstance>(),
            DiscoveredHeroIds = data.DiscoveredHeroIds ?? new List<string>(),
            MemorialEchoHeroIds = data.MemorialEchoHeroIds ?? new List<string>(),
            DungeonProgress = ToEntryList(data.DungeonProgress),
            FacilityLevels = ToEntryList(data.FacilityLevels),
            FacilityUpgradeQueue = data.FacilityUpgradeQueue ?? new List<FacilityUpgradeProgress>(),
            Stamina = data.Stamina,
            LastStaminaRegenTime = data.LastStaminaRegenTime,
            LastGoldCollectionTime = data.LastGoldCollectionTime,
            LastExpCollectionTime = data.LastExpCollectionTime,
            LastHospitalProcessTime = data.LastHospitalProcessTime
        };
    }

    private GameSaveData FromFileData(SaveFileData fileData)
    {
        if (fileData == null)
        {
            return null;
        }

        return new GameSaveData
        {
            Gold = fileData.Gold,
            Gems = fileData.Gems,
            AttributeStones = fileData.AttributeStones,
            MemorialFragments = fileData.MemorialFragments,
            Heroes = fileData.Heroes ?? new List<HeroInstance>(),
            Pity = fileData.Pity ?? new PityData(),
            Items = fileData.Items ?? new List<ItemInstance>(),
            DiscoveredHeroIds = fileData.DiscoveredHeroIds ?? new List<string>(),
            MemorialEchoHeroIds = fileData.MemorialEchoHeroIds ?? new List<string>(),
            DungeonProgress = ToDictionary(fileData.DungeonProgress),
            FacilityLevels = ToDictionary(fileData.FacilityLevels),
            FacilityUpgradeQueue = fileData.FacilityUpgradeQueue ?? new List<FacilityUpgradeProgress>(),
            Stamina = fileData.Stamina,
            LastStaminaRegenTime = fileData.LastStaminaRegenTime,
            LastGoldCollectionTime = fileData.LastGoldCollectionTime,
            LastExpCollectionTime = fileData.LastExpCollectionTime,
            LastHospitalProcessTime = fileData.LastHospitalProcessTime
        };
    }

    private List<IntEntry> ToEntryList(Dictionary<string, int> dictionary)
    {
        List<IntEntry> list = new List<IntEntry>();
        if (dictionary == null)
        {
            return list;
        }

        foreach (var pair in dictionary)
        {
            list.Add(new IntEntry { Key = pair.Key, Value = pair.Value });
        }

        return list;
    }

    private Dictionary<string, int> ToDictionary(List<IntEntry> entries)
    {
        Dictionary<string, int> dict = new Dictionary<string, int>();
        if (entries == null)
        {
            return dict;
        }

        foreach (IntEntry entry in entries)
        {
            if (entry != null && !string.IsNullOrEmpty(entry.Key))
            {
                dict[entry.Key] = entry.Value;
            }
        }

        return dict;
    }

    [Serializable]
    private class SaveFileData
    {
        public int Gold;
        public int Gems;
        public int AttributeStones;
        public int MemorialFragments;
        public List<HeroInstance> Heroes = new List<HeroInstance>();
        public PityData Pity = new PityData();
        public List<ItemInstance> Items = new List<ItemInstance>();
        public List<string> DiscoveredHeroIds = new List<string>();
        public List<string> MemorialEchoHeroIds = new List<string>();
        public List<IntEntry> DungeonProgress = new List<IntEntry>();
        public List<IntEntry> FacilityLevels = new List<IntEntry>();
        public List<FacilityUpgradeProgress> FacilityUpgradeQueue = new List<FacilityUpgradeProgress>();
        public int Stamina = 100;
        public long LastStaminaRegenTime;
        public long LastGoldCollectionTime;
        public long LastExpCollectionTime;
        public long LastHospitalProcessTime;
    }

    [Serializable]
    private class IntEntry
    {
        public string Key;
        public int Value;
    }
}
