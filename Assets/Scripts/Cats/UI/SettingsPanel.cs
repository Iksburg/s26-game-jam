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

        private void Awake()
        {
            _musicSlider.onValueChanged.AddListener(OnMusicChanged);
            _soundSlider.onValueChanged.AddListener(OnSoundChanged);
            _fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            _backButton.onClick.AddListener(Close);
        }

        /// <summary>Заполняется билдером сцены (editor wiring).</summary>
        public void Configure(GameObject root, Slider musicSlider, Slider soundSlider,
            Text musicValueLabel, Text soundValueLabel, Toggle fullscreenToggle, Button backButton)
        {
            _root = root;
            _musicSlider = musicSlider;
            _soundSlider = soundSlider;
            _musicValueLabel = musicValueLabel;
            _soundValueLabel = soundValueLabel;
            _fullscreenToggle = fullscreenToggle;
            _backButton = backButton;
        }

        public void Open()
        {
            // Текущие значения — без вызова слушателей, чтобы не переписывать их же.
            _musicSlider.SetValueWithoutNotify(GameSettings.MusicVolume);
            _soundSlider.SetValueWithoutNotify(GameSettings.SoundVolume);
            _fullscreenToggle.SetIsOnWithoutNotify(GameSettings.Fullscreen);
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

        private void RefreshLabels()
        {
            if (_musicValueLabel != null)
                _musicValueLabel.text = $"{Mathf.RoundToInt(_musicSlider.value * 100f)}%";
            if (_soundValueLabel != null)
                _soundValueLabel.text = $"{Mathf.RoundToInt(_soundSlider.value * 100f)}%";
        }
    }
}
