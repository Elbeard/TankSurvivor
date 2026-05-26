# Стриминг карты чанками — дизайн и примеры кода

> Документ для **TankSurvivor**: как устроены **чанки** (chunks), как они создаются впереди по движению танка и убираются сзади.  
> **Реализация:** [Assets/ChunkWorld/README.md](../Assets/ChunkWorld/README.md) — `ChunkWorldStreamer`, каталог текстур.  
> **Дизайн:** [project-design.md](project-design.md).

Связанные файлы: [vision.md](vision.md), [implementation-plan.md](implementation-plan.md) (фаза 5), [roadmap.md](roadmap.md).

> **Реализация:** `Assets/ChunkWorld` (`ChunkWorldStreamer`, `ChunkWorldConfig`). Разделы **7–8** ниже — исторический псевдокод старого `WorldChunks`/`ChunkStreamer`; живой API — [Assets/ChunkWorld/README.md](../Assets/ChunkWorld/README.md).

---

## 1. Что такое чанк в этом проекте

**Чанк** — квадратный кусок мира фиксированного размера. В примерах ниже часто **20×20** для наглядности.

**Сейчас в репозитории (`ChunkWorld`):**

| Параметр | Значение |
|----------|----------|
| Текстуры | 1024×1024, **PPU 100** → спрайт ≈ **10.24×10.24** мировых единиц |
| Размер чанка | `DefaultChunkWorldConfig` → **Chunk Size Source = Match Sprite** |
| Пол | **Ground Layout = Fit Chunk** (масштаб `chunkSize / sprite.bounds`, без растягивания до огромного scale) |
| Биом на уровне | **Fixed Single → Grass** (каталог: Grass, Send, Stone, Taiga) |
| Сцена | **Tools → ChunkWorld → Setup Open Scene** |

Старый модуль **`Assets/WorldChunks`** (prefab-варианты, `chunkSize` 4800) **удалён**.

- У каждого чанка есть **логическая координата** `(chunkX, chunkY)` — целые числа, не зависят от того, загружен он в сцену или нет.
- **Мировая позиция** левого нижнего угла чанка:
  - `worldX = chunkX * chunkSize`
  - `worldY = chunkY * chunkSize`

```
Мировые координаты (метры Unity)

     chunkY=1  ┌────────┬────────┐
               │(-1,1)  │ (0,1)  │
     chunkY=0  ├────────┼────────┤
               │(-1,0)  │ (0,0)  │  ← чанк (0,0) от (0,0) до (20,20)
     chunkY=-1 └────────┴────────┘
               chunkX=-1  chunkX=0
                    ↑ chunkSize = 20
```

---

## 2. Как это должно выглядеть в игре

### 2.1. Ощущение игрока

1. Танк едет в любую сторону — **впереди по ходу** появляется земля, стены по краям арены.
2. **Сзади** через 1–2 чанка старый кусок **исчезает** (объект убран из сцены).
3. Если **вернуться назад** в недавнюю зону:
   - либо чанк **ещё в сцене** (буфер сзади),
   - либо **пересоздаётся тот же ландшафт** (один и тот же seed для `(chunkX, chunkY)`).

Враги и снаряды **не привязаны** к чанку земли — они на отдельных слоях, иначе исчезнут вместе с землёй.

### 2.2. Иерархия в сцене (Hierarchy)

```
Scene_1
├── --- GAME ---
│   ├── GameManager
│   ├── Player
│   ├── AudioManager
│   └── ...
├── --- WORLD ---
│   └── ChunkRoot                    ← пустой Transform, (0,0,0)
│       ├── Chunk_-1_0               ← активные чанки только здесь
│       ├── Chunk_0_0
│       ├── Chunk_1_0
│       └── ...
├── --- SPAWNING ---
│   └── EdgeSpawnPoints (опционально)
└── --- UI ---
    └── Canvas
```

Каждый активный чанк:

```
Chunk_0_0                          position = (0, 0, 0)
├── Ground                         Sorting Layer: Ground
│   └── Sprite или Tilemap
├── Colliders                      Layer: Ground / Wall
│   └── TilemapCollider2D + CompositeCollider2D
│       или дочерние BoxCollider2D
└── Decor (опционально)            камни, трещины — только визуал
```

### 2.3. Префаб чанка (вариант A — рекомендуемый для старта)

**`Assets/Prefabs/World/Chunk_Grass_Plains.prefab`**

- Корень: `Chunk` + компонент `WorldChunk` (метаданные: тип, tier).
- Дети: спрайт земли на весь 20×20, `BoxCollider2D` или несколько коллайдеров по краям.

Несколько префабов: `Chunk_Grass_A`, `Chunk_Grass_B`, `Chunk_Rock_Edge` — выбор по **детерминированному** random из координат.

### 2.4. Tilemap (вариант B — позже)

```
Chunk_0_0
└── Grid (cell size 1)
    ├── Tilemap_Ground
    ├── Tilemap_Collision
    └── Tilemap_Decor
```

Генератор заполняет тайлы в диапазоне `[0..19] × [0..19]` локально в этом Grid.

---

## 3. Архитектура системы (структура классов)

```
                    ┌─────────────────┐
                    │  ChunkStreamer  │  ← главный оркестратор, Update
                    └────────┬────────┘
                             │
         ┌───────────────────┼───────────────────┐
         ▼                   ▼                   ▼
┌─────────────────┐ ┌───────────────┐ ┌─────────────────┐
│ IChunkProvider  │ │ ChunkPool     │ │ ChunkCoordUtil  │
│ (что спавнить)  │ │ (пул prefab)  │ │ world ↔ chunk   │
└────────┬────────┘ └───────────────┘ └─────────────────┘
         │
    ┌────┴────┐
    ▼         ▼
Prefab      Procedural
Provider    TilemapProvider (позже)
```

| Класс | Ответственность |
|-------|-----------------|
| `ChunkStreamer` | Знает позицию игрока, направление движения, какие coord должны быть активны; spawn/despawn |
| `ChunkCoordUtil` | `WorldToChunk`, `ChunkToWorldOrigin`, `GetChunkCenter` |
| `IChunkProvider` | По `(chunkX, chunkY)` вернуть, **что** создавать (prefab id или tile data) |
| `PrefabChunkProvider` | `hash(seed, x, y) % variants.Length` → prefab |
| `ChunkPool` | `Get()` / `Release(chunk)` вместо Destroy |
| `WorldChunk` | MonoBehaviour на instance: coord, tier, `OnSpawn`/`OnDespawn` |
| `ChunkSettings` | ScriptableObject: chunkSize, chunksAhead, chunksBehind, chunksSide, worldSeed |

---

## 4. Сколько чанков держать активными

### 4.1. Симметричная сетка 3×3 (9 чанков)

```
[(-1,1)] [(0,1)] [(1,1)]
[(-1,0)] [(0,0)] [(1,0)]   ← игрок в (0,0) или в центре чанка
[(-1,-1)] [(0,-1)] [(1,-1)]
```

- **Плюс:** простейший код.
- **Минус:** мало буфера сзади при развороте.

### 4.2. Асимметрично по направлению движения (рекомендуется)

Если танк едет **вправо** (`moveDir ≈ (1, 0)`):

```
        [бок]
[сзади] [P] [вперёд] [вперёд+2]
        [бок]
```

Параметры в `ChunkSettings`:

| Поле | Пример | Смысл |
|------|--------|--------|
| `chunksAhead` | 2 | сколько чанков впереди по moveDir |
| `chunksBehind` | 2 | сколько сзади **не удалять сразу** |
| `chunksSide` | 1 | полоса сбоку |
| `despawnDistance` | 3 | удалить, если чанк дальше 3 от игрока по Manhattan |

Итого активных: порядка **12–15** — для простых prefab это нормально.

### 4.3. Перфоманс

Ориентир: **до 25 простых чанков** (1–2 collider, 1 sprite) — обычно ок на PC.  
Смотреть **Profiler**: Physics 2D, Renderer, количество активных colliders.

---

## 5. Детерминированная генерация (возврат «та же карта»)

### 5.1. Проблема

Удалили чанк `(5, 3)` → игрок вернулся → создали снова.  
Если `Random.Range` без seed — **другой** prefab → неприятно.

### 5.2. Решение: seed от координат

```csharp
// Псевдокод — идея, не финальный файл проекта
int GetVariantIndex(int chunkX, int chunkY, int worldSeed, int variantCount)
{
    unchecked
    {
        int hash = 17;
        hash = hash * 31 + chunkX;
        hash = hash * 31 + chunkY;
        hash = hash * 31 + worldSeed;
        hash = Mathf.Abs(hash);
        return hash % variantCount;
    }
}
```

Один и тот же `(chunkX, chunkY, worldSeed)` → **всегда один variant** → ландшафт совпадает.

### 5.3. Кэш (опционально, позже)

Если на чанке нужны **следы** (сломанный объект):

```csharp
// Dictionary<Vector2Int, ChunkSaveData>
// ChunkSaveData: variantIndex, list<destroyedDecorIds>
```

При despawn — сохранить в кэш (лимит 50 записей, LRU).  
При spawn — если есть в кэше, восстановить состояние.

Для **только земли** на MVP достаточно **п. 5.2** без Dictionary.

---

## 6. Алгоритм каждый кадр (или раз в 0.2 с)

```
1. playerChunk = WorldToChunk(player.position)
2. moveDir = GetMoveDirection(player)  // velocity или lastInput, порог 0.1
3. required = BuildRequiredSet(playerChunk, moveDir, settings)
   // HashSet<Vector2Int> все coord, которые должны быть загружены

4. foreach coord in required:
       if not activeChunks.Contains(coord):
           SpawnChunk(coord)

5. foreach coord in activeChunks copy:
       if not required.Contains(coord):
           if Distance(coord, playerChunk) > despawnDistance:
               DespawnChunk(coord)
```

**Не** пересчитывать каждый кадр при стоянии на месте — `moveDir.magnitude < 0.1` → не менять набор (или только 3×3 вокруг игрока).

---

## 7. Примеры кода (эталон для будущей реализации)

### 7.1. ChunkSettings (ScriptableObject)

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "ChunkSettings", menuName = "World/Chunk Settings")]
public class ChunkSettings : ScriptableObject
{
    [Header("Размер одного чанка в мировых единицах")]
    public float chunkSize = 20f;

    [Header("Сид мира (один на забег)")]
    public int worldSeed = 12345;

    [Header("Сколько чанков держать (по направлению движения)")]
    public int chunksAhead = 2;
    public int chunksBehind = 2;
    public int chunksSide = 1;

    [Header("Удалять чанк, если дальше N чанков от игрока")]
    public int despawnDistance = 3;

    [Header("Если скорость ниже — только квадрат вокруг игрока")]
    public float minMoveSpeedForDirectedChunks = 0.5f;
}
```

### 7.2. ChunkCoordUtil (статический helper)

```csharp
using UnityEngine;

public static class ChunkCoordUtil
{
    public static Vector2Int WorldToChunk(Vector2 worldPos, float chunkSize)
    {
        int x = Mathf.FloorToInt(worldPos.x / chunkSize);
        int y = Mathf.FloorToInt(worldPos.y / chunkSize);
        return new Vector2Int(x, y);
    }

    public static Vector3 ChunkToWorldOrigin(Vector2Int coord, float chunkSize)
    {
        return new Vector3(coord.x * chunkSize, coord.y * chunkSize, 0f);
    }

    public static Vector2 ChunkToWorldCenter(Vector2Int coord, float chunkSize)
    {
        float half = chunkSize * 0.5f;
        return new Vector2(
            coord.x * chunkSize + half,
            coord.y * chunkSize + half);
    }

    public static int ChunkManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
```

### 7.3. WorldChunk (на instance в сцене)

```csharp
using UnityEngine;

public class WorldChunk : MonoBehaviour
{
    public Vector2Int Coord { get; private set; }
    public int VariantIndex { get; private set; }

    public void Initialize(Vector2Int coord, int variantIndex)
    {
        Coord = coord;
        VariantIndex = variantIndex;
        name = $"Chunk_{coord.x}_{coord.y}";
    }
}
```

### 7.4. IChunkProvider + PrefabChunkProvider

```csharp
using UnityEngine;

public interface IChunkProvider
{
    GameObject GetPrefabForChunk(Vector2Int coord, int worldSeed);
}

public class PrefabChunkProvider : IChunkProvider
{
    private readonly GameObject[] _variants;

    public PrefabChunkProvider(GameObject[] variants)
    {
        _variants = variants;
    }

    public GameObject GetPrefabForChunk(Vector2Int coord, int worldSeed)
    {
        if (_variants == null || _variants.Length == 0)
            return null;

        int index = GetVariantIndex(coord.x, coord.y, worldSeed, _variants.Length);
        return _variants[index];
    }

    private static int GetVariantIndex(int chunkX, int chunkY, int worldSeed, int count)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + chunkX;
            hash = hash * 31 + chunkY;
            hash = hash * 31 + worldSeed;
            return Mathf.Abs(hash) % count;
        }
    }
}
```

### 7.5. ChunkPool (упрощённо)

```csharp
using System.Collections.Generic;
using UnityEngine;

public class ChunkPool
{
    private readonly Transform _parent;
    private readonly Stack<WorldChunk> _pool = new();

    public ChunkPool(Transform parent)
    {
        _parent = parent;
    }

    public WorldChunk Get(GameObject prefab, Vector2Int coord, Vector3 worldOrigin)
    {
        WorldChunk chunk;
        if (_pool.Count > 0)
        {
            chunk = _pool.Pop();
            chunk.gameObject.SetActive(true);
        }
        else
        {
            var go = Object.Instantiate(prefab, _parent);
            chunk = go.GetComponent<WorldChunk>();
            if (chunk == null)
                chunk = go.AddComponent<WorldChunk>();
        }

        chunk.transform.position = worldOrigin;
        return chunk;
    }

    public void Release(WorldChunk chunk)
    {
        chunk.gameObject.SetActive(false);
        _pool.Push(chunk);
    }
}
```

### 7.6. ChunkStreamer (ядро)

```csharp
using System.Collections.Generic;
using UnityEngine;

public class ChunkStreamer : MonoBehaviour
{
    [SerializeField] private ChunkSettings _settings;
    [SerializeField] private Transform _chunkRoot;
    [SerializeField] private Transform _player;
    [SerializeField] private GameObject[] _chunkPrefabs;

    private readonly Dictionary<Vector2Int, WorldChunk> _active = new();
    private IChunkProvider _provider;
    private ChunkPool _pool;
    private Rigidbody2D _playerBody;
    private Vector2 _lastMoveDir = Vector2.right;

    private void Awake()
    {
        _provider = new PrefabChunkProvider(_chunkPrefabs);
        _pool = new ChunkPool(_chunkRoot);
        _playerBody = _player.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        UpdateChunks();
    }

    private void UpdateChunks()
    {
        Vector2 playerPos = _player.position;
        Vector2Int playerChunk = ChunkCoordUtil.WorldToChunk(playerPos, _settings.chunkSize);

        Vector2 moveDir = GetMoveDirection();
        HashSet<Vector2Int> required = BuildRequiredChunks(playerChunk, moveDir);

        // Spawn
        foreach (Vector2Int coord in required)
        {
            if (_active.ContainsKey(coord))
                continue;

            GameObject prefab = _provider.GetPrefabForChunk(coord, _settings.worldSeed);
            if (prefab == null)
                continue;

            Vector3 origin = ChunkCoordUtil.ChunkToWorldOrigin(coord, _settings.chunkSize);
            WorldChunk instance = _pool.Get(prefab, coord, origin);
            int variant = /* тот же hash % length */;
            instance.Initialize(coord, variant);
            _active[coord] = instance;
        }

        // Despawn
        var toRemove = new List<Vector2Int>();
        foreach (var kv in _active)
        {
            if (required.Contains(kv.Key))
                continue;

            int dist = ChunkCoordUtil.ChunkManhattanDistance(kv.Key, playerChunk);
            if (dist > _settings.despawnDistance)
                toRemove.Add(kv.Key);
        }

        foreach (Vector2Int coord in toRemove)
        {
            WorldChunk chunk = _active[coord];
            _active.Remove(coord);
            _pool.Release(chunk);
        }
    }

    private Vector2 GetMoveDirection()
    {
        if (_playerBody != null && _playerBody.velocity.sqrMagnitude > 0.1f)
        {
            _lastMoveDir = _playerBody.velocity.normalized;
            return _lastMoveDir;
        }
        return _lastMoveDir;
    }

    private HashSet<Vector2Int> BuildRequiredChunks(Vector2Int center, Vector2 moveDir)
    {
        var set = new HashSet<Vector2Int>();

        bool moving = _playerBody != null &&
                      _playerBody.velocity.magnitude >= _settings.minMoveSpeedForDirectedChunks;

        if (!moving)
        {
            // Стоим — симметричный квадрат 3×3
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                set.Add(new Vector2Int(center.x + dx, center.y + dy));
            return set;
        }

        // Ось вперёд/назад + бока (упрощённо: по доминирующей оси velocity)
        Vector2Int forward = DominantAxis(moveDir);
        Vector2Int right = new Vector2Int(-forward.y, forward.x);

        for (int a = -_settings.chunksBehind; a <= _settings.chunksAhead; a++)
        {
            for (int s = -_settings.chunksSide; s <= _settings.chunksSide; s++)
            {
                Vector2Int coord = center + forward * a + right * s;
                set.Add(coord);
            }
        }

        return set;
    }

    private static Vector2Int DominantAxis(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            return dir.x >= 0 ? Vector2Int.right : Vector2Int.left;
        return dir.y >= 0 ? Vector2Int.up : Vector2Int.down;
    }
}
```

> В реальном проекте: вынести `BuildRequiredChunks` в отдельный класс, вызывать `Update` раз в 0.15 с через coroutine, обработать отрицательные coord.

---

## 8. Как связать с врагами и спавном с краёв

```
EdgeSpawnController
  → берёт активные coord из ChunkStreamer (или сам считает границы)
  → точка спавна = центр чанка "впереди" + Random внутри чанка
  → враг spawn НЕ parent к Chunk_*, а в корень Enemies/
```

Иначе при DespawnChunk враг исчезнет.

---

## 9. Visual tier (магазин за score)

```csharp
// PresentationManager меняет tier → ChunkStreamer перечитывает _chunkPrefabs
public void SetVisualTier(int tier)
{
    _chunkPrefabs = _tierDatabase.GetPrefabs(tier);
    // Опционально: пересоздать только активные чанки
    RefreshAllActiveChunks();
}
```

Tier 0: серые тайлы. Tier 1: трава + трава. Tier 2: декор, другой post-process на камере (не на чанке).

---

## 10. Порядок внедрения в проект (когда будете кодить)

| Шаг | Что | Результат |
|-----|-----|-----------|
| 1 | Один prefab `Chunk_Flat`, `ChunkStreamer` 3×3 без удаления | Видно сетку чанков |
| 2 | Despawn + pool | Память стабильна |
| 3 | Детерминированный variant | Возврат = та же земля |
| 4 | Асимметрия ahead/behind | Удобнее ездить назад |
| 5 | 3–5 prefab variants | Разнообразие |
| 6 | Edge spawn на активных coord | Геймплей survivor |

**До чанков** по roadmap лучше **фаза 1** (HP, score) — но чанки можно параллельно на тестовой сцене.

---

## 11. Папки в проекте (будущее)

```
Assets/
  Prefabs/
    World/
      Chunk_Flat.prefab
      Chunk_Grass_A.prefab
      ...
  ScriptableObjects/
    World/
      ChunkSettings.asset
      ChunkTierDatabase.asset
  Scripts/
    World/
      ChunkStreamer.cs
      ChunkCoordUtil.cs
      WorldChunk.cs
      ChunkPool.cs
      PrefabChunkProvider.cs
      ChunkSettings.cs
```

---

## 12. Чеклист «готово, когда»

- [ ] Танк едет 30 сек — нет дыр в земле впереди
- [ ] Profiler: стабильное число активных чанков (не растёт бесконечно)
- [ ] Вернулся на 1 чанк назад — тот же variant (seed)
- [ ] Враги не исчезают при despawn чанка
- [ ] `chunkSize` согласован с камерой (видно ~1–2 чанка до горизонта)

---

## 13. Визуальная схема потока данных

```
     [Player position]
            │
            ▼
     WorldToChunk ──► playerChunk (0, 2)
            │
            ▼
     velocity ──► moveDir, BuildRequiredChunks
            │
            ├─► Spawn: PrefabProvider(coord) ──► Pool.Get ──► _active[coord]
            │
            └─► Despawn: dist > 3 ──► Pool.Release ──► remove from _active

     worldSeed + (chunkX, chunkY) ──► всегда один prefab при повторном Spawn
```

---

*Документ создан как эталон. Реализацию в `Assets/Scripts/World/` добавляем отдельной задачей по запросу.*
