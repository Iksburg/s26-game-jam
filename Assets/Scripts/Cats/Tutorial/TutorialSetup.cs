using UnityEngine;
using UnityEngine.UI;

namespace CatWorld.Cats.Tutorial
{
    /// <summary>
    /// Автоматически настраивает и запускает обучение при старте сцены.
    /// Добавьте этот компонент на Canvas каждой сцены, где нужно обучение.
    /// </summary>
    public class TutorialSetup : MonoBehaviour
    {
        [SerializeField] private TutorialManager _tutorialManager;
        [SerializeField] private TutorialInitializer _tutorialInitializer;
        [SerializeField] private TutorialHintPanel _hintPanel;
        [SerializeField] private bool _enableOnGameplay = true;
        [SerializeField] private bool _enableOnMainMenu = false;

        private void Awake()
        {
            // Если компоненты не назначены вручную, пытаемся найти их на сцене
            if (_tutorialManager == null)
                _tutorialManager = FindObjectOfType<TutorialManager>();

            if (_tutorialInitializer == null)
                _tutorialInitializer = FindObjectOfType<TutorialInitializer>();

            if (_hintPanel == null)
                _hintPanel = FindObjectOfType<TutorialHintPanel>();

            // Создаём компоненты, если их нет
            if (_tutorialManager == null)
                CreateTutorialSystem();
        }

        private void Start()
        {
            if (!ShouldRunTutorial())
                return;

            if (TutorialManager.HasCompletedTutorial)
                return; // Обучение уже пройдено

            InitializeTutorialForScene();
        }

        private bool ShouldRunTutorial()
        {
            string sceneName = gameObject.scene.name;
            
            if (sceneName == "MainMenu" && _enableOnMainMenu)
                return true;
            
            if (sceneName == "CatSpawn" && _enableOnGameplay)
                return true;

            return false;
        }

        private void CreateTutorialSystem()
        {
            // Создаём объект Tutorial Manager
            var tutorialGo = new GameObject("TutorialManager");
            tutorialGo.transform.SetParent(transform);
            _tutorialManager = tutorialGo.AddComponent<TutorialManager>();
            _tutorialManager.DisableAutoStart(); // Контролируем из TutorialSetup

            // Создаём HintPanel
            var hintPanelGo = new GameObject("TutorialHintPanel");
            hintPanelGo.transform.SetParent(transform);
            _hintPanel = hintPanelGo.AddComponent<TutorialHintPanel>();

            var hintRect = hintPanelGo.AddComponent<RectTransform>();
            hintRect.offsetMin = Vector2.zero;
            hintRect.offsetMax = Vector2.zero;

            // Создаём Text элемент для подсказки
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(hintPanelGo.transform);
            var textComponent = textGo.AddComponent<Text>();
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = 24;
            textComponent.fontStyle = FontStyle.Bold;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.color = Color.white;

            var textRect = textGo.AddComponent<RectTransform>();
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            _hintPanel.SetTextComponent(textComponent);

            // Создаём Background для панели
            var bgImage = hintPanelGo.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            // Создаём кнопку "Далее"
            var nextButtonGo = new GameObject("NextButton");
            nextButtonGo.transform.SetParent(hintPanelGo.transform);
            var nextButton = nextButtonGo.AddComponent<Button>();
            var nextButtonImage = nextButtonGo.AddComponent<Image>();
            nextButtonImage.color = new Color(0.2f, 0.6f, 0.2f, 1f);

            var nextButtonRect = nextButtonGo.AddComponent<RectTransform>();
            nextButtonRect.sizeDelta = new Vector2(150f, 50f);
            nextButtonRect.anchoredPosition = new Vector3(0, -60f, 0);

            // Текст кнопки
            var btnTextGo = new GameObject("Text");
            btnTextGo.transform.SetParent(nextButtonGo.transform);
            var btnText = btnTextGo.AddComponent<Text>();
            btnText.text = "Далее →";
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnText.fontSize = 20;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.white;

            var btnTextRect = btnTextGo.AddComponent<RectTransform>();
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;

            _hintPanel.SetNextButton(nextButton);

            // Добавляем CanvasGroup для фейдинга
            hintPanelGo.AddComponent<CanvasGroup>();

            _tutorialManager.SetHintPanel(_hintPanel);

            // Инициализатор
            _tutorialInitializer = tutorialGo.AddComponent<TutorialInitializer>();
            _tutorialInitializer.SetTutorialManager(_tutorialManager);
        }

        private void InitializeTutorialForScene()
        {
            if (_tutorialInitializer == null)
                return;

            string sceneName = gameObject.scene.name;

            if (sceneName == "MainMenu")
            {
                _tutorialInitializer.InitializeMainMenuTutorial();
            }
            else if (sceneName == "CatSpawn")
            {
                _tutorialInitializer.InitializeGameplayTutorial();
            }
        }
    }
}
