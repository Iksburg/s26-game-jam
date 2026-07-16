using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// Визуал миски: показывает спрайт «полная», пока остаток соответствующего
    /// ресурса (корм для NeedType.Food, вода для NeedType.Water) больше нуля,
    /// и «пустая» — когда ресурс закончился. Переход мгновенный, без анимаций.
    /// Спрайты назначаются в инспекторе.
    /// </summary>
    [RequireComponent(typeof(NeedStation))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class NeedStationView : MonoBehaviour
    {
        [SerializeField] private Sprite _filledSprite;
        [SerializeField] private Sprite _emptySprite;
        [Tooltip("Порядок отрисовки миски. Держите выше фона и, при желании, ниже котов.")]
        [SerializeField] private int _sortingOrder;

        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private NeedStation _station;

        private FarmResources _resources;

        private void Awake()
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();
            if (_station == null)
                _station = GetComponent<NeedStation>();
            _renderer.sortingOrder = _sortingOrder;
        }

        private void OnEnable()
        {
            if (_resources == null)
                _resources = FindFirstObjectByType<FarmResources>();
            if (_resources != null)
                _resources.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (_resources != null)
                _resources.Changed -= Refresh;
        }

        private void Refresh()
        {
            if (_renderer == null || _station == null)
                return;

            int amount = _station.Type == NeedType.Food
                ? (_resources != null ? _resources.Food : 0)
                : (_resources != null ? _resources.Water : 0);

            _renderer.sprite = amount > 0 ? _filledSprite : _emptySprite;
        }
    }
}
