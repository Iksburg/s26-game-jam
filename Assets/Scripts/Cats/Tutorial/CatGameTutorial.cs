using UnityEngine;
using UnityEngine.UI;
using CatWorld.Cats.Tutorial; // Пространство имен твоего туториала

namespace CatWorld.Cats.Tutorial
{
    /// <summary>
    /// Сценарий обучения для основной механики игры (покупка, разведение, продажа).
    /// </summary>
    public class CatGameTutorial : MonoBehaviour
    {
        [Header("Ссылки на UI элементы (перетащи из инспектора)")]
        [Tooltip("Кнопка открытия магазина")]
        public Button shopButton;
        
        [Tooltip("Кнопка закрытия магазина")]
        public Button shopCloseButton;
        
        [Tooltip("Кнопка покупки кота внутри магазина")]
        public Button buyCatButton;
        
        [Tooltip("Поле ввода имени кота")]
        public InputField nameInputField;
        
        [Tooltip("Кнопка подтверждения покупки")]
        public Button confirmBuyButton;
        
        [Tooltip("Кнопка подтверждения имени рожденного котенка")]
        public Button confirmNameButton;
        
        [Tooltip("Кнопка/Радио выбора пола (Девочка)")]
        public Image selectFemaleImage;
        
        [Tooltip("Зона дома, куда нужно перетащить котов (Image или RectTransform)")]
        public SpriteRenderer houseDropZone;
        
        [Tooltip("Кнопка продажи в магазине")]
        public Button sellTabButton;
        
        [Tooltip("Кнопка покупки еды")]
        public Button buyFoodButton;
        
        [Tooltip("Кнопка покупки воды")]
        public Button buyWaterButton;

        private TutorialManager _manager;

        private void Start()
        {
            // Проверяем, проходило ли уже обучение глобально
            if (TutorialManager.HasCompletedTutorial)
            {
                enabled = false; // Отключаем скрипт, если обучение уже пройдено
                return;
            }

            InitializeTutorial();
        }

        private void InitializeTutorial()
        {
            // Находим или создаем менеджер
            _manager = FindObjectOfType<TutorialManager>();
            if (_manager == null)
            {
                var go = new GameObject("TutorialManager");
                _manager = go.AddComponent<TutorialManager>();
                _manager.DisableAutoStart();
            }

            // --- УПРОЩЕННЫЙ СЦЕНАРИЙ ---

// 1. Приветствие
            _manager.AddStep("Привет, мяу! Давай быстро разберемся, как играть, мяу!", targetImage: null, duration: 3f);

// 2. Покупка ресурсов (сначала готовимся)
            _manager.AddStep("Сначала открой магазин, мяу.", shopButton, waitForClick: true);
            _manager.AddStep("Купи немного Еды 🥕", buyFoodButton, waitForClick: true);
            _manager.AddStep("И не забудь купить Воду 💧", buyWaterButton, waitForClick: true);
            _manager.AddStep("Теперь закрой магазин, мяу.", shopCloseButton, waitForClick: true);

// 3. Покупка котов
            _manager.AddStep("Теперь нам нужно купить котов. Давай еще раз открой магазин, мяу.", shopButton, waitForClick: true);
            _manager.AddStep("Купи первого кота. Выбери пол: Мальчик", buyCatButton, waitForClick: true);
            _manager.AddStep("Дай ему кошачье имя, мяу.", nameInputField.GetComponent<Image>(), duration: 2f);
            _manager.AddStep("Подтверди покупку.", confirmBuyButton, waitForClick: true);

            _manager.AddStep("Купи второго кота (девочку).", buyCatButton, waitForClick: true);
            _manager.AddStep("Выбери пол: Девочка ♀", selectFemaleImage, duration: 1f);
            _manager.AddStep("Дай ей имя, мяу.", nameInputField.GetComponent<Image>(), duration: 2f);
            _manager.AddStep("Подтверди покупку.", confirmBuyButton, waitForClick: true);
            _manager.AddStep("Закрой магазин, мяу.", shopCloseButton, waitForClick: true);

// 4. Разведение
            _manager.AddStep("Перетащи кота-мальчика в Домик 🏠", targetSprite: houseDropZone, duration: 4f);
            _manager.AddStep("Перетащи кота-девочку в Домик 🏠", targetSprite: houseDropZone, duration: 4f);

// 5. Финал
            _manager.AddStep("Готово! У котов скоро появится котенок. Удачной игры, мяу! 🎉", targetImage: null, duration: 4f);

            // Запуск
            _manager.StartTutorial();
        }
    }
}
