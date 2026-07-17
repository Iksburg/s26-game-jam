using UnityEngine;
using UnityEngine.UI;

namespace CatWorld.Cats
{
    /// <summary>
    /// Универсальная карточка кота: иконка с его цветом, имя, пол, стадия роста,
    /// показатели сытости/жажды/чистоты (обновляются в реальном времени), блок
    /// черт характера (пока заготовка) и кнопки «Семейное древо» / закрытия.
    /// Открывается из мини-меню по ПКМ.
    /// </summary>
    public class CatCardPanel : MonoBehaviour
    {
        /// <summary>Максимальная высота иконки пола; ширина следует пропорциям спрайта.</summary>
        private const float MaxSexIconHeight = 56f;

        [SerializeField] private GameObject _root;

        [Header("Иконка и имя")]
        [SerializeField] private Image _icon;
        [SerializeField] private Text _nameLabel;

        [Header("Пол (спрайты назначаются в инспекторе)")]
        [SerializeField] private Image _sexIcon;
        [SerializeField] private Sprite _maleSprite;
        [SerializeField] private Sprite _femaleSprite;

        [Header("Стадия и показатели")]
        [SerializeField] private Text _stageLabel;
        [SerializeField] private Text _satietyLabel;
        [SerializeField] private Text _waterLabel;
        [SerializeField] private Text _cleanlinessLabel;

        [Header("Черты характера")]
        [Tooltip("Контейнер под черты. Пока пусто — заготовка под будущую задачу.")]
        [SerializeField] private Transform _traitsContainer;
        [SerializeField] private Text _traitsPlaceholder;

        [Header("Кнопки")]
        [SerializeField] private Button _familyTreeButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private FamilyTreePanel _familyTree;

        private Cat _cat;

        public bool IsOpen => _root != null && _root.activeSelf;

        private void Awake()
        {
            // ВАЖНО: компонент висит на неактивном в сцене объекте, поэтому Awake
            // выполняется только при первом открытии. Никакого SetActive(false)
            // здесь быть не должно — иначе первое открытие тут же гасит панель
            // (панель сохранена выключенной билдером сцены).
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);
            if (_familyTreeButton != null)
                _familyTreeButton.onClick.AddListener(OpenFamilyTree);
        }

        /// <summary>Заполняется билдером сцены (editor wiring).</summary>
        public void Configure(GameObject root, Image icon, Text nameLabel, Image sexIcon,
            Text stageLabel, Text satietyLabel, Text waterLabel, Text cleanlinessLabel,
            Transform traitsContainer, Text traitsPlaceholder,
            Button familyTreeButton, Button closeButton, FamilyTreePanel familyTree)
        {
            _root = root;
            _icon = icon;
            _nameLabel = nameLabel;
            _sexIcon = sexIcon;
            _stageLabel = stageLabel;
            _satietyLabel = satietyLabel;
            _waterLabel = waterLabel;
            _cleanlinessLabel = cleanlinessLabel;
            _traitsContainer = traitsContainer;
            _traitsPlaceholder = traitsPlaceholder;
            _familyTreeButton = familyTreeButton;
            _closeButton = closeButton;
            _familyTree = familyTree;
        }

        /// <summary>Открывает карточку для конкретного кота.</summary>
        public void Open(Cat cat)
        {
            _cat = cat;
            if (_root != null)
                _root.SetActive(true);
            RefreshStatic();
            RefreshNeeds();
        }

        public void Close()
        {
            _cat = null;
            if (_root != null)
                _root.SetActive(false);
        }

        private void Update()
        {
            if (!IsOpen)
                return;

            // Кот мог быть удалён (пожилой ушёл с фермы) — закрываем карточку.
            if (_cat == null)
            {
                Close();
                return;
            }

            RefreshNeeds();
        }

        /// <summary>Данные, меняющиеся редко: иконка, имя, пол, стадия, черты.</summary>
        private void RefreshStatic()
        {
            if (_cat == null)
                return;

            if (_icon != null)
            {
                var view = _cat.GetComponent<CatView>();
                if (view != null)
                    _icon.sprite = view.CurrentSprite;
                _icon.color = _cat.FurColor; // иконка в цвете конкретного кота
            }

            if (_nameLabel != null)
                _nameLabel.text = _cat.Name;

            if (_sexIcon != null)
            {
                Sprite sexSprite = _cat.Sex == Sex.Male ? _maleSprite : _femaleSprite;
                _sexIcon.sprite = sexSprite;
                // Пока спрайты пола не назначены — не показываем пустой квадрат.
                _sexIcon.enabled = sexSprite != null;
                // Спрайты М/Ж разного размера — подгоняем RectTransform под текущий.
                if (sexSprite != null)
                {
                    _sexIcon.SetNativeSize();
                    ClampIconHeight(_sexIcon.rectTransform, MaxSexIconHeight);
                }
            }

            if (_stageLabel != null)
                _stageLabel.text = GetStageName(_cat.Stage);

            RefreshTraits();
        }

        /// <summary>Показатели обновляются каждый кадр, пока карточка открыта.</summary>
        private void RefreshNeeds()
        {
            if (_satietyLabel != null)
                _satietyLabel.text = $"Сытость: {_cat.Satiety:F0}%";
            if (_waterLabel != null)
                _waterLabel.text = $"Жажда: {_cat.Water:F0}%";
            if (_cleanlinessLabel != null)
                _cleanlinessLabel.text = $"Чистота: {_cat.Cleanliness:F0}%";
        }

        /// <summary>
        /// Заготовка блока черт: контейнер уже связан, наполнение появится
        /// в задаче черт характера.
        /// </summary>
        private void RefreshTraits()
        {
            if (_traitsPlaceholder == null)
                return;

            int count = _cat.InnateTraits.Count + _cat.AcquiredTraits.Count;
            _traitsPlaceholder.text = count == 0
                ? "Черты характера: пока нет"
                : $"Черты характера: {count}";
        }

        private void OpenFamilyTree()
        {
            if (_familyTree != null)
                _familyTree.Open(_cat);
        }

        /// <summary>Вписывает нативный размер иконки в лимит высоты, сохраняя пропорции.</summary>
        private static void ClampIconHeight(RectTransform rect, float maxHeight)
        {
            Vector2 size = rect.sizeDelta;
            if (size.y <= maxHeight || size.y <= 0f)
                return;
            float k = maxHeight / size.y;
            rect.sizeDelta = new Vector2(size.x * k, maxHeight);
        }

        private static string GetStageName(LifeStage stage)
        {
            switch (stage)
            {
                case LifeStage.Kitten: return "Котёнок";
                case LifeStage.Adult: return "Взрослый";
                default: return "Пожилой";
            }
        }
    }
}
