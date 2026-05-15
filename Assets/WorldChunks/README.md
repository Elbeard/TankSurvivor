# World Chunks — стриминг карты

Папка **`Assets/WorldChunks`** содержит рабочую систему чанков для TankSurvivor: земля появляется впереди танка и убирается сзади, при возврате — тот же вариант (детерминированный seed).

Подробный дизайн: [docs/chunk-streaming-design.md](../docs/chunk-streaming-design.md).

---

## Быстрый старт

1. В Unity: верхнее меню **Tools → Repair World Chunk Prefabs** (или **Tools → World Chunks → Repair All…**).  
   Если пункта нет — откройте **Console**: красные ошибки компиляции блокируют меню Tools.
2. Откройте сцену **`Scene_1`** (или перетащите **`Prefabs/WorldChunkSystem`** на сцену).
3. Нажмите **Play** — чанки появятся вокруг танка.

> Если на prefab серые «Missing Script»: выполните пункт 1, удалите старый `WorldChunkSystem` со сцены и добавьте prefab снова.

---

## Структура папки

```
WorldChunks/
├── README.md                 ← этот файл
├── Settings/
│   └── DefaultChunkSettings.asset
├── Prefabs/
│   ├── WorldChunkSystem.prefab   ← ChunkStreamer + ChunkRoot
│   ├── Chunk_Grass_A.prefab       ← после Generate Example Prefabs
│   ├── Chunk_Grass_B.prefab
│   └── Chunk_Rock_C.prefab
├── Scripts/
│   ├── ChunkSettings.cs
│   ├── ChunkCoordUtil.cs
│   ├── ChunkHash.cs
│   ├── WorldChunk.cs
│   ├── IChunkProvider.cs
│   ├── PrefabChunkProvider.cs
│   ├── ChunkRequiredSetBuilder.cs
│   ├── ChunkPool.cs
│   ├── ChunkStreamer.cs
│   └── WorldChunkRuntimeFactory.cs
└── Editor/
    └── WorldChunkPrefabGenerator.cs
```

---

## Компоненты

| Скрипт | Назначение |
|--------|------------|
| **ChunkStreamer** | Главный оркестратор на сцене |
| **ChunkSettings** | ScriptableObject: размер чанка, seed, буфер ahead/behind |
| **ChunkCoordUtil** | world ↔ chunk координаты |
| **ChunkHash** | Детерминированный выбор варианта |
| **PrefabChunkProvider** | Выбор prefab по coord + seed |
| **ChunkPool** | Пул без Destroy |
| **WorldChunk** | Метаданные на instance (coord, variant) |
| **WorldChunkRuntimeFactory** | Fallback: 3 цветных варианта без asset |

---

## Настройка в Inspector

На **WorldChunkSystem → ChunkStreamer**:

| Поле | Рекомендация |
|------|----------------|
| Settings | `DefaultChunkSettings` |
| Chunk Root | дочерний `ChunkRoot` (уже в prefab) |
| Player | пусто — найдёт `Player` автоматически |
| Chunk Prefabs | пусто = runtime factory; или 3 prefab после Generate |
| Auto Find Player | ✓ |

В **DefaultChunkSettings**:

- **Chunk Size** — 4800 (сторона квадрата; 20 → ×12 → ×20).
- **Chunks Ahead / Behind / Side** — буфер по направлению движения.
- **Despawn Distance** — когда удалять чанк вне required set.
- **Update Interval** — 0.15 с (не каждый кадр).

---

## Как это выглядит в Play Mode

```
Hierarchy:
WorldChunkSystem
└── ChunkRoot
    ├── Chunk_0_0
    ├── Chunk_1_0
    ├── Chunk_0_1
    └── ... (по мере движения)

Каждый Chunk_*:
├── Ground      (спрайт 4800×4800)
├── Decor       (камни — разное число у вариантов A/B/C)
└── без коллайдера (земля только визуал; top-down)
```

При движении вправо впереди появляются новые `Chunk_2_0`, `Chunk_3_0`… сзади `Chunk_-2_0` уходит в пул.

---

## API для других систем

```csharp
// Активные coord (спавн врагов с краёв карты)
IReadOnlyCollection<Vector2Int> coords = chunkStreamer.GetActiveCoords();

// Центр чанка в мире
Vector2 center = ChunkCoordUtil.ChunkToWorldCenter(coord, settings.chunkSize);
```

Врагов и снаряды **не делайте дочерними** к `Chunk_*` — иначе исчезнут при despawn.

---

## Меню редактора

- **Tools → Repair World Chunk Prefabs** — главный пункт
- **Tools → World Chunks → …** — то же и доп. команды

---

## Отладка

- Выделите **WorldChunkSystem** в сцене — жёлтый Gizmo показывает чанк под игроком.
- Console: предупреждение, если нет ни prefab, ни runtime factory.

---

## Дальше

- Привязать visual tier из магазина score → смена массива `_chunkPrefabs` + `ReloadAllChunks()`.
- `EdgeSpawnController` читает `GetActiveCoords()` для спавна с периметра активной зоны.

---

## Соглашения

- Комментарии в скриптах — **русский**, файлы `*.cs` — **UTF-8 с BOM** (`.editorconfig`).
- Git-коммиты — **английский**: [docs/dev-conventions.md](../../docs/dev-conventions.md).
