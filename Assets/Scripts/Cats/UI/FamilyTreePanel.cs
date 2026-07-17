using UnityEngine;
using UnityEngine.UI;

namespace CatWorld.Cats
{
    /// <summary>
    /// Окно семейного древа. Пока пустая заготовка: открывается из карточки кота
    /// и закрывается крестиком. Наполнение деревом (как в Sims 2, по концепту) —
    /// отдельная задача родословной.
    /// </summary>
    public class FamilyTreePanel : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Button _closeButton;

        private Cat _cat;

        private void Awake()
        {
            // Компонент на неактивном объекте: Awake выполняется при первом
            // открытии, поэтому здесь нельзя вызывать SetActive(false) —
            // первое Open() сразу закрывало бы окно (в сцене оно и так выключено).
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);
        }

        /// <summary>Заполняется билдером сцены (editor wiring).</summary>
        public void Configure(GameObject root, Button closeButton)
        {
            _root = root;
            _closeButton = closeButton;
        }

        /// <summary>Открывает древо для кота (данные подключатся в задаче родословной).</summary>
        public void Open(Cat cat)
        {
            _cat = cat;
            if (_root != null)
                _root.SetActive(true);
        }

        public void Close()
        {
            if (_root != null)
                _root.SetActive(false);
        }
    }
}
