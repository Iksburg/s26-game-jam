# Система обучения (Tutorial System)

## Описание

Система обучения предоставляет:
- **Всплывающие подсказки** с текстом инструкций
- **Подсветка кнопок и элементов UI** с эффектом мигания
- **Пошаговое руководство** через игру
- **Сохранение статуса** обучения в PlayerPrefs

## Структура компонентов

### TutorialManager
Главный управляющий компонент обучения. Управляет последовательностью шагов и координирует работу других компонентов.

**Методы:**
- `AddStep(string hintText, Button targetButton, bool waitForClick, float duration)` - добавить шаг с целевой кнопкой
- `AddStep(string hintText, Image targetImage, float duration)` - добавить шаг с целевым изображением
- `StartTutorial()` - запустить обучение
- `StopTutorial()` - остановить обучение
- `SkipTutorial()` - пропустить и отметить как завершённое
- `NextStep()` - перейти к следующему шагу
- `SetHintPanel(TutorialHintPanel)` - установить панель подсказок

### TutorialHintPanel
Всплывающая панель с подсказкой. Отслеживает целевой элемент и остаётся рядом с ним.

**Методы:**
- `ShowHint(string hintText, RectTransform targetRect, bool showNextButton)` - показать подсказку
- `HideHint()` - скрыть подсказку
- `SetTextComponent(Text)` - установить Text компонент
- `SetNextButton(Button)` - установить кнопку "Далее"

### TutorialHighlight
Подсветка UI элемента с эффектом мигания.

**Методы:**
- `HighlightElement(Image targetImage)` - подсветить элемент
- `RemoveHighlight()` - убрать подсветку

### TutorialOverlay
Затемнение фона при обучении.

**Методы:**
- `Show()` - показать затемнение
- `Hide()` - скрыть затемнение

### TutorialInitializer
Инициализатор обучения. Подготавливает сценарий обучения для разных сцен.

**Методы:**
- `InitializeMainMenuTutorial()` - настроить обучение для главного меню
- `InitializeGameplayTutorial()` - настроить обучение для игровой сцены
- `SetTutorialManager(TutorialManager)` - установить менеджер обучения

### TutorialSetup
Компонент для автоматической инициализации обучения при старте сцены.

**Параметры:**
- `Enable On Gameplay` - включить обучение в игровой сцене
- `Enable On Main Menu` - включить обучение в главном меню

## Использование

### 1. Автоматическое добавление компонентов
Самый простой способ - добавить компонент `TutorialSetup` на Canvas сцены:

```csharp
// Добавьте TutorialSetup компонент на Canvas в Unity Editor
// или программно:
var tutorialSetup = canvasObject.AddComponent<TutorialSetup>();
```

### 2. Ручное создание и использование
Если вы хотите больше контроля:

```csharp
// Создайте TutorialManager
var tutorialManager = gameObject.AddComponent<TutorialManager>();

// Добавьте шаги обучения
tutorialManager.AddStep(
    "Нажмите кнопку 'Новая игра'",
    newGameButton,
    waitForClick: false,
    duration: 3f
);

tutorialManager.AddStep(
    "Добро пожаловать на ферму!",
    farmPanel,
    duration: 2f
);

// Запустите обучение
tutorialManager.StartTutorial();
```

## Параметры шагов

### Шаг с кнопкой
```csharp
AddStep(
    hintText: "Текст подсказки",
    targetButton: buttonReference,
    waitForClick: true,      // Ждёт клика по кнопке
    duration: 0f             // Не используется если waitForClick=true
)
```

### Шаг с изображением
```csharp
AddStep(
    hintText: "Текст подсказки",
    targetImage: imageReference,
    duration: 3f             // Автоматически переходит к следующему через 3 секунды
)
```

### Шаг без целевого элемента
```csharp
AddStep(
    hintText: "Завершающий текст",
    targetImage: null,
    duration: 2f
)
```

## Управление статусом обучения

```csharp
// Проверить, пройдено ли обучение
if (TutorialManager.HasCompletedTutorial)
{
    Debug.Log("Обучение уже пройдено");
}

// Сбросить статус (для переиграния)
TutorialManager.ResetTutorial();
```

## Интеграция с существующими сценами

1. Откройте сцену в Unity Editor
2. Выберите Canvas объект
3. Добавьте компонент `TutorialSetup` через Inspector
4. Установите флаги `Enable On Gameplay` или `Enable On Main Menu` в зависимости от типа сцены

## Стилизация

Текст подсказек используют встроенный шрифт `LegacyRuntime.ttf`. Для кастомизации:
- Цвет фона панели: отредактируйте цвет в `TutorialSetup.CreateTutorialSystem()`
- Размер и стиль шрифта: измените параметры Text компонента
- Расстояние панели от элемента: измените `_offsetDistance` в `TutorialHintPanel`

## Примеры

### Пример 1: Простое обучение для главного меню
```csharp
var tutorialManager = gameObject.AddComponent<TutorialManager>();
tutorialManager.AddStep("Добро пожаловать!", newGameButton, duration: 2f);
tutorialManager.AddStep("Нажмите для начала", newGameButton, waitForClick: true);
tutorialManager.StartTutorial();
```

### Пример 2: Обучение для игровой механики
```csharp
tutorialManager.AddStep("Это ваши ресурсы", resourcesPanel, duration: 2f);
tutorialManager.AddStep("Кликните на кота", null as Image, duration: 3f);
tutorialManager.AddStep("Используйте магазин", shopButton, waitForClick: false, duration: 2f);
tutorialManager.StartTutorial();
```

## Отладка

Если обучение не работает:
1. Проверьте, что `TutorialSetup` добавлен на Canvas
2. Убедитесь, что сцена имеет правильное имя ("MainMenu" или "CatSpawn")
3. Проверьте консоль на ошибки
4. Используйте `TutorialManager.ResetTutorial()` для сброса статуса
