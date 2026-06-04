public class CurrencyService : ICurrencyService
{
    private readonly GameStateService _gameState;

    public CurrencyService()
    {
        _gameState = ServiceRegistry.Instance.Resolve<GameStateService>();
    }

    public int GetGold() => _gameState.Data.Gold;
    public int GetGems() => _gameState.Data.Gems;
    public int GetAttributeStones() => _gameState.Data.AttributeStones;
    public int GetMemorialFragments() => _gameState.Data.MemorialFragments;
    public int GetDungeonExp() => _gameState.Data.DungeonExp;

    public bool SpendGold(int amount)
    {
        if (_gameState.Data.Gold < amount) return false;
        _gameState.Data.Gold -= amount;
        _gameState.Save();
        return true;
    }

    public bool SpendGems(int amount)
    {
        if (_gameState.Data.Gems < amount) return false;
        _gameState.Data.Gems -= amount;
        _gameState.Save();
        return true;
    }

    public bool SpendAttributeStones(int amount)
    {
        if (_gameState.Data.AttributeStones < amount) return false;
        _gameState.Data.AttributeStones -= amount;
        _gameState.Save();
        return true;
    }

    public bool SpendMemorialFragments(int amount)
    {
        if (_gameState.Data.MemorialFragments < amount) return false;
        _gameState.Data.MemorialFragments -= amount;
        _gameState.Save();
        return true;
    }

    public bool SpendDungeonExp(int amount)
    {
        if (_gameState.Data.DungeonExp < amount) return false;
        _gameState.Data.DungeonExp -= amount;
        _gameState.Save();
        return true;
    }

    public void AddGold(int amount)
    {
        _gameState.Data.Gold += amount;
        _gameState.Save();
    }

    public void AddGems(int amount)
    {
        _gameState.Data.Gems += amount;
        _gameState.Save();
    }

    public void AddAttributeStones(int amount)
    {
        _gameState.Data.AttributeStones += amount;
        _gameState.Save();
    }

    public void AddMemorialFragments(int amount)
    {
        _gameState.Data.MemorialFragments += amount;
        _gameState.Save();
    }

    public void AddDungeonExp(int amount)
    {
        if (amount <= 0) return;
        _gameState.Data.DungeonExp += amount;
        _gameState.Save();
    }
}
