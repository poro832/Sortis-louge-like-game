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
                new FateOption("화염의 분노", "피해 x1.5", damageMultiplier: 1.5f),
                new FateOption("잔불", "피해 x1 + 다음 턴 화상 3", burnDamage: 3),
                dominant),

            Suit.Swords => new FateChoice(
                new FateOption("통찰", "카드 2장 추가 드로우", bonusDraw: 2),
                new FateOption("약점 노출", "적 약화 (다음 공격 -25%)", applyWeaken: true),
                dominant),

            Suit.Cups => new FateChoice(
                new FateOption("요새화", "방어 x2", blockMultiplier: 2f),
                new FateOption("재생", "방어 x1 + 회복 3", bonusHeal: 3),
                dominant),

            Suit.Pentacles => new FateChoice(
                new FateOption("충전", "다음 턴 에너지 +1", bonusEnergy: 1),
                new FateOption("수확", "카드 1장 추가 드로우", bonusDraw: 1),
                dominant),

            _ => new FateChoice(
                new FateOption("균형의 운명", "모든 효과 +20%",
                    damageMultiplier: 1.2f, blockMultiplier: 1.2f),
                new FateOption("예지", "카드 1장 추가 드로우", bonusDraw: 1),
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
