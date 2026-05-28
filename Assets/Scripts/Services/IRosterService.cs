using System.Collections.Generic;

public interface IRosterService
{
    /// <summary>Returns all heroes (alive and dead).</summary>
    List<HeroInstance> GetAll();

    /// <summary>Returns only living heroes.</summary>
    List<HeroInstance> GetAlive();

    /// <summary>Returns only dead heroes.</summary>
    List<HeroInstance> GetDead();

    /// <summary>Returns hero count by alive status.</summary>
    int GetCount(bool aliveOnly);

    /// <summary>Marks a hero as dead and saves.</summary>
    void MarkDead(string instanceId);

    /// <summary>Permanently removes a hero from the roster (e.g. Synthesis).</summary>
    void Remove(string instanceId);
}
