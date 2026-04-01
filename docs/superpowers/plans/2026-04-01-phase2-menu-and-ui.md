# Phase 2: 메인 메뉴 + 모드 선택 + 전투 UI 개선

## Context
현재 Sortis는 Battle.tscn 하나로 바로 전투에 진입한다. 게임답게 시작 화면, 모드 선택, 그리고 전투 UI를 시각적으로 개선해야 한다.

---

## Part A: 인프라 (씬 전환 시스템)

### Task A1: Enums 추가 + GameContext
**수정:** `Scripts/Core/Enums.cs` — `GameMode`, `BattlePhase` enum 추가
**생성:** `Scripts/Core/GameContext.cs` — 씬 간 공유 상태 (선택된 모드 등)

```
GameMode: Practice, Story, Endless
BattlePhase: Placement, Activation, Fate, EnemyTurn
```

### Task A2: SceneManager 싱글톤
**생성:** `Scripts/Core/SceneManager.cs` — autoload Node
- `GameContext Context` 보유
- `ChangeScene(string path)` 메서드
- static `Instance` 패턴

### Task A3: project.godot 업데이트
- `run/main_scene` → `"res://Scenes/MainMenu.tscn"`
- `[autoload]` 섹션에 `SceneManager="*res://Scripts/Core/SceneManager.cs"` 추가

---

## Part B: 메뉴 시스템

### Task B1: 메인 메뉴
**생성:** `Scenes/MainMenu.tscn` + `Scripts/UI/MainMenuUI.cs`

```
┌─────────────────────────────────┐
│         S O R T I S             │
│    타로 로그라이크 덱빌더       │
│                                 │
│        [ 새 게임 ]              │
│        [ 이어하기 (준비중) ]    │
│        [ 카드 도감 (준비중) ]   │
│        [ 설정 (준비중) ]        │
│        [ 크레딧 (준비중) ]      │
│        [ 종료 ]                 │
└─────────────────────────────────┘
```
- 배경: 어두운 남색 (#0D0D1A), 골드 테마 (#D4AF37)
- "새 게임" → ModeSelect 씬으로 이동
- "종료" → `GetTree().Quit()`
- 나머지 버튼: disabled + "(준비중)" 표시

### Task B2: 모드 선택 화면
**생성:** `Scenes/ModeSelect.tscn` + `Scripts/UI/ModeSelectUI.cs`

```
┌──────────────────────────────────────┐
│          게임 모드 선택               │
│                                      │
│  ┌──────┐  ┌──────┐  ┌──────┐       │
│  │연습  │  │스토리│  │무한  │       │
│  │모드  │  │모드  │  │모드  │       │
│  │      │  │준비중│  │준비중│       │
│  │[선택]│  │      │  │      │       │
│  └──────┘  └──────┘  └──────┘       │
│                                      │
│           [ 뒤로가기 ]               │
└──────────────────────────────────────┘
```
- 3개 ���드 카드형 패널 (HBoxContainer)
- 연습 모드만 활성, 클릭 시 `GameContext.Mode = Practice` → Battle.tscn
- 나머지 "준비중" 오버레이

### Task B3: BattleUI에 메뉴 복귀 버튼
**수정:** `Scripts/UI/BattleUI.cs`
- `ShowBattleResult()`에 "메인 메뉴" 버튼 추가 (재시작 옆)
- SceneManager로 MainMenu.tscn 이동

---

## Part C: 전투 UI 개선

### Task C1: HealthBar 컴포넌트 + 적 HP바/인텐트
**생성:** `Scripts/UI/Components/HealthBar.cs`
- `Control` 상속, `_Draw()` 오버라이드로 커스텀 그리기
- 배경 바 + 채워진 바 + 테두리 + "현재/최대" 텍스트
- `SetValue(int current, int max)` → `QueueRedraw()`

**수정:** `Scripts/UI/BattleUI.cs` — 적 영역
- `_enemyInfoLabel` 텍스트 → HealthBar + 인텐트 아이콘(공격⚔/방어🛡 유니코드 or 컬러 심볼)
- 적 이름 Label + HP바 + 인텐트 표시를 HBoxContainer로 구성

### Task C2: 플레이어 HP바 + 에너지 표시
**생성:** `Scripts/UI/Components/EnergyDisplay.cs`
- HBoxContainer, 에너지를 골드/회색 원으로 표시
- `SetEnergy(int current, int max)`

**수정:** `Scripts/UI/BattleUI.cs` — 플레이어 영역
- `_playerInfoLabel` 텍스트 → HealthBar(녹색) + EnergyDisplay + 방어/연속적중 아이콘

### Task C3: 카드 효과 애니메이션
**생성:** `Scripts/UI/Components/DamagePopup.cs`
- Label 상속, `CreateTween()`으로 위로 떠오르며 페이드아웃 (0.8초)
- `static DamagePopup Create(string text, Color color, Vector2 pos)`

**수정:** `Scripts/UI/BattleUI.cs`
- 오버레이 컨테이너 추가 (RefreshUI에서 안 지워지는 별도 레이어)
- `OnActivatePressed()` → 적에게 피해 팝업, 플레이어에 방어 팝업
- `OnFateSelected()` → 운명 효과 팝업
- 적 공격 시 → 플레이어에 피해 팝업 (빨간색)

### Task C4: 턴/페이즈 진행 표시
**생성:** `Scripts/UI/Components/PhaseIndicator.cs`
- HBoxContainer, 4개 페이즈 Label: 배치 → 발동 → 운명 → 적 턴
- `SetActivePhase(BattlePhase)` — 활성 페이즈 골드 강조, 나���지 회색

**수정:** `Scripts/UI/BattleUI.cs`
- 상단에 PhaseIndicator + "턴 N" Label 배치
- 각 전환 시점���서 페이즈 업데이트

---

## 구현 순서

| 순서 | Task | 내용 | 의존성 |
|------|------|------|--------|
| 1 | A1 | GameContext + enum | 없음 |
| 2 | A2 | SceneManager | A1 |
| 3 | A3 | project.godot | A2 |
| 4 | B1 | 메인 메뉴 | A2, A3 |
| 5 | B2 | 모드 선택 | B1 |
| 6 | B3 | 전투→메뉴 복귀 | B2 |
| 7 | C1 | 적 HP바 + 인텐트 | 없음 |
| 8 | C2 | 플레이어 HP바 + 에너지 | C1 (HealthBar 재사용) |
| 9 | C4 | 페이즈 표시 | 없음 |
| 10 | C3 | 카드 효과 애니메이션 | 없음 (마지��� 권장) |

## 파일 목록

**생성 (10개):**
- `Scripts/Core/GameContext.cs`
- `Scripts/Core/SceneManager.cs`
- `Scripts/UI/MainMenuUI.cs`
- `Scripts/UI/ModeSelectUI.cs`
- `Scripts/UI/Components/HealthBar.cs`
- `Scripts/UI/Components/EnergyDisplay.cs`
- `Scripts/UI/Components/DamagePopup.cs`
- `Scripts/UI/Components/PhaseIndicator.cs`
- `Scenes/MainMenu.tscn`
- `Scenes/ModeSelect.tscn`

**수정 (3개):**
- `Scripts/Core/Enums.cs` — GameMode, BattlePhase 추가
- `Scripts/UI/BattleUI.cs` — HP바, 에너지, 애니메이션, 페이즈, 메뉴 버튼
- `project.godot` — main_scene 변경 + autoload 추가

## 검증 방법
1. **A1~A3 후:** 빌드 성공 확인
2. **B1 후:** 게임 실행 → 메인 메뉴 표시, "종료" 클릭 시 종료
3. **B2 후:** 새 게임 → 모드 선택 → 연습 모드 → 전투 진입 확인
4. **B3 후:** 전투 종료 → "메인 메뉴" → 메인 메뉴 복귀 (전체 루프)
5. **C1~C4 후:** 전투에서 시각적 HP바, 에너지 표시, 피해 팝업, 페이즈 표시 확인
6. **전체:** 빌드 경고 0개, 오류 0개
