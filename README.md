# TankSurvivor

2D **top-down tank shooter**: drive, aim with the mouse, shoot. Enemies pressure you from the **edges of the arena**. Earn **score** during a run and spend it in a shop to upgrade **graphics** and **music**—the game should look and sound richer as you invest points.

## Quick start

1. Open the project in **Unity** (`ProjectSettings/ProjectVersion.txt`).
2. Open **`Assets/Scenes/Scene_1.unity`**.
3. Press **Play**.

## Controls

| Action | Input |
|--------|--------|
| Move | WASD / left stick |
| Turret | Mouse (Look) |
| Fire | LMB / **Fire** (Input System) |

[docs/controls.md](docs/controls.md)

## Язык / Language

- Разработчик: **русский** (чтение и речь).
- Ответы агента в этом репозитории: **по-русски** (см. `.cursor/rules/language-russian.mdc`).
- Комментарии в `*.cs`: **русский**, UTF-8 BOM; **git-коммиты**: **английский** — [docs/dev-conventions.md](docs/dev-conventions.md).
- Голос → текст в поле чата: [docs/cursor-russian-voice.md](docs/cursor-russian-voice.md).

## Documentation

| Doc | Content |
|-----|---------|
| [docs/cursor-russian-voice.md](docs/cursor-russian-voice.md) | Русский в чате и настройка микрофона Cursor |
| [docs/vision.md](docs/vision.md) | **Core vision** — score shop, visual/audio tiers, map edges |
| [docs/implementation-plan.md](docs/implementation-plan.md) | Phased plan mapped to this repo |
| [docs/game-overview.md](docs/game-overview.md) | Overview (RU) |
| [docs/gameplay-loop.md](docs/gameplay-loop.md) | Session loop (RU) |
| [docs/systems.md](docs/systems.md) | Code & systems status |
| [docs/roadmap.md](docs/roadmap.md) | Checklist |
| [docs/chunk-streaming-design.md](docs/chunk-streaming-design.md) | Чанки карты: дизайн и алгоритм |
| [Assets/WorldChunks/README.md](Assets/WorldChunks/README.md) | **Рабочая реализация** чанков в проекте |
| [docs/dev-conventions.md](docs/dev-conventions.md) | Кодировка, комментарии, язык коммитов |

## Current vs vision

| Vision | Status |
|--------|--------|
| Tank drive + shoot | ✅ |
| Edge enemy pressure | ⏳ Planned |
| Score → buy better graphics | ⏳ Planned |
| Score → buy better music | ⏳ Planned (audio stack started) |
| Procedural map (chunk streaming) | ✅ `Assets/WorldChunks` |

## Key assets

- `Assets/Prefabs/Player.prefab` — `TankGun`, `ProjectilePool`, movement, turret
- `Assets/Prefabs/SimpleEnemy.prefab`
- `Assets/Prefabs/Projectile.prefab`
- `Assets/Sounds/Explosion.asset` — gunshot `SimpleAudioEvent`
- `Scene_1` — `AudioManager`, `AudioPool`, `WorldChunkSystem`
- `Assets/WorldChunks/` — стриминг карты чанками

## Stack

Unity 2D · Input System · `ObjectPool` (projectiles, audio sources)
