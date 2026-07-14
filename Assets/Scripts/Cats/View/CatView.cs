using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>Способ отображения стадии жизни на визуале кота.</summary>
    public enum StageVisualMode
    {
        /// <summary>Единственный спрайт (заглушка или арт), меняется только масштаб.</summary>
        ScaleOnly,
        /// <summary>Спрайт подменяется по стадии (котёнок/взрослый/пожилой); масштаб тоже применяется.</summary>
        SpriteSwap
    }

    /// <summary>
    /// Визуал кота: белый базовый спрайт, тонируемый цветом шерсти
    /// (по концепту все коты строятся из одного белого спрайта).
    /// Пока художественного спрайта нет — генерирует белый круг в рантайме.
    /// Стадия жизни отражается масштабом (ScaleOnly, по умолчанию) или
    /// подменой спрайта (SpriteSwap).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class CatView : MonoBehaviour
    {
        // Разрешение текстуры заглушки — это КАЧЕСТВО, а не размер в мире.
        private const int FallbackSpriteResolution = 256;
        // Диаметр заглушки в мировых юнитах. Итоговый размер кота на сцене =
        // этот диаметр × transform.localScale (масштаб задаётся на префабе / по возрасту).
        private const float FallbackWorldDiameter = 1.5f;

        private static Sprite _fallbackSprite;

        [SerializeField] private SpriteRenderer _renderer;
        [Tooltip("Порядок отрисовки. Держите выше фона (у фонового спрайта — 0 или меньше).")]
        [SerializeField] private int _sortingOrder = 10;

        [Header("Визуал стадий жизни")]
        [SerializeField] private StageVisualMode _stageVisualMode = StageVisualMode.ScaleOnly;
        [Tooltip("Спрайты для режима SpriteSwap. Пустой слот — спрайт стадии не подменяется.")]
        [SerializeField] private Sprite _kittenSprite;
        [SerializeField] private Sprite _adultSprite;
        [SerializeField] private Sprite _seniorSprite;

        private void Awake()
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();
            _renderer.sortingOrder = _sortingOrder; // кот всегда над фоном
            if (_renderer.sprite == null)
                _renderer.sprite = GetFallbackSprite();
        }

        /// <summary>Тонирует базовый белый спрайт цветом шерсти.</summary>
        public void ApplyColor(Color furColor)
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();
            _renderer.color = furColor;
        }

        /// <summary>
        /// Применяет визуал стадии жизни: масштаб всегда; в режиме SpriteSwap
        /// дополнительно подменяет спрайт (если назначен для стадии).
        /// </summary>
        public void ApplyStage(LifeStage stage, float scale)
        {
            transform.localScale = new Vector3(scale, scale, 1f);

            if (_stageVisualMode != StageVisualMode.SpriteSwap)
                return;

            Sprite stageSprite;
            switch (stage)
            {
                case LifeStage.Kitten: stageSprite = _kittenSprite; break;
                case LifeStage.Adult: stageSprite = _adultSprite; break;
                default: stageSprite = _seniorSprite; break;
            }

            if (stageSprite == null)
                return; // fallback: остаёмся на текущем спрайте

            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();
            _renderer.sprite = stageSprite;
        }

        /// <summary>
        /// Разворачивает спрайт по направлению движения. Базовый арт смотрит
        /// вправо, поэтому для движения влево включается flipX.
        /// </summary>
        public void SetFacingRight(bool facingRight)
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();
            _renderer.flipX = !facingRight;
        }

        /// <summary>Белый круг-заглушка, пока нет художественного спрайта кота.</summary>
        private static Sprite GetFallbackSprite()
        {
            if (_fallbackSprite != null)
                return _fallbackSprite;

            int size = FallbackSpriteResolution;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            float center = (size - 1) * 0.5f;
            float radius = size * 0.5f - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    bool inside = dx * dx + dy * dy <= radius * radius;
                    texture.SetPixel(x, y, inside ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            // PPU развязан с разрешением: размер в мире задаёт только диаметр.
            float pixelsPerUnit = FallbackSpriteResolution / FallbackWorldDiameter;
            _fallbackSprite = Sprite.Create(texture, new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), pixelsPerUnit);
            _fallbackSprite.hideFlags = HideFlags.HideAndDontSave;
            return _fallbackSprite;
        }
    }
}
