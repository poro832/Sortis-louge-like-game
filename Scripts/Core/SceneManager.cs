using Godot;

namespace Sortis.Core;

/// <summary>
/// Autoload 싱글톤 — 씬 전환(페이드 효과 포함) 및 GameContext 보유.
/// </summary>
public partial class SceneManager : Node
{
    public static SceneManager Instance { get; private set; } = null!;
    public GameContext Context { get; private set; } = new();

    private ColorRect _fadeRect = null!;
    private const float FadeDuration = 0.35f;

    public override void _Ready()
    {
        Instance = this;

        // 페이드용 오버레이 (CanvasLayer로 항상 최상단)
        var canvas = new CanvasLayer();
        canvas.Layer = 100;
        AddChild(canvas);

        _fadeRect = new ColorRect();
        _fadeRect.Color = new Color(0, 0, 0, 1);
        _fadeRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _fadeRect.MouseFilter = Control.MouseFilterEnum.Ignore;
        canvas.AddChild(_fadeRect);

        // 시작 시 페이드 인
        FadeIn();
    }

    public void ChangeScene(string path)
    {
        // 페이드 아웃 → 씬 전환 → 페이드 인
        var tween = CreateTween();
        tween.TweenProperty(_fadeRect, "color:a", 1.0f, FadeDuration);
        tween.TweenCallback(Callable.From(() =>
        {
            GetTree().ChangeSceneToFile(path);
            FadeIn();
        }));
    }

    private void FadeIn()
    {
        var tween = CreateTween();
        _fadeRect.Color = new Color(0, 0, 0, 1);
        tween.TweenProperty(_fadeRect, "color:a", 0.0f, FadeDuration);
    }
}
