# Конфигурация обучения (Tutorial Configuration)

Этот файл описывает различные способы конфигурирования системы обучения для разных сценариев.

## 1. Конфигурация для главного меню

### Текущая конфигурация (MainMenuTutorial)
- Шаг 1: Добро пожаловать в мир кошек
- Шаг 2: Информация о кнопке "Продолжить"
- Шаг 3: Информация о кнопке "Настройки"

### Как изменить?
Отредактируйте метод `InitializeMainMenuTutorial()` в `TutorialInitializer.cs`:

```csharp
public void InitializeMainMenuTutorial()
{
    if (_tutorialManager == null) return;

    // Шаг 1
    _tutorialManager.AddStep(
        "Ваш текст здесь",
        GetButtonByName("YourButtonName"),
        waitForClick: false,
        duration: 3f
    );

    _tutorialManager.StartTutorial();
}
```

## 2. Конфигурация для игровой сцены

### Текущая конфигурация (GameplayTutorial)
- Шаг 1: Информация о ресурсах
- Шаг 2: Как кликать на котов
- Шаг 3: Создание новых котов
- Шаг 4: Магазин
- Шаг 5: Завершение

### Как изменить?
Отредактируйте метод `InitializeGameplayTutorial()` в `TutorialInitializer.cs`:

```csharp
public void InitializeGameplayTutorial()
{
    if (_tutorialManager == null) return;

    // Добавьте ваши шаги
    _tutorialManager.StartTutorial();
}
```

## 3. Типы шагов

### Тип 1: Шаг с кнопкой и автопереходом
```csharp
_tutorialManager.AddStep(
    hintText: "Текст подсказки",
    targetButton: button,
    waitForClick: false,
    duration: 3f
);
// Переход произойдет автоматически через 3 секунды
```

### Тип 2: Шаг с кнопкой и ожиданием клика
```csharp
_tutorialManager.AddStep(
    hintText: "Нажмите эту кнопку",
    targetButton: button,
    waitForClick: true,
    duration: 0f
);
// Переход произойдет только после клика
```

### Тип 3: Шаг с изображением
```csharp
_tutorialManager.AddStep(
    hintText: "Это ваши ресурсы",
    targetImage: resourcesPanel,
    duration: 2f
);
```

### Тип 4: Текстовое сообщение без подсветки
```csharp
_tutorialManager.AddStep(
    hintText: "Завершение обучения!",
    targetImage: null as Image,
    duration: 2f
);
```

## 4. Поиск элементов по имени

Метод `GetButtonByName()` ищет кнопку по названию GameObject:

```csharp
private Button GetButtonByName(string buttonName)
{
    var buttons = FindObjectsOfType<Button>();
    foreach (var btn in buttons)
    {
        if (btn.gameObject.name == buttonName)
            return btn;
    }
    return null;
}
```

### Как узнать имя кнопки?
1. Откройте сцену в Unity Editor
2. Выберите кнопку в Hierarchy
3. Посмотрите её имя в инспекторе

### Примеры имён:
- "NewGameButton" - кнопка "Новая игра"
- "ContinueButton" - кнопка "Продолжить"
- "SettingsButton" - кнопка "Настройки"
- "QuitButton" - кнопка "Выход"

## 5. Параметры длительности

### Рекомендуемые значения:
- **0.5f** - очень короткая подсказка (менее второй)
- **1f** - короткая подсказка (быстрая информация)
- **2f** - нормальная подсказка (успеть прочитать)
- **3f** - длинная подсказка (время подумать)
- **5f** - очень длинная подсказка (сложная механика)

### Для ожидания клика:
```csharp
waitForClick: true  // Ждет максимум 30 секунд, потом автопереход
```

## 6. Кастомизация текстов

### Эмодзи в текстах
```csharp
"Добро пожаловать в мир кошек! 🐱"
"Это ваши ресурсы: Корм 🥕 и Вода 💧"
```

### Многострочные тексты
```csharp
"Первая строка\n\nВторая строка\n\nТретья строка"
```

### Форматирование (базовое)
```csharp
"<b>Жирный текст</b>"
"<color=yellow>Жёлтый текст</color>"
```

## 7. Дополнительные возможности

### Проверка завершения
```csharp
if (TutorialManager.HasCompletedTutorial)
{
    // Обучение уже было пройдено
}
```

### Сброс обучения (для тестирования)
```csharp
TutorialManager.ResetTutorial();
// Теперь обучение запустится снова при перезагрузке сцены
```

### Остановка обучения
```csharp
tutorialManager.StopTutorial();
```

### Пропуск обучения
```csharp
tutorialManager.SkipTutorial();
// Обучение закончится, но будет отмечено как завершённое
```

## 8. Отладочные советы

### Просмотр всех кнопок на сцене
```csharp
var buttons = FindObjectsOfType<Button>();
foreach (var btn in buttons)
{
    Debug.Log($"Кнопка найдена: {btn.gameObject.name}");
}
```

### Просмотр всех изображений
```csharp
var images = FindObjectsOfType<Image>();
foreach (var img in images)
{
    Debug.Log($"Изображение найдено: {img.gameObject.name}");
}
```

### Включение отладочных логов
Добавьте в `TutorialManager.ShowStep()`:
```csharp
Debug.Log($"[Tutorial] Шаг {stepIndex}: {step.HintText}");
```

## 9. Сценарии использования

### Сценарий 1: Обучение только один раз
```csharp
// Автоматически! TutorialSetup уже это делает
// Просто добавьте компонент на Canvas
```

### Сценарий 2: Повторное обучение
```csharp
// Добавьте кнопку в меню:
if (GUILayout.Button("Переиграть обучение"))
{
    TutorialManager.ResetTutorial();
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}
```

### Сценарий 3: Выборочное обучение для новых игроков
```csharp
bool isNewPlayer = !PlayerPrefs.HasKey("FirstTimeGame");
if (isNewPlayer)
{
    tutorialManager.StartTutorial();
    PlayerPrefs.SetInt("FirstTimeGame", 1);
}
```

## 10. Пример полного сценария

```csharp
public void SetupCompleteGameTutorial()
{
    var manager = gameObject.AddComponent<TutorialManager>();

    // Введение
    manager.AddStep("Добро пожаловать на ферму! 🐱", 
                    null as Image, duration: 2f);

    // Ресурсы
    manager.AddStep("Слева видны ваши ресурсы: Корм и Вода\nКоты в них нуждаются!", 
                    resourcesImage, duration: 3f);

    // Первый кот
    manager.AddStep("Кликните на этого кота, чтобы узнать о нём", 
                    catImage, duration: 2f);

    // Создание новых котов
    manager.AddStep("Кликните эту кнопку, чтобы создать новых котов!", 
                    addCatButton, waitForClick: true);

    // Магазин
    manager.AddStep("В магазине можно купить редких котов", 
                    shopButton, duration: 2f);

    // Финиш
    manager.AddStep("Вы готовы! Начинайте своё путешествие! 🚀", 
                    null as Image, duration: 2f);

    manager.StartTutorial();
}
```

---

**Готовые шаблоны для вашей игры находятся в `TutorialInitializer.cs`**

Используйте эти примеры как основу для создания собственных сценариев обучения!
