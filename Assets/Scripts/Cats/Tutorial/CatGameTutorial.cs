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
        
        [Tooltip("Кнопка/Радио выбора пола (Девочка)")]
        public Toggle selectFemaleButton;
        
        [Tooltip("Зона дома, куда нужно перетащить котов (Image или RectTransform)")]
        public Image houseDropZone;
        
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
                _manager.DisableAutoStart(); // Мы управляем запуском вручную
            }

            // --- НАСТРОЙКА ШАГОВ ---

            // 1) Добро пожаловать!
            _manager.AddStep("Добро пожаловать в мир котов! 🐱\nДавай научимся основам.", null, duration: 3f);

            // 2) Откройте магазин
            _manager.AddStep("Сначала нам нужен кот. Открой магазин.", shopButton, waitForClick: true);

            // 3) Купите кота
            _manager.AddStep("Купи кота.", buyCatButton, waitForClick: true);

            // 4) Введите имя кота
            _manager.AddStep("Придумай имя своему новому другу.", null, duration: 5f);
            // Примечание: Тут мы просто подсвечиваем поле. Логика ввода текста остается за игроком.
            // Если нужно ждать именно ввода, потребуется доработка TutorialStep, но пока оставим таймер.
            
            // 5) Подтвердите покупку
            _manager.AddStep("Подтверди создание кота.", confirmBuyButton, waitForClick: true);
            
            _manager.AddStep("Закрой магазин", shopCloseButton, waitForClick: true);
            
            _manager.AddStep("Посмотри на своего нового друга! Но ему одиноко одному.", null, duration: 3f);

            // 6) Откройте еще раз магазин
            _manager.AddStep("Отлично! Теперь давай заведем ему пару. Снова открой магазин.", shopButton, waitForClick: true);

            // 7) Купите еще одного кота
            _manager.AddStep("Купи второго кота.", buyCatButton, waitForClick: true);

            // 8) Выберите пол нового кота - девочка
            _manager.AddStep("На этот раз выбери пол: Девочка ♀", null, duration: 2f);

            // 9) Введите имя кота
            _manager.AddStep("Дай ей имя.", nameInputField.GetComponent<Image>(), duration: 5f);

            // 10) Подтвердите покупку
            _manager.AddStep("Подтверди покупку.", confirmBuyButton, waitForClick: true);
            
            _manager.AddStep("Закрой магазин", shopCloseButton, waitForClick: true);

            // 11) Перетащите котов в дом для разведения
            _manager.AddStep(
                "Перетащи кота в Дом! 🏠\n(Зажми кота и тяни сюда)", 
                houseDropZone, 
                duration: 0f, 
                waitForDrop: true 
            );

            // 12) Дождитесь появления нового котенка
            _manager.AddStep("Жди... Скоро появится котенок! 👶", null, duration: 5f); 

            // 13) Выберите имя нового котенка
            _manager.AddStep("Ура! Котенок родился! Дай ему имя.", nameInputField.GetComponent<Image>(), duration: 3f);

            // 14) Зайдите в магазин
            _manager.AddStep("Котят стало много? Пора заработать монеты. Иди в магазин.", shopButton, waitForClick: true);

            // 15) Нажмите продажа
            _manager.AddStep("Переключись на вкладку 'Продажа'.", sellTabButton, waitForClick: true);

            // 16) Продайте нового котенка
            _manager.AddStep("Продай новорожденного котенка.", null, duration: 5f);

            // 17) Зайдите в магазин (снова, чтобы купить ресурсы)
            _manager.AddStep("Молодцы! Но коты хотят есть. Снова в магазин.", shopButton, waitForClick: true);

            // 18) Купите вашим котам еды и воды
            _manager.AddStep("Купи немного Еды 🥕", buyFoodButton, waitForClick: true);
            _manager.AddStep("И не забудь про Воду 💧", buyWaterButton, waitForClick: true);

            // 19) На этом все! Удачной игры!
            _manager.AddStep("Обучение завершено! 🎉\nТеперь ты готов к самостоятельной игре. Удачи!", null, duration: 4f);

            // Запуск
            _manager.StartTutorial();
        }
    }
}
