using Godot;
using Sortis.Core;

namespace Sortis.UI.Components;

/// <summary>
/// 전투 페이즈 표시 — 4개 페이즈 중 현재 활성 페이즈를 골드로 강조.
/// </summary>
public partial class PhaseIndicator : HBoxContainer
{
    private static readonly Color GoldColor = new("#D4AF37");
    private static readonly Color DimColor = new("#555555");

    private readonly Label[] _phaseLabels = new Label[4];
    private static readonly string[] PhaseNames = { "배치", "발동", "운명", "적 턴" };

    public override void _Ready()
    {
        Alignment = AlignmentMode.Center;
        AddThemeConstantOverride("separation", 8);

        for (int i = 0; i < 4; i++)
        {
            if (i > 0)
            {
                var arrow = new Label { Text = "→" };
                arrow.AddThemeFontSizeOverride("font_size", 14);
                arrow.AddThemeColorOverride("font_color", DimColor);
                AddChild(arrow);
            }

            _phaseLabels[i] = new Label { Text = PhaseNames[i] };
            _phaseLabels[i].AddThemeFontSizeOverride("font_size", 14);
            _phaseLabels[i].AddThemeColorOverride("font_color", DimColor);
            AddChild(_phaseLabels[i]);
        }
    }

    public void SetActivePhase(BattlePhase phase)
    {
        int activeIndex = (int)phase;
        for (int i = 0; i < 4; i++)
        {
            var color = i == activeIndex ? GoldColor : DimColor;
            _phaseLabels[i].AddThemeColorOverride("font_color", color);
        }
    }
}
