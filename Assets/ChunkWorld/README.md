# ChunkWorld — модуль карты

Независимый модуль: **чанки**, **биомы**, **текстуры из ScriptableObject**. Игра TankSurvivor подключает его через `ChunkWorldSceneBootstrap` и `ChunkWorldConfig`.

Дизайн: [docs/project-design.md](../../docs/project-design.md).

---

## Быстрый старт

1. Текстуры в `Content/Textures/` (Grass, Send, Stone, Taiga).
2. **Tools → ChunkWorld → Create Default Settings Assets**
3. **Tools → ChunkWorld → Setup Open Scene (Replace Legacy WorldChunks)**
4. **Play** — на сцене `ChunkWorldSystem` + bootstrap связывает танк.

---

## Структура

```
ChunkWorld/
├── Runtime/              ChunkWorld.Runtime.asmdef
│   └── Scripts/
│       ├── Config/       ChunkWorldConfig
│       ├── Data/         биомы, BiomeTextureCatalog
│       ├── Core/         coord, hash, required set
│       └── Streaming/    ChunkWorldStreamer, pool, builder
├── Editor/               меню Tools → ChunkWorld
├── Resources/            ChunkQuad.png (запасной спрайт)
└── Content/
    ├── Settings/         DefaultBiomeTextures, DefaultChunkWorldConfig
    ├── Textures/         PNG биомов
    ├── Prefabs/          ChunkWorldSystem.prefab
    └── DecorPrefabs/     (позже — объекты на чанке)
```

---

## ScriptableObject

| Asset | Назначение |
|-------|------------|
| `DefaultBiomeTextures.asset` | Спрайт пола на каждый биом — менять в Inspector |
| `DefaultChunkWorldConfig.asset` | Каталог + chunkSize, seed, буфер стриминга |

### Размер текстуры и PPU (важно)

**Pixels Per Unit (100)** — это не «размер чанка 100×100». Это сколько **пикселей текстуры = 1 метр Unity**.

| Текстура | PPU | Размер в мире (1 спрайт) |
|----------|-----|-------------------------|
| 1024×1024 | 100 | **10.24 × 10.24** единиц |
| 1024×1024 | 1024 | **1 × 1** единица |

В **DefaultChunkWorldConfig**:

- **Chunk Size Source = Match Sprite** — сторона чанка = размер эталонного спрайта (рекомендуется для 1024@PPU100).
- **Manual** — своё число; для одной текстуры на чанк без растягивания ставьте ≈ **10.24**, не 4800.

**Ground Layout:** Fit Chunk — одна картинка на чанк; Tile — повтор плитки.

**Только трава на уровне:** в `DefaultChunkWorldConfig` → **Biome Selection = Fixed Single**, **Fixed Biome = Grass**. Чтобы снова смешивать биомы — **Procedural**.

Импорт PNG: **Sprite (2D)**, **Mesh Type = Full Rect** (не Tight), в каталоге — именно **Sprite**, не Texture2D.

---

## Сцена игры

| Объект | Компонент |
|--------|-----------|
| `ChunkWorldSystem` | `ChunkWorldStreamer` + `ChunkRoot` |
| `ChunkWorldBootstrap` | `ChunkWorldSceneBootstrap` → находит `Player` |

Модуль **не ссылается** на класс `Player`; связь только из `Assets/Scripts/ChunkWorldSceneBootstrap.cs`.

---

## API

- `ChunkWorldStreamer.ReloadAllChunks()` — после смены тира графики в магазине
- `GetActiveCoords()` — для спавна врагов с краёв карты

---

## Меню Editor

- **Create Default Settings Assets**
- **Refresh Biome Textures From Folders**
- **Create ChunkWorld System Prefab**
- **Setup Open Scene (Replace Legacy WorldChunks)**

Старую папку **`Assets/WorldChunks`** удалили — всё здесь.
