# Sortis — Tarot Roguelike Deckbuilder

> **"Manipulate fate through tarot readings"**

**Sortis**는 타로 카드를 기반으로 한 로그라이크 덱빌더 게임입니다.
플레이어는 떠돌이 점술사가 되어, 타로 스프레드(배치)로 적의 운명을 바꾸고 자신의 운명을 개척합니다.

## Core Features

### 🃏 Spread System
일반 덱빌더처럼 카드를 한 장씩 내는 것이 아니라, **스프레드 슬롯에 배치한 뒤 일괄 발동**합니다.

| Spread | Slots | Unlock |
|--------|-------|--------|
| One Card | 1 | Start |
| Three Card | 3 (Past/Present/Future) | Start |
| Cross | 5 | Act 2 |
| Celtic Cross | 10 | Act 3 |

### 🔮 Prophecy System
매 턴 제시되는 **예언 조건**을 충족하면 보너스 배율이 상승합니다.
연속 적중으로 폭발적인 콤보를 만들어내세요.

### ↩️ Reversed Cards
타로의 역방향 리딩을 차용 — 카드가 뒤집혀 나오면 효과가 변형됩니다.
리스크와 리턴을 저울질하는 전략적 선택지입니다.

## Card Structure (78 Cards)

- **Minor Arcana (56)** — 4 Suits × 14 Cards
  - ♦ **Wands** (Fire) → Attack / Direct Damage
  - ⚔ **Swords** (Air) → Skills / Draw & Debuff
  - 🏆 **Cups** (Water) → Defense / Block & Heal
  - ⭐ **Pentacles** (Earth) → Resource / Energy & Gold

- **Major Arcana (22)** — Run-changing power cards
  - The Fool, The Magician, Death, The Tower, The World...

## Tech Stack

| | |
|---|---|
| **Engine** | Godot 4 |
| **Language** | C# / .NET 8 |
| **Platform** | Steam (PC) |
| **Genre** | Roguelike Deckbuilder |
| **Inspiration** | Balatro, Slay the Spire, Inscryption |

## Project Structure

```
Sortis/
├── Scripts/
│   ├── Cards/          # Card data & effects
│   ├── Combat/         # Battle & spread system
│   ├── Prophecy/       # Prophecy condition engine
│   ├── Map/            # Run map generation
│   └── Core/           # Game state, enums, resources
├── Scenes/             # Godot scene files
├── Resources/          # Card definitions, balance data
└── Assets/             # Art, audio, UI
```

## Development Status

🚧 **Pre-production** — Core system prototyping phase

## License

All rights reserved. This repository is for development purposes only.
