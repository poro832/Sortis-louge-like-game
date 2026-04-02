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
    private TextureRect _frameBg = null!;
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

        // 투명 패널 — 프레임 텍스처가 배경 역할
        var baseStyle = new StyleBoxFlat();
        baseStyle.BgColor = new Color("#1C1C30", 0.6f);
        baseStyle.SetCornerRadiusAll(10);
        baseStyle.SetBorderWidthAll(0);
        AddThemeStyleboxOverride("panel", baseStyle);

        // 카드 프레임 텍스처 배경
        _frameBg = new TextureRect();
        _frameBg.Texture = GD.Load<Texture2D>("res://Assets/Art/Cards/card_frame.png");
        _frameBg.Position = Vector2.Zero;
        _frameBg.Size = new Vector2(130, 175);
        _frameBg.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        _frameBg.StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered;
        _frameBg.Modulate = new Color(1, 1, 1, 0.3f);
        _frameBg.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_frameBg);

        // 카드 수트 일러스트 배경
        _illustBg = new TextureRect();
        _illustBg.Position = new Vector2(10, 40);
        _illustBg.Size = new Vector2(110, 80);
        _illustBg.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        _illustBg.StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered;
        _illustBg.Modulate = new Color(1, 1, 1, 0.2f);
        _illustBg.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_illustBg);

        BuildCardLayout();
    }

    private void BuildCardLayout()
    {
        // 헤더 (수트 색상 바)
        _headerPanel = new Panel();
        _headerPanel.Position = new Vector2(0, 0);
        _headerPanel.Size = new Vector2(130, 38);
        AddChild(_headerPanel);

        // 코스트 뱃지
        _costLabel = new Label();
        _costLabel.Position = new Vector2(6, 6);
        _costLabel.Size = new Vector2(26, 26);
        _costLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _costLabel.VerticalAlignment = VerticalAlignment.Center;
        _costLabel.AddThemeColorOverride("font_color", Colors.White);
        FontManager.ApplyBody(_costLabel, 16);
        AddChild(_costLabel);

        // 수트 아이콘 (이미지)
        _suitIconTex = new TextureRect();
        _suitIconTex.Position = new Vector2(98, 5);
        _suitIconTex.Size = new Vector2(28, 28);
        _suitIconTex.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        _suitIconTex.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        AddChild(_suitIconTex);

        // 카드 이름
        _nameLabel = new Label();
        _nameLabel.Position = new Vector2(4, 42);
        _nameLabel.Size = new Vector2(122, 28);
        _nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _nameLabel.AddThemeColorOverride("font_color", new Color("#E8E8E8"));
        FontManager.ApplyTitle(_nameLabel, 13);
        AddChild(_nameLabel);

        // 구분선 역할 Label
        var divider = new ColorRect();
        divider.Position = new Vector2(10, 72);
        divider.Size = new Vector2(110, 1);
        divider.Color = new Color(1, 1, 1, 0.15f);
        AddChild(divider);

        // 스탯
        _statsLabel = new Label();
        _statsLabel.Position = new Vector2(8, 78);
        _statsLabel.Size = new Vector2(114, 70);
        _statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _statsLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        _statsLabel.AddThemeColorOverride("font_color", new Color("#CCCCCC"));
        FontManager.ApplyBody(_statsLabel, 13);
        AddChild(_statsLabel);

        // 역방향 표시
        _orientLabel = new Label();
        _orientLabel.Position = new Vector2(4, 152);
        _orientLabel.Size = new Vector2(122, 20);
        _orientLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _orientLabel.AddThemeFontSizeOverride("font_size", 11);
        _orientLabel.AddThemeColorOverride("font_color", new Color("#8E44AD"));
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

        // 헤더 색상
        var headerStyle = new StyleBoxFlat();
        headerStyle.BgColor = new Color(suitColor, 0.7f);
        headerStyle.CornerRadiusTopLeft = 10;
        headerStyle.CornerRadiusTopRight = 10;
        _headerPanel.AddThemeStyleboxOverride("panel", headerStyle);

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
        QueueRedraw();
        _hoverTween?.Kill();
        _hoverTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
        _hoverTween.TweenProperty(this, "scale", new Vector2(1.08f, 1.08f), 0.15);
    }

    private void OnMouseExit()
    {
        _isHovered = false;
        QueueRedraw();
        _hoverTween?.Kill();
        _hoverTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
        _hoverTween.TweenProperty(this, "scale", Vector2.One, 0.15);
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
