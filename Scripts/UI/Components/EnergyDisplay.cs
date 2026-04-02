using Godot;

namespace Sortis.UI.Components;

/// <summary>
/// 에너지 표시 — 골드/회색 원으로 에너지 시각화.
/// </summary>
public partial class EnergyDisplay : Control
{
    private int _current;
    private int _max = 3;
    private static readonly Color GoldColor = new("#D4AF37");
    private static readonly Color EmptyColor = new("#333333");

    public EnergyDisplay()
    {
        CustomMinimumSize = new Vector2(120, 28);
    }

    public void SetEnergy(int current, int max)
    {
        _current = current;
        _max = max > 0 ? max : 1;
        CustomMinimumSize = new Vector2(max * 30, 28);
        QueueRedraw();
    }

    public override void _Draw()
    {
        float radius = 10f;
        float spacing = 28f;
        float startX = radius + 2f;
        float centerY = Size.Y / 2f;

        for (int i = 0; i < _max; i++)
        {
            var center = new Vector2(startX + i * spacing, centerY);
            var color = i < _current ? GoldColor : EmptyColor;
            DrawCircle(center, radius, color);
            DrawArc(center, radius, 0, Mathf.Tau, 24, new Color("#555555"), 1.5f);
        }
    }
}
