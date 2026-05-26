# Соглашения разработки

Правила для людей и для агента Cursor. Дублируются в `.cursor/rules/`.

---

## Языки

| Что | Язык |
|-----|------|
| Чат с агентом, `docs/` (кроме `vision.md`, `implementation-plan.md`) | **Русский** |
| Комментарии в `*.cs` (`//`, `/* */`, `///`) | **Русский** |
| Имена типов, методов, полей, Unity API | **Английский** |
| **Сообщения git-коммитов** (subject и body) | **Английский** |
| Корневой `README.md` | Смешанный (краткий EN + ссылки на RU) |

### Почему коммиты на английском

- История в GitHub/GitLab читается без проблем с кодировкой в терминале и CI.
- Subject в одну строку, императив: `fix:`, `feat:`, `docs:` (Conventional Commits по желанию).

Примеры:

```text
fix: avoid MissingReferenceException when exiting Play in ProjectilePool

feat: add world chunk streaming with ChunkStreamer and editor repair menu

docs: document dev conventions and UTF-8 BOM for Cyrillic comments
```

---

## Кодировка исходников

- Файлы **`*.cs`** — **UTF-8 с BOM** (см. `.editorconfig`, `.vscode/settings.json` → `utf8bom`).
- Без BOM кириллица в комментариях в Windows/Unity может отображаться как «ромбы» ().
- Если файл уже с битой кириллицей — переписать комментарии, не копировать кракозябры.

---

## Unity

- Не коммитить `Library/`, `Temp/`, `Logs/`, `UserSettings/`.
- GUID в `.meta` — ровно **32** hex-символа.
- После правок карты/текстур: **Tools → ChunkWorld →** Refresh Biome Textures / Setup Open Scene (см. [Assets/ChunkWorld/README.md](../Assets/ChunkWorld/README.md)).
- Не коммитить **`Assets/Samples/`** (автоимпорт примеров пакетов Unity).

---

## Cursor rules

| Файл | Назначение |
|------|------------|
| `language-russian.mdc` | Ответы агента и комментарии в коде |
| `git-commits-english.mdc` | Сообщения коммитов |
| `verify-after-changes.mdc` | Проверка после правок |
| `project-design-context.mdc` | Читать `docs/project-design.md` для задач по карте и прогрессии |

---

## Связанные документы

- [systems.md](systems.md) — статус систем и скриптов
- [chunk-streaming-design.md](chunk-streaming-design.md) — дизайн чанков
- [cursor-russian-voice.md](cursor-russian-voice.md) — голосовой ввод в Cursor
