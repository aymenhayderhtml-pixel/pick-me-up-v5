using System;
using UnityEngine;

public class FacilityTickManager : MonoBehaviour
{
    public static FacilityTickManager Instance { get; private set; }

    [SerializeField] private float tickInterval = 60f;

    private IFacilityService facilityService;
    private float tickTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        facilityService = ServiceRegistry.Instance?.Resolve<IFacilityService>();
        ProcessOfflineTime();
    }

    private void Update()
    {
        tickTimer += Time.unscaledDeltaTime;
        if (tickTimer >= tickInterval)
        {
            tickTimer = 0f;
            ProcessTick();
        }
    }

    private void ProcessTick()
    {
        facilityService?.ProcessPassiveGeneration();
    }

    private void ProcessOfflineTime()
    {
        if (facilityService == null) return;
        facilityService.ProcessPassiveGeneration();
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause) ProcessOfflineTime();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
