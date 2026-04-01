using System.Collections.Generic;
using System.Linq;
using Sortis.Cards;
using Sortis.Core;

namespace Sortis.Combat;

/// <summary>
/// 스프레드 — 타로 리딩의 배치법을 전투 메커닉으로 변환한 핵심 시스템.
/// 카드를 슬롯에 배치한 뒤 일괄 발동한다.
/// </summary>
public class Spread
{
    public string Name { get; }
    public List<SpreadSlot> Slots { get; }

    public int SlotCount => Slots.Count;
    public bool IsFull => Slots.All(s => s.IsOccupied);
    public IEnumerable<Card> PlacedCards => Slots
        .Where(s => s.IsOccupied)
        .Select(s => s.PlacedCard!);

    private Spread(string name, List<SpreadSlot> slots)
    {
        Name = name;
        Slots = slots;
    }

    /// <summary>원 카드 스프레드 — 1장 즉시 발동 (튜토리얼용)</summary>
    public static Spread OneCard() => new("원 카드", new()
    {
        new SpreadSlot(SlotPosition.Present)
    });

    /// <summary>쓰리 카드 스프레드 — 과거/현재/미래 기본 전투 배치</summary>
    public static Spread ThreeCard() => new("쓰리 카드", new()
    {
        new SpreadSlot(SlotPosition.Past),
        new SpreadSlot(SlotPosition.Present),
        new SpreadSlot(SlotPosition.Future)
    });

    /// <summary>크로스 스프레드 — 5슬롯, 액트 2에서 언락</summary>
    public static Spread Cross() => new("크로스", new()
    {
        new SpreadSlot(SlotPosition.Above),
        new SpreadSlot(SlotPosition.Left),
        new SpreadSlot(SlotPosition.Center),
        new SpreadSlot(SlotPosition.Right),
        new SpreadSlot(SlotPosition.Below)
    });

    /// <summary>
    /// 스프레드 발동 — 슬롯 순서대로 카드 효과를 처리하고 결과를 반환한다.
    /// </summary>
    public SpreadResult Activate()
    {
        int totalDamage = 0;
        int totalBlock = 0;
        int totalDraw = 0;
        var suitCounts = new Dictionary<Suit, int>();

        foreach (var slot in Slots)
        {
            if (slot.PlacedCard is not { } card) continue;

            totalDamage += card.GetDamage();
            totalBlock += card.GetBlock();
            totalDraw += card.GetDraw();

            // 슬롯 위치별 보너스
            totalDamage += GetPositionBonus(slot.Position, card);

            // 수트 카운트 (예언 조건 판정용)
            if (!card.Data.IsMajorArcana)
            {
                suitCounts.TryGetValue(card.Data.Suit, out int count);
                suitCounts[card.Data.Suit] = count + 1;
            }
        }

        return new SpreadResult(totalDamage, totalBlock, totalDraw, suitCounts);
    }

    private static int GetPositionBonus(SlotPosition position, Card card)
    {
        return position switch
        {
            SlotPosition.Past => card.IsReversed ? 2 : 0,      // 과거+역방향 = 추가 디버프 데미지
            SlotPosition.Center => 1,                           // 중앙 슬롯 보너스
            SlotPosition.Future => card.Data.IsMajorArcana ? 3 : 0, // 미래+메이저 = 강화
            _ => 0
        };
    }
}

/// <summary>스프레드 발동 결과</summary>
public record SpreadResult(
    int TotalDamage,
    int TotalBlock,
    int TotalDraw,
    Dictionary<Suit, int> SuitCounts
);
