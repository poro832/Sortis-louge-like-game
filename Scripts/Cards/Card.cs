using Sortis.Core;

namespace Sortis.Cards;

/// <summary>
/// 런타임에 존재하는 카드 인스턴스.
/// 같은 CardData를 공유하더라도 각 카드는 독립적인 방향(정/역)과 상태를 가진다.
/// </summary>
public class Card
{
    public CardData Data { get; }
    public CardOrientation Orientation { get; set; }

    public Card(CardData data, CardOrientation orientation = CardOrientation.Upright)
    {
        Data = data;
        Orientation = orientation;
    }

    public bool IsReversed => Orientation == CardOrientation.Reversed;

    /// <summary>정/역방향에 따른 실제 데미지</summary>
    public int GetDamage() => IsReversed ? Data.ReversedDamage : Data.Damage;

    /// <summary>정/역방향에 따른 실제 블록</summary>
    public int GetBlock() => IsReversed ? Data.ReversedBlock : Data.Block;

    /// <summary>정/역방향에 따른 실제 드로우</summary>
    public int GetDraw() => IsReversed ? Data.ReversedDraw : Data.Draw;

    public override string ToString()
    {
        string dir = IsReversed ? " (Reversed)" : "";
        return Data.IsMajorArcana
            ? $"[{Data.MajorNumber}] {Data.CardName}{dir}"
            : $"{Data.CardName} of {Data.Suit}{dir}";
    }
}
