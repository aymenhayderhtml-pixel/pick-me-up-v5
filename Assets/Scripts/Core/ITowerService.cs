public interface ITowerService
{
    int CurrentFloor { get; }
    int HighestFloor { get; }
    void StartRun();
    void CompleteFloor();
    void GameOver();
    void ResetRun();
    void LoadProgress();
    void SaveProgress();
}