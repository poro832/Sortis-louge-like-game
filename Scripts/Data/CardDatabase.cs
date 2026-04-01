using System.Collections.Generic;
using Sortis.Cards;
using Sortis.Core;

namespace Sortis.Data;

public static class CardDatabase
{
    public static List<CardData> CreateStarterDeck()
    {
        return new List<CardData>
        {
            // --- 완드 (공격) ---
            CreateCard("완드 III", Suit.Wands, 3, cost: 1,
                damage: 3, revDamage: 2, revDesc: "피해 2 + 화상 2"),
            CreateCard("완드 V", Suit.Wands, 5, cost: 1,
                damage: 5, revDamage: 3, revDesc: "피해 3 + 약화"),
            CreateCard("완드 VII", Suit.Wands, 7, cost: 2,
                damage: 7, revDamage: 5, revDraw: 1, revDesc: "피해 5 + 드로우 1"),

            // --- 검 (기술) ---
            CreateCard("검 II", Suit.Swords, 2, cost: 1,
                damage: 2, draw: 1, revDamage: 1, revDraw: 2,
                desc: "피해 2 + 드로우 1", revDesc: "피해 1 + 드로우 2"),
            CreateCard("검 IV", Suit.Swords, 4, cost: 1,
                damage: 4, revDamage: 2, revDesc: "피해 2 + 약화"),
            CreateCard("검 VI", Suit.Swords, 6, cost: 2,
                damage: 6, draw: 1, revDamage: 3, revDraw: 2,
                desc: "피해 6 + 드로우 1", revDesc: "피해 3 + 드로우 2"),

            // --- 컵 (방어) ---
            CreateCard("컵 II", Suit.Cups, 2, cost: 1,
                block: 3, revBlock: 2, revHeal: 1,
                desc: "방어 3", revDesc: "방어 2 + 회복 1"),
            CreateCard("컵 IV", Suit.Cups, 4, cost: 1,
                block: 5, revBlock: 3, revHeal: 2,
                desc: "방어 5", revDesc: "방어 3 + 회복 2"),
            CreateCard("컵 VI", Suit.Cups, 6, cost: 2,
                block: 8, revBlock: 5, revHeal: 3,
                desc: "방어 8", revDesc: "방어 5 + 회복 3"),

            // --- 펜타클 (자원) ---
            CreateCard("펜타클 II", Suit.Pentacles, 2, cost: 1,
                draw: 1, block: 2, revDraw: 2,
                desc: "드로우 1 + 방어 2", revDesc: "드로우 2"),
            CreateCard("펜타클 IV", Suit.Pentacles, 4, cost: 1,
                desc: "에너지 +1", revDraw: 1, revBlock: 1,
                revDesc: "드로우 1 + 방어 1"),
            CreateCard("펜타클 VI", Suit.Pentacles, 6, cost: 0,
                draw: 1, revBlock: 2,
                desc: "드로우 1", revDesc: "방어 2"),
        };
    }

    private static CardData CreateCard(string name, Suit suit, int number,
        int cost = 1, int damage = 0, int block = 0, int draw = 0, int heal = 0,
        int revDamage = 0, int revBlock = 0, int revDraw = 0, int revHeal = 0,
        string desc = "", string revDesc = "")
    {
        var card = new CardData();
        card.CardName = name;
        card.Type = CardType.Minor;
        card.Suit = suit;
        card.Number = number;
        card.EnergyCost = cost;
        card.Damage = damage;
        card.Block = block;
        card.Draw = draw;
        card.Heal = heal;
        card.Description = desc;
        card.ReversedDamage = revDamage;
        card.ReversedBlock = revBlock;
        card.ReversedDraw = revDraw;
        card.ReversedHeal = revHeal;
        card.ReversedDescription = revDesc;
        return card;
    }
}
