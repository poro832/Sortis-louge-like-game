using System;
using System.Collections.Generic;
using System.Linq;
using Sortis.Cards;
using Sortis.Core;
using Sortis.Prophecy;

namespace Sortis.Combat;

public class BattleManager
{
    private readonly Deck _deck;
    private readonly ProphecyEngine _prophecy;
    private Spread _currentSpread;

    public Enemy Enemy { get; private set; }
    public int PlayerHp { get; private set; }
    public int PlayerMaxHp { get; }
    public int PlayerBlock { get; private set; }
    public int Energy { get; private set; }
    public int MaxEnergy { get; set; } = 3;
    public int BonusEnergy { get; set; }
    public int DrawPerTurn { get; set; } = 5;
    public int BonusDraw { get; set; }
    public int TurnNumber { get; private set; }
    public Spread CurrentSpread => _currentSpread;
    public Deck Deck => _deck;
    public ProphecyCondition? CurrentProphecy => _prophecy.CurrentProphecy;
    public int ConsecutiveHits => _prophecy.ConsecutiveHits;

    public bool IsPlayerDead => PlayerHp <= 0;
    public bool IsEnemyDead => Enemy.IsDead;
    public bool IsBattleOver => IsPlayerDead || IsEnemyDead;

    public BattleManager(Deck deck, Enemy enemy, int maxHp = 80)
    {
        _deck = deck;
        _prophecy = new ProphecyEngine();
        _currentSpread = Spread.ThreeCard();
        Enemy = enemy;
        PlayerMaxHp = maxHp;
        PlayerHp = maxHp;
    }

    public void StartBattle()
    {
        TurnNumber = 0;
        StartTurn();
    }

    public void StartTurn()
    {
        TurnNumber++;
        Energy = MaxEnergy + BonusEnergy;
        BonusEnergy = 0;
        PlayerBlock = 0;
        _currentSpread = Spread.ThreeCard();

        _deck.Draw(DrawPerTurn + BonusDraw);
        BonusDraw = 0;

        var prophecy = _prophecy.GenerateNewProphecy();
        OnProphecyRevealed?.Invoke(prophecy);
    }

    public bool PlaceCard(Card card, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _currentSpread.SlotCount)
            return false;
        if (Energy < card.Data.EnergyCost)
            return false;
        if (!_deck.Hand.Contains(card))
            return false;
        if (_currentSpread.Slots[slotIndex].PlaceCard(card))
        {
            _deck.RemoveFromHand(card);
            Energy -= card.Data.EnergyCost;
            return true;
        }
        return false;
    }

    public bool RemoveCardFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _currentSpread.SlotCount)
            return false;
        var slot = _currentSpread.Slots[slotIndex];
        if (slot.PlacedCard is { } card)
        {
            Energy += card.Data.EnergyCost;
            slot.RemoveCard();
            _deck.ReturnToHand(card);
            return true;
        }
        return false;
    }

    public ActivationResult ActivateSpread()
    {
        var result = _currentSpread.Activate();

        // 예언 적중 판정
        var prophecyResult = _prophecy.Evaluate(_currentSpread, result);

        int finalDamage = result.TotalDamage;
        int finalBlock = result.TotalBlock;
        if (prophecyResult.IsHit)
        {
            finalDamage = (int)(finalDamage * prophecyResult.Multiplier);
            OnProphecyHit?.Invoke(prophecyResult);
        }

        // 운명 선택지 생성
        var fateChoice = FateChoice.Generate(result.SuitCounts);

        // Heal 처리
        int totalHeal = 0;
        foreach (var slot in _currentSpread.Slots)
        {
            if (slot.PlacedCard is { } card)
                totalHeal += card.GetHeal();
        }

        PlayerBlock += finalBlock;
        if (totalHeal > 0)
            PlayerHp = Math.Min(PlayerMaxHp, PlayerHp + totalHeal);

        // 추가 드로우 (다음 턴에 적용)
        if (result.TotalDraw > 0)
            BonusDraw += result.TotalDraw;

        // 배치된 카드를 버림패로
        foreach (var slot in _currentSpread.Slots)
        {
            if (slot.RemoveCard() is { } card)
                _deck.Discard(card);
        }

        return new ActivationResult(result, prophecyResult, fateChoice, finalDamage, finalBlock);
    }

    public void ApplyFateOption(FateOption option, ActivationResult activation)
    {
        int fateDamage = (int)(activation.FinalDamage * option.DamageMultiplier);
        int fateBlock = (int)(activation.FinalBlock * option.BlockMultiplier) - activation.FinalBlock;

        // 적에게 데미지
        if (fateDamage > 0)
            Enemy.TakeDamage(fateDamage);

        // 추가 블록
        if (fateBlock > 0)
            PlayerBlock += fateBlock;

        // 추가 드로우 (다음 턴에 적용)
        if (option.BonusDraw > 0)
            BonusDraw += option.BonusDraw;

        // 회복
        if (option.BonusHeal > 0)
            PlayerHp = Math.Min(PlayerMaxHp, PlayerHp + option.BonusHeal);

        // 다음턴 에너지 보너스
        if (option.BonusEnergy > 0)
            BonusEnergy += option.BonusEnergy;

        OnFateChosen?.Invoke(option);
    }

    public void ExecuteEnemyTurn()
    {
        if (Enemy.IsDead) return;

        if (Enemy.CurrentIntent == IntentType.Attack)
        {
            TakeDamage(Enemy.CurrentIntentValue);
        }
        Enemy.ExecuteTurn();

        OnEnemyActed?.Invoke(Enemy);
    }

    public void EndTurn()
    {
        _deck.DiscardAllHand();
    }

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
    public event Action<FateOption>? OnFateChosen;
    public event Action<Enemy>? OnEnemyActed;
}

public record ActivationResult(
    SpreadResult SpreadResult,
    ProphecyResult ProphecyResult,
    FateChoice FateChoice,
    int FinalDamage,
    int FinalBlock
);
