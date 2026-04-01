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
