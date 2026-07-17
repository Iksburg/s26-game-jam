using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace CatWorld.Cats
{
    /// <summary>
    /// Ловит ПКМ по коту и показывает рядом с ним мини-меню «Осмотреть».
    /// Клик мимо кота закрывает меню. Проект на новом Input System.
    /// </summary>
    public class CatClickDetector : MonoBehaviour
    {
        [SerializeField] private CatContextMenu _contextMenu;

        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
        }

        /// <summary>Заполняется билдером сцены (editor wiring).</summary>
        public void Configure(CatContextMenu contextMenu)
        {
            _contextMenu = contextMenu;
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.rightButton.wasPressedThisFrame)
                return;

            // Клик по UI не должен доходить до котов.
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (_camera == null)
                _camera = Camera.main;
            if (_camera == null || _contextMenu == null)
                return;

            Vector3 screenPos = mouse.position.ReadValue();
            Vector2 worldPos = _camera.ScreenToWorldPoint(screenPos);

            // OverlapPointAll: под точкой лежит ещё и триггер FarmBounds — ищем именно кота.
            var hits = Physics2D.OverlapPointAll(worldPos);
            Cat cat = null;
            foreach (var hit in hits)
            {
                cat = hit.GetComponentInParent<Cat>();
                if (cat != null)
                    break;
            }

            if (cat != null)
                _contextMenu.Show(cat);
            else
                _contextMenu.Hide();
        }
    }
}
