using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CatWorld.Cats
{
    /// <summary>
    /// Логика главного меню: Новая игра / Продолжить / Настройки / Выход.
    /// «Продолжить» недоступна, пока нет сохранения (SaveData.HasSave —
    /// заглушка до задачи сохранений; загрузка сейвов подключится там же).
    /// Компонент держится на активном Canvas.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private SettingsPanel _settingsPanel;
        [Tooltip("Имя игровой сцены (должна быть в Build Settings).")]
        [SerializeField] private string _gameSceneName = "CatSpawn";

        private void Awake()
        {
            _newGameButton.onClick.AddListener(StartNewGame);
            _continueButton.onClick.AddListener(ContinueGame);
            _settingsButton.onClick.AddListener(OpenSettings);
            _quitButton.onClick.AddListener(QuitGame);
        }

        private void Start()
        {
            _continueButton.interactable = SaveData.HasSave;
        }

        /// <summary>Заполняется билдером сцены (editor wiring).</summary>
        public void Configure(Button newGameButton, Button continueButton,
            Button settingsButton, Button quitButton, SettingsPanel settingsPanel)
        {
            _newGameButton = newGameButton;
            _continueButton = continueButton;
            _settingsButton = settingsButton;
            _quitButton = quitButton;
            _settingsPanel = settingsPanel;
        }

        private void StartNewGame()
        {
            // Явно сбрасываем флаг: иначе новая игра подхватила бы сейв,
            // если до этого нажимали «Продолжить».
            SaveSystem.LoadRequested = false;
            SceneManager.LoadScene(_gameSceneName);
        }

        private void ContinueGame()
        {
            if (!SaveData.HasSave)
                return;

            // Сцена сама восстановит состояние: GameSaveService прочитает флаг.
            SaveSystem.LoadRequested = true;
            SceneManager.LoadScene(_gameSceneName);
        }

        private void OpenSettings()
        {
            if (_settingsPanel != null)
                _settingsPanel.Open();
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
