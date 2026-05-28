using System.Collections.Generic;
using UnityEngine;

public class RosterService : IRosterService
{
    private readonly GameStateService _gameState;

    public RosterService()
    {
        _gameState = ServiceRegistry.Instance.Resolve<GameStateService>();
    }

    public List<HeroInstance> GetAll()
    {
        // Return a copy so callers cannot mutate the master list accidentally
        return new List<HeroInstance>(_gameState.Data.Heroes);
    }

    public List<HeroInstance> GetAlive()
    {
        return _gameState.Data.Heroes.FindAll(h => h.IsAlive);
    }

    public List<HeroInstance> GetDead()
    {
        return _gameState.Data.Heroes.FindAll(h => !h.IsAlive);
    }

    public int GetCount(bool aliveOnly)
    {
        if (aliveOnly)
            return _gameState.Data.Heroes.FindAll(h => h.IsAlive).Count;
        return _gameState.Data.Heroes.Count;
    }

    public void MarkDead(string instanceId)
    {
        HeroInstance hero = _gameState.Data.Heroes.Find(h => h.InstanceId == instanceId);
        if (hero == null)
        {
            Debug.LogWarning($"[RosterService] MarkDead: hero with id {instanceId} not found.");
            return;
        }

        if (!hero.IsAlive)
        {
            Debug.LogWarning($"[RosterService] MarkDead: hero {instanceId} is already dead.");
            return;
        }

        hero.IsAlive = false;
        _gameState.Save();
        Debug.Log($"[RosterService] Hero {instanceId} marked as dead.");
    }

    public void Remove(string instanceId)
    {
        int removed = _gameState.Data.Heroes.RemoveAll(h => h.InstanceId == instanceId);
        if (removed > 0)
        {
            _gameState.Save();
            Debug.Log($"[RosterService] Hero {instanceId} permanently removed.");
        }
        else
        {
            Debug.LogWarning($"[RosterService] Remove: hero with id {instanceId} not found.");
        }
    }
}
