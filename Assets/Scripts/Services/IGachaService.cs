using System.Collections.Generic;

public interface IGachaService
{
    /// <summary>Single summon on standard banner (costs Gold).</summary>
    HeroInstance SummonStandard();

    /// <summary>10x summon on standard banner (costs Gold x10).</summary>
    List<HeroInstance> SummonStandardTen();

    /// <summary>Single summon on premium banner (costs Gems).</summary>
    HeroInstance SummonPremium();

    /// <summary>10x summon on premium banner (costs Gems x10).</summary>
    List<HeroInstance> SummonPremiumTen();

    /// <summary>Current pity counts (read-only view).</summary>
    PityData GetPity();
}
