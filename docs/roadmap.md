# Roadmap

Ориентир: [vision.md](vision.md) · Общий дизайн: [project-design.md](project-design.md) · План: [implementation-plan.md](implementation-plan.md)

## Сделано

- [x] Top-down танк: движение, башня, стрельба
- [x] Пул снарядов + звук выстрела (`AudioPool`)
- [x] Враг: преследование
- [x] Каркас музыки/эмбиента (`AudioManager`, mixer)
- [x] Документация видения и плана
- [x] Стриминг карты (`Assets/ChunkWorld`, `ChunkWorldStreamer`)
- [x] `ProjectilePool`: без ошибки при выходе из Play (`MissingReferenceException`)
- [x] Соглашения: `.editorconfig` (UTF-8 BOM), Cursor rules, [dev-conventions.md](dev-conventions.md)

## Фаза 1 — Score и бой (фундамент экономики)

- [ ] `Health` (игрок, враг)
- [ ] Урон снаряда (`Projectile`)
- [ ] Смерть врага → событие / очки
- [ ] `ScoreManager` + UI счёта
- [ ] `GameManager`: время забега, Game Over

## Фаза 2 — Края карты

- [ ] Точки спавна по периметру (`EdgeSpawnController`)
- [ ] `WaveSpawner` с краёв
- [ ] Границы арены (коллайдер / bounds камеры)
- [ ] Тюнинг радиуса агро врага

## Фаза 3 — Магазин за score ⭐ (ядро видения)

- [ ] `PresentationTier` (ScriptableObject)
- [ ] `PresentationManager` — применить тир (спрайты, VFX, post-process)
- [ ] `AudioTier` + переключение музыки/микшера
- [ ] UI магазина: цена, покупка, списание score
- [ ] Минимум 2 визуальных тира (хотя бы VFX без нового арта)

## Фаза 4 — Аудио-прогрессия

- [ ] Клипы на `AudioManager._levelTracks`
- [ ] Покупка тира → новый трек / mixer snapshot
- [ ] Связка с магазином

## Фаза 5 — Генерация карты у краёв

Код: **`Assets/ChunkWorld/`** · Дизайн: [chunk-streaming-design.md](chunk-streaming-design.md) · Модуль: [Assets/ChunkWorld/README.md](../Assets/ChunkWorld/README.md)

- [x] `ChunkWorldConfig` + `DefaultChunkWorldConfig.asset`
- [x] `ChunkWorldStreamer` + coord/hash + пул + builder
- [x] Каталог текстур биомов (`DefaultBiomeTextures`, PNG 1024@PPU100)
- [x] Детерминированный seed по coord
- [x] Асимметрия ahead/behind
- [x] Match Sprite / Fit Chunk (корректный масштаб пола)
- [x] Уровень: Fixed Single → Grass
- [x] Миграция с `Assets/WorldChunks` (удалён)
- [ ] Procedural биомы + декор + тиры из магазина — [project-design.md](project-design.md)

## Фаза 6 — Полировка

- [ ] Комбо / множитель score
- [ ] Рекорд (`PlayerPrefs`)
- [ ] Больше типов врагов
- [ ] Баланс цен в магазине

## Техдолг

- [ ] Подогнать `Camera` orthographic size под чанк ~10.24 u (или буфер/PPU для обзора нескольких чанков)
- [ ] Убрать дублирование `GameInput` / `PlayerInputHandler`
- [ ] Кэш ссылки на игрока у врага
- [ ] Переименовать `Explosion.asset` → `Gunshot` (ясность)

## Не в приоритете (пока)

- Мета-прогресс между забегами (перманентные unlock)
- Сложный roguelite draft из 3 карт (можно заменить прямым магазином тиров)
