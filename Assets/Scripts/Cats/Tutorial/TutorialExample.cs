using UnityEngine;
using UnityEngine.UI;

namespace CatWorld.Cats.Tutorial
{
    /// <summary>
    /// Пример использования системы обучения.
    /// Подключите этот компонент к главному Canvas или используйте код как справку.
    /// </summary>
    public class TutorialExample : MonoBehaviour
    {
        [SerializeField] private Button _resetTutorialButton;

        private void Start()
        {
            if (_resetTutorialButton != null)
                _resetTutorialButton.onClick.AddListener(ResetTutorial);
        }

        /// <summary>
        /// Пример 1: Создание простого обучения программно
        /// </summary>
        public void CreateSimpleTutorial()
        {
            var tutorialManager = gameObject.AddComponent<TutorialManager>();

            // Добавляем шаги
            tutorialManager.AddStep(
                "Добро пожаловать в CatWorld! 🐱",
                GetComponent<Image>(),
                duration: 3f
            );

            tutorialManager.AddStep(
                "Это самый простой способ создать обучение",
                GetComponent<Image>(),
                duration: 2f
            );

            // Запускаем
            tutorialManager.StartTutorial();
        }

        /// <summary>
        /// Пример 2: Обучение с ожиданием клика по кнопке
        /// </summary>
        public void CreateInteractiveTutorial()
        {
            var tutorialManager = gameObject.AddComponent<TutorialManager>();

            var button = FindObjectOfType<Button>();
            if (button != null)
            {
                tutorialManager.AddStep(
                    "Нажмите эту кнопку, чтобы продолжить",
                    button,
                    waitForClick: true
                );

                tutorialManager.StartTutorial();
            }
        }

        /// <summary>
        /// Пример 3: Полное обучение для игры
        /// </summary>
        public void CreateFullGameTutorial()
        {
            var tutorialManager = gameObject.AddComponent<TutorialManager>();

            // Шаг 1
            tutorialManager.AddStep(
                "Добро пожаловать на ферму котов! 🐱\n\nЭто ваш первый кот.",
                FindObjectOfType<Image>(),
                duration: 3f
            );

            // Шаг 2
            tutorialManager.AddStep(
                "Слева вверху видны ресурсы: Корм 🥕 и Вода 💧\nКоты нуждаются в них для выживания!",
                FindObjectOfType<Image>(),
                duration: 3f
            );

            // Шаг 3
            tutorialManager.AddStep(
                "Кликните на кота, чтобы увидеть его информацию",
                FindObjectOfType<Image>(),
                duration: 3f
            );

            // Шаг 4
            tutorialManager.AddStep(
                "Нажмите кнопку 'Добавить кота' внизу,\nчтобы создать семейство!",
                FindObjectOfType<Button>(),
                duration: 3f
            );

            // Завершающий шаг
            tutorialManager.AddStep(
                "Отлично! Теперь вы готовы!\n\nУдачи в разведении котов! 🚀",
                FindObjectOfType<Image>(),
                duration: 3f
            );

            tutorialManager.StartTutorial();
        }

        private void ResetTutorial()
        {
            TutorialManager.ResetTutorial();
            Debug.Log("Обучение сброшено. Перезагрузите сцену.");
        }

        /// <summary>
        /// Проверить статус обучения
        /// </summary>
        public void CheckTutorialStatus()
        {
            if (TutorialManager.HasCompletedTutorial)
            {
                Debug.Log("✓ Обучение уже пройдено!");
            }
            else
            {
                Debug.Log("✗ Обучение ещё не пройдено");
            }
        }
    }
}
