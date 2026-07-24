using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace CatWorld.Cats
{
    /// <summary>
    /// Какашка на ферме: появляется после того, как кот поел, и лежит на месте,
    /// где кот был в момент спавна. Убирается кликом ЛКМ. Спрайт назначается
    /// в инспекторе на префабе, размер и порядок отрисовки настраиваются здесь.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class Poop : MonoBehaviour
    {
        [Tooltip("Размер спрайта какашки (масштаб). Клик-область масштабируется вместе с ним.")]
        [SerializeField] private float _size = 1f;
        [Tooltip("Порядок отрисовки. Выше фона, обычно ниже котов (10).")]
        [SerializeField] private int _sortingOrder = 5;

        private Camera _camera;
        private Collider2D _collider;

        private void Awake()
        {
            transform.localScale = new Vector3(_size, _size, 1f);
            _collider = GetComponent<Collider2D>();
            GetComponent<SpriteRenderer>().sortingOrder = _sortingOrder;
        }

        private void Start()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            var mouse = Mouse.current;
            // Реагируем только на кадр нажатия ЛКМ — раскаст не идёт каждый кадр.
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
                return;

            // Клик по UI не должен убирать какашку под окном.
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (_camera == null)
                _camera = Camera.main;
            if (_camera == null || _collider == null)
                return;

            Vector2 worldPoint = _camera.ScreenToWorldPoint(mouse.position.ReadValue());
            if (_collider.OverlapPoint(worldPoint))
                Destroy(gameObject);
        }
    }
}
