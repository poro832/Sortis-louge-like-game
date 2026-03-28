using System;
using System.Collections.Generic;
using Sortis.Combat;
using Sortis.Core;

namespace Sortis.Prophecy;

/// <summary>
/// 예언 엔진 — 턴마다 예언 조건을 생성하고, 적중 여부에 따라 배율을 관리한다.
/// 연속 적중 시 배율이 누적 상승하여 Balatro 스타일의 폭발적 보너스를 만든다.
/// </summary>
public class ProphecyEngine
{
    private readonly Random _rng = new();
    private readonly List<ProphecyCondition> _conditionPool = new();

    public ProphecyCondition? CurrentProphecy { get; private set; }
    public int ConsecutiveHits { get; private set; }
    public float BonusMultiplier => 1.0f + (ConsecutiveHits * 0.5f); // 연속 적중당 +0.5배

    public ProphecyEngine()
    {
        BuildConditionPool();
    }

    /// <summary>새 턴 시작 — 랜덤 예언 조건 생성</summary>
    public ProphecyCondition GenerateNewProphecy()
    {
        CurrentProphecy = _conditionPool[_rng.Next(_conditionPool.Count)];
        return CurrentProphecy;
    }

    /// <summary>스프레드 발동 후 예언 적중 판정</summary>
    public ProphecyResult Evaluate(Spread spread, SpreadResult spreadResult)
    {
        if (CurrentProphecy == null)
            return new ProphecyResult(false, 1.0f);

        bool hit = CurrentProphecy.Evaluate(spread, spreadResult);

        if (hit)
        {
            ConsecutiveHits++;
            return new ProphecyResult(true, BonusMultiplier);
        }
        else
        {
            ConsecutiveHits = 0;
            return new ProphecyResult(false, 1.0f);
        }
    }

    private void BuildConditionPool()
    {
        // 수트별 조건 (2~3장 요구)
        foreach (Suit suit in Enum.GetValues<Suit>())
        {
            _conditionPool.Add(new SuitCountCondition(suit, 2));
            _conditionPool.Add(new SuitCountCondition(suit, 3));
        }

        // 메이저 아르카나 조건
        _conditionPool.Add(new MajorArcanaCondition(1));

        // 역방향 카드 조건
        _conditionPool.Add(new ReversedCardCondition());
        _conditionPool.Add(new ReversedCardCondition(SlotPosition.Past));
        _conditionPool.Add(new ReversedCardCondition(SlotPosition.Future));
    }
}

/// <summary>예언 판정 결과</summary>
public record ProphecyResult(bool IsHit, float Multiplier);
