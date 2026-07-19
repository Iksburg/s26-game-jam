using System; // Добавлено для Action
using Cats.Spawning;
using UnityEngine;
using UnityEngine.UI;

namespace CatWorld.Cats
{
    /// <summary>
    /// Окно появления нового кота: игрок выбирает пол и вводит имя (при покупке)
    /// или просто вводит имя (при рождении в комнате разведения).
    /// Пока окно открыто, игра приостанавливается. Подтверждение недоступно, пока имя пустое.
    /// </summary>
    public class SpawnCatPanel : MonoBehaviour
    {
        /// <summary>Максимальная длина имени кота (включая пробелы).</summary>
        public const int MaxNameLength = 24;

        [SerializeField] private CatSpawner _spawner;
        [SerializeField] private Button _openButton;
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private InputField _nameInput;
        [SerializeField] private Toggle _maleToggle;
        [SerializeField] private Toggle _femaleToggle;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        // Делегат, который выполнится при нажатии Confirm в режиме разведения
        private Action<string> _onNameConfirmedCallback;

        private void Awake()
        {
            _openButton.onClick.AddListener(() => OpenForPurchase()); // Старое поведение покупки
            _confirmButton.onClick.AddListener(Confirm);
            _cancelButton.onClick.AddListener(Close);
            _nameInput.onValueChanged.AddListener(OnNameChanged);
            _nameInput.characterLimit = MaxNameLength; 
            _panelRoot.SetActive(false);
        }

        /// <summary> Открытие панели в стандартном режиме покупки (с выбором пола) </summary>
        private void OpenForPurchase()
        {
            _onNameConfirmedCallback = null; // Сбрасываем кастомный колбэк разведения
            
            _nameInput.text = string.Empty;
            _maleToggle.gameObject.SetActive(true);
            _femaleToggle.gameObject.SetActive(true);
            _maleToggle.isOn = true;
            _femaleToggle.isOn = false;
            
            SetupCommonOpenState();
        }

        /// <summary> Публичный метод для комнаты разведения. Пол кота уже известен и заблокирован в UI. </summary>
        public void OpenForBreeding(Sex fixedSex, Action<string> onNameConfirmed)
        {
            _onNameConfirmedCallback = onNameConfirmed; // Запоминаем, кому вернуть имя кота

            _nameInput.text = string.Empty;
            
            // Настраиваем переключатели под уже рассчитанный генетикой пол и скрываем/блокируем их
            _maleToggle.isOn = fixedSex == Sex.Male;
            _femaleToggle.isOn = fixedSex == Sex.Female;
            _maleToggle.gameObject.SetActive(false);
            _femaleToggle.gameObject.SetActive(false);

            SetupCommonOpenState();
        }

        private void SetupCommonOpenState()
        {
            _panelRoot.SetActive(true);
            Time.timeScale = 0f; // игра на паузе, пока игрок не назовёт кота
            RefreshConfirmButton();
        }

        private void Close()
        {
            _panelRoot.SetActive(false);
            Time.timeScale = 1f;
        }

        private void Confirm()
        {
            string catName = _nameInput.text.Trim();
            if (string.IsNullOrEmpty(catName))
                return;
            
            if (catName.Length > MaxNameLength)
                catName = catName.Substring(0, MaxNameLength);

            // Если панель была открыта комнатой разведения:
            if (_onNameConfirmedCallback != null)
            {
                _onNameConfirmedCallback.Invoke(catName); // Передаем имя обратно в SexRoomController
            }
            else // Иначе дефолтное поведение покупки через спавнер
            {
                Sex sex = _maleToggle.isOn ? Sex.Male : Sex.Female;
                _spawner.SpawnCat(sex, catName);
            }

            Close();
        }

        private void OnNameChanged(string _)
        {
            RefreshConfirmButton();
        }

        private void RefreshConfirmButton()
        {
            _confirmButton.interactable = !string.IsNullOrWhiteSpace(_nameInput.text);
        }

        /// <summary>Заполняется билдером сцены (editor-time wiring).</summary>
        public void Configure(CatSpawner spawner, Button openButton, GameObject panelRoot,
            InputField nameInput, Toggle maleToggle, Toggle femaleToggle,
            Button confirmButton, Button cancelButton)
        {
            _spawner = spawner;
            _openButton = openButton;
            _panelRoot = panelRoot;
            _nameInput = nameInput;
            _maleToggle = maleToggle;
            _femaleToggle = femaleToggle;
            _confirmButton = confirmButton;
            _cancelButton = cancelButton;
        }
    }
}
