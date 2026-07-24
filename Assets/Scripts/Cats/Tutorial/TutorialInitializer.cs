using UnityEngine;
using UnityEngine.UI;

namespace CatWorld.Cats.Tutorial
{
    /// <summary>
    /// Инициализатор обучения: подготавливает и запускает сценарий обучения.
    /// Можно использовать для разных сцен и разных наборов подсказок.
    /// </summary>
    public class TutorialInitializer : MonoBehaviour
    {
        public TutorialManager _tutorialManager;

        /// <summary>Настроить обучение для главного меню.</summary>
        public void SetTutorialManager(TutorialManager tutorialManager)
        {
            _tutorialManager = tutorialManager;
        }

        public void InitializeMainMenuTutorial()
        {
            if (_tutorialManager == null)
                return;

            _tutorialManager.AddStep(
                "Добро пожаловать в мир кошек! 🐱\n\nЧтобы начать, нажмите 'Новая игра'",
                GetButtonByName("NewGameButton"),
                waitForClick: false,
                duration: 3f
            );

            _tutorialManager.AddStep(
                "Вы также можете продолжить сохранённую игру",
                GetButtonByName("ContinueButton"),
                waitForClick: false,
                duration: 2f
            );

            _tutorialManager.AddStep(
                "Настройки игры находятся здесь",
                GetButtonByName("SettingsButton"),
                waitForClick: false,
                duration: 2f
            );

            _tutorialManager.StartTutorial();
        }

        /// <summary>Настроить обучение для игровой сцены.</summary>
        public void InitializeGameplayTutorial()
        {
            if (_tutorialManager == null)
                return;

            _tutorialManager.AddStep(
                "Тут показаны ваши ресурсы: Корм 🥕 и Вода 💧\nКосмические коты очень голодны! 😸",
                GetImageByName("ResourcesPanel"),
                duration: 3f
            );

            _tutorialManager.AddStep(
                "Кликните на кота, чтобы взаимодействовать с ним",
                GetImageByName("CatsContainer"),
                duration: 3f
            );

            _tutorialManager.AddStep(
                "Нажмите эту кнопку, чтобы создать новое семейство коров... ух, котов! 🎉",
                GetButtonByName("SpawnCatButton"),
                waitForClick: false,
                duration: 3f
            );

            _tutorialManager.AddStep(
                "Используйте магазин для покупки редких котов",
                GetButtonByName("ShopButton"),
                waitForClick: false,
                duration: 2f
            );

            _tutorialManager.AddStep(
                "Всё готово! Теперь создавайте и выращивайте своих котов! 🚀",
                null as Image,
                duration: 2f
            );

            _tutorialManager.StartTutorial();
        }

        private Button GetButtonByName(string buttonName)
        {
            var buttons = FindObjectsOfType<Button>();
            foreach (var btn in buttons)
            {
                if (btn.gameObject.name == buttonName)
                    return btn;
            }
            Debug.LogWarning($"Кнопка '{buttonName}' не найдена!");
            return null;
        }

        private Image GetImageByName(string imageName)
        {
            var images = FindObjectsOfType<Image>();
            foreach (var img in images)
            {
                if (img.gameObject.name == imageName)
                    return img;
            }
            Debug.LogWarning($"Изображение '{imageName}' не найдено!");
            return null;
        }

        public void SkipTutorial()
        {
            if (_tutorialManager != null)
                _tutorialManager.SkipTutorial();
        }
    }
}
