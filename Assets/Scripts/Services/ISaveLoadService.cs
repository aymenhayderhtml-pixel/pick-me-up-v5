public interface ISaveLoadService
{
    void Save(GameSaveData data);
    GameSaveData Load();
}
