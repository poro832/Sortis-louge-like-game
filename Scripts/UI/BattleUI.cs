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
    private bool _slotsConnected;

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
        Log("전투 시작! 그림자 기사가 나타났다.");
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

        _activateButton = new Button { Text = "스프레드 발동" };
        _activateButton.CustomMinimumSize = new Vector2(160, 40);
        _activateButton.Pressed += OnActivatePressed;

        _endTurnButton = new Button { Text = "턴 종료" };
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

        _restartButton = new Button { Text = "재시작" };
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

        // 같은 카드 다시 클릭 -> 선택 해제
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
            Log("에너지가 부족합니다!");
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
            Log("카드를 최소 1장 배치하세요!");
            return;
        }

        _pendingActivation = _battle.ActivateSpread();
        var r = _pendingActivation;

        string logMsg = $"스프레드 발동! 피해:{r.FinalDamage} 방어:{r.FinalBlock}";
        if (r.ProphecyResult.IsHit)
            logMsg += $" | 예언 적중! x{r.ProphecyResult.Multiplier:F1}";
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

        Log($"운명 선택: {option.Name}");

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
            // 턴 종료 -> 다음 턴
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

        Log("턴 종료. 적이 공격합니다!");
        RefreshUI();
    }

    private void OnProphecyRevealed(ProphecyCondition condition)
    {
        _prophecyLabel.Text = $"예언: {condition.Description}";
    }

    private void OnProphecyHit(ProphecyResult result)
    {
        _prophecyLabel.Text += $"  >>> 적중! x{result.Multiplier:F1}";
    }

    private void OnFateChosen(FateOption option) { }

    // --- UI 갱신 ---

    private void RefreshUI()
    {
        // 적 정보
        var e = _battle.Enemy;
        string intentIcon = e.CurrentIntent == IntentType.Attack ? "공격" : "방어";
        _enemyInfoLabel.Text = $"{e.Name}  체력: {e.Hp}/{e.MaxHp}  방어: {e.Block}  의도: {intentIcon} {e.CurrentIntentValue}";

        // 플레이어 정보
        _playerInfoLabel.Text = $"체력: {_battle.PlayerHp}/{_battle.PlayerMaxHp}   에너지: {_battle.Energy}/{_battle.MaxEnergy}   방어: {_battle.PlayerBlock}   연속 적중: {_battle.ConsecutiveHits}";

        // 슬롯 갱신
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

        // 슬롯 클릭 연결 (최초 1회)
        if (!_slotsConnected)
        {
            foreach (var slot in _slotUIs)
                slot.SlotClicked += OnSlotClicked;
            _slotsConnected = true;
        }
    }

    private void ShowBattleResult()
    {
        _activateButton.Disabled = true;
        _endTurnButton.Disabled = true;

        if (_battle.IsEnemyDead)
        {
            _resultLabel.Text = "승리!";
            _resultLabel.AddThemeColorOverride("font_color", new Color("#D4AF37"));
        }
        else
        {
            _resultLabel.Text = "패배...";
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
