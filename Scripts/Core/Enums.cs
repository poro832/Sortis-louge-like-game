namespace Sortis.Core;

/// <summary>
/// 4개 마이너 아르카나 수트 — 각 수트가 전투 역할을 담당한다.
/// </summary>
public enum Suit
{
    Wands,      // 완드 — 공격 (직접 데미지)
    Swords,     // 검   — 기술 (드로우/디버프)
    Cups,       // 컵   — 방어 (블록/회복)
    Pentacles   // 펜타클 — 자원 (에너지/골드)
}

/// <summary>
/// 카드 종류: 마이너(숫자/궁정) vs 메이저 아르카나
/// </summary>
public enum CardType
{
    Minor,
    Major
}

/// <summary>
/// 궁정 카드 등급
/// </summary>
public enum CourtRank
{
    None,       // 숫자 카드 (1~10)
    Page,       // 시종 — 비용 0, 시너지 트리거
    Knight,     // 기사 — 위치별 조건부 강화
    Queen,      // 여왕 — 지속 효과
    King        // 왕   — 고비용 고위력
}

/// <summary>
/// 스프레드 슬롯 위치 — 타로 리딩의 시간축
/// </summary>
public enum SlotPosition
{
    Past,       // 과거 — 디버프 부여
    Present,    // 현재 — 즉시 데미지/방어
    Future,     // 미래 — 다음 턴 효과 예약
    Center,     // 중앙 (크로스 스프레드)
    Above,      // 상단
    Below,      // 하단
    Left,       // 좌측
    Right       // 우측
}

/// <summary>
/// 카드 방향 — 정방향이면 기본 효과, 역방향이면 변형 효과
/// </summary>
public enum CardOrientation
{
    Upright,    // 정방향
    Reversed    // 역방향 — 효과 반전/변형
}

/// <summary>
/// 게임 모드
/// </summary>
public enum GameMode
{
    Practice,   // 연습 모드 — 즉시 전투
    Story,      // 스토리 모드 — 맵 진행
    Endless     // 무한 모드 — 끝없는 전투
}

/// <summary>
/// 전투 페이즈
/// </summary>
public enum BattlePhase
{
    Placement,  // 카드 배치
    Activation, // 스프레드 발동
    Fate,       // 운명 선택
    EnemyTurn   // 적 턴
}
