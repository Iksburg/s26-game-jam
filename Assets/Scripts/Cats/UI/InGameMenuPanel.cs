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
        [Tooltip("Имя сцены главного меню (должна быть в Build Settings).")]
        [SerializeField] private string _mainMenuSceneName = "MainMenu";

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
            Text statusLabel, SettingsPanel settingsPanel)
        {
            _root = root;
            _openButton = openButton;
            _closeButton = closeButton;
            _saveButton = saveButton;
            _settingsButton = settingsButton;
            _mainMenuButton = mainMenuButton;
            _statusLabel = statusLabel;
            _settingsPanel = settingsPanel;
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

        /// <summary>
        /// Заглушка сохранения: реальная запись появится в задаче сохранений
        /// (писать под ключом SaveData.SaveKey, тогда оживёт и «Продолжить»).
        /// </summary>
        private void SaveGame()
        {
            Debug.Log("[InGameMenu] Сохранение игры — заглушка, система сохранений ещё не реализована.");
            if (_statusLabel != null)
                _statusLabel.text = "Сохранение появится позже";
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
