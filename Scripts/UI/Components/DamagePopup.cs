using Godot;

namespace Sortis.UI.Components;

/// <summary>
/// 피해/방어 팝업 — 위로 떠오르며 페이드아웃.
/// </summary>
public partial class DamagePopup : Label
{
    public static DamagePopup Create(string text, Color color, Vector2 pos)
    {
        var popup = new DamagePopup();
        popup.Text = text;
        popup.AddThemeColorOverride("font_color", color);
        popup.AddThemeFontSizeOverride("font_size", 22);
        popup.Position = pos;
        popup.ZIndex = 100;
        return popup;
    }

    public override void _Ready()
    {
        var tween = CreateTween();
        tween.SetParallel();
        tween.TweenProperty(this, "position:y", Position.Y - 60f, 0.8f)
            .SetEase(Tween.EaseType.Out);
        tween.TweenProperty(this, "modulate:a", 0f, 0.8f)
            .SetDelay(0.3f);
        tween.SetParallel(false);
        tween.TweenCallback(Callable.From(QueueFree));
    }
}
