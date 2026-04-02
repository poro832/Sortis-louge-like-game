using Godot;

namespace Sortis.UI.Components;

/// <summary>
/// 화면에 떠다니는 신비로운 파티클 — 별가루/마법 먼지.
/// </summary>
public partial class MysticParticles : CpuParticles2D
{
    public enum ParticleStyle { Gold, Purple, Mixed }

    private ParticleStyle _style;

    public MysticParticles(ParticleStyle style = ParticleStyle.Gold)
    {
        _style = style;
    }

    public override void _Ready()
    {
        Emitting = true;
        Amount = 50;
        Lifetime = 5.0;
        SpeedScale = 0.8;
        Explosiveness = 0.0f;
        Randomness = 1.0f;

        // 전체 화면에 퍼지도록 설정
        EmissionShape = EmissionShapeEnum.Rectangle;
        EmissionRectExtents = new Vector2(640, 360);
        Position = new Vector2(640, 360);

        // 느리게 위로 떠오름
        Direction = new Vector2(0, -1);
        Spread = 45f;
        InitialVelocityMin = 10f;
        InitialVelocityMax = 30f;
        Gravity = new Vector2(0, -3);

        // 파티클 크기 — 충분히 보이도록
        ScaleAmountMin = 2.5f;
        ScaleAmountMax = 5.0f;

        // 크기 커브: 페이드인 → 유지 → 페이드아웃
        var scaleCurve = new Curve();
        scaleCurve.AddPoint(new Vector2(0, 0));
        scaleCurve.AddPoint(new Vector2(0.15f, 1));
        scaleCurve.AddPoint(new Vector2(0.75f, 1));
        scaleCurve.AddPoint(new Vector2(1, 0));
        ScaleAmountCurve = scaleCurve;

        // 색상 — 밝고 눈에 띄게
        switch (_style)
        {
            case ParticleStyle.Gold:
                Color = new Color("#D4AF37", 0.6f);
                break;
            case ParticleStyle.Purple:
                Color = new Color("#9B59B6", 0.5f);
                break;
            case ParticleStyle.Mixed:
                var gradient = new Gradient();
                gradient.SetColor(0, new Color("#D4AF37", 0.55f));
                gradient.AddPoint(0.5f, new Color("#9B59B6", 0.45f));
                gradient.SetColor(2, new Color("#5DADE2", 0.4f));
                ColorRamp = gradient;
                break;
        }

        // 배경 위, UI 아래에 배치
        ZIndex = 0;
    }
}
