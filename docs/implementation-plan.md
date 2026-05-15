# Implementation plan — from current repo to the vision

This plan maps the [vision](vision.md) to **TankSurvivor** as it exists today (`Scene_1`, `Player`, `TankGun`, `AudioPool`, etc.).

---

## Phase 0 — Already done ✅

| Item | Assets / scripts |
|------|------------------|
| Top-down tank move + aim | `Player`, `PlayerMovement`, `TowerRotation` |
| Shooting + projectile pool | `TankGun`, `ProjectilePool`, `Projectile` |
| Enemy chase | `SimpleEnemy`, `EnemyAwarenessController`, `EnemyMovement` |
| Gunshot SFX | `AudioPool`, `Explosion.asset` (gunshots) |
| Audio stack stub | `AudioManager`, `AudioMixer` |

---

## Phase 1 — Score & combat loop (1–2 weeks)

**Goal:** Score has a source; run can end.

| Task | Suggestion |
|------|------------|
| `Health` component | HP, `TakeDamage`, `OnDeath` event |
| Enable projectile damage | Uncomment / fix `Projectile.OnTriggerEnter2D`, set `damageLayers` |
| Enemy death | Disable or pool enemy; fire event `OnEnemyKilled` |
| `ScoreManager` | Singleton or `GameManager` field: `AddScore(int)`, `CurrentScore` |
| Award rules | e.g. +10 per kill, +1 per second survived |
| Minimal UI | `Canvas`: score text, HP bar (`UnityEngine.UI` or TMP) |
| Fill `GameManager` | Start run, track time, subscribe to death → Game Over |

**Touches existing code:** `Projectile.cs`, new `Health.cs`, `GameManager.cs`, enemy prefab.

---

## Phase 2 — Edge pressure (3–5 days)

**Goal:** Threat from map borders (matches “around the edges”).

| Task | Suggestion |
|------|------------|
| `EdgeSpawnController` | Empty transforms on N/E/S/W or circle of points outside camera |
| `WaveSpawner` | Timer + spawn `SimpleEnemy` at random edge point |
| Camera bounds | Optional `Collider2D` bounds so player stays on arena |
| Tune awareness | Raise `EnemyAwarenessController._playerAwarenessDistance` |

No procedural map yet—use a **fixed arena** with invisible walls and spawns at edges.

---

## Phase 3 — Score shop & presentation tiers (1–2 weeks) ⭐ Core vision

**Goal:** Spend score to improve **graphics** (and optionally music).

### Data

```text
Assets/ScriptableObjects/
  PresentationTier.asset   (tier index, cost, sprite sets, prefab refs)
  AudioTier.asset          (tier index, cost, music clip, mixer snapshot)
```

Example `PresentationTier` fields:

- `tierIndex`, `cost`
- `playerSprite`, `enemySprite` (or material color)
- `enableMuzzleFlash`, `projectileTrailPrefab`
- `globalVolumeProfile` (URP/HDRP or post stack reference)

### Code

| Class | Responsibility |
|-------|----------------|
| `UpgradeShop` | UI list of tiers; `TryPurchase(tier)` checks `ScoreManager` |
| `PresentationManager` | `ApplyVisualTier(int)` — swap sprites, enable VFX, set volume |
| `AudioProgression` | `ApplyAudioTier(int)` — `AudioManager.PlayNextTrack()` or mixer snapshot |

### UI flow

- Button **Shop** (or auto-open between waves).
- Show: *“Visual Level 2 — 500 score”* → Buy → deduct score → `PresentationManager.Apply(2)`.

### Quick win for first visual tier

Without new art:

- Tier 1: enable **trail** on projectile, **muzzle flash** particle at `FirePoint`
- Tier 2: swap to **tinted** sprites / `Material` color on tank
- Tier 3: enable **post-processing** profile on main camera

Player **sees** the purchase immediately—validates the fantasy.

---

## Phase 4 — Audio progression (3–5 days)

**Goal:** Music improves when bought.

| Task | Suggestion |
|------|------------|
| Assign clips | Fill `AudioManager._levelTracks` with 2–3 loops (low → high energy) |
| `AudioTier` purchases | Tier 0: ambience only; Tier 1: music; Tier 2: louder mix / extra stem |
| Mixer | Expose `AudioMixer` groups: Music, SFX; snapshot per tier |
| Shop integration | Same `UpgradeShop`, category **Audio** |

Existing `PlayNextTrack` / fade coroutines can switch tracks on purchase.

---

## Phase 5 — Map generation at edges ✅ (baseline in repo)

**Implemented:** `Assets/WorldChunks/` — `ChunkStreamer`, `ChunkPool`, 3 prefab variants, editor **Tools → Repair World Chunk Prefabs**. Wired in `Scene_1` as `WorldChunkSystem`.

| Doc | Content |
|-----|---------|
| [Assets/WorldChunks/README.md](../Assets/WorldChunks/README.md) | Setup, API, inspector |
| [chunk-streaming-design.md](chunk-streaming-design.md) | Design (chunk size in design doc is conceptual; project uses **4800** in `DefaultChunkSettings`) |

**Still open for vision:**

- Tie chunk prefabs to **presentation tier** from score shop → `ChunkStreamer.ReloadAllChunks()`.
- `EdgeSpawnController` using `GetActiveCoords()` for border spawns.
- Optional colliders / tilemap later (current chunks are visual-only, top-down).

Original approaches for reference:

| Approach | Pros |
|----------|------|
| **Tilemap chunks** | `RuleTile` + prefab chunks |
| **Prefab chunks** | ✅ current — hand-authored variants + pool |
| **Simple grid** | Noise at frontier |

---

## Phase 6 — Polish & meta (optional)

- Combo multiplier for score
- High score `PlayerPrefs`
- Between-run meta (permanent unlock) — only if you want roguelite meta; vision works fine **in-run only**

---

## Recommended file structure (new)

```text
Assets/Scripts/
  Core/           GameManager, ScoreManager
  Combat/         Health, Damageable
  Spawning/       EdgeSpawnController, WaveSpawner
  Progression/    UpgradeShop, PresentationManager, AudioProgression
  Map/            ChunkGenerator, MapBounds        (Phase 5)
Assets/ScriptableObjects/
  Upgrades/       PresentationTier, AudioTier, WaveConfig
```

---

## Suggested order (minimal path to “feel the vision”)

1. **Health + score + kill** → numbers go up  
2. **Edge spawner** → pressure from borders  
3. **Shop + 2 visual tiers** → score buys visible upgrade  
4. **1 audio tier** → music unlock  
5. **Chunk map at edges** → scale arena  

Steps 1–4 prove the unique hook with the current art and systems.

---

## Risks & scope control

| Risk | Mitigation |
|------|------------|
| Too much art per tier | Start with VFX/post-process before new sprites |
| Shop pauses flow | Open shop only between waves or hold Tab |
| Proc gen scope creep | Ship edge spawns first; chunks in Phase 5 only |
| `GameManager` empty | Make it the only session entry point early |

---

## Next coding session (if you want code next)

Smallest vertical slice:

1. `Health` + score on kill  
2. `EdgeSpawnController` with 4 spawn points  
3. `PresentationManager` + one `PresentationTier` that enables muzzle flash  

That is playable proof of the full vision loop in one sitting.
