using Sortis.Cards;
using Sortis.Core;

namespace Sortis.Combat;

/// <summary>
/// 스프레드의 개별 슬롯.
/// 각 슬롯은 위치(과거/현재/미래 등)를 가지며, 카드를 배치할 수 있다.
/// </summary>
public class SpreadSlot
{
    public SlotPosition Position { get; }
    public Card? PlacedCard { get; private set; }
    public bool IsOccupied => PlacedCard != null;

    public SpreadSlot(SlotPosition position)
    {
        Position = position;
    }

    public bool PlaceCard(Card card)
    {
        if (IsOccupied) return false;
        PlacedCard = card;
        return true;
    }

    public Card? RemoveCard()
    {
        var card = PlacedCard;
        PlacedCard = null;
        return card;
    }
}
