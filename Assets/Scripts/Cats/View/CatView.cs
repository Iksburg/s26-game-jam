using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// Визуал кота: белый базовый спрайт, тонируемый цветом шерсти
    /// (по концепту все коты строятся из одного белого спрайта).
    /// Пока художественного спрайта нет — генерирует белый круг в рантайме.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class CatView : MonoBehaviour
    {
        private const int FallbackSpriteSize = 64;

        private static Sprite _fallbackSprite;

        [SerializeField] private SpriteRenderer _renderer;

        private void Awake()
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();
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

            int size = FallbackSpriteSize;
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
            _fallbackSprite = Sprite.Create(texture, new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), FallbackSpriteSize);
            _fallbackSprite.hideFlags = HideFlags.HideAndDontSave;
            return _fallbackSprite;
        }
    }
}
