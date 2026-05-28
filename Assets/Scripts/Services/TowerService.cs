using System.IO;
using UnityEngine;

public class TowerService : ITowerService
{
    [System.Serializable]
    public class TowerSaveData
    {
        public int highestFloor;
    }

    private int _currentFloor = 0;
    private int _highestFloor = 0;
    private readonly string _savePath;

    public TowerService()
    {
        _savePath = Path.Combine(Application.persistentDataPath, "tower_save.json");
    }

    public int CurrentFloor => _currentFloor;
    public int HighestFloor => _highestFloor;

    public void StartRun()
    {
        _currentFloor = 1;
        SaveProgress();
    }

    public void CompleteFloor()
    {
        _currentFloor++;

        if (_currentFloor > _highestFloor)
        {
            _highestFloor = _currentFloor;
        }

        SaveProgress();
    }

    public void GameOver()
    {
        _currentFloor = 0;
    }

    public void ResetRun()
    {
        _currentFloor = 0;
        _highestFloor = 0;
        SaveProgress();
    }

    public void LoadProgress()
    {
        if (File.Exists(_savePath))
        {
            try
            {
                string json = File.ReadAllText(_savePath);
                TowerSaveData data = JsonUtility.FromJson<TowerSaveData>(json);

                if (data != null)
                {
                    _highestFloor = data.highestFloor;
                    _currentFloor = 0;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to load tower save: {e.Message}");
                SetDefaults();
            }
        }
        else
        {
            SetDefaults();
        }
    }

    public void SaveProgress()
    {
        try
        {
            TowerSaveData data = new TowerSaveData
            {
                highestFloor = _highestFloor
            };

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(_savePath, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save tower progress: {e.Message}");
        }
    }

    private void SetDefaults()
    {
        _currentFloor = 0;
        _highestFloor = 0;
    }
}