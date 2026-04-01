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
