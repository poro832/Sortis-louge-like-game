using System.Collections.Generic;
using System.Linq;
using Sortis.Cards;
using Sortis.Combat;
using Sortis.Core;

namespace Sortis.Prophecy;

/// <summary>
/// 예언 조건 — 매 턴 제시되며, 충족 시 보너스 배율을 부여한다.
/// "이번 턴에 검 수트 3장 이상 배치하라" 같은 조건을 표현한다.
/// </summary>
public abstract class ProphecyCondition
{
    public abstract string Description { get; }
    public abstract bool Evaluate(Spread spread, SpreadResult result);
}

/// <summary>특정 수트 N장 이상 배치 조건</summary>
public class SuitCountCondition : ProphecyCondition
{
    private readonly Suit _suit;
    private readonly int _required;

    public SuitCountCondition(Suit suit, int required)
    {
        _suit = suit;
        _required = required;
    }

    public override string Description =>
        $"{_suit} 수트 {_required}장 이상 배치";

    public override bool Evaluate(Spread spread, SpreadResult result)
    {
        result.SuitCounts.TryGetValue(_suit, out int count);
        return count >= _required;
    }
}

/// <summary>메이저 아르카나 포함 조건</summary>
public class MajorArcanaCondition : ProphecyCondition
{
    private readonly int _required;

    public MajorArcanaCondition(int required = 1)
    {
        _required = required;
    }

    public override string Description =>
        $"메이저 아르카나 {_required}장 이상 포함";

    public override bool Evaluate(Spread spread, SpreadResult result)
    {
        int count = spread.PlacedCards.Count(c => c.Data.IsMajorArcana);
        return count >= _required;
    }
}

/// <summary>역방향 카드 배치 조건</summary>
public class ReversedCardCondition : ProphecyCondition
{
    private readonly SlotPosition? _targetSlot;

    public ReversedCardCondition(SlotPosition? targetSlot = null)
    {
        _targetSlot = targetSlot;
    }

    public override string Description =>
        _targetSlot.HasValue
            ? $"{_targetSlot.Value} 슬롯에 역방향 카드 배치"
            : "역방향 카드 1장 이상 배치";

    public override bool Evaluate(Spread spread, SpreadResult result)
    {
        if (_targetSlot.HasValue)
        {
            var slot = spread.Slots.FirstOrDefault(s => s.Position == _targetSlot.Value);
            return slot?.PlacedCard?.IsReversed == true;
        }
        return spread.PlacedCards.Any(c => c.IsReversed);
    }
}
