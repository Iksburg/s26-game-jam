using Cats.Genome;
using Cats.Genome.Breeding;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace CatWorld.Cats
{
    /// <summary>
    /// Лочит ПКМ по коту для вызова меню «Осмотреть» и ЛКМ для Drag & Drop перетаскивания кота.
    /// </summary>
    public class CatClickDetector : MonoBehaviour
    {
        [SerializeField] private CatContextMenu _contextMenu;

        private Camera _camera;
        
        // Поля для реализации Drag & Drop
        private Cat _draggedCat;
        private Vector3 _dragOffset;
        private bool _isDragging;

        private void Start()
        {
            _camera = Camera.main;
        }

        public void Configure(CatContextMenu contextMenu)
        {
            _contextMenu = contextMenu;
        }

        private void Update()
        {
            if (_camera == null) _camera = Camera.main;
            var mouse = Mouse.current;
            if (mouse == null || _camera == null) return;

            // 1. Логика Drag & Drop (выполняется, пока мы держим кота)
            if (_isDragging)
            {
                HandleDragging(mouse);
                return; 
            }

            // Игнорируем клики по UI для начала любых действий (ПКМ или ЛКМ)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            // 2. Обработка ПКМ (Контекстное меню)
            if (mouse.rightButton.wasPressedThisFrame)
            {
                HandleRightClick(mouse);
            }
            // 3. Обработка ЛКМ (Начало Drag & Drop)
            else if (mouse.leftButton.wasPressedThisFrame)
            {
                HandleLeftClickStart(mouse);
            }
        }

        private void HandleRightClick(Mouse mouse)
        {
            if (_contextMenu == null) return;

            Cat cat = TryFindCatUnderMouse(mouse);

            if (cat != null)
                _contextMenu.Show(cat);
            else
                _contextMenu.Hide();
        }

        private void HandleLeftClickStart(Mouse mouse)
        {
            Cat cat = TryFindCatUnderMouse(mouse);

            if (cat != null)
            {
                _draggedCat = cat;
                _isDragging = true;

                // Закрываем контекстное меню при начале перетаскивания, чтобы оно не висело в воздухе
                if (_contextMenu != null) _contextMenu.Hide();

                // Вычисляем смещение, чтобы кот не «прыгал» центром в точку курсора при клике
                Vector3 screenPos = mouse.position.ReadValue();
                Vector3 worldPos = _camera.ScreenToWorldPoint(screenPos);
                worldPos.z = _draggedCat.transform.position.z; // сохраняем Z-координату 2D-объекта

                _dragOffset = _draggedCat.transform.position - worldPos;

                // Опционально: здесь можно отключить коту Rigidbody2D/ИИ на время перетаскивания
                // _draggedCat.GetComponent<Rigidbody2D>().simulated = false;
            }
        }

        private void HandleDragging(Mouse mouse)
        {
            // Если игрок отпустил левую кнопку мыши — завершаем Drag & Drop
            if (!mouse.leftButton.isPressed)
            {
                FinishDragging(mouse);
                return;
            }

            // Перемещаем кота за курсором с учетом начального смещения
            Vector3 screenPos = mouse.position.ReadValue();
            Vector3 worldPos = _camera.ScreenToWorldPoint(screenPos);
            worldPos.z = _draggedCat.transform.position.z;

            _draggedCat.transform.position = worldPos + _dragOffset;
        }

        private void FinishDragging(Mouse mouse)
        {
            _isDragging = false;

            // Опционально: возвращаем физику коту
            // _draggedCat.GetComponent<Rigidbody2D>().simulated = true;

            // Проверяем, куда именно мы бросили кота
            Vector3 screenPos = mouse.position.ReadValue();
            Vector2 worldPos = _camera.ScreenToWorldPoint(screenPos);

            // Ищем под курсором триггер/коллайдер дома для разведения
            var hits = Physics2D.OverlapPointAll(worldPos);
            foreach (var hit in hits)
            {
                // Предполагается, что на доме висит скрипт SexRoomController
                var house = hit.GetComponent<SexRoomController>(); 
                if (house != null)
                {
                    // Передаем кота в дом для разведения
                    house.AddCat(_draggedCat);
                    break;
                }
            }

            _draggedCat = null;
        }

        /// <summary>Вспомогательный метод для поиска кота под курсором.</summary>
        private Cat TryFindCatUnderMouse(Mouse mouse)
        {
            Vector3 screenPos = mouse.position.ReadValue();
            Vector2 worldPos = _camera.ScreenToWorldPoint(screenPos);

            var hits = Physics2D.OverlapPointAll(worldPos);
            foreach (var hit in hits)
            {
                Cat cat = hit.GetComponentInParent<Cat>();
                if (cat != null)
                    return cat;
            }
            return null;
        }
    }
}
