using UnityEngine;
using UnityEngine.UI;

namespace CatWorld.Cats
{
    /// <summary>
    /// Окно появления нового кота: игрок выбирает пол и вводит имя.
    /// Пока окно открыто, игра приостанавливается (по концепту: до ввода
    /// имени игровой процесс останавливается). Подтверждение недоступно,
    /// пока имя пустое.
    /// </summary>
    public class SpawnCatPanel : MonoBehaviour
    {
        [SerializeField] private CatSpawner _spawner;
        [SerializeField] private Button _openButton;
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private InputField _nameInput;
        [SerializeField] private Toggle _maleToggle;
        [SerializeField] private Toggle _femaleToggle;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        private void Awake()
        {
            _openButton.onClick.AddListener(Open);
            _confirmButton.onClick.AddListener(Confirm);
            _cancelButton.onClick.AddListener(Close);
            _nameInput.onValueChanged.AddListener(OnNameChanged);
            _panelRoot.SetActive(false);
        }

        private void Open()
        {
            _nameInput.text = string.Empty;
            _maleToggle.isOn = true;
            _femaleToggle.isOn = false;
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

            Sex sex = _maleToggle.isOn ? Sex.Male : Sex.Female;
            _spawner.SpawnCat(sex, catName);
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
