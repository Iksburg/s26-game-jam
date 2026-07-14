using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// Проходимая зона фермы произвольной формы, заданная PolygonCollider2D.
    /// Дизайнер обводит вершины полигона по линии забора на фоновом спрайте —
    /// контур становится точной границей, за которую коты не выходят.
    /// Коллайдер используется только для проверки вхождения точки (без физики).
    /// </summary>
    [RequireComponent(typeof(PolygonCollider2D))]
    public class FarmBounds : MonoBehaviour
    {
        private const int MaxSampleAttempts = 32;

        [SerializeField] private PolygonCollider2D _polygon;

        private void Reset()
        {
            _polygon = GetComponent<PolygonCollider2D>();
            _polygon.isTrigger = true;
        }

        private void Awake()
        {
            if (_polygon == null)
                _polygon = GetComponent<PolygonCollider2D>();
        }

        /// <summary>Находится ли точка внутри проходимой зоны.</summary>
        public bool Contains(Vector2 worldPoint)
        {
            if (_polygon == null)
                _polygon = GetComponent<PolygonCollider2D>();
            return _polygon.OverlapPoint(worldPoint);
        }

        /// <summary>
        /// Случайная точка внутри зоны. Сэмплит внутри AABB коллайдера, пока
        /// не попадёт в полигон. Если за MaxSampleAttempts не удалось —
        /// возвращает центр зоны (гарантированно осмысленный fallback).
        /// </summary>
        public Vector2 GetRandomPointInside()
        {
            if (_polygon == null)
                _polygon = GetComponent<PolygonCollider2D>();

            Bounds bounds = _polygon.bounds;
            for (int i = 0; i < MaxSampleAttempts; i++)
            {
                var candidate = new Vector2(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y));
                if (_polygon.OverlapPoint(candidate))
                    return candidate;
            }
            return bounds.center;
        }
    }
}
