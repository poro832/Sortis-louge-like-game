using System;
using Sortis.Cards;
using Sortis.Prophecy;

namespace Sortis.Combat;

/// <summary>
/// 전투 매니저 — Sortis 전투의 한 턴 흐름을 관리한다.
///
/// 턴 흐름:
///   1. 에너지 충전 + 카드 드로우
///   2. 예언 조건 제시
///   3. 플레이어가 핸드에서 스프레드 슬롯에 카드 배치
///   4. 스프레드 발동 → 효과 처리 (과거→현재→미래)
///   5. 예언 적중 판정 → 보너스 배율 적용
///   6. 적 인텐트 실행
///   7. 남은 핸드 → 버림패
/// </summary>
public class BattleManager
{
    private readonly Deck _deck;
    private readonly ProphecyEngine _prophecy;
    private Spread _currentSpread;

    public int PlayerHp { get; private set; }
    public int PlayerMaxHp { get; }
    public int PlayerBlock { get; private set; }
    public int Energy { get; private set; }
    public int MaxEnergy { get; set; } = 3;
    public int DrawPerTurn { get; set; } = 5;
    public int TurnNumber { get; private set; }

    public BattleManager(Deck deck, int maxHp = 80)
    {
        _deck = deck;
        _prophecy = new ProphecyEngine();
        _currentSpread = Spread.ThreeCard(); // 기본 스프레드
        PlayerMaxHp = maxHp;
        PlayerHp = maxHp;
    }

    /// <summary>전투 시작</summary>
    public void StartBattle()
    {
        TurnNumber = 0;
        StartTurn();
    }

    /// <summary>턴 시작 — 에너지 충전, 카드 드로우, 예언 생성</summary>
    public void StartTurn()
    {
        TurnNumber++;
        Energy = MaxEnergy;
        PlayerBlock = 0;
        _currentSpread = Spread.ThreeCard(); // 턴마다 스프레드 리셋

        _deck.Draw(DrawPerTurn);

        var prophecy = _prophecy.GenerateNewProphecy();
        OnProphecyRevealed?.Invoke(prophecy);
    }

    /// <summary>핸드에서 카드를 스프레드 슬롯에 배치</summary>
    public bool PlaceCard(Card card, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _currentSpread.SlotCount)
            return false;

        if (Energy < card.Data.EnergyCost)
            return false;

        if (_currentSpread.Slots[slotIndex].PlaceCard(card))
        {
            Energy -= card.Data.EnergyCost;
            return true;
        }
        return false;
    }

    /// <summary>스프레드 발동 — 배치된 카드들의 효과를 일괄 처리</summary>
    public SpreadResult ActivateSpread()
    {
        var result = _currentSpread.Activate();

        // 예언 적중 판정
        var prophecyResult = _prophecy.Evaluate(_currentSpread, result);

        int finalDamage = result.TotalDamage;
        if (prophecyResult.IsHit)
        {
            finalDamage = (int)(finalDamage * prophecyResult.Multiplier);
            OnProphecyHit?.Invoke(prophecyResult);
        }

        PlayerBlock += result.TotalBlock;

        // 추가 드로우
        if (result.TotalDraw > 0)
            _deck.Draw(result.TotalDraw);

        // 배치된 카드를 버림패로
        foreach (var slot in _currentSpread.Slots)
        {
            if (slot.RemoveCard() is { } card)
                _deck.DiscardFromHand(card);
        }

        OnSpreadActivated?.Invoke(result, finalDamage);

        return result;
    }

    /// <summary>턴 종료 — 남은 핸드 버리기</summary>
    public void EndTurn()
    {
        _deck.DiscardAllHand();
    }

    /// <summary>플레이어가 데미지를 받을 때 (블록 우선 소모)</summary>
    public void TakeDamage(int damage)
    {
        int remaining = damage - PlayerBlock;
        PlayerBlock = Math.Max(0, PlayerBlock - damage);

        if (remaining > 0)
            PlayerHp = Math.Max(0, PlayerHp - remaining);
    }

    // --- Events ---
    public event Action<ProphecyCondition>? OnProphecyRevealed;
    public event Action<ProphecyResult>? OnProphecyHit;
    public event Action<SpreadResult, int>? OnSpreadActivated;
}
