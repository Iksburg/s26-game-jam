using Cats.Genome.Breeding;
using UnityEngine;
using UnityEngine.EventSystems; // Добавлено для PointerEventData
using UnityEngine.InputSystem;
using CatWorld.Cats.Tutorial; // Добавлено для TutorialManager

namespace CatWorld.Cats
{
    public class CatClickDetector : MonoBehaviour
    {
        [SerializeField] private CatContextMenu _contextMenu;
        private Camera _camera;
        
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

            if (_isDragging)
            {
                HandleDragging(mouse);
                return; 
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (mouse.rightButton.wasPressedThisFrame)
            {
                HandleRightClick(mouse);
            }
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
                if (_contextMenu != null) _contextMenu.Hide();

                Vector3 screenPos = mouse.position.ReadValue();
                Vector3 worldPos = _camera.ScreenToWorldPoint(screenPos);
                worldPos.z = _draggedCat.transform.position.z;
                _dragOffset = _draggedCat.transform.position - worldPos;
            }
        }

        private void HandleDragging(Mouse mouse)
        {
            if (!mouse.leftButton.isPressed)
            {
                FinishDragging(mouse);
                return;
            }

            Vector3 screenPos = mouse.position.ReadValue();
            Vector3 worldPos = _camera.ScreenToWorldPoint(screenPos);
            worldPos.z = _draggedCat.transform.position.z;
            _draggedCat.transform.position = worldPos + _dragOffset;
        }

        private void FinishDragging(Mouse mouse)
        {
            _isDragging = false;

            Vector3 screenPos = mouse.position.ReadValue();
            Vector2 worldPos = _camera.ScreenToWorldPoint(screenPos);

            var hits = Physics2D.OverlapPointAll(worldPos);
            foreach (var hit in hits)
            {
                var house = hit.GetComponent<SexRoomController>(); 
                if (house != null)
                {
                    house.AddCat(_draggedCat);
                    break;
                }
            }

            _draggedCat = null;
        }

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