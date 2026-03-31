# Phase 1 전투 프로토타입 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 카드 12장 + 적 1종으로 "스프레드 배치 → 운명 선택 → 예언 적중" 전투 한 판이 동작하는 Godot 4 프로토타입을 만든다.

**Architecture:** 기존 로직 클래스(BattleManager, Deck, Spread, ProphecyEngine)를 유지하면서, Enemy/FateChoice 로직을 추가하고, Godot UI 노드(CardUI, SlotUI, BattleUI)가 로직 클래스를 호출하는 구조. UI와 로직의 분리를 유지한다.

**Tech Stack:** Godot 4.6.1 / C# .NET 8 / Godot.NET.Sdk 4.6.1

---

## 파일 구조

### 새로 생성

| 파일 | 역할 |
|------|------|
| `Scripts/Combat/Enemy.cs` | 적 HP, 인텐트, 행동 패턴 (3턴 주기) |
| `Scripts/Combat/FateChoice.cs` | 수트 비율 기반 운명 선택지 2개 생성 |
| `Scripts/Data/CardDatabase.cs` | 12장 샘플 카드를 코드로 생성 |
| `Scripts/UI/CardUI.cs` | 카드 비주얼 노드 (Panel + Labels + 클릭) |
| `Scripts/UI/SlotUI.cs` | 스프레드 슬롯 비주얼 (카드 수신 표시) |
| `Scripts/UI/FateChoiceUI.cs` | 운명 선택지 2개 팝업 |
| `Scripts/UI/BattleUI.cs` | 전투 화면 전체 컨트롤러 (Main Node) |
| `Scenes/Battle.tscn` | 전투 씬 (BattleUI가 루트) |

### 기존 수정

| 파일 | 변경 |
|------|------|
| `Scripts/Cards/CardData.cs` | Heal 필드 추가 |
| `Scripts/Cards/Card.cs` | GetHeal() 메서드 추가 |
| `Scripts/Combat/BattleManager.cs` | Enemy 통합, FateChoice 통합, Heal 처리, 카드 회수 로직 수정 |
| `Scenes/Main.tscn` | Battle 씬으로 교체 |

---

## Task 1: CardData에 Heal 필드 추가

**Files:**
- Modify: `Scripts/Cards/CardData.cs`
- Modify: `Scripts/Cards/Card.cs`

- [ ] **Step 1: CardData에 Heal/ReversedHeal 추가**

```csharp
// CardData.cs — 역방향 효과 블록 아래에 추가
[Export] public int Heal { get; set; }
[Export] public int ReversedHeal { get; set; }
```

- [ ] **Step 2: Card에 GetHeal() 추가**

```csharp
// Card.cs — GetDraw() 메서드 아래에 추가
public int GetHeal() => IsReversed ? Data.ReversedHeal : Data.Heal;
```

- [ ] **Step 3: 빌드 확인**

Run: `cd C:/Users/ldd82/Sortis && C:/Users/ldd82/.dotnet/dotnet.exe build Sortis.sln`
Expected: 빌드 성공, 오류 0개

- [ ] **Step 4: 커밋**

```bash
cd C:/Users/ldd82/Sortis
git add Scripts/Cards/CardData.cs Scripts/Cards/Card.cs
git commit -m "feat: CardData에 Heal/ReversedHeal 필드 추가"
```

---

## Task 2: Enemy 시스템

**Files:**
- Create: `Scripts/Combat/Enemy.cs`

- [ ] **Step 1: Enemy.cs 작성**

```csharp
using System;

namespace Sortis.Combat;

public enum IntentType
{
    Attack,
    Defend
}

public class Enemy
{
    public string Name { get; }
    public int Hp { get; private set; }
    public int MaxHp { get; }
    public int Block { get; private set; }
    public IntentType CurrentIntent { get; private set; }
    public int CurrentIntentValue { get; private set; }
    public bool IsDead => Hp <= 0;

    private int _turnIndex;
    private readonly (IntentType type, int value)[] _pattern;

    public Enemy(string name, int maxHp, (IntentType type, int value)[] pattern)
    {
        Name = name;
        MaxHp = maxHp;
        Hp = maxHp;
        _pattern = pattern;
        _turnIndex = 0;
        UpdateIntent();
    }

    public static Enemy ShadowKnight() => new("Shadow Knight", 40, new[]
    {
        (IntentType.Attack, 12),
        (IntentType.Attack, 8),
        (IntentType.Defend, 6)
    });

    public void TakeDamage(int damage)
    {
        int remaining = damage - Block;
        Block = Math.Max(0, Block - damage);
        if (remaining > 0)
            Hp = Math.Max(0, Hp - remaining);
    }

    public int ExecuteIntent()
    {
        int value = CurrentIntentValue;
        if (CurrentIntent == IntentType.Defend)
            Block += value;

        _turnIndex = (_turnIndex + 1) % _pattern.Length;
        UpdateIntent();

        return CurrentIntent == IntentType.Attack ? 0 : value;
        // Attack인 경우 BattleManager가 플레이어에게 데미지 적용
    }

    public int GetAttackDamage()
    {
        // 현재 인텐트가 Attack이면 그 값을 반환 (ExecuteIntent 전에 호출)
        return CurrentIntent == IntentType.Attack ? CurrentIntentValue : 0;
    }

    public void ExecuteTurn()
    {
        if (CurrentIntent == IntentType.Defend)
            Block += CurrentIntentValue;

        _turnIndex = (_turnIndex + 1) % _pattern.Length;
        UpdateIntent();
    }

    private void UpdateIntent()
    {
        var current = _pattern[_turnIndex];
        CurrentIntent = current.type;
        CurrentIntentValue = current.value;
    }
}
```

- [ ] **Step 2: 빌드 확인**

Run: `cd C:/Users/ldd82/Sortis && C:/Users/ldd82/.dotnet/dotnet.exe build Sortis.sln`
Expected: 빌드 성공

- [ ] **Step 3: 커밋**

```bash
cd C:/Users/ldd82/Sortis
git add Scripts/Combat/Enemy.cs
git commit -m "feat: Enemy 클래스 추가 (Shadow Knight 패턴 포함)"
```

---

## Task 3: FateChoice 시스템

**Files:**
- Create: `Scripts/Combat/FateChoice.cs`

- [ ] **Step 1: FateChoice.cs 작성**

```csharp
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
                new FateOption("Fury of Flame", "Damage x1.5", damageMultiplier: 1.5f),
                new FateOption("Lingering Burn", "Damage x1 + Burn 3 next turn", burnDamage: 3),
                dominant),

            Suit.Swords => new FateChoice(
                new FateOption("Insight", "Draw 2 extra cards", bonusDraw: 2),
                new FateOption("Expose Weakness", "Weaken enemy (-25% next attack)", applyWeaken: true),
                dominant),

            Suit.Cups => new FateChoice(
                new FateOption("Fortify", "Block x2", blockMultiplier: 2f),
                new FateOption("Renewal", "Block x1 + Heal 3", bonusHeal: 3),
                dominant),

            Suit.Pentacles => new FateChoice(
                new FateOption("Energize", "Next turn Energy +1", bonusEnergy: 1),
                new FateOption("Harvest", "+10 Gold (not implemented yet)", bonusDraw: 1),
                dominant),

            _ => new FateChoice(
                new FateOption("Balanced Fate", "All effects +20%",
                    damageMultiplier: 1.2f, blockMultiplier: 1.2f),
                new FateOption("Foresight", "Draw 1 extra card", bonusDraw: 1),
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
```

- [ ] **Step 2: 빌드 확인**

Run: `cd C:/Users/ldd82/Sortis && C:/Users/ldd82/.dotnet/dotnet.exe build Sortis.sln`
Expected: 빌드 성공

- [ ] **Step 3: 커밋**

```bash
cd C:/Users/ldd82/Sortis
git add Scripts/Combat/FateChoice.cs
git commit -m "feat: FateChoice 운명 선택지 시스템 (수트 비율 기반)"
```

---

## Task 4: CardDatabase — 12장 샘플 카드

**Files:**
- Create: `Scripts/Data/CardDatabase.cs`

- [ ] **Step 1: CardDatabase.cs 작성**

```csharp
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
```

- [ ] **Step 2: 빌드 확인**

Run: `cd C:/Users/ldd82/Sortis && C:/Users/ldd82/.dotnet/dotnet.exe build Sortis.sln`
Expected: 빌드 성공

- [ ] **Step 3: 커밋**

```bash
cd C:/Users/ldd82/Sortis
git add Scripts/Data/CardDatabase.cs
git commit -m "feat: CardDatabase 12장 스타터 덱 데이터"
```

---

## Task 5: BattleManager 업데이트 — Enemy + FateChoice 통합

**Files:**
- Modify: `Scripts/Combat/BattleManager.cs`

- [ ] **Step 1: BattleManager에 Enemy, FateChoice, Heal 통합**

BattleManager.cs 전체를 아래로 교체:

```csharp
using System;
using System.Collections.Generic;
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

        _deck.Draw(DrawPerTurn);

        var prophecy = _prophecy.GenerateNewProphecy();
        OnProphecyRevealed?.Invoke(prophecy);
    }

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

    public bool RemoveCardFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _currentSpread.SlotCount)
            return false;
        var slot = _currentSpread.Slots[slotIndex];
        if (slot.PlacedCard is { } card)
        {
            Energy += card.Data.EnergyCost;
            slot.RemoveCard();
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

        // 추가 드로우
        if (result.TotalDraw > 0)
            _deck.Draw(result.TotalDraw);

        // 배치된 카드를 버림패로
        foreach (var slot in _currentSpread.Slots)
        {
            if (slot.RemoveCard() is { } card)
                _deck.DiscardFromHand(card);
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

        // 추가 드로우
        if (option.BonusDraw > 0)
            _deck.Draw(option.BonusDraw);

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
```

- [ ] **Step 2: 빌드 확인**

Run: `cd C:/Users/ldd82/Sortis && C:/Users/ldd82/.dotnet/dotnet.exe build Sortis.sln`
Expected: 빌드 성공

- [ ] **Step 3: 커밋**

```bash
cd C:/Users/ldd82/Sortis
git add Scripts/Combat/BattleManager.cs
git commit -m "feat: BattleManager에 Enemy, FateChoice, Heal 통합"
```

---

## Task 6: CardUI — 카드 비주얼 노드

**Files:**
- Create: `Scripts/UI/CardUI.cs`

- [ ] **Step 1: CardUI.cs 작성**

```csharp
using Godot;
using Sortis.Cards;
using Sortis.Core;
using System;

namespace Sortis.UI;

public partial class CardUI : Panel
{
    private Card? _card;
    private Label _nameLabel = null!;
    private Label _costLabel = null!;
    private Label _statsLabel = null!;
    private bool _isSelected;

    [Signal] public delegate void CardClickedEventHandler(CardUI cardUI);

    public Card? Card => _card;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            QueueRedraw();
        }
    }

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(120, 160);

        _nameLabel = new Label { Name = "NameLabel" };
        _costLabel = new Label { Name = "CostLabel" };
        _statsLabel = new Label { Name = "StatsLabel" };

        _nameLabel.Position = new Vector2(8, 30);
        _nameLabel.Size = new Vector2(104, 25);
        _nameLabel.HorizontalAlignment = HorizontalAlignment.Center;

        _costLabel.Position = new Vector2(4, 4);
        _costLabel.Size = new Vector2(24, 24);
        _costLabel.HorizontalAlignment = HorizontalAlignment.Center;

        _statsLabel.Position = new Vector2(8, 100);
        _statsLabel.Size = new Vector2(104, 50);
        _statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _statsLabel.AutowrapMode = TextServer.AutowrapMode.Word;

        AddChild(_nameLabel);
        AddChild(_costLabel);
        AddChild(_statsLabel);

        GuiInput += OnGuiInput;
    }

    public void SetCard(Card card)
    {
        _card = card;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (_card == null) return;

        _nameLabel.Text = _card.Data.CardName;
        _costLabel.Text = _card.Data.EnergyCost.ToString();

        string stats = "";
        int dmg = _card.GetDamage();
        int blk = _card.GetBlock();
        int drw = _card.GetDraw();
        int heal = _card.GetHeal();

        if (dmg > 0) stats += $"DMG {dmg}\n";
        if (blk > 0) stats += $"BLK {blk}\n";
        if (drw > 0) stats += $"DRW {drw}\n";
        if (heal > 0) stats += $"HEAL {heal}";
        _statsLabel.Text = stats.TrimEnd();

        // 수트별 배경색
        var style = new StyleBoxFlat();
        style.BgColor = GetSuitColor(_card.Data.Suit);
        style.CornerRadiusTopLeft = 8;
        style.CornerRadiusTopRight = 8;
        style.CornerRadiusBottomLeft = 8;
        style.CornerRadiusBottomRight = 8;

        if (_card.IsReversed)
        {
            style.BorderColor = new Color("#8E44AD");
            style.BorderWidthTop = 3;
            style.BorderWidthBottom = 3;
            style.BorderWidthLeft = 3;
            style.BorderWidthRight = 3;
            RotationDegrees = 180;
        }
        else
        {
            RotationDegrees = 0;
        }

        AddThemeStyleboxOverride("panel", style);
    }

    public override void _Draw()
    {
        if (_isSelected)
        {
            var rect = new Rect2(Vector2.Zero, Size);
            DrawRect(rect, new Color("#FFD700"), false, 3);
        }
    }

    private void OnGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            EmitSignal(SignalName.CardClicked, this);
        }
    }

    private static Color GetSuitColor(Suit suit) => suit switch
    {
        Suit.Wands => new Color("#C0392B"),
        Suit.Swords => new Color("#2980B9"),
        Suit.Cups => new Color("#27AE60"),
        Suit.Pentacles => new Color("#F39C12"),
        _ => new Color("#555555")
    };
}
```

- [ ] **Step 2: 빌드 확인**

Run: `cd C:/Users/ldd82/Sortis && C:/Users/ldd82/.dotnet/dotnet.exe build Sortis.sln`
Expected: 빌드 성공

- [ ] **Step 3: 커밋**

```bash
cd C:/Users/ldd82/Sortis
git add Scripts/UI/CardUI.cs
git commit -m "feat: CardUI 카드 비주얼 노드 (수트 색상, 클릭 이벤트)"
```

---

## Task 7: SlotUI — 스프레드 슬롯 비주얼

**Files:**
- Create: `Scripts/UI/SlotUI.cs`

- [ ] **Step 1: SlotUI.cs 작성**

```csharp
using Godot;
using Sortis.Cards;
using Sortis.Core;
using System;

namespace Sortis.UI;

public partial class SlotUI : Panel
{
    private Label _positionLabel = null!;
    private Label _cardInfoLabel = null!;
    private int _slotIndex;
    private bool _hasCard;

    [Signal] public delegate void SlotClickedEventHandler(int slotIndex);

    public int SlotIndex => _slotIndex;
    public bool HasCard => _hasCard;

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(130, 170);

        _positionLabel = new Label { Name = "PositionLabel" };
        _positionLabel.Position = new Vector2(0, 4);
        _positionLabel.Size = new Vector2(130, 25);
        _positionLabel.HorizontalAlignment = HorizontalAlignment.Center;

        _cardInfoLabel = new Label { Name = "CardInfoLabel" };
        _cardInfoLabel.Position = new Vector2(8, 60);
        _cardInfoLabel.Size = new Vector2(114, 80);
        _cardInfoLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _cardInfoLabel.AutowrapMode = TextServer.AutowrapMode.Word;

        AddChild(_positionLabel);
        AddChild(_cardInfoLabel);

        var style = new StyleBoxFlat();
        style.BgColor = new Color("#2C2C2C");
        style.BorderColor = new Color("#666666");
        style.BorderWidthTop = 2;
        style.BorderWidthBottom = 2;
        style.BorderWidthLeft = 2;
        style.BorderWidthRight = 2;
        style.CornerRadiusTopLeft = 8;
        style.CornerRadiusTopRight = 8;
        style.CornerRadiusBottomLeft = 8;
        style.CornerRadiusBottomRight = 8;
        AddThemeStyleboxOverride("panel", style);

        GuiInput += OnGuiInput;
    }

    public void Setup(int index, SlotPosition position)
    {
        _slotIndex = index;
        _positionLabel.Text = position switch
        {
            SlotPosition.Past => "Past",
            SlotPosition.Present => "Present",
            SlotPosition.Future => "Future",
            _ => position.ToString()
        };
        _cardInfoLabel.Text = "(empty)";
        _hasCard = false;
    }

    public void ShowCard(Card card)
    {
        _hasCard = true;
        string info = card.Data.CardName;
        if (card.IsReversed) info += " (R)";
        info += "\n";
        if (card.GetDamage() > 0) info += $"DMG {card.GetDamage()} ";
        if (card.GetBlock() > 0) info += $"BLK {card.GetBlock()} ";
        if (card.GetDraw() > 0) info += $"DRW {card.GetDraw()}";
        _cardInfoLabel.Text = info;

        var style = new StyleBoxFlat();
        style.BgColor = new Color("#3D3D5C");
        style.BorderColor = new Color("#9B59B6");
        style.BorderWidthTop = 2;
        style.BorderWidthBottom = 2;
        style.BorderWidthLeft = 2;
        style.BorderWidthRight = 2;
        style.CornerRadiusTopLeft = 8;
        style.CornerRadiusTopRight = 8;
        style.CornerRadiusBottomLeft = 8;
        style.CornerRadiusBottomRight = 8;
        AddThemeStyleboxOverride("panel", style);
    }

    public void ClearCard()
    {
        _hasCard = false;
        _cardInfoLabel.Text = "(empty)";

        var style = new StyleBoxFlat();
        style.BgColor = new Color("#2C2C2C");
        style.BorderColor = new Color("#666666");
        style.BorderWidthTop = 2;
        style.BorderWidthBottom = 2;
        style.BorderWidthLeft = 2;
        style.BorderWidthRight = 2;
        style.CornerRadiusTopLeft = 8;
        style.CornerRadiusTopRight = 8;
        style.CornerRadiusBottomLeft = 8;
        style.CornerRadiusBottomRight = 8;
        AddThemeStyleboxOverride("panel", style);
    }

    private void OnGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            EmitSignal(SignalName.SlotClicked, _slotIndex);
        }
    }
}
```

- [ ] **Step 2: 빌드 확인**

Run: `cd C:/Users/ldd82/Sortis && C:/Users/ldd82/.dotnet/dotnet.exe build Sortis.sln`
Expected: 빌드 성공

- [ ] **Step 3: 커밋**

```bash
cd C:/Users/ldd82/Sortis
git add Scripts/UI/SlotUI.cs
git commit -m "feat: SlotUI 스프레드 슬롯 비주얼"
```

---

## Task 8: FateChoiceUI — 운명 선택지 팝업

**Files:**
- Create: `Scripts/UI/FateChoiceUI.cs`

- [ ] **Step 1: FateChoiceUI.cs 작성**

```csharp
using Godot;
using Sortis.Combat;
using System;

namespace Sortis.UI;

public partial class FateChoiceUI : PanelContainer
{
    private HBoxContainer _container = null!;
    private FateChoice? _fateChoice;

    [Signal] public delegate void FateSelectedEventHandler(int optionIndex);

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(500, 200);
        Visible = false;

        _container = new HBoxContainer { Name = "Container" };
        _container.AddThemeConstantOverride("separation", 20);
        AddChild(_container);
    }

    public void ShowChoices(FateChoice fateChoice)
    {
        _fateChoice = fateChoice;

        foreach (var child in _container.GetChildren())
            child.QueueFree();

        AddOptionButton(fateChoice.OptionA, 0);
        AddOptionButton(fateChoice.OptionB, 1);

        Visible = true;
    }

    public void Hide()
    {
        Visible = false;
    }

    private void AddOptionButton(FateOption option, int index)
    {
        var panel = new PanelContainer();
        panel.CustomMinimumSize = new Vector2(220, 160);

        var style = new StyleBoxFlat();
        style.BgColor = new Color("#1A1A2E");
        style.BorderColor = new Color("#D4AF37");
        style.BorderWidthTop = 2;
        style.BorderWidthBottom = 2;
        style.BorderWidthLeft = 2;
        style.BorderWidthRight = 2;
        style.CornerRadiusTopLeft = 10;
        style.CornerRadiusTopRight = 10;
        style.CornerRadiusBottomLeft = 10;
        style.CornerRadiusBottomRight = 10;
        style.ContentMarginLeft = 12;
        style.ContentMarginRight = 12;
        style.ContentMarginTop = 12;
        style.ContentMarginBottom = 12;
        panel.AddThemeStyleboxOverride("panel", style);

        var vbox = new VBoxContainer();

        var title = new Label();
        title.Text = option.Name;
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeFontSizeOverride("font_size", 18);

        var desc = new Label();
        desc.Text = option.Description;
        desc.HorizontalAlignment = HorizontalAlignment.Center;
        desc.AutowrapMode = TextServer.AutowrapMode.Word;

        var button = new Button();
        button.Text = "Choose";
        int capturedIndex = index;
        button.Pressed += () => OnOptionSelected(capturedIndex);

        vbox.AddChild(title);
        vbox.AddChild(desc);
        vbox.AddChild(button);
        panel.AddChild(vbox);
        _container.AddChild(panel);
    }

    private void OnOptionSelected(int index)
    {
        EmitSignal(SignalName.FateSelected, index);
    }
}
```

- [ ] **Step 2: 빌드 확인**

Run: `cd C:/Users/ldd82/Sortis && C:/Users/ldd82/.dotnet/dotnet.exe build Sortis.sln`
Expected: 빌드 성공

- [ ] **Step 3: 커밋**

```bash
cd C:/Users/ldd82/Sortis
git add Scripts/UI/FateChoiceUI.cs
git commit -m "feat: FateChoiceUI 운명 선택지 팝업"
```

---

## Task 9: BattleUI — 전투 화면 전체 컨트롤러

**Files:**
- Create: `Scripts/UI/BattleUI.cs`

이것이 전투 화면의 메인 노드. 모든 UI 요소를 코드로 생성하고, BattleManager와 연결한다.

- [ ] **Step 1: BattleUI.cs 작성**

```csharp
using Godot;
using System;
using System.Collections.Generic;
using Sortis.Cards;
using Sortis.Combat;
using Sortis.Core;
using Sortis.Data;
using Sortis.Prophecy;

namespace Sortis.UI;

public partial class BattleUI : Control
{
    private BattleManager _battle = null!;
    private readonly List<CardUI> _cardUIs = new();
    private readonly List<SlotUI> _slotUIs = new();
    private Card? _selectedCard;

    // UI 요소들
    private Label _enemyInfoLabel = null!;
    private Label _prophecyLabel = null!;
    private Label _playerInfoLabel = null!;
    private Label _logLabel = null!;
    private HBoxContainer _handContainer = null!;
    private HBoxContainer _slotContainer = null!;
    private Button _activateButton = null!;
    private Button _endTurnButton = null!;
    private FateChoiceUI _fateChoiceUI = null!;
    private Label _resultLabel = null!;
    private Button _restartButton = null!;
    private ActivationResult? _pendingActivation;

    public override void _Ready()
    {
        BuildLayout();
        StartNewBattle();
    }

    private void StartNewBattle()
    {
        // 덱 생성
        var deck = new Deck();
        var cardDatas = CardDatabase.CreateStarterDeck();
        var cards = new List<Card>();
        var rng = new Random();
        foreach (var data in cardDatas)
        {
            // 20% 확률로 역방향
            var orientation = rng.NextDouble() < 0.2
                ? CardOrientation.Reversed
                : CardOrientation.Upright;
            cards.Add(new Card(data, orientation));
        }
        deck.Initialize(cards);

        // 적 생성
        var enemy = Enemy.ShadowKnight();

        // 전투 시작
        _battle = new BattleManager(deck, enemy);
        _battle.OnProphecyRevealed += OnProphecyRevealed;
        _battle.OnProphecyHit += OnProphecyHit;
        _battle.OnFateChosen += OnFateChosen;

        _resultLabel.Visible = false;
        _restartButton.Visible = false;
        _activateButton.Disabled = false;
        _endTurnButton.Disabled = false;

        _battle.StartBattle();
        RefreshUI();
        Log("Battle Start! Shadow Knight appears.");
    }

    // --- UI 구축 ---

    private void BuildLayout()
    {
        var bg = new ColorRect();
        bg.Color = new Color("#1A1A2E");
        bg.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(bg);

        var mainVBox = new VBoxContainer();
        mainVBox.SetAnchorsPreset(LayoutPreset.FullRect);
        mainVBox.AddThemeConstantOverride("separation", 8);

        // 상단: 적 정보
        _enemyInfoLabel = new Label();
        _enemyInfoLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _enemyInfoLabel.AddThemeFontSizeOverride("font_size", 20);
        mainVBox.AddChild(_enemyInfoLabel);

        // 예언 표시
        _prophecyLabel = new Label();
        _prophecyLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _prophecyLabel.AddThemeFontSizeOverride("font_size", 16);
        _prophecyLabel.AddThemeColorOverride("font_color", new Color("#D4AF37"));
        mainVBox.AddChild(_prophecyLabel);

        // 중간: 스프레드 슬롯
        var slotCenter = new CenterContainer();
        _slotContainer = new HBoxContainer();
        _slotContainer.AddThemeConstantOverride("separation", 15);
        for (int i = 0; i < 3; i++)
        {
            var slot = new SlotUI();
            _slotUIs.Add(slot);
            _slotContainer.AddChild(slot);
        }
        slotCenter.AddChild(_slotContainer);
        mainVBox.AddChild(slotCenter);

        // 버튼 행
        var buttonRow = new CenterContainer();
        var buttonHBox = new HBoxContainer();
        buttonHBox.AddThemeConstantOverride("separation", 30);

        _activateButton = new Button { Text = "Activate Spread" };
        _activateButton.CustomMinimumSize = new Vector2(160, 40);
        _activateButton.Pressed += OnActivatePressed;

        _endTurnButton = new Button { Text = "End Turn" };
        _endTurnButton.CustomMinimumSize = new Vector2(120, 40);
        _endTurnButton.Pressed += OnEndTurnPressed;

        buttonHBox.AddChild(_activateButton);
        buttonHBox.AddChild(_endTurnButton);
        buttonRow.AddChild(buttonHBox);
        mainVBox.AddChild(buttonRow);

        // 로그
        _logLabel = new Label();
        _logLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _logLabel.CustomMinimumSize = new Vector2(0, 40);
        _logLabel.AddThemeColorOverride("font_color", new Color("#AAAAAA"));
        mainVBox.AddChild(_logLabel);

        // 하단: 플레이어 정보
        _playerInfoLabel = new Label();
        _playerInfoLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _playerInfoLabel.AddThemeFontSizeOverride("font_size", 18);
        mainVBox.AddChild(_playerInfoLabel);

        // 핸드
        var handScroll = new ScrollContainer();
        handScroll.CustomMinimumSize = new Vector2(0, 180);
        handScroll.VerticalScrollMode = ScrollContainer.ScrollMode.Disabled;
        _handContainer = new HBoxContainer();
        _handContainer.AddThemeConstantOverride("separation", 8);
        handScroll.AddChild(_handContainer);
        mainVBox.AddChild(handScroll);

        AddChild(mainVBox);

        // 운명 선택지 (오버레이)
        _fateChoiceUI = new FateChoiceUI();
        _fateChoiceUI.SetAnchorsPreset(LayoutPreset.Center);
        _fateChoiceUI.FateSelected += OnFateSelected;
        AddChild(_fateChoiceUI);

        // 결과 표시
        _resultLabel = new Label();
        _resultLabel.SetAnchorsPreset(LayoutPreset.Center);
        _resultLabel.AddThemeFontSizeOverride("font_size", 32);
        _resultLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _resultLabel.Visible = false;
        AddChild(_resultLabel);

        _restartButton = new Button { Text = "Restart" };
        _restartButton.SetAnchorsPreset(LayoutPreset.Center);
        _restartButton.Position = new Vector2(-50, 40);
        _restartButton.CustomMinimumSize = new Vector2(100, 40);
        _restartButton.Visible = false;
        _restartButton.Pressed += () => StartNewBattle();
        AddChild(_restartButton);
    }

    // --- 이벤트 핸들러 ---

    private void OnCardClicked(CardUI cardUI)
    {
        if (_battle.IsBattleOver) return;

        // 같은 카드 다시 클릭 → 선택 해제
        if (_selectedCard == cardUI.Card)
        {
            _selectedCard = null;
            foreach (var c in _cardUIs) c.IsSelected = false;
            return;
        }

        _selectedCard = cardUI.Card;
        foreach (var c in _cardUIs) c.IsSelected = c == cardUI;
    }

    private void OnSlotClicked(int slotIndex)
    {
        if (_battle.IsBattleOver) return;

        // 슬롯에 카드가 있으면 회수
        if (_slotUIs[slotIndex].HasCard)
        {
            _battle.RemoveCardFromSlot(slotIndex);
            _slotUIs[slotIndex].ClearCard();
            RefreshUI();
            return;
        }

        // 선택된 카드가 있으면 배치
        if (_selectedCard == null) return;

        if (_battle.PlaceCard(_selectedCard, slotIndex))
        {
            _slotUIs[slotIndex].ShowCard(_selectedCard);
            _selectedCard = null;
            RefreshUI();
        }
        else
        {
            Log("Not enough energy!");
        }
    }

    private void OnActivatePressed()
    {
        // 배치된 카드가 없으면 무시
        bool hasAny = false;
        foreach (var slot in _slotUIs)
            if (slot.HasCard) { hasAny = true; break; }
        if (!hasAny)
        {
            Log("Place at least one card first!");
            return;
        }

        _pendingActivation = _battle.ActivateSpread();
        var r = _pendingActivation;

        string logMsg = $"Spread activated! DMG:{r.FinalDamage} BLK:{r.FinalBlock}";
        if (r.ProphecyResult.IsHit)
            logMsg += $" | PROPHECY HIT! x{r.ProphecyResult.Multiplier:F1}";
        Log(logMsg);

        // 운명 선택지 표시
        _fateChoiceUI.ShowChoices(r.FateChoice);
        _activateButton.Disabled = true;
        _endTurnButton.Disabled = true;

        RefreshUI();
    }

    private void OnFateSelected(int optionIndex)
    {
        if (_pendingActivation == null) return;

        var option = optionIndex == 0
            ? _pendingActivation.FateChoice.OptionA
            : _pendingActivation.FateChoice.OptionB;

        _battle.ApplyFateOption(option, _pendingActivation);
        _fateChoiceUI.Hide();

        Log($"Fate chosen: {option.Name}");

        // 적 턴
        if (!_battle.IsEnemyDead)
        {
            _battle.ExecuteEnemyTurn();
        }

        // 전투 종료 체크
        if (_battle.IsBattleOver)
        {
            ShowBattleResult();
        }
        else
        {
            // 턴 종료 → 다음 턴
            _battle.EndTurn();
            _battle.StartTurn();
            _activateButton.Disabled = false;
            _endTurnButton.Disabled = false;
        }

        _pendingActivation = null;
        RefreshUI();
    }

    private void OnEndTurnPressed()
    {
        if (_battle.IsBattleOver) return;

        // 슬롯 초기화
        foreach (var slot in _slotUIs) slot.ClearCard();

        // 적 턴
        _battle.ExecuteEnemyTurn();

        if (_battle.IsBattleOver)
        {
            ShowBattleResult();
            RefreshUI();
            return;
        }

        _battle.EndTurn();
        _battle.StartTurn();

        Log("Turn ended. Enemy attacks!");
        RefreshUI();
    }

    private void OnProphecyRevealed(ProphecyCondition condition)
    {
        _prophecyLabel.Text = $"Prophecy: {condition.Description}";
    }

    private void OnProphecyHit(ProphecyResult result)
    {
        _prophecyLabel.Text += $"  >>> HIT! x{result.Multiplier:F1}";
    }

    private void OnFateChosen(FateOption option) { }

    // --- UI 갱신 ---

    private void RefreshUI()
    {
        // 적 정보
        var e = _battle.Enemy;
        string intentIcon = e.CurrentIntent == IntentType.Attack ? "ATK" : "DEF";
        _enemyInfoLabel.Text = $"{e.Name}  HP: {e.Hp}/{e.MaxHp}  BLK: {e.Block}  Intent: {intentIcon} {e.CurrentIntentValue}";

        // 플레이어 정보
        _playerInfoLabel.Text = $"HP: {_battle.PlayerHp}/{_battle.PlayerMaxHp}   Energy: {_battle.Energy}/{_battle.MaxEnergy}   Block: {_battle.PlayerBlock}   Streak: {_battle.ConsecutiveHits}";

        // 슬롯 초기화
        for (int i = 0; i < _slotUIs.Count; i++)
        {
            _slotUIs[i].Setup(i, _battle.CurrentSpread.Slots[i].Position);
            if (_battle.CurrentSpread.Slots[i].PlacedCard is { } placedCard)
                _slotUIs[i].ShowCard(placedCard);
        }

        // 핸드 갱신
        foreach (var child in _handContainer.GetChildren())
            child.QueueFree();
        _cardUIs.Clear();
        _selectedCard = null;

        foreach (var card in _battle.Deck.Hand)
        {
            var cardUI = new CardUI();
            _handContainer.AddChild(cardUI);
            cardUI.SetCard(card);
            cardUI.CardClicked += OnCardClicked;
            _cardUIs.Add(cardUI);
        }

        // 슬롯 클릭 재연결
        foreach (var slot in _slotUIs)
        {
            // 이전 연결 해제 후 재연결
            if (!slot.IsConnected(SlotUI.SignalName.SlotClicked, new Callable(this, nameof(OnSlotClicked))))
                slot.SlotClicked += OnSlotClicked;
        }
    }

    private void ShowBattleResult()
    {
        _activateButton.Disabled = true;
        _endTurnButton.Disabled = true;

        if (_battle.IsEnemyDead)
        {
            _resultLabel.Text = "VICTORY!";
            _resultLabel.AddThemeColorOverride("font_color", new Color("#D4AF37"));
        }
        else
        {
            _resultLabel.Text = "DEFEAT...";
            _resultLabel.AddThemeColorOverride("font_color", new Color("#C0392B"));
        }
        _resultLabel.Visible = true;
        _restartButton.Visible = true;
    }

    private void Log(string message)
    {
        _logLabel.Text = message;
    }
}
```

- [ ] **Step 2: 빌드 확인**

Run: `cd C:/Users/ldd82/Sortis && C:/Users/ldd82/.dotnet/dotnet.exe build Sortis.sln`
Expected: 빌드 성공

- [ ] **Step 3: 커밋**

```bash
cd C:/Users/ldd82/Sortis
git add Scripts/UI/BattleUI.cs
git commit -m "feat: BattleUI 전투 화면 전체 컨트롤러"
```

---

## Task 10: Battle 씬 + Main 씬 연결

**Files:**
- Create: `Scenes/Battle.tscn`
- Modify: `Scenes/Main.tscn` → 삭제 후 Battle.tscn을 메인으로 등록
- Modify: `project.godot`

- [ ] **Step 1: Battle.tscn 생성**

```
[gd_scene load_steps=2 format=3]

[ext_resource type="Script" path="res://Scripts/UI/BattleUI.cs" id="1"]

[node name="BattleUI" type="Control"]
layout_mode = 3
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
grow_horizontal = 2
grow_vertical = 2
script = ExtResource("1")
```

- [ ] **Step 2: project.godot의 main_scene을 Battle.tscn으로 변경**

`run/main_scene="res://Scenes/Battle.tscn"`

- [ ] **Step 3: 기존 Main.tscn 삭제**

```bash
rm C:/Users/ldd82/Sortis/Scenes/Main.tscn
```

- [ ] **Step 4: 빌드 + 실행 확인**

Run: `cd C:/Users/ldd82/Sortis && C:/Users/ldd82/.dotnet/dotnet.exe build Sortis.sln`
Expected: 빌드 성공

Godot 에디터에서 F5로 실행하여 전투 화면이 뜨는지 확인.

- [ ] **Step 5: 커밋**

```bash
cd C:/Users/ldd82/Sortis
git add Scenes/Battle.tscn project.godot
git rm Scenes/Main.tscn
git commit -m "feat: Battle 씬 생성 및 메인 씬으로 등록"
```

---

## 실행 순서 요약

| Task | 내용 | 의존성 |
|------|------|--------|
| 1 | CardData Heal 필드 | 없음 |
| 2 | Enemy 시스템 | 없음 |
| 3 | FateChoice 시스템 | 없음 |
| 4 | CardDatabase 12장 | Task 1 |
| 5 | BattleManager 업데이트 | Task 1, 2, 3 |
| 6 | CardUI | Task 1 |
| 7 | SlotUI | 없음 |
| 8 | FateChoiceUI | Task 3 |
| 9 | BattleUI 컨트롤러 | Task 4, 5, 6, 7, 8 |
| 10 | Battle 씬 연결 | Task 9 |

**병렬 가능:** Task 1, 2, 3은 동시 진행 가능. Task 6, 7, 8은 동시 진행 가능.
