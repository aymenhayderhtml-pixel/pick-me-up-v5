public class GameStateService
{
    public GameSaveData Data { get; private set; }

    public GameStateService(GameSaveData data)
    {
        Data = data;
    }

    public void Save()
    {
        ISaveLoadService saveLoadService = ServiceRegistry.Instance.Resolve<ISaveLoadService>();
        saveLoadService.Save(Data);
    }
}
