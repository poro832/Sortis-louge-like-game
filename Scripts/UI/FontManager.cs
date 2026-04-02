using Godot;

namespace Sortis.UI;

/// <summary>
/// 커스텀 폰트 로딩 — Cinzel (본문), Cinzel Decorative (제목)
/// </summary>
public static class FontManager
{
    private static Font? _title;
    private static Font? _body;

    /// <summary>Cinzel Decorative Bold — 타이틀/헤더용</summary>
    public static Font Title => _title ??= LoadFont("res://Assets/Fonts/CinzelDecorative-Bold.ttf");

    /// <summary>Cinzel Variable — 본문/버튼/카드용</summary>
    public static Font Body => _body ??= LoadFont("res://Assets/Fonts/Cinzel-Variable.ttf");

    private static Font LoadFont(string path)
    {
        var font = GD.Load<Font>(path);
        if (font == null)
            GD.PrintErr($"[FontManager] 폰트 로드 실패: {path}");
        return font!;
    }

    /// <summary>Label에 Cinzel 본문 폰트를 적용</summary>
    public static void ApplyBody(Control control, int? size = null)
    {
        control.AddThemeFontOverride("font", Body);
        if (size.HasValue)
            control.AddThemeFontSizeOverride("font_size", size.Value);
    }

    /// <summary>Label에 Cinzel Decorative 제목 폰트를 적용</summary>
    public static void ApplyTitle(Control control, int? size = null)
    {
        control.AddThemeFontOverride("font", Title);
        if (size.HasValue)
            control.AddThemeFontSizeOverride("font_size", size.Value);
    }
}
