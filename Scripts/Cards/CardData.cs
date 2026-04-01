using Godot;
using Sortis.Core;

namespace Sortis.Cards;

/// <summary>
/// 카드 한 장의 정적 데이터를 정의하는 리소스.
/// Godot 에디터에서 .tres 파일로 관리한다.
/// </summary>
[GlobalClass]
public partial class CardData : Resource
{
    [Export] public string CardName { get; set; } = "";
    [Export] public CardType Type { get; set; }
    [Export] public Suit Suit { get; set; }
    [Export] public int Number { get; set; }          // 1~10 (궁정 카드는 11~14)
    [Export] public CourtRank Court { get; set; } = CourtRank.None;
    [Export] public int EnergyCost { get; set; } = 1;

    // --- 정방향 효과 ---
    [Export] public int Damage { get; set; }
    [Export] public int Block { get; set; }
    [Export] public int Draw { get; set; }
    [Export(PropertyHint.MultilineText)]
    public string Description { get; set; } = "";

    [Export] public int Heal { get; set; }

    // --- 역방향 효과 ---
    [Export] public int ReversedDamage { get; set; }
    [Export] public int ReversedBlock { get; set; }
    [Export] public int ReversedDraw { get; set; }
    [Export] public int ReversedHeal { get; set; }
    [Export(PropertyHint.MultilineText)]
    public string ReversedDescription { get; set; } = "";

    // --- 메이저 아르카나 전용 ---
    [Export] public bool IsMajorArcana { get; set; }
    [Export] public int MajorNumber { get; set; }     // 0 (Fool) ~ XXI (World)
}
