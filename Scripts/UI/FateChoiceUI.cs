using Godot;
using Sortis.Combat;
using System;

namespace Sortis.UI;

public partial class FateChoiceUI : PanelContainer
{
    private static readonly Color GoldColor = new("#D4AF37");
    private static readonly Color BgColor = new("#0D0D1A");

    private VBoxContainer _root = null!;
    private HBoxContainer _container = null!;
    private FateChoice? _fateChoice;

    [Signal] public delegate void FateSelectedEventHandler(int optionIndex);

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(520, 260);
        Visible = false;

        var style = new StyleBoxFlat();
        style.BgColor = new Color(BgColor, 0.95f);
        style.BorderColor = GoldColor;
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(14);
        style.ShadowColor = new Color(GoldColor, 0.2f);
        style.ShadowSize = 12;
        style.SetContentMarginAll(20);
        AddThemeStyleboxOverride("panel", style);

        _root = new VBoxContainer();
        _root.AddThemeConstantOverride("separation", 16);
        AddChild(_root);

        var title = new Label();
        title.Text = "⚜ 운명을 선택하세요 ⚜";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeFontSizeOverride("font_size", 22);
        title.AddThemeColorOverride("font_color", GoldColor);
        _root.AddChild(title);

        _container = new HBoxContainer();
        _container.Alignment = BoxContainer.AlignmentMode.Center;
        _container.AddThemeConstantOverride("separation", 20);
        _root.AddChild(_container);
    }

    public void ShowChoices(FateChoice fateChoice)
    {
        _fateChoice = fateChoice;

        foreach (var child in _container.GetChildren())
            child.QueueFree();

        AddOptionCard(fateChoice.OptionA, 0);
        AddOptionCard(fateChoice.OptionB, 1);

        Visible = true;
    }

    public new void Hide()
    {
        Visible = false;
    }

    private void AddOptionCard(FateOption option, int index)
    {
        var panel = new PanelContainer();
        panel.CustomMinimumSize = new Vector2(220, 170);

        var style = new StyleBoxFlat();
        style.BgColor = new Color("#14142A");
        style.BorderColor = new Color("#4A4A6A");
        style.SetBorderWidthAll(1);
        style.SetCornerRadiusAll(10);
        style.SetContentMarginAll(14);
        panel.AddThemeStyleboxOverride("panel", style);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 10);

        var titleLabel = new Label();
        titleLabel.Text = option.Name;
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 18);
        titleLabel.AddThemeColorOverride("font_color", GoldColor);
        vbox.AddChild(titleLabel);

        var desc = new Label();
        desc.Text = option.Description;
        desc.HorizontalAlignment = HorizontalAlignment.Center;
        desc.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        desc.AddThemeFontSizeOverride("font_size", 13);
        desc.AddThemeColorOverride("font_color", new Color("#CCCCCC"));
        vbox.AddChild(desc);

        var spacer = new Control();
        spacer.SizeFlagsVertical = SizeFlags.ExpandFill;
        vbox.AddChild(spacer);

        var button = new Button();
        button.Text = "선택";
        button.CustomMinimumSize = new Vector2(0, 36);

        var btnStyle = new StyleBoxFlat();
        btnStyle.BgColor = new Color(GoldColor, 0.15f);
        btnStyle.BorderColor = GoldColor;
        btnStyle.SetBorderWidthAll(1);
        btnStyle.SetCornerRadiusAll(6);
        btnStyle.SetContentMarginAll(6);
        button.AddThemeStyleboxOverride("normal", btnStyle);
        button.AddThemeColorOverride("font_color", GoldColor);
        button.AddThemeFontSizeOverride("font_size", 15);

        int capturedIndex = index;
        button.Pressed += () => OnOptionSelected(capturedIndex);
        vbox.AddChild(button);

        panel.AddChild(vbox);
        _container.AddChild(panel);
    }

    private void OnOptionSelected(int index)
    {
        EmitSignal(SignalName.FateSelected, index);
    }
}
