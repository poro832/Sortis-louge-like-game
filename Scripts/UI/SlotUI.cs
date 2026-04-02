using Godot;
using Sortis.Cards;
using Sortis.Core;
using System;

namespace Sortis.UI;

public partial class SlotUI : Panel
{
    private TextureRect _frameBg = null!;
    private Label _positionIcon = null!;
    private Label _positionLabel = null!;
    private Label _cardInfoLabel = null!;
    private int _slotIndex;
    private bool _hasCard;
    private bool _isHovered;
    private SlotPosition _position;

    [Signal] public delegate void SlotClickedEventHandler(int slotIndex);

    public int SlotIndex => _slotIndex;
    public bool HasCard => _hasCard;

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(140, 185);
        MouseEntered += () => { _isHovered = true; QueueRedraw(); };
        MouseExited += () => { _isHovered = false; QueueRedraw(); };
        GuiInput += OnGuiInput;

        ApplyEmptyStyle();

        // 슬롯 프레임 텍스처 배경
        _frameBg = new TextureRect();
        _frameBg.Position = Vector2.Zero;
        _frameBg.Size = new Vector2(140, 185);
        _frameBg.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        _frameBg.StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered;
        _frameBg.Modulate = new Color(1, 1, 1, 0.25f);
        _frameBg.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_frameBg);

        // 위치 심볼
        _positionIcon = new Label();
        _positionIcon.Position = new Vector2(0, 12);
        _positionIcon.Size = new Vector2(140, 40);
        _positionIcon.HorizontalAlignment = HorizontalAlignment.Center;
        _positionIcon.AddThemeFontSizeOverride("font_size", 28);
        AddChild(_positionIcon);

        // 위치 이름
        _positionLabel = new Label();
        _positionLabel.Position = new Vector2(0, 50);
        _positionLabel.Size = new Vector2(140, 24);
        _positionLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _positionLabel.AddThemeColorOverride("font_color", new Color("#888888"));
        FontManager.ApplyTitle(_positionLabel, 13);
        AddChild(_positionLabel);

        // 카드 정보
        _cardInfoLabel = new Label();
        _cardInfoLabel.Position = new Vector2(8, 80);
        _cardInfoLabel.Size = new Vector2(124, 90);
        _cardInfoLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _cardInfoLabel.VerticalAlignment = VerticalAlignment.Center;
        _cardInfoLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        _cardInfoLabel.AddThemeFontSizeOverride("font_size", 13);
        _cardInfoLabel.AddThemeColorOverride("font_color", new Color("#777777"));
        AddChild(_cardInfoLabel);
    }

    public void Setup(int index, SlotPosition position)
    {
        _slotIndex = index;
        _position = position;
        _frameBg.Texture = GD.Load<Texture2D>(GetSlotFramePath(position));
        _positionIcon.Text = GetPositionSymbol(position);
        _positionLabel.Text = position switch
        {
            SlotPosition.Past => "과거",
            SlotPosition.Present => "현재",
            SlotPosition.Future => "미래",
            _ => position.ToString()
        };

        if (!_hasCard)
        {
            _cardInfoLabel.Text = "카드를 배치하세요";
            _cardInfoLabel.AddThemeColorOverride("font_color", new Color("#555555"));
        }
    }

    public void ShowCard(Card card)
    {
        _hasCard = true;

        string info = card.Data.CardName;
        if (card.IsReversed) info += " ↻";
        info += "\n";
        if (card.GetDamage() > 0) info += $"⚔{card.GetDamage()} ";
        if (card.GetBlock() > 0) info += $"🛡{card.GetBlock()} ";
        if (card.GetDraw() > 0) info += $"✦{card.GetDraw()}";
        _cardInfoLabel.Text = info;
        _cardInfoLabel.AddThemeColorOverride("font_color", new Color("#E8E8E8"));

        var style = new StyleBoxFlat();
        style.BgColor = new Color("#252545");
        style.BorderColor = new Color("#9B59B6");
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(10);
        // 안쪽 글로우 효과 — 그림자로 표현
        style.ShadowColor = new Color("#9B59B6", 0.25f);
        style.ShadowSize = 6;
        AddThemeStyleboxOverride("panel", style);

        _positionIcon.AddThemeColorOverride("font_color", new Color("#D4AF37"));

        // 배치 펄스 애니메이션 — 스케일 + 밝기
        PivotOffset = Size / 2f;
        var tw = CreateTween().SetParallel();
        tw.TweenProperty(this, "scale", new Vector2(1.1f, 1.1f), 0.12)
            .SetEase(Tween.EaseType.Out);
        tw.TweenProperty(this, "modulate", new Color(1.5f, 1.3f, 1.8f), 0.12);
        tw.Chain();
        tw.TweenProperty(this, "scale", Vector2.One, 0.25)
            .SetEase(Tween.EaseType.InOut);
        tw.TweenProperty(this, "modulate", Colors.White, 0.25);
    }

    public void ClearCard()
    {
        _hasCard = false;
        _cardInfoLabel.Text = "카드를 배치하세요";
        _cardInfoLabel.AddThemeColorOverride("font_color", new Color("#555555"));
        _positionIcon.RemoveThemeColorOverride("font_color");
        ApplyEmptyStyle();
    }

    public override void _Draw()
    {
        if (!_hasCard && _isHovered)
        {
            // 호버 시 골드 점선 테두리 효과
            var rect = new Rect2(Vector2.Zero, Size);
            DrawRect(rect, new Color("#D4AF37", 0.3f), false, 2f);
        }
    }

    private void ApplyEmptyStyle()
    {
        var style = new StyleBoxFlat();
        style.BgColor = new Color("#181828");
        style.BorderColor = new Color("#3A3A5A");
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(10);
        AddThemeStyleboxOverride("panel", style);
    }

    private void OnGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            EmitSignal(SignalName.SlotClicked, _slotIndex);
        }
    }

    private static string GetPositionSymbol(SlotPosition pos) => pos switch
    {
        SlotPosition.Past => "☽",
        SlotPosition.Present => "☀",
        SlotPosition.Future => "☆",
        _ => "◇"
    };

    private static string GetSlotFramePath(SlotPosition pos) => pos switch
    {
        SlotPosition.Past => "res://Assets/Art/Slots/slot_frame_past.png",
        SlotPosition.Present => "res://Assets/Art/Slots/slot_frame_present.png",
        SlotPosition.Future => "res://Assets/Art/Slots/slot_frame_future.png",
        _ => "res://Assets/Art/Slots/slot_frame_present.png"
    };
}
