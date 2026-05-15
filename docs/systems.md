# Системы и код

## Схема зависимостей (упрощённо)

```
PlayerInputActions (Fire, Move, Look)
        │
        ├─► PlayerInputHandler ──► Player ──► PlayerMovement
        │                      └─► TowerRotation
        │
        └─► TankGun ──► ProjectilePool ──► Projectile
                │
                └─► AudioPool.TryPlaySound ◄── SimpleAudioEvent (gunshots)

EnemyAwarenessController ──► EnemyMovement

ChunkStreamer ──► ChunkPool ──► WorldChunk (prefab variants)
        │
        └─► ChunkRequiredSetBuilder, PrefabChunkProvider, ChunkHash
```

## Таблица систем

| Система | Скрипты | Префабы / ассеты | Статус |
|---------|---------|------------------|--------|
| Ввод | `PlayerInputHandler`, `PlayerInputActions`, `GameInput`* | `PlayerInputActions.inputactions` | ✅ Работает |
| Движение игрока | `Player`, `PlayerMovement` | `Player.prefab` | ✅ |
| Башня | `TowerRotation` | Дочерний объект Tower | ✅ |
| Стрельба | `TankGun` | `Player`, `FirePoint` | ✅ |
| Пул снарядов | `ProjectilePool`, `Projectile` | `Projectile.prefab` | ✅ (фикс выхода из Play) |
| Стриминг карты | `ChunkStreamer`, `ChunkPool`, `WorldChunk`, … | `Assets/WorldChunks/` | ✅ |
| Урон | `Projectile` (закомментирован) | — | ⏳ Нет `Health` |
| Враг: обнаружение | `EnemyAwarenessController` | `SimpleEnemy.prefab` | ✅ |
| Враг: движение | `EnemyMovement` | `SimpleEnemy.prefab` | ✅ |
| SFX (выстрелы) | `AudioPool`, `SimpleAudioEvent` | `Explosion.asset`, `AudioPool` в сцене | ✅ |
| Музыка / эмбиент | `Audio.AudioManager` | `Scene_1`, `AudioMixer` | ⚠️ Нужны клипы в инспекторе |
| Игровой менеджер | `GameManager` | — | ⏳ Пустой класс |
| UI | `Canvas.prefab` | — | ⏳ Без логики |

\* `GameInput` — дублирует доступ к вводу, в `Player` не используется.

## Игрок

### `Player`

Координатор: в `FixedUpdate` вызывает движение и поворот башни.

### `PlayerMovement`

- `Rigidbody2D.velocity` от сглаженного ввода (`SmoothDamp`).
- Поворот корпуса по направлению движения.

### `TowerRotation`

- `Camera.main.ScreenToWorldPoint` + угол к курсору.

### `TankGun`

- Подписка на `InputActionReference` (**Fire**).
- `Shoot()`: звук → `GetProjectile()` → позиция `firePoint` → импульс `firePoint.up * speed`.
- Звук: случайный клип из `SimpleAudioEvent` → `AudioPool.Instance.TryPlaySound(..., High)`.

На префабе `audioEvent` может быть пустым; в **Scene_1** переопределён на `Assets/Sounds/Explosion.asset` (gunshot_1–3).

### `ProjectilePool`

- `UnityEngine.Pool.ObjectPool<Projectile>`, capacity 10, max 20.
- На префабе **Player** (рядом с `TankGun`).
- **Выход из Play:** в `OnDestroy` при `!Application.isPlaying` не вызывается `Dispose()` (иначе `MissingReferenceException`: снаряды уже уничтожены из‑за `Projectile.OnEnable` → `SetParent(null)`).
- В `actionOnDestroy` перед `Destroy` проверка `if (projectile)` (Unity-overload для уничтоженных объектов).
- Комментарии в файле — на русском, кодировка UTF-8 BOM (см. [dev-conventions.md](dev-conventions.md)).

### `Projectile`

- По истечении `lifetime` — `ReturnToPool()`.
- `OnTriggerEnter2D` с уроном — **закомментирован**.

## Враг

### `EnemyAwarenessController`

- В `Awake` ищет `Player` через `FindObjectOfType`.
- В `FixedUpdate`: дистанция до игрока, `AwareOfPlayer`, `DirectionToPlayer`.

### `EnemyMovement`

- Если `AwareOfPlayer` — velocity = `transform.up * speed` после поворота к цели.
- Иначе velocity = 0.

## Аудио

### `AudioPool` (глобальный синглтон)

- Пул `AudioSource`, приоритеты `SoundPriority` (Low / Medium / High).
- Вытеснение низкоприоритетного звука при нехватке источников.

### `AudioManager` (namespace `Audio`)

- Музыка с fade, смена треков, ambience.
- Поле `_soundPool` объявлено, но **не связано** с `AudioPool` в текущем коде.

### ScriptableObject

- `AudioEvent` — абстрактный базовый класс.
- `SimpleAudioEvent` — массив клипов, диапазоны volume/pitch.

## Инфраструктура

### `SingletonBehaviour<T>`

`Instance` через `FindObjectOfType` при первом обращении.

## World Chunks (`Assets/WorldChunks/`)

| Компонент | Роль |
|-----------|------|
| `ChunkStreamer` | Следит за игроком, спавнит/убирает чанки |
| `ChunkSettings` | Размер чанка (сейчас **4800**), seed, буфер ahead/behind |
| `ChunkPool` | Пул экземпляров без `Destroy` |
| `PrefabChunkProvider` + `ChunkHash` | Детерминированный выбор варианта A/B/C |
| `WorldChunkRuntimeFactory` | Fallback-визуал и **Tools → Repair World Chunk Prefabs** |

Подробнее: [Assets/WorldChunks/README.md](../Assets/WorldChunks/README.md), [chunk-streaming-design.md](chunk-streaming-design.md).

На сцене: **WorldChunkSystem** (prefab с `ChunkStreamer` + дочерний `ChunkRoot`). Земля **без коллайдера** (только визуал).

## Сцена Scene_1 (ожидаемые объекты)

- **Player** (инстанс префаба)
- **Enemy** (инстанс `SimpleEnemy`)
- **AudioManager** — компонент `Audio.AudioManager`
- **AudioPool** — компонент `AudioPool`
- **WorldChunkSystem** — стриминг чанков

## Папки

```
Assets/
  Input/           — PlayerInputActions
  Prefabs/         — Player, Enemy, Projectile, Canvas, AudioPoolElement
  Scenes/          — Scene_1
  ScriptableObjects/ — AudioEvent, SimpleAudioEvent
  Scripts/         — игровая логика
  Sounds/          — ogg, mixer, Explosion.asset
  WorldChunks/     — стриминг карты, prefab чанков, Editor Tools
```

## Соглашения разработки

Кодировка, язык комментариев и коммитов: [dev-conventions.md](dev-conventions.md).
