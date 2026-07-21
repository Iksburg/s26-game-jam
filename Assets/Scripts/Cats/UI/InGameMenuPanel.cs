using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CatWorld.Cats
{
    /// <summary>
    /// Внутриигровое меню (пауза): сохранение, настройки, выход в главное меню.
    /// Открывается кнопкой «Меню», закрывается крестиком. Пока открыто — игра
    /// на паузе (Time.timeScale = 0), фон затемнён и перехватывает клики.
    /// Компонент держится на активном Canvas, окно — выключенный дочерний root.
    /// </summary>
    public class InGameMenuPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Text _statusLabel;
        [SerializeField] private SettingsPanel _settingsPanel;
        [SerializeField] private GameSaveService _saveService;
        [Tooltip("Имя сцены главного меню (должна быть в Build Settings).")]
        [SerializeField] private string _mainMenuSceneName = "MainMenu";
        [Tooltip("Сколько секунд показывать сообщение о сохранении.")]
        [SerializeField] private float _statusDuration = 5f;

        private Coroutine _statusRoutine;

        public bool IsOpen => _root != null && _root.activeSelf;

        private void Awake()
        {
            // Никакого SetActive(false) здесь: окно и так сохранено выключенным,
            // а компонент висит на активном Canvas.
            _openButton.onClick.AddListener(Open);
            _closeButton.onClick.AddListener(Close);
            _saveButton.onClick.AddListener(SaveGame);
            _settingsButton.onClick.AddListener(OpenSettings);
            _mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        /// <summary>Заполняется билдером сцены (editor wiring).</summary>
        public void Configure(GameObject root, Button openButton, Button closeButton,
            Button saveButton, Button settingsButton, Button mainMenuButton,
            Text statusLabel, SettingsPanel settingsPanel, GameSaveService saveService)
        {
            _root = root;
            _openButton = openButton;
            _closeButton = closeButton;
            _saveButton = saveButton;
            _settingsButton = settingsButton;
            _mainMenuButton = mainMenuButton;
            _statusLabel = statusLabel;
            _settingsPanel = settingsPanel;
            _saveService = saveService;
        }

        public void Open()
        {
            if (_statusLabel != null)
                _statusLabel.text = string.Empty;
            _root.SetActive(true);
            Time.timeScale = 0f; // пауза, пока меню открыто
        }

        public void Close()
        {
            _root.SetActive(false);
            Time.timeScale = 1f;
        }

        private void SaveGame()
        {
            if (_saveService == null)
                _saveService = FindFirstObjectByType<GameSaveService>();

            if (_saveService == null)
            {
                ShowStatus("Не удалось сохранить игру");
                Debug.LogError("[InGameMenu] В сцене нет GameSaveService.");
                return;
            }

            _saveService.SaveGame();
            ShowStatus("Игра сохранена");
        }

        private void ShowStatus(string message)
        {
            if (_statusLabel == null)
                return;

            _statusLabel.text = message;
            if (_statusRoutine != null)
                StopCoroutine(_statusRoutine);
            _statusRoutine = StartCoroutine(ClearStatusAfterDelay());
        }

        private IEnumerator ClearStatusAfterDelay()
        {
            // Realtime: меню держит игру на паузе (timeScale = 0),
            // и обычный WaitForSeconds никогда бы не досчитал.
            yield return new WaitForSecondsRealtime(_statusDuration);
            if (_statusLabel != null)
                _statusLabel.text = string.Empty;
            _statusRoutine = null;
        }

        private void OpenSettings()
        {
            if (_settingsPanel != null)
                _settingsPanel.Open();
        }

        private void GoToMainMenu()
        {
            // Снимаем паузу: timeScale глобальный и остался бы нулевым в меню.
            Time.timeScale = 1f;
            SceneManager.LoadScene(_mainMenuSceneName);
        }
    }
}
