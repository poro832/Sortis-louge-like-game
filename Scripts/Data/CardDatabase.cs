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
            // --- Wands (공격) ---
            CreateCard("Wands III", Suit.Wands, 3, cost: 1,
                damage: 3, revDamage: 2, revDesc: "2 DMG + Burn 2"),
            CreateCard("Wands V", Suit.Wands, 5, cost: 1,
                damage: 5, revDamage: 3, revDesc: "3 DMG + Weaken"),
            CreateCard("Wands VII", Suit.Wands, 7, cost: 2,
                damage: 7, revDamage: 5, revDraw: 1, revDesc: "5 DMG + Draw 1"),

            // --- Swords (기술) ---
            CreateCard("Swords II", Suit.Swords, 2, cost: 1,
                damage: 2, draw: 1, revDamage: 1, revDraw: 2,
                desc: "2 DMG + Draw 1", revDesc: "1 DMG + Draw 2"),
            CreateCard("Swords IV", Suit.Swords, 4, cost: 1,
                damage: 4, revDamage: 2, revDesc: "2 DMG + Weaken"),
            CreateCard("Swords VI", Suit.Swords, 6, cost: 2,
                damage: 6, draw: 1, revDamage: 3, revDraw: 2,
                desc: "6 DMG + Draw 1", revDesc: "3 DMG + Draw 2"),

            // --- Cups (방어) ---
            CreateCard("Cups II", Suit.Cups, 2, cost: 1,
                block: 3, revBlock: 2, revHeal: 1,
                desc: "3 Block", revDesc: "2 Block + Heal 1"),
            CreateCard("Cups IV", Suit.Cups, 4, cost: 1,
                block: 5, revBlock: 3, revHeal: 2,
                desc: "5 Block", revDesc: "3 Block + Heal 2"),
            CreateCard("Cups VI", Suit.Cups, 6, cost: 2,
                block: 8, revBlock: 5, revHeal: 3,
                desc: "8 Block", revDesc: "5 Block + Heal 3"),

            // --- Pentacles (자원) ---
            CreateCard("Pentacles II", Suit.Pentacles, 2, cost: 1,
                draw: 1, block: 2, revDraw: 2,
                desc: "Draw 1 + 2 Block", revDesc: "Draw 2"),
            CreateCard("Pentacles IV", Suit.Pentacles, 4, cost: 1,
                desc: "Energy +1", revDraw: 1, revBlock: 1,
                revDesc: "Draw 1 + 1 Block"),
            CreateCard("Pentacles VI", Suit.Pentacles, 6, cost: 0,
                draw: 1, revBlock: 2,
                desc: "Draw 1", revDesc: "2 Block"),
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
