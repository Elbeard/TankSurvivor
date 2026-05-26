# Game vision — TankSurvivor

## Elevator pitch

**Top-down tank shooter** on a growing battlefield. The tank drives, aims with the mouse, and shoots. Enemies pressure the player—often from the **edges of the map**. The longer you survive and the more you score, the more you can **spend score in-run** to upgrade the experience: **better graphics** (sprites, effects, environment), and optionally **better music / sound layers**.

The hook is not only “get stronger”—it is **“the game looks and sounds richer as you invest your run’s score.”**

## Core pillars

| Pillar | Description |
|--------|-------------|
| **Combat** | Move (WASD), rotate turret (mouse), shoot (Fire). Readable, arcade feel. |
| **Arena** | Playable space that can **expand or refresh at the edges** (procedural chunks / edge spawns). |
| **Score economy** | Points from kills, survival time, maybe combos. Score is **currency** during the run. |
| **Presentation upgrades** | Spend score on **visual tier** and **audio tier** upgrades—not only stat boosts. |
| **Pressure** | Enemies and waves keep tension; map edges are natural spawn lanes. |

## Map & edges (design intent)

Two complementary ideas (can combine):

1. **Edge spawning** — Enemies (and later pickups) appear just off-screen or along the border; player reads threat direction from the edge they came from.
2. **Edge generation** — As the player pushes toward the border, **new terrain tiles / chunks** are generated so the arena grows outward. The center stays the “safe” learned space; the frontier is dangerous and new.

For MVP, **edge spawning** is faster to ship with the current project. **Chunk generation** is the long-term map vision.

**Design doc (RU):** [project-design.md](project-design.md) — biomes as chunk base, decor prefab sets on top, visual tiers from score shop reload visible chunks.

## Score → upgrades (main progression fantasy)

During a **single run**:

```
Kill enemies / survive → earn Score
        ↓
Open shop (pause, between waves, or always-on UI)
        ↓
Spend Score on upgrades
        ├─ Visual tier  (sprites, VFX, lighting, tiles, UI polish)
        ├─ Audio tier   (music track, ambience, SFX mix)
        └─ (Optional) Combat stats (damage, fire rate) — secondary to vision
```

### Visual upgrade examples (tiers)

| Tier | What changes |
|------|----------------|
| 0 — Boot | Flat sprites, minimal particles, simple ground |
| 1 — Enhanced | Tank/enemy sprite swap, muzzle flash, shell trails |
| 2 — Rich | Hit sparks, explosion sprites, ground detail / tile variation |
| 3 — Premium | Post-processing (bloom, color grading), screen shake, weather/light |

Implementation idea: **`PresentationTier` ScriptableObject** lists prefabs/materials/VFX per tier; `PresentationManager` applies tier to player, enemies, tilemaps, and global volume.

### Audio upgrade examples

| Tier | What changes |
|------|----------------|
| 0 | Single loop or silence + basic SFX |
| 1 | Full music loop via existing `AudioManager` |
| 2 | Layered ambience + punchier mixer snapshot |
| 3 | Intensity stems (add percussion layer when tier 3 bought) |

Reuse: `AudioManager`, `AudioPool`, `AudioMixer`, `SimpleAudioEvent`.

## What is already built (foundation)

- Tank movement, turret, shooting (`TankGun`, `ProjectilePool`)
- Enemy chase (`EnemyAwarenessController`, `EnemyMovement`)
- Gunshot SFX pool (`AudioPool`, `SimpleAudioEvent`)
- Music/ambience skeleton (`Audio.AudioManager`)
- Scene: `Scene_1`

## What this vision adds on top

- **Score** + UI
- **Shop** (spend score on presentation tiers)
- **Health**, damage, enemy death (so score has a source)
- **Edge wave spawner**
- **Map chunks at edges** (later)
- **GameManager** as session owner (score, tier, game over)

## Success criteria (prototype)

A player can:

1. Drive and shoot for 2–3 minutes.
2. Earn score from kills.
3. Open a shop and buy **at least one visual tier** (noticeable sprite or VFX change).
4. Optionally buy **one audio tier** (music starts or improves).
5. Feel enemies coming from **map edges**.

## Related docs

- [gameplay-loop.md](gameplay-loop.md) — session flow with shop
- [implementation-plan.md](implementation-plan.md) — phased tasks mapped to this repo
- [roadmap.md](roadmap.md) — checklist
