using Godot;
using Sortis.Cards;
using Sortis.Core;
using System;

namespace Sortis.UI;

public partial class CardUI : Panel
{
    private Card? _card;
    private bool _isSelected;
    private bool _isHovered;
    private Tween? _hoverTween;
    private float _baseY;

    // 레이아웃 요소
    private TextureRect _illustBg = null!;
    private Panel _headerPanel = null!;
    private Label _costLabel = null!;
    private TextureRect _suitIconTex = null!;
    private Label _nameLabel = null!;
    private Label _statsLabel = null!;
    private Label _orientLabel = null!;

    [Signal] public delegate void CardClickedEventHandler(CardUI cardUI);

    public Card? Card => _card;
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; QueueRedraw(); }
    }

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(130, 175);
        PivotOffset = new Vector2(65, 87);
        MouseEntered += OnMouseEnter;
        MouseExited += OnMouseExit;
        GuiInput += OnGuiInput;

        ClipContents = true;

        // 카드 배경 (단색 + 테두리 — 수트 색상은 SetCard에서 적용)
        var baseStyle = new StyleBoxFlat();
        baseStyle.BgColor = new Color("#10102A", 0.9f);
        baseStyle.SetCornerRadiusAll(8);
        baseStyle.BorderColor = new Color("#3A3A6A", 0.5f);
        baseStyle.SetBorderWidthAll(1);
        AddThemeStyleboxOverride("panel", baseStyle);

        // 수트 일러스트 (카드 전체 배경으로 깔림)
        _illustBg = new TextureRect();
        _illustBg.Position = new Vector2(0, 0);
        _illustBg.Size = new Vector2(130, 175);
        _illustBg.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        _illustBg.StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered;
        _illustBg.Modulate = new Color(1, 1, 1, 0.25f);
        _illustBg.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_illustBg);

        BuildCardLayout();
    }

    private void BuildCardLayout()
    {
        // 상단 얇은 수트 색상 악센트 라인 (헤더 대신)
        _headerPanel = new Panel();
        _headerPanel.Position = new Vector2(0, 0);
        _headerPanel.Size = new Vector2(130, 4);
        AddChild(_headerPanel);

        // 코스트 뱃지 (원형 배경)
        _costLabel = new Label();
        _costLabel.Position = new Vector2(4, 8);
        _costLabel.Size = new Vector2(24, 24);
        _costLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _costLabel.VerticalAlignment = VerticalAlignment.Center;
        _costLabel.AddThemeColorOverride("font_color", Colors.White);
        _costLabel.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.9f));
        _costLabel.AddThemeConstantOverride("shadow_offset_x", 1);
        _costLabel.AddThemeConstantOverride("shadow_offset_y", 1);
        FontManager.ApplyBody(_costLabel, 18);
        AddChild(_costLabel);

        // 수트 아이콘 (이미지)
        _suitIconTex = new TextureRect();
        _suitIconTex.Position = new Vector2(102, 8);
        _suitIconTex.Size = new Vector2(22, 22);
        _suitIconTex.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        _suitIconTex.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        AddChild(_suitIconTex);

        // 카드 이름
        _nameLabel = new Label();
        _nameLabel.Position = new Vector2(4, 34);
        _nameLabel.Size = new Vector2(122, 24);
        _nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _nameLabel.AddThemeColorOverride("font_color", new Color("#F0E6D0"));
        _nameLabel.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.9f));
        _nameLabel.AddThemeConstantOverride("shadow_offset_x", 1);
        _nameLabel.AddThemeConstantOverride("shadow_offset_y", 1);
        FontManager.ApplyTitle(_nameLabel, 13);
        AddChild(_nameLabel);

        // 구분선
        var divider = new ColorRect();
        divider.Position = new Vector2(10, 60);
        divider.Size = new Vector2(110, 1);
        divider.Color = new Color(1, 1, 1, 0.2f);
        AddChild(divider);

        // 스탯
        _statsLabel = new Label();
        _statsLabel.Position = new Vector2(8, 66);
        _statsLabel.Size = new Vector2(114, 80);
        _statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _statsLabel.VerticalAlignment = VerticalAlignment.Center;
        _statsLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        _statsLabel.AddThemeColorOverride("font_color", new Color("#EEEEEE"));
        _statsLabel.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.9f));
        _statsLabel.AddThemeConstantOverride("shadow_offset_x", 1);
        _statsLabel.AddThemeConstantOverride("shadow_offset_y", 1);
        FontManager.ApplyBody(_statsLabel, 14);
        AddChild(_statsLabel);

        // 역방향 표시
        _orientLabel = new Label();
        _orientLabel.Position = new Vector2(4, 152);
        _orientLabel.Size = new Vector2(122, 20);
        _orientLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _orientLabel.AddThemeFontSizeOverride("font_size", 11);
        _orientLabel.AddThemeColorOverride("font_color", new Color("#B06ADB"));
        _orientLabel.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.8f));
        _orientLabel.AddThemeConstantOverride("shadow_offset_x", 1);
        _orientLabel.AddThemeConstantOverride("shadow_offset_y", 1);
        AddChild(_orientLabel);
    }

    public void SetCard(Card card)
    {
        _card = card;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (_card == null) return;

        var suitColor = GetSuitColor(_card.Data.Suit);

        // 상단 얇은 악센트 라인
        var headerStyle = new StyleBoxFlat();
        headerStyle.BgColor = suitColor;
        headerStyle.CornerRadiusTopLeft = 8;
        headerStyle.CornerRadiusTopRight = 8;
        _headerPanel.AddThemeStyleboxOverride("panel", headerStyle);

        // 카드 테두리를 수트 색상으로 은은하게
        var cardStyle = new StyleBoxFlat();
        cardStyle.BgColor = new Color("#10102A", 0.9f);
        cardStyle.SetCornerRadiusAll(8);
        cardStyle.BorderColor = new Color(suitColor, 0.4f);
        cardStyle.SetBorderWidthAll(1);
        AddThemeStyleboxOverride("panel", cardStyle);

        _costLabel.Text = _card.Data.EnergyCost.ToString();
        _suitIconTex.Texture = GD.Load<Texture2D>(GetSuitIconPath(_card.Data.Suit));
        _illustBg.Texture = GD.Load<Texture2D>(GetCardIllustPath(_card.Data.Suit));
        _nameLabel.Text = _card.Data.CardName;

        // 스탯 조합
        string stats = "";
        int dmg = _card.GetDamage();
        int blk = _card.GetBlock();
        int drw = _card.GetDraw();
        int heal = _card.GetHeal();

        if (dmg > 0) stats += $"⚔ 피해 {dmg}\n";
        if (blk > 0) stats += $"🛡 방어 {blk}\n";
        if (drw > 0) stats += $"✦ 드로우 {drw}\n";
        if (heal > 0) stats += $"♥ 회복 {heal}";
        _statsLabel.Text = stats.TrimEnd();

        _orientLabel.Text = _card.IsReversed ? "↻ 역방향" : "";
    }

    public override void _Draw()
    {
        var rect = new Rect2(Vector2.Zero, Size);

        if (_isSelected)
        {
            // 선택 시 골드 테두리 + 외곽 글로우
            DrawRect(new Rect2(-3, -3, Size.X + 6, Size.Y + 6), new Color("#D4AF37", 0.3f));
            DrawRect(rect, new Color("#D4AF37"), false, 3f);
        }
        else if (_isHovered)
        {
            // 호버 시 밝은 테두리
            DrawRect(rect, new Color("#FFFFFF", 0.4f), false, 2f);
        }

        // 역방향 카드 상단 보라색 바
        if (_card?.IsReversed == true)
        {
            DrawRect(new Rect2(0, Size.Y - 4, Size.X, 4), new Color("#8E44AD"));
        }
    }

    private void OnMouseEnter()
    {
        _isHovered = true;
        ZIndex = 100;
        QueueRedraw();
        _hoverTween?.Kill();
        _hoverTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
        _hoverTween.TweenProperty(this, "scale", new Vector2(1.15f, 1.15f), 0.15);
        _hoverTween.Parallel().TweenProperty(this, "position:y", -20f, 0.15);
    }

    private void OnMouseExit()
    {
        _isHovered = false;
        ZIndex = 0;
        QueueRedraw();
        _hoverTween?.Kill();
        _hoverTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
        _hoverTween.TweenProperty(this, "scale", Vector2.One, 0.15);
        _hoverTween.Parallel().TweenProperty(this, "position:y", 0f, 0.15);
    }

    public void PlaySelectPulse()
    {
        var tw = CreateTween();
        tw.TweenProperty(this, "scale", new Vector2(1.15f, 1.15f), 0.1);
        tw.TweenProperty(this, "scale", new Vector2(1.05f, 1.05f), 0.1);
    }

    private void OnGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left && mb.DoubleClick)
        {
            PlaySelectPulse();
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

    private static string GetSuitIconPath(Suit suit) => suit switch
    {
        Suit.Wands => "res://Assets/Art/SuitIcons/wands.png",
        Suit.Swords => "res://Assets/Art/SuitIcons/swords.png",
        Suit.Cups => "res://Assets/Art/SuitIcons/cups.png",
        Suit.Pentacles => "res://Assets/Art/SuitIcons/pentacles.png",
        _ => "res://Assets/Art/SuitIcons/wands.png"
    };

    private static string GetCardIllustPath(Suit suit) => suit switch
    {
        Suit.Wands => "res://Assets/Art/Cards/Illustrations/card_wands.png",
        Suit.Swords => "res://Assets/Art/Cards/Illustrations/card_swords.png",
        Suit.Cups => "res://Assets/Art/Cards/Illustrations/card_cups.png",
        Suit.Pentacles => "res://Assets/Art/Cards/Illustrations/card_pentacles.png",
        _ => "res://Assets/Art/Cards/Illustrations/card_wands.png"
    };
}
