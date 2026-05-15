# Roadmap

Ориентир: [vision.md](vision.md) · Детальный план: [implementation-plan.md](implementation-plan.md)

## Сделано

- [x] Top-down танк: движение, башня, стрельба
- [x] Пул снарядов + звук выстрела (`AudioPool`)
- [x] Враг: преследование
- [x] Каркас музыки/эмбиента (`AudioManager`, mixer)
- [x] Документация видения и плана
- [x] Стриминг карты чанками (`Assets/WorldChunks`, `Scene_1` / `WorldChunkSystem`)
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

Код: **`Assets/WorldChunks/`** · Дизайн: [chunk-streaming-design.md](chunk-streaming-design.md)

- [x] `ChunkSettings` + `DefaultChunkSettings.asset`
- [x] `ChunkStreamer` + `ChunkCoordUtil` + пул
- [x] 3 варианта чанков (runtime / меню Generate Example Prefabs)
- [x] Детерминированный seed по coord
- [x] Асимметрия ahead/behind
- [ ] Визуальное качество чанков привязать к visual tier (опционально)

## Фаза 6 — Полировка

- [ ] Комбо / множитель score
- [ ] Рекорд (`PlayerPrefs`)
- [ ] Больше типов врагов
- [ ] Баланс цен в магазине

## Техдолг

- [ ] Подогнать `Camera` orthographic size под `chunkSize` (4800) или уменьшить чанк для тестов
- [ ] Убрать дублирование `GameInput` / `PlayerInputHandler`
- [ ] Кэш ссылки на игрока у врага
- [ ] Переименовать `Explosion.asset` → `Gunshot` (ясность)

## Не в приоритете (пока)

- Мета-прогресс между забегами (перманентные unlock)
- Сложный roguelite draft из 3 карт (можно заменить прямым магазином тиров)
