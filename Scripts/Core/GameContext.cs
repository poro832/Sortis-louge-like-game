namespace Sortis.Core;

/// <summary>
/// 씬 간 공유 상태 — SceneManager가 보유한다.
/// </summary>
public class GameContext
{
    public GameMode Mode { get; set; } = GameMode.Practice;
    public int RunSeed { get; set; }
}
