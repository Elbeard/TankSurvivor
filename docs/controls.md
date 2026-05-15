# Управление

## Input System

Файл действий: `Assets/Input/PlayerInputActions.inputactions`  
Генерируемый класс: `Assets/Input/PlayerInputActions.cs`

Карта **Player**:

| Action | Тип | Привязки (типичные) |
|--------|-----|---------------------|
| **Move** | Vector2 | WASD, стрелки, левый стик |
| **Look** | Vector2 | Позиция мыши |
| **Fire** | Button | ЛКМ, триггер геймпада и др. |

## Как это подключено в коде

### Движение и башня

`PlayerInputHandler` на игроке (интерфейс `IPlayerActions` или callbacks `OnMove` / `OnLook`):

- `MovementInput` → `PlayerMovement.Move()`
- `MousePosition` → `TowerRotation.Rotate()`

### Стрельба

`TankGun` использует **`InputActionReference`** на действие **Fire** (назначено в префабе `Player`, не через `PlayerInputHandler`).

При `performed` вызывается `Shoot()` с учётом `fireRate`.

## Настройка в Unity

1. Выберите **Player** на сцене или в префабе.
2. **Tank Gun** → `Fire Action` должен ссылаться на `Player/Fire` из `PlayerInputActions`.
3. **Fire Point** — дочерний Transform, ось **up** = направление выстрела.

## Отладка

- Если не стреляет: проверьте, что `fireAction` не `None`, Input System package включён, Player Input / ссылки актуальны.
- Если башня не крутится: нужна камера с тегом `MainCamera`.
