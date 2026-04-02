using Godot;

namespace Sortis.UI.Components;

/// <summary>
/// 커스텀 HP 바 — 라운드 코너 + 텍스트 오버레이.
/// </summary>
public partial class HealthBar : Control
{
    private int _current;
    private int _max = 1;
    private Color _fillColor = new("#27AE60");
    private Color _bgColor = new("#1A1A1A");
    private Color _borderColor = new("#444444");

    public HealthBar()
    {
        CustomMinimumSize = new Vector2(200, 22);
    }

    public void SetColors(Color fill, Color bg, Color border)
    {
        _fillColor = fill;
        _bgColor = bg;
        _borderColor = border;
    }

    public void SetValue(int current, int max)
    {
        _current = current;
        _max = max > 0 ? max : 1;
        QueueRedraw();
    }

    public override void _Draw()
    {
        var rect = new Rect2(Vector2.Zero, Size);
        float ratio = Mathf.Clamp((float)_current / _max, 0f, 1f);

        // 배경 (라운드)
        DrawRect(rect, _bgColor);

        // 채워진 부분
        if (ratio > 0)
        {
            var fillRect = new Rect2(Vector2.Zero, new Vector2(Size.X * ratio, Size.Y));
            DrawRect(fillRect, _fillColor);

            // 하이라이트 (위쪽 밝은 줄)
            var highlightRect = new Rect2(1, 1, Size.X * ratio - 2, Size.Y * 0.35f);
            DrawRect(highlightRect, new Color(1, 1, 1, 0.12f));
        }

        // 테두리
        DrawRect(rect, _borderColor, false, 1.5f);

        // 텍스트
        var text = $"{_current}/{_max}";
        var font = ThemeDB.FallbackFont;
        int fontSize = 12;
        var textSize = font.GetStringSize(text, HorizontalAlignment.Center, -1, fontSize);
        var textPos = new Vector2(
            (Size.X - textSize.X) / 2f,
            (Size.Y + textSize.Y) / 2f - 2f
        );
        // 텍스트 그림자
        DrawString(font, textPos + new Vector2(1, 1), text, HorizontalAlignment.Left, -1, fontSize, new Color(0, 0, 0, 0.6f));
        DrawString(font, textPos, text, HorizontalAlignment.Left, -1, fontSize, Colors.White);
    }
}
