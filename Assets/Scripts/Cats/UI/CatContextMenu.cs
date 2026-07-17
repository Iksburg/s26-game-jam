using UnityEngine;
using UnityEngine.UI;

namespace CatWorld.Cats
{
    /// <summary>
    /// Мини-меню, появляющееся справа от кота по ПКМ. Содержит кнопку
    /// «Осмотреть», открывающую карточку кота.
    /// </summary>
    public class CatContextMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private RectTransform _rect;
        [SerializeField] private Button _inspectButton;
        [SerializeField] private CatCardPanel _card;
        [Tooltip("Смещение меню вправо от кота, в мировых юнитах.")]
        [SerializeField] private float _worldOffsetX = 0.6f;

        private Cat _cat;
        private Camera _camera;

        private void Awake()
        {
            // Компонент на неактивном объекте: Awake выполняется при первом Show().
            // SetActive(false) здесь гасил бы меню в момент первого открытия
            // (в сцене меню и так сохранено выключенным).
            if (_inspectButton != null)
                _inspectButton.onClick.AddListener(Inspect);
        }

        private void Start()
        {
            _camera = Camera.main;
        }

        /// <summary>Заполняется билдером сцены (editor wiring).</summary>
        public void Configure(GameObject root, RectTransform rect, Button inspectButton, CatCardPanel card)
        {
            _root = root;
            _rect = rect;
            _inspectButton = inspectButton;
            _card = card;
        }

        /// <summary>Показывает меню справа от указанного кота.</summary>
        public void Show(Cat cat)
        {
            _cat = cat;
            if (_root == null || _rect == null)
                return;

            _root.SetActive(true);
            PositionNextTo(cat);
        }

        public void Hide()
        {
            _cat = null;
            if (_root != null)
                _root.SetActive(false);
        }

        private void Update()
        {
            // Меню следует за котом, пока открыто (кот продолжает гулять).
            if (_root != null && _root.activeSelf)
            {
                if (_cat == null)
                    Hide();
                else
                    PositionNextTo(_cat);
            }
        }

        private void PositionNextTo(Cat cat)
        {
            if (_camera == null)
                _camera = Camera.main;
            if (_camera == null)
                return;

            Vector3 worldPos = cat.transform.position + new Vector3(_worldOffsetX, 0f, 0f);
            _rect.position = _camera.WorldToScreenPoint(worldPos);
        }

        private void Inspect()
        {
            if (_card != null && _cat != null)
                _card.Open(_cat);
            Hide();
        }
    }
}
