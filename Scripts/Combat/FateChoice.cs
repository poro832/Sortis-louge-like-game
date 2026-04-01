using System.Collections.Generic;
using System.Linq;
using Sortis.Core;

namespace Sortis.Combat;

public class FateOption
{
    public string Name { get; }
    public string Description { get; }
    public float DamageMultiplier { get; }
    public float BlockMultiplier { get; }
    public int BonusDraw { get; }
    public int BonusHeal { get; }
    public int BonusEnergy { get; }
    public int BurnDamage { get; }
    public bool ApplyWeaken { get; }

    public FateOption(string name, string description,
        float damageMultiplier = 1f, float blockMultiplier = 1f,
        int bonusDraw = 0, int bonusHeal = 0, int bonusEnergy = 0,
        int burnDamage = 0, bool applyWeaken = false)
    {
        Name = name;
        Description = description;
        DamageMultiplier = damageMultiplier;
        BlockMultiplier = blockMultiplier;
        BonusDraw = bonusDraw;
        BonusHeal = bonusHeal;
        BonusEnergy = bonusEnergy;
        BurnDamage = burnDamage;
        ApplyWeaken = applyWeaken;
    }
}

public class FateChoice
{
    public FateOption OptionA { get; }
    public FateOption OptionB { get; }
    public Suit? DominantSuit { get; }

    public FateChoice(FateOption optionA, FateOption optionB, Suit? dominantSuit)
    {
        OptionA = optionA;
        OptionB = optionB;
        DominantSuit = dominantSuit;
    }

    public static FateChoice Generate(Dictionary<Suit, int> suitCounts)
    {
        Suit? dominant = GetDominantSuit(suitCounts);

        return dominant switch
        {
            Suit.Wands => new FateChoice(
                new FateOption("Fury of Flame", "Damage x1.5", damageMultiplier: 1.5f),
                new FateOption("Lingering Burn", "Damage x1 + Burn 3 next turn", burnDamage: 3),
                dominant),

            Suit.Swords => new FateChoice(
                new FateOption("Insight", "Draw 2 extra cards", bonusDraw: 2),
                new FateOption("Expose Weakness", "Weaken enemy (-25% next attack)", applyWeaken: true),
                dominant),

            Suit.Cups => new FateChoice(
                new FateOption("Fortify", "Block x2", blockMultiplier: 2f),
                new FateOption("Renewal", "Block x1 + Heal 3", bonusHeal: 3),
                dominant),

            Suit.Pentacles => new FateChoice(
                new FateOption("Energize", "Next turn Energy +1", bonusEnergy: 1),
                new FateOption("Harvest", "+10 Gold (not implemented yet)", bonusDraw: 1),
                dominant),

            _ => new FateChoice(
                new FateOption("Balanced Fate", "All effects +20%",
                    damageMultiplier: 1.2f, blockMultiplier: 1.2f),
                new FateOption("Foresight", "Draw 1 extra card", bonusDraw: 1),
                null)
        };
    }

    private static Suit? GetDominantSuit(Dictionary<Suit, int> suitCounts)
    {
        if (suitCounts.Count == 0) return null;

        int maxCount = suitCounts.Values.Max();
        var dominants = suitCounts.Where(kv => kv.Value == maxCount).ToList();

        return dominants.Count == 1 ? dominants[0].Key : null;
    }
}
