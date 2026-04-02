using Godot;
using Sortis.Core;
using Sortis.UI.Components;

namespace Sortis.UI;

/// <summary>
/// 메인 메뉴 — 새 게임, 이어하기, 도감, 설정, 크레딧, 종료
/// </summary>
public partial class MainMenuUI : Control
{
    private static readonly Color BgColor = new("#0D0D1A");
    private static readonly Color GoldColor = new("#D4AF37");
    private static readonly Color DimGold = new("#8B7325");
    private static readonly Color TextWhite = new("#E8E8E8");
    private static readonly Color TextGray = new("#666666");

    public override void _Ready()
    {
        BuildUI();
    }

    private void BuildUI()
    {
        // 셰이더 배경
        var bg = new ColorRect();
        bg.Color = Colors.White;
        bg.SetAnchorsPreset(LayoutPreset.FullRect);
        var shaderMat = new ShaderMaterial();
        shaderMat.Shader = GD.Load<Shader>("res://Shaders/menu_bg.gdshader");
        bg.Material = shaderMat;
        AddChild(bg);

        // 파티클
        AddChild(new MysticParticles(MysticParticles.ParticleStyle.Gold));

        // 중앙 VBox — CenterContainer로 확실한 중앙 배치
        var center = new CenterContainer();
        center.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(center);

        var vbox = new VBoxContainer();
        vbox.CustomMinimumSize = new Vector2(400, 0);
        vbox.AddThemeConstantOverride("separation", 12);
        center.AddChild(vbox);

        // 타이틀
        var title = new Label();
        title.Text = "S O R T I S";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeColorOverride("font_color", GoldColor);
        FontManager.ApplyTitle(title, 48);
        vbox.AddChild(title);

        // 서브타이틀
        var subtitle = new Label();
        subtitle.Text = "타로 로그라이크 덱빌더";
        subtitle.HorizontalAlignment = HorizontalAlignment.Center;
        subtitle.AddThemeColorOverride("font_color", TextWhite);
        FontManager.ApplyBody(subtitle, 18);
        vbox.AddChild(subtitle);

        // 간격
        var spacer = new Control();
        spacer.CustomMinimumSize = new Vector2(0, 40);
        vbox.AddChild(spacer);

        // 버튼들
        AddMenuButton(vbox, "새 게임", true, OnNewGame);
        AddMenuButton(vbox, "이어하기 (준비중)", false, null);
        AddMenuButton(vbox, "카드 도감 (준비중)", false, null);
        AddMenuButton(vbox, "설정 (준비중)", false, null);
        AddMenuButton(vbox, "크레딧 (준비중)", false, null);
        AddMenuButton(vbox, "종료", true, OnQuit);
    }

    private void AddMenuButton(VBoxContainer parent, string text, bool enabled, System.Action? onPressed)
    {
        var btn = new Button();
        btn.Text = text;
        btn.CustomMinimumSize = new Vector2(300, 48);
        btn.Disabled = !enabled;

        // 스타일
        var styleNormal = new StyleBoxFlat();
        styleNormal.BgColor = enabled ? new Color(GoldColor, 0.15f) : new Color(0.1f, 0.1f, 0.1f, 0.5f);
        styleNormal.BorderColor = enabled ? GoldColor : DimGold;
        styleNormal.SetBorderWidthAll(enabled ? 2 : 1);
        styleNormal.SetCornerRadiusAll(6);
        styleNormal.SetContentMarginAll(8);
        btn.AddThemeStyleboxOverride("normal", styleNormal);

        var styleHover = new StyleBoxFlat();
        styleHover.BgColor = new Color(GoldColor, 0.3f);
        styleHover.BorderColor = GoldColor;
        styleHover.SetBorderWidthAll(2);
        styleHover.SetCornerRadiusAll(6);
        styleHover.SetContentMarginAll(8);
        btn.AddThemeStyleboxOverride("hover", styleHover);

        var styleDisabled = new StyleBoxFlat();
        styleDisabled.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);
        styleDisabled.BorderColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        styleDisabled.SetBorderWidthAll(1);
        styleDisabled.SetCornerRadiusAll(6);
        styleDisabled.SetContentMarginAll(8);
        btn.AddThemeStyleboxOverride("disabled", styleDisabled);

        btn.AddThemeColorOverride("font_color", enabled ? GoldColor : TextGray);
        btn.AddThemeColorOverride("font_hover_color", TextWhite);
        btn.AddThemeColorOverride("font_disabled_color", TextGray);
        FontManager.ApplyBody(btn, 20);

        if (onPressed != null)
            btn.Pressed += () => onPressed();

        parent.AddChild(btn);
    }

    private void OnNewGame()
    {
        SceneManager.Instance.ChangeScene("res://Scenes/ModeSelect.tscn");
    }

    private void OnQuit()
    {
        GetTree().Quit();
    }
}
