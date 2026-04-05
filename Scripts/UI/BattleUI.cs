using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Sortis.Cards;
using Sortis.Combat;
using Sortis.Core;
using Sortis.Data;
using Sortis.Prophecy;
using Sortis.UI.Components;

namespace Sortis.UI;

public partial class BattleUI : Control
{
    // 테마 색상
    private static readonly Color BgColor = new("#0E0E1C");
    private static readonly Color PanelBg = new("#151530");
    private static readonly Color GoldColor = new("#D4AF37");
    private static readonly Color TextWhite = new("#E8E8E8");
    private static readonly Color TextGray = new("#888888");

    private BattleManager _battle = null!;
    private readonly List<CardUI> _cardUIs = new();
    private readonly List<SlotUI> _slotUIs = new();
    private bool _slotsConnected;

    // UI 요소들
    private Label _enemyNameLabel = null!;
    private HealthBar _enemyHealthBar = null!;
    private Label _enemyIntentLabel = null!;
    private Label _prophecyLabel = null!;
    private Label _playerInfoLabel = null!;
    private HealthBar _playerHealthBar = null!;
    private EnergyDisplay _playerEnergyDisplay = null!;
    private Label _logLabel = null!;
    private HBoxContainer _handContainer = null!;
    private HBoxContainer _slotContainer = null!;
    private Button _activateButton = null!;
    private Button _endTurnButton = null!;
    private FateChoiceUI _fateChoiceUI = null!;
    private Label _resultLabel = null!;
    private Button _restartButton = null!;
    private Button _menuButton = null!;
    private PhaseIndicator _phaseIndicator = null!;
    private Label _turnLabel = null!;
    private Control _popupLayer = null!;
    private PanelContainer _pauseOverlay = null!;
    private Label _spreadPreviewLabel = null!;
    private ActivationResult? _pendingActivation;

    public override void _Ready()
    {
        BuildLayout();
        StartNewBattle();
    }

    private void StartNewBattle()
    {
        var deck = new Deck();
        var cardDatas = CardDatabase.CreateStarterDeck();
        var cards = new List<Card>();
        var rng = new Random();
        foreach (var data in cardDatas)
        {
            var orientation = rng.NextDouble() < 0.2
                ? CardOrientation.Reversed
                : CardOrientation.Upright;
            cards.Add(new Card(data, orientation));
        }
        deck.Initialize(cards);

        var enemy = Enemy.ShadowKnight();
        _battle = new BattleManager(deck, enemy);
        _battle.OnProphecyRevealed += OnProphecyRevealed;
        _battle.OnProphecyHit += OnProphecyHit;
        _battle.OnFateChosen += OnFateChosen;

        _resultLabel.Visible = false;
        _restartButton.Visible = false;
        _menuButton.Visible = false;
        _activateButton.Disabled = false;
        _endTurnButton.Disabled = false;

        _battle.StartBattle();
        RefreshUI();
        Log("전투 시작! 그림자 기사가 나타났다.");
    }

    // --- UI 구축 (오버레이 방식) ---

    private void BuildLayout()
    {
        // ═══ 레이어 0: 셰이더 배경 (전체 화면) ═══
        var bg = new ColorRect();
        bg.Color = Colors.White;
        bg.SetAnchorsPreset(LayoutPreset.FullRect);
        var bgShader = new ShaderMaterial();
        bgShader.Shader = GD.Load<Shader>("res://Shaders/atmosphere.gdshader");
        bg.Material = bgShader;
        AddChild(bg);

        // 분위기 파티클
        AddChild(new MysticParticles(MysticParticles.ParticleStyle.Mixed));

        // ═══ 레이어 1: 하단 그라데이션 (카드 영역 가독성) ═══
        var bottomGrad = new ColorRect();
        bottomGrad.AnchorLeft = 0; bottomGrad.AnchorRight = 1;
        bottomGrad.AnchorTop = 0.55f; bottomGrad.AnchorBottom = 1;
        bottomGrad.OffsetLeft = 0; bottomGrad.OffsetRight = 0;
        bottomGrad.OffsetTop = 0; bottomGrad.OffsetBottom = 0;
        bottomGrad.Color = new Color(0, 0, 0, 0.6f);
        bottomGrad.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(bottomGrad);

        // ═══ 상단 좌측: 턴 + 페이즈 ═══
        var topLeft = new HBoxContainer();
        topLeft.AnchorLeft = 0; topLeft.AnchorTop = 0;
        topLeft.OffsetLeft = 16; topLeft.OffsetTop = 10;
        topLeft.AddThemeConstantOverride("separation", 12);

        _turnLabel = new Label();
        _turnLabel.AddThemeColorOverride("font_color", GoldColor);
        FontManager.ApplyBody(_turnLabel, 18);
        topLeft.AddChild(_turnLabel);

        _phaseIndicator = new PhaseIndicator();
        topLeft.AddChild(_phaseIndicator);

        AddChild(topLeft);

        // ═══ 상단 중앙: 적 정보 (플로팅) ═══
        var enemyArea = new VBoxContainer();
        enemyArea.AnchorLeft = 0.5f; enemyArea.AnchorTop = 0;
        enemyArea.OffsetLeft = -180; enemyArea.OffsetTop = 10;
        enemyArea.CustomMinimumSize = new Vector2(360, 0);
        enemyArea.AddThemeConstantOverride("separation", 4);

        _enemyNameLabel = new Label();
        _enemyNameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _enemyNameLabel.AddThemeColorOverride("font_color", new Color("#E74C3C"));
        FontManager.ApplyTitle(_enemyNameLabel, 22);
        enemyArea.AddChild(_enemyNameLabel);

        _enemyHealthBar = new HealthBar();
        _enemyHealthBar.CustomMinimumSize = new Vector2(280, 20);
        _enemyHealthBar.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        _enemyHealthBar.SetColors(new Color("#C0392B"), new Color("#1A1A1A"), new Color("#444444"));
        enemyArea.AddChild(_enemyHealthBar);

        _enemyIntentLabel = new Label();
        _enemyIntentLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _enemyIntentLabel.AddThemeFontSizeOverride("font_size", 16);
        enemyArea.AddChild(_enemyIntentLabel);

        _prophecyLabel = new Label();
        _prophecyLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _prophecyLabel.AddThemeFontSizeOverride("font_size", 14);
        _prophecyLabel.AddThemeColorOverride("font_color", GoldColor);
        enemyArea.AddChild(_prophecyLabel);

        AddChild(enemyArea);

        // ═══ 중앙: 스프레드 슬롯 (화면 중앙에 플로팅) ═══
        var spreadArea = new VBoxContainer();
        spreadArea.AnchorLeft = 0.5f; spreadArea.AnchorTop = 0.5f;
        spreadArea.OffsetLeft = -240; spreadArea.OffsetTop = -120;
        spreadArea.CustomMinimumSize = new Vector2(480, 0);
        spreadArea.AddThemeConstantOverride("separation", 8);

        var spreadTitle = new Label();
        spreadTitle.Text = "⚜ 스프레드 ⚜";
        spreadTitle.HorizontalAlignment = HorizontalAlignment.Center;
        spreadTitle.AddThemeColorOverride("font_color", new Color("#8A8AAA"));
        FontManager.ApplyTitle(spreadTitle, 13);
        spreadArea.AddChild(spreadTitle);

        var slotCenter = new CenterContainer();
        _slotContainer = new HBoxContainer();
        _slotContainer.AddThemeConstantOverride("separation", 16);
        for (int i = 0; i < 3; i++)
        {
            var slot = new SlotUI();
            _slotUIs.Add(slot);
            _slotContainer.AddChild(slot);
        }
        slotCenter.AddChild(_slotContainer);
        spreadArea.AddChild(slotCenter);

        AddChild(spreadArea);

        // ═══ 스프레드 효과 미리보기 (슬롯 우측) ═══
        var previewPanel = new PanelContainer();
        previewPanel.AnchorLeft = 0.5f; previewPanel.AnchorTop = 0.5f;
        previewPanel.OffsetLeft = 260; previewPanel.OffsetTop = -100;
        previewPanel.CustomMinimumSize = new Vector2(200, 0);
        var previewStyle = new StyleBoxFlat();
        previewStyle.BgColor = new Color("#0D0D1A", 0.75f);
        previewStyle.SetCornerRadiusAll(8);
        previewStyle.BorderColor = new Color(GoldColor, 0.3f);
        previewStyle.SetBorderWidthAll(1);
        previewStyle.SetContentMarginAll(10);
        previewPanel.AddThemeStyleboxOverride("panel", previewStyle);

        _spreadPreviewLabel = new Label();
        _spreadPreviewLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        _spreadPreviewLabel.AddThemeFontSizeOverride("font_size", 12);
        _spreadPreviewLabel.AddThemeColorOverride("font_color", new Color("#CCCCDD"));
        _spreadPreviewLabel.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.7f));
        _spreadPreviewLabel.AddThemeConstantOverride("shadow_offset_x", 1);
        _spreadPreviewLabel.AddThemeConstantOverride("shadow_offset_y", 1);
        _spreadPreviewLabel.Text = "카드를 배치하면\n효과를 미리 볼 수 있습니다";
        previewPanel.AddChild(_spreadPreviewLabel);
        AddChild(previewPanel);

        // ═══ 로그 (스프레드 아래 중앙) ═══
        _logLabel = new Label();
        _logLabel.AnchorLeft = 0.5f; _logLabel.AnchorTop = 0.5f;
        _logLabel.OffsetLeft = -300; _logLabel.OffsetTop = 85;
        _logLabel.CustomMinimumSize = new Vector2(600, 24);
        _logLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _logLabel.AddThemeFontSizeOverride("font_size", 14);
        _logLabel.AddThemeColorOverride("font_color", new Color("#AAAACC"));
        AddChild(_logLabel);

        // ═══ 좌하단: 플레이어 HP + 에너지 ═══
        var playerArea = new VBoxContainer();
        playerArea.AnchorLeft = 0; playerArea.AnchorBottom = 1;
        playerArea.OffsetLeft = 16; playerArea.OffsetBottom = -12;
        playerArea.AnchorTop = 1;
        playerArea.OffsetTop = -80;
        playerArea.AddThemeConstantOverride("separation", 6);

        var hpRow = new HBoxContainer();
        hpRow.AddThemeConstantOverride("separation", 8);
        var hpLabel = new Label { Text = "HP" };
        hpLabel.AddThemeFontSizeOverride("font_size", 14);
        hpLabel.AddThemeColorOverride("font_color", new Color("#27AE60"));
        hpRow.AddChild(hpLabel);

        _playerHealthBar = new HealthBar();
        _playerHealthBar.CustomMinimumSize = new Vector2(160, 18);
        _playerHealthBar.SetColors(new Color("#27AE60"), new Color("#1A1A1A"), new Color("#444444"));
        hpRow.AddChild(_playerHealthBar);

        _playerInfoLabel = new Label();
        _playerInfoLabel.AddThemeFontSizeOverride("font_size", 14);
        _playerInfoLabel.AddThemeColorOverride("font_color", TextWhite);
        hpRow.AddChild(_playerInfoLabel);

        playerArea.AddChild(hpRow);

        var energyRow = new HBoxContainer();
        energyRow.AddThemeConstantOverride("separation", 8);

        _playerEnergyDisplay = new EnergyDisplay();
        energyRow.AddChild(_playerEnergyDisplay);

        playerArea.AddChild(energyRow);

        AddChild(playerArea);

        // ═══ 우하단: 버튼 (세로 배치) ═══
        var btnArea = new VBoxContainer();
        btnArea.AnchorRight = 1; btnArea.AnchorBottom = 1;
        btnArea.AnchorLeft = 1; btnArea.AnchorTop = 1;
        btnArea.OffsetRight = -16; btnArea.OffsetLeft = -200;
        btnArea.OffsetBottom = -12; btnArea.OffsetTop = -100;
        btnArea.AddThemeConstantOverride("separation", 8);

        _activateButton = MakeGoldButton("⚡ 스프레드 발동", 180);
        _activateButton.Pressed += OnActivatePressed;
        btnArea.AddChild(_activateButton);

        _endTurnButton = MakeGoldButton("턴 종료", 180);
        _endTurnButton.Pressed += OnEndTurnPressed;
        btnArea.AddChild(_endTurnButton);

        AddChild(btnArea);

        // ═══ 하단 중앙: 핸드 영역 (카드) ═══
        var handArea = new Control();
        handArea.AnchorLeft = 0.15f; handArea.AnchorRight = 0.85f;
        handArea.AnchorTop = 1; handArea.AnchorBottom = 1;
        handArea.OffsetTop = -200; handArea.OffsetBottom = -10;
        handArea.MouseFilter = MouseFilterEnum.Ignore;

        _handContainer = new HBoxContainer();
        _handContainer.SetAnchorsPreset(LayoutPreset.FullRect);
        _handContainer.Alignment = BoxContainer.AlignmentMode.Center;
        _handContainer.AddThemeConstantOverride("separation", -15);
        _handContainer.MouseFilter = MouseFilterEnum.Ignore;
        handArea.AddChild(_handContainer);

        AddChild(handArea);

        // ═══ 오버레이들 ═══

        // 운명 선택지
        var fateCenter = new CenterContainer();
        fateCenter.SetAnchorsPreset(LayoutPreset.FullRect);
        fateCenter.MouseFilter = MouseFilterEnum.Ignore;
        _fateChoiceUI = new FateChoiceUI();
        _fateChoiceUI.FateSelected += OnFateSelected;
        fateCenter.AddChild(_fateChoiceUI);
        AddChild(fateCenter);

        // 결과 표시
        var resultOverlay = new CenterContainer();
        resultOverlay.SetAnchorsPreset(LayoutPreset.FullRect);
        resultOverlay.MouseFilter = MouseFilterEnum.Ignore;
        var resultVBox = new VBoxContainer();
        resultVBox.AddThemeConstantOverride("separation", 16);

        _resultLabel = new Label();
        _resultLabel.HorizontalAlignment = HorizontalAlignment.Center;
        FontManager.ApplyTitle(_resultLabel, 40);
        _resultLabel.Visible = false;
        resultVBox.AddChild(_resultLabel);

        var resultBtnRow = new HBoxContainer();
        resultBtnRow.Alignment = BoxContainer.AlignmentMode.Center;
        resultBtnRow.AddThemeConstantOverride("separation", 16);

        _restartButton = MakeGoldButton("재시작", 130);
        _restartButton.Visible = false;
        _restartButton.Pressed += () => StartNewBattle();
        resultBtnRow.AddChild(_restartButton);

        _menuButton = MakeGoldButton("메인 메뉴", 130);
        _menuButton.Visible = false;
        _menuButton.Pressed += () => SceneManager.Instance.ChangeScene("res://Scenes/MainMenu.tscn");
        resultBtnRow.AddChild(_menuButton);

        resultVBox.AddChild(resultBtnRow);
        resultOverlay.AddChild(resultVBox);
        AddChild(resultOverlay);

        // 팝업 레이어
        _popupLayer = new Control();
        _popupLayer.SetAnchorsPreset(LayoutPreset.FullRect);
        _popupLayer.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_popupLayer);

        // ESC 일시정지
        _pauseOverlay = BuildPauseOverlay();
        _pauseOverlay.Visible = false;
        AddChild(_pauseOverlay);
    }

    // --- 헬퍼: 골드 테마 버튼 ---
    private static Button MakeGoldButton(string text, int width)
    {
        var btn = new Button { Text = text };
        btn.CustomMinimumSize = new Vector2(width, 40);

        var normal = new StyleBoxFlat();
        normal.BgColor = new Color(GoldColor, 0.12f);
        normal.BorderColor = GoldColor;
        normal.SetBorderWidthAll(1);
        normal.SetCornerRadiusAll(6);
        normal.SetContentMarginAll(8);
        btn.AddThemeStyleboxOverride("normal", normal);

        var hover = new StyleBoxFlat();
        hover.BgColor = new Color(GoldColor, 0.25f);
        hover.BorderColor = GoldColor;
        hover.SetBorderWidthAll(2);
        hover.SetCornerRadiusAll(6);
        hover.SetContentMarginAll(8);
        btn.AddThemeStyleboxOverride("hover", hover);

        var pressed = new StyleBoxFlat();
        pressed.BgColor = new Color(GoldColor, 0.35f);
        pressed.BorderColor = new Color("#FFFFFF");
        pressed.SetBorderWidthAll(2);
        pressed.SetCornerRadiusAll(6);
        pressed.SetContentMarginAll(8);
        btn.AddThemeStyleboxOverride("pressed", pressed);

        var disabled = new StyleBoxFlat();
        disabled.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.5f);
        disabled.BorderColor = new Color("#333333");
        disabled.SetBorderWidthAll(1);
        disabled.SetCornerRadiusAll(6);
        disabled.SetContentMarginAll(8);
        btn.AddThemeStyleboxOverride("disabled", disabled);

        btn.AddThemeColorOverride("font_color", GoldColor);
        btn.AddThemeColorOverride("font_hover_color", new Color("#FFFFFF"));
        btn.AddThemeColorOverride("font_disabled_color", new Color("#555555"));
        FontManager.ApplyBody(btn, 16);

        return btn;
    }

    // --- 일시정지 ---

    private PanelContainer BuildPauseOverlay()
    {
        var panel = new PanelContainer();
        panel.SetAnchorsPreset(LayoutPreset.FullRect);

        var dimBg = new ColorRect();
        dimBg.Color = new Color(0, 0, 0, 0.7f);
        dimBg.SetAnchorsPreset(LayoutPreset.FullRect);
        panel.AddChild(dimBg);

        var center = new CenterContainer();
        center.SetAnchorsPreset(LayoutPreset.FullRect);
        panel.AddChild(center);

        var pausePanel = new PanelContainer();
        var pStyle = new StyleBoxFlat();
        pStyle.BgColor = new Color("#0D0D1A", 0.95f);
        pStyle.BorderColor = GoldColor;
        pStyle.SetBorderWidthAll(2);
        pStyle.SetCornerRadiusAll(14);
        pStyle.ShadowColor = new Color(GoldColor, 0.15f);
        pStyle.ShadowSize = 10;
        pStyle.SetContentMarginAll(30);
        pausePanel.AddThemeStyleboxOverride("panel", pStyle);
        center.AddChild(pausePanel);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 14);
        pausePanel.AddChild(vbox);

        var title = new Label();
        title.Text = "⚜ 일시정지 ⚜";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeColorOverride("font_color", GoldColor);
        FontManager.ApplyTitle(title, 28);
        vbox.AddChild(title);

        var spacer = new Control();
        spacer.CustomMinimumSize = new Vector2(0, 8);
        vbox.AddChild(spacer);

        var resumeBtn = MakeGoldButton("계속하기", 220);
        resumeBtn.Pressed += () => TogglePause();
        vbox.AddChild(resumeBtn);

        var restartBtn = MakeGoldButton("재시작", 220);
        restartBtn.Pressed += () =>
        {
            GetTree().Paused = false;
            _pauseOverlay.Visible = false;
            StartNewBattle();
        };
        vbox.AddChild(restartBtn);

        var menuBtn = MakeGoldButton("메인 메뉴로", 220);
        menuBtn.Pressed += () =>
        {
            GetTree().Paused = false;
            SceneManager.Instance.ChangeScene("res://Scenes/MainMenu.tscn");
        };
        vbox.AddChild(menuBtn);

        panel.ProcessMode = ProcessModeEnum.Always;
        return panel;
    }

    private void TogglePause()
    {
        bool pausing = !_pauseOverlay.Visible;
        _pauseOverlay.Visible = pausing;
        GetTree().Paused = pausing;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            TogglePause();
            GetViewport().SetInputAsHandled();
        }
    }

    private void SpawnPopup(string text, Color color, Vector2 pos)
    {
        var popup = DamagePopup.Create(text, color, pos);
        _popupLayer.AddChild(popup);
    }

    // --- 이벤트 핸들러 ---

    private void OnCardClicked(CardUI cardUI)
    {
        if (_battle.IsBattleOver) return;
        if (cardUI.Card == null) return;

        // 첫 번째 빈 슬롯을 찾아 자동 배치 (과거→현재→미래)
        for (int i = 0; i < _slotUIs.Count; i++)
        {
            if (!_slotUIs[i].HasCard)
            {
                if (_battle.PlaceCard(cardUI.Card, i))
                {
                    _slotUIs[i].ShowCard(cardUI.Card);

                    RefreshUI();
                    return;
                }
                else
                {
                    Log("에너지가 부족합니다!");
                    return;
                }
            }
        }
        Log("모든 슬롯이 가득 찼습니다!");
    }

    private void OnSlotClicked(int slotIndex)
    {
        if (_battle.IsBattleOver) return;

        // 슬롯 클릭 시 카드 회수
        if (_slotUIs[slotIndex].HasCard)
        {
            _battle.RemoveCardFromSlot(slotIndex);
            _slotUIs[slotIndex].ClearCard();
            RefreshUI();
        }
    }

    private void OnActivatePressed()
    {
        bool hasAny = false;
        foreach (var slot in _slotUIs)
            if (slot.HasCard) { hasAny = true; break; }
        if (!hasAny)
        {
            Log("카드를 최소 1장 배치하세요!");
            return;
        }

        _phaseIndicator.SetActivePhase(BattlePhase.Activation);
        _pendingActivation = _battle.ActivateSpread();
        var r = _pendingActivation;

        string logMsg = $"스프레드 발동! 피해:{r.FinalDamage} 방어:{r.FinalBlock}";
        if (r.ProphecyResult.IsHit)
            logMsg += $" | 예언 적중! x{r.ProphecyResult.Multiplier:F1}";
        Log(logMsg);

        if (r.FinalDamage > 0)
            SpawnPopup($"-{r.FinalDamage}", new Color("#E74C3C"), new Vector2(640, 80));
        if (r.FinalBlock > 0)
            SpawnPopup($"+{r.FinalBlock} 방어", new Color("#3498DB"), new Vector2(640, 520));

        _phaseIndicator.SetActivePhase(BattlePhase.Fate);
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
        SpawnPopup(option.Name, GoldColor, new Vector2(540, 300));

        _phaseIndicator.SetActivePhase(BattlePhase.EnemyTurn);
        int playerHpBeforeEnemy = _battle.PlayerHp;
        if (!_battle.IsEnemyDead)
        {
            _battle.ExecuteEnemyTurn();
            int dmgTaken = playerHpBeforeEnemy - _battle.PlayerHp;
            if (dmgTaken > 0)
                SpawnPopup($"-{dmgTaken}", new Color("#E74C3C"), new Vector2(640, 520));
        }

        if (_battle.IsBattleOver)
        {
            ShowBattleResult();
        }
        else
        {
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

        foreach (var slot in _slotUIs) slot.ClearCard();

        int hpBeforeEnd = _battle.PlayerHp;
        _battle.ExecuteEnemyTurn();
        int dmgEnd = hpBeforeEnd - _battle.PlayerHp;
        if (dmgEnd > 0)
            SpawnPopup($"-{dmgEnd}", new Color("#E74C3C"), new Vector2(640, 520));

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
        _prophecyLabel.Text = $"✧ 예언: {condition.Description} ✧";
    }

    private void OnProphecyHit(ProphecyResult result)
    {
        _prophecyLabel.Text += $"  >>> 적중! x{result.Multiplier:F1}";
    }

    private void OnFateChosen(FateOption option) { }

    // --- UI 갱신 ---

    private void RefreshUI()
    {
        _turnLabel.Text = $"턴 {_battle.TurnNumber}";
        _phaseIndicator.SetActivePhase(BattlePhase.Placement);

        var e = _battle.Enemy;
        _enemyNameLabel.Text = $"⚔ {e.Name}";
        _enemyHealthBar.SetValue(e.Hp, e.MaxHp);
        string intentIcon = e.CurrentIntent == IntentType.Attack ? "⚔" : "🛡";
        var intentColor = e.CurrentIntent == IntentType.Attack ? "#E74C3C" : "#3498DB";
        _enemyIntentLabel.Text = $"의도: {intentIcon} {e.CurrentIntentValue}  방어:{e.Block}";
        _enemyIntentLabel.AddThemeColorOverride("font_color", new Color(intentColor));

        _playerHealthBar.SetValue(_battle.PlayerHp, _battle.PlayerMaxHp);
        _playerEnergyDisplay.SetEnergy(_battle.Energy, _battle.MaxEnergy);
        _playerInfoLabel.Text = $"🛡{_battle.PlayerBlock}  연속 적중: {_battle.ConsecutiveHits}";

        for (int i = 0; i < _slotUIs.Count; i++)
        {
            _slotUIs[i].ClearCard();
            _slotUIs[i].Setup(i, _battle.CurrentSpread.Slots[i].Position);
            if (_battle.CurrentSpread.Slots[i].PlacedCard is { } placedCard)
                _slotUIs[i].ShowCard(placedCard);
        }

        foreach (var child in _handContainer.GetChildren())
            child.QueueFree();
        _cardUIs.Clear();

        foreach (var card in _battle.Deck.Hand)
        {
            var cardUI = new CardUI();
            _handContainer.AddChild(cardUI);
            cardUI.SetCard(card);
            cardUI.CardClicked += OnCardClicked;
            _cardUIs.Add(cardUI);
        }

        if (!_slotsConnected)
        {
            foreach (var slot in _slotUIs)
                slot.SlotClicked += OnSlotClicked;
            _slotsConnected = true;
        }

        UpdateSpreadPreview();
    }

    private void ShowBattleResult()
    {
        _activateButton.Disabled = true;
        _endTurnButton.Disabled = true;

        if (_battle.IsEnemyDead)
        {
            _resultLabel.Text = "⚜ 승리! ⚜";
            _resultLabel.AddThemeColorOverride("font_color", GoldColor);
        }
        else
        {
            _resultLabel.Text = "패배...";
            _resultLabel.AddThemeColorOverride("font_color", new Color("#C0392B"));
        }
        _resultLabel.Visible = true;
        _restartButton.Visible = true;
        _menuButton.Visible = true;
    }

    private void UpdateSpreadPreview()
    {
        var spread = _battle.CurrentSpread;
        bool hasAny = spread.Slots.Any(s => s.IsOccupied);

        if (!hasAny)
        {
            _spreadPreviewLabel.Text = "카드를 배치하면\n효과를 미리 볼 수 있습니다";
            return;
        }

        var result = spread.Activate();
        string text = "── 스프레드 효과 ──\n";

        // 슬롯별 보너스 설명
        foreach (var slot in spread.Slots)
        {
            if (slot.PlacedCard is not { } card) continue;
            string pos = slot.Position switch
            {
                SlotPosition.Past => "과거",
                SlotPosition.Present => "현재",
                SlotPosition.Future => "미래",
                _ => slot.Position.ToString()
            };

            text += $"\n[{pos}] {card.Data.CardName}";
            if (card.IsReversed && slot.Position == SlotPosition.Past)
                text += "\n  → 역방향 보너스 +2 피해";
            if (card.Data.IsMajorArcana && slot.Position == SlotPosition.Future)
                text += "\n  → 메이저 아르카나 +3 피해";
        }

        // 합산
        text += $"\n\n── 합산 ──";
        if (result.TotalDamage > 0) text += $"\n⚔ 총 피해: {result.TotalDamage}";
        if (result.TotalBlock > 0) text += $"\n🛡 총 방어: {result.TotalBlock}";
        if (result.TotalDraw > 0) text += $"\n✦ 다음 턴 드로우: +{result.TotalDraw}";

        // 예언 적중 가능 여부
        var prophecy = _battle.CurrentProphecy;
        if (prophecy != null)
        {
            bool wouldHit = prophecy.Evaluate(spread, result);
            if (wouldHit)
                text += $"\n\n✧ 예언 적중! x{1.0f + _battle.ConsecutiveHits * 0.5f + 0.5f:F1}";
            else
                text += $"\n\n✧ 예언 미적중";
        }

        // 지배 수트 → 운명 선택지 미리보기
        if (result.SuitCounts.Count > 0)
        {
            var dominant = result.SuitCounts.OrderByDescending(kv => kv.Value).First();
            bool isDominant = result.SuitCounts.Count(kv => kv.Value == dominant.Value) == 1;
            string suitName = dominant.Key switch
            {
                Suit.Wands => "완드",
                Suit.Swords => "검",
                Suit.Cups => "컵",
                Suit.Pentacles => "펜타클",
                _ => "?"
            };

            if (isDominant)
                text += $"\n\n⚜ 지배 수트: {suitName}";
            else
                text += $"\n\n⚜ 지배 수트: 없음 (균형)";

            var fate = FateChoice.Generate(result.SuitCounts);
            text += $"\n운명A: {fate.OptionA.Name}";
            text += $"\n  {fate.OptionA.Description}";
            text += $"\n운명B: {fate.OptionB.Name}";
            text += $"\n  {fate.OptionB.Description}";
        }

        _spreadPreviewLabel.Text = text;
    }

    private void Log(string message)
    {
        _logLabel.Text = message;
    }
}
