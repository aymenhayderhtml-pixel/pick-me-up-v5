public interface ICurrencyService
{
    int GetGold();
    int GetGems();
    int GetAttributeStones();
    int GetMemorialFragments();
    
    bool SpendGold(int amount);
    bool SpendGems(int amount);
    bool SpendAttributeStones(int amount);
    bool SpendMemorialFragments(int amount);
    
    void AddGold(int amount);
    void AddGems(int amount);
    void AddAttributeStones(int amount);
    void AddMemorialFragments(int amount);
}
