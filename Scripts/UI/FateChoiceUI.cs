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

    public new void Hide()
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
        button.Text = "선택";
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
