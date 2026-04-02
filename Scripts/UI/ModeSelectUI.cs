using Godot;
using Sortis.Core;
using Sortis.UI.Components;

namespace Sortis.UI;

/// <summary>
/// 모드 선택 화면 — 연습/스토리/무한 모드 중 택 1
/// </summary>
public partial class ModeSelectUI : Control
{
    private static readonly Color BgColor = new("#0D0D1A");
    private static readonly Color GoldColor = new("#D4AF37");
    private static readonly Color TextWhite = new("#E8E8E8");
    private static readonly Color TextGray = new("#666666");
    private static readonly Color CardBg = new("#1A1A2E");
    private static readonly Color CardBorder = new("#2A2A4E");

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

        // 전체 VBox — CenterContainer로 확실한 중앙 배치
        var center = new CenterContainer();
        center.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(center);

        var root = new VBoxContainer();
        root.AddThemeConstantOverride("separation", 30);
        center.AddChild(root);

        // 제목
        var title = new Label();
        title.Text = "게임 모드 선택";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeColorOverride("font_color", GoldColor);
        FontManager.ApplyTitle(title, 36);
        root.AddChild(title);

        // 카드 컨테이너
        var hbox = new HBoxContainer();
        hbox.Alignment = BoxContainer.AlignmentMode.Center;
        hbox.AddThemeConstantOverride("separation", 24);
        root.AddChild(hbox);

        AddModeCard(hbox, "연습 모드", "즉시 전투 시작\n기본 덱으로 전투를\n체험합니다.", true, GameMode.Practice);
        AddModeCard(hbox, "스토리 모드", "타로 마스터의 여정을\n따라가며 운명을\n개척합니다.", false, GameMode.Story);
        AddModeCard(hbox, "무한 모드", "끝없는 전투 속에서\n최고 기록에\n도전합니다.", false, GameMode.Endless);

        // 뒤로가기
        var backBtn = new Button();
        backBtn.Text = "뒤로가기";
        backBtn.CustomMinimumSize = new Vector2(200, 44);
        backBtn.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        backBtn.AddThemeColorOverride("font_color", TextWhite);
        FontManager.ApplyBody(backBtn, 18);

        var backStyle = new StyleBoxFlat();
        backStyle.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        backStyle.BorderColor = TextGray;
        backStyle.SetBorderWidthAll(1);
        backStyle.SetCornerRadiusAll(6);
        backStyle.SetContentMarginAll(8);
        backBtn.AddThemeStyleboxOverride("normal", backStyle);

        backBtn.Pressed += OnBack;
        root.AddChild(backBtn);
    }

    private void AddModeCard(HBoxContainer parent, string modeName, string description, bool enabled, GameMode mode)
    {
        var panel = new PanelContainer();
        panel.CustomMinimumSize = new Vector2(220, 280);

        var panelStyle = new StyleBoxFlat();
        panelStyle.BgColor = enabled ? CardBg : new Color(CardBg, 0.5f);
        panelStyle.BorderColor = enabled ? GoldColor : CardBorder;
        panelStyle.SetBorderWidthAll(enabled ? 2 : 1);
        panelStyle.SetCornerRadiusAll(10);
        panelStyle.SetContentMarginAll(16);
        panel.AddThemeStyleboxOverride("panel", panelStyle);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 16);
        panel.AddChild(vbox);

        // 모드 이름
        var nameLabel = new Label();
        nameLabel.Text = modeName;
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        nameLabel.AddThemeColorOverride("font_color", enabled ? GoldColor : TextGray);
        FontManager.ApplyTitle(nameLabel, 22);
        vbox.AddChild(nameLabel);

        // 설명
        var descLabel = new Label();
        descLabel.Text = description;
        descLabel.HorizontalAlignment = HorizontalAlignment.Center;
        descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        descLabel.AddThemeColorOverride("font_color", enabled ? TextWhite : TextGray);
        FontManager.ApplyBody(descLabel, 14);
        vbox.AddChild(descLabel);

        // 간격 채우기
        var spacer = new Control();
        spacer.SizeFlagsVertical = SizeFlags.ExpandFill;
        vbox.AddChild(spacer);

        if (enabled)
        {
            var selectBtn = new Button();
            selectBtn.Text = "선택";
            selectBtn.CustomMinimumSize = new Vector2(0, 40);

            var btnStyle = new StyleBoxFlat();
            btnStyle.BgColor = new Color(GoldColor, 0.2f);
            btnStyle.BorderColor = GoldColor;
            btnStyle.SetBorderWidthAll(2);
            btnStyle.SetCornerRadiusAll(6);
            btnStyle.SetContentMarginAll(6);
            selectBtn.AddThemeStyleboxOverride("normal", btnStyle);
            selectBtn.AddThemeColorOverride("font_color", GoldColor);
            FontManager.ApplyBody(selectBtn, 16);

            selectBtn.Pressed += () => OnModeSelected(mode);
            vbox.AddChild(selectBtn);
        }
        else
        {
            var disabledLabel = new Label();
            disabledLabel.Text = "준비중";
            disabledLabel.HorizontalAlignment = HorizontalAlignment.Center;
            disabledLabel.AddThemeColorOverride("font_color", TextGray);
            disabledLabel.AddThemeFontSizeOverride("font_size", 16);
            vbox.AddChild(disabledLabel);
        }

        parent.AddChild(panel);
    }

    private void OnModeSelected(GameMode mode)
    {
        SceneManager.Instance.Context.Mode = mode;
        SceneManager.Instance.ChangeScene("res://Scenes/Battle.tscn");
    }

    private void OnBack()
    {
        SceneManager.Instance.ChangeScene("res://Scenes/MainMenu.tscn");
    }
}
