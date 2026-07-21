using UnityEngine;
using UnityEngine.UI;

namespace CatWorld.Cats
{
    /// <summary>
    /// Окно настроек: ползунки громкости музыки и звуков (по отдельности),
    /// переключатель полноэкранного режима, кнопка «Назад».
    /// Значения применяются сразу и сохраняются через GameSettings.
    /// Компонент держится на активном объекте (Canvas), окно — дочерний root.
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _soundSlider;
        [SerializeField] private Text _musicValueLabel;
        [SerializeField] private Text _soundValueLabel;
        [SerializeField] private Toggle _fullscreenToggle;
        [SerializeField] private Button _backButton;

        [Header("Автосохранение")]
        [SerializeField] private Toggle _autoSaveToggle;
        [SerializeField] private Toggle _interval1Toggle;
        [SerializeField] private Toggle _interval5Toggle;
        [SerializeField] private Toggle _interval15Toggle;
        [SerializeField] private Text _intervalLabel;

        private void Awake()
        {
            _musicSlider.onValueChanged.AddListener(OnMusicChanged);
            _soundSlider.onValueChanged.AddListener(OnSoundChanged);
            _fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            _backButton.onClick.AddListener(Close);

            _autoSaveToggle.onValueChanged.AddListener(OnAutoSaveChanged);
            _interval1Toggle.onValueChanged.AddListener(isOn => OnIntervalChanged(isOn, 1));
            _interval5Toggle.onValueChanged.AddListener(isOn => OnIntervalChanged(isOn, 5));
            _interval15Toggle.onValueChanged.AddListener(isOn => OnIntervalChanged(isOn, 15));
        }

        /// <summary>Заполняется билдером сцены (editor wiring).</summary>
        public void Configure(GameObject root, Slider musicSlider, Slider soundSlider,
            Text musicValueLabel, Text soundValueLabel, Toggle fullscreenToggle, Button backButton,
            Toggle autoSaveToggle, Toggle interval1Toggle, Toggle interval5Toggle,
            Toggle interval15Toggle, Text intervalLabel)
        {
            _root = root;
            _musicSlider = musicSlider;
            _soundSlider = soundSlider;
            _musicValueLabel = musicValueLabel;
            _soundValueLabel = soundValueLabel;
            _fullscreenToggle = fullscreenToggle;
            _backButton = backButton;
            _autoSaveToggle = autoSaveToggle;
            _interval1Toggle = interval1Toggle;
            _interval5Toggle = interval5Toggle;
            _interval15Toggle = interval15Toggle;
            _intervalLabel = intervalLabel;
        }

        public void Open()
        {
            // Текущие значения — без вызова слушателей, чтобы не переписывать их же.
            _musicSlider.SetValueWithoutNotify(GameSettings.MusicVolume);
            _soundSlider.SetValueWithoutNotify(GameSettings.SoundVolume);
            _fullscreenToggle.SetIsOnWithoutNotify(GameSettings.Fullscreen);

            _autoSaveToggle.SetIsOnWithoutNotify(GameSettings.AutoSaveEnabled);
            int interval = GameSettings.AutoSaveIntervalMinutes;
            _interval1Toggle.SetIsOnWithoutNotify(interval == 1);
            _interval5Toggle.SetIsOnWithoutNotify(interval == 5);
            _interval15Toggle.SetIsOnWithoutNotify(interval == 15);
            RefreshIntervalAvailability();

            RefreshLabels();
            _root.SetActive(true);
        }

        public void Close()
        {
            _root.SetActive(false);
        }

        private void OnMusicChanged(float value)
        {
            GameSettings.MusicVolume = value;
            RefreshLabels();
        }

        private void OnSoundChanged(float value)
        {
            GameSettings.SoundVolume = value;
            RefreshLabels();
        }

        private void OnFullscreenChanged(bool value)
        {
            GameSettings.Fullscreen = value;
        }

        private void OnAutoSaveChanged(bool value)
        {
            GameSettings.AutoSaveEnabled = value;
            RefreshIntervalAvailability();
        }

        private void OnIntervalChanged(bool isOn, int minutes)
        {
            // ToggleGroup дёргает событие и у выключаемого переключателя — реагируем
            // только на тот, который стал активным.
            if (isOn)
                GameSettings.AutoSaveIntervalMinutes = minutes;
        }

        /// <summary>Пока автосохранение выключено, выбор интервала недоступен.</summary>
        private void RefreshIntervalAvailability()
        {
            bool enabled = _autoSaveToggle.isOn;
            _interval1Toggle.interactable = enabled;
            _interval5Toggle.interactable = enabled;
            _interval15Toggle.interactable = enabled;
            if (_intervalLabel != null)
                _intervalLabel.color = enabled
                    ? new Color(0.28f, 0.24f, 0.18f)
                    : new Color(0.62f, 0.58f, 0.52f);
        }

        private void RefreshLabels()
        {
            if (_musicValueLabel != null)
                _musicValueLabel.text = $"{Mathf.RoundToInt(_musicSlider.value * 100f)}%";
            if (_soundValueLabel != null)
                _soundValueLabel.text = $"{Mathf.RoundToInt(_soundSlider.value * 100f)}%";
        }
    }
}
