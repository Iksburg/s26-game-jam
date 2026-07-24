using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace CatWorld.Cats
{
    [RequireComponent(typeof(Cat))]
    public class CatThoughts : MonoBehaviour
    {
        [Header("Настройки плашки")]
        [SerializeField] private float _displayDuration = 2f;
        [SerializeField] private Vector3 _offset = new Vector3(0, 1.5f, 0); // Поднял чуть выше
        [SerializeField] private Color _backgroundColor = new Color(1f, 1f, 1f, 0.95f);
        [SerializeField] private Color _borderColor = new Color(0.4f, 0.3f, 0.2f, 1f);
        [SerializeField] private Color _textColor = new Color(0.2f, 0.15f, 0.1f, 1f);
        [SerializeField] private int _fontSize = 24; // Увеличил шрифт для надежности
        [SerializeField] private float _bubbleWidth = 200f;
        [SerializeField] private float _bubbleHeight = 60f;
        
        private Cat _cat;
        private RectTransform _bubbleRect;
        private Text _bubbleText;
        private Coroutine _hideCoroutine;
        private Canvas _targetCanvas;

        private const float HUNGRY_THRESHOLD = 30f;
        private const float SATIATED_THRESHOLD = 80f;
        private const float DIRTY_THRESHOLD = 30f;

        private void Awake()
        {
            _cat = GetComponent<Cat>();
            InitializeUI();
        }

        private void Update()
        {
            if (_bubbleRect == null || !_bubbleRect.gameObject.activeSelf) return;
            UpdatePosition();
        }

        /// <summary>
        /// Корректное обновление позиции через anchoredPosition
        /// </summary>
        private void UpdatePosition()
        {
            if (_targetCanvas == null || Camera.main == null) return;

            // Переводим мировую позицию в экранные координаты
            Vector3 worldPos = transform.position + _offset;
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPos);

            // Конвертируем экранные координаты в локальные координаты Canvas
            // Это решает проблему "центра экрана" и масштаба
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _targetCanvas.transform as RectTransform, 
                screenPoint, 
                _targetCanvas.worldCamera, 
                out Vector2 localPoint
            );

            _bubbleRect.anchoredPosition = localPoint;
        }

        public void ShowFedThought() => ShowBubble("Мурр~ Вкусно!");
        public void ShowDrankThought() => ShowBubble("Хорошая водичка!");

        public void CheckNeedsAndShow()
        {
            if (_cat.Satiety <= HUNGRY_THRESHOLD) ShowBubble("Голоден...");
            else if (_cat.Water <= HUNGRY_THRESHOLD) ShowBubble("Пить хочу...");
            else if (_cat.Cleanliness <= DIRTY_THRESHOLD) ShowBubble("Фу, грязно!");
            else if (_cat.Satiety >= SATIATED_THRESHOLD && 
                     _cat.Water >= SATIATED_THRESHOLD && 
                     _cat.Cleanliness >= SATIATED_THRESHOLD)
            {
                ShowBubble("Доволен ♪");
            }
        }

        public void ShowOldAgeDeparture() => ShowBubble("Старость не радость...", 3f);
        public void ShowUnmetNeedsDeparture(string reason) => ShowBubble($"Ухожу... ({reason})", 3f);

        private void ShowBubble(string text, float duration = -1f)
        {
            if (duration < 0f) duration = _displayDuration;
            if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);

            _bubbleText.text = text;
            _bubbleRect.gameObject.SetActive(true);
            
            // Сброс позиции сразу при показе, чтобы не мерцало
            UpdatePosition(); 

            _hideCoroutine = StartCoroutine(HideAfter(duration));
        }

        private IEnumerator HideAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            _bubbleRect.gameObject.SetActive(false);
            _hideCoroutine = null;
        }

        private void InitializeUI()
        {
            // 1. Ищем Canvas (обязательно с камерой)
            _targetCanvas = FindObjectOfType<Canvas>();
            if (_targetCanvas == null)
            {
                var go = new GameObject("CatThoughtsCanvas");
                _targetCanvas = go.AddComponent<Canvas>();
                _targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _targetCanvas.worldCamera = Camera.main; // Критически важно!
                go.AddComponent<GraphicRaycaster>();
                DontDestroyOnLoad(go);
            }
            else if (_targetCanvas.worldCamera == null)
            {
                _targetCanvas.worldCamera = Camera.main;
            }

            // 2. Создаём плашку
            var bubbleGO = new GameObject("ThoughtBubble");
            bubbleGO.transform.SetParent(_targetCanvas.transform, false);
            _bubbleRect = bubbleGO.AddComponent<RectTransform>();
            _bubbleRect.anchorMin = Vector2.one * 0.5f;
            _bubbleRect.anchorMax = Vector2.one * 0.5f;
            _bubbleRect.pivot = new Vector2(0.5f, 0f);
            _bubbleRect.sizeDelta = new Vector2(_bubbleWidth, _bubbleHeight);

            // 3. Фон
            var image = bubbleGO.AddComponent<Image>();
            image.sprite = CreateRoundedRectSprite();
            image.type = Image.Type.Sliced;
            image.color = _backgroundColor;

            // 4. Текст — надёжный fallback
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(bubbleGO.transform, false);
            _bubbleText = textGO.AddComponent<Text>();

            // Попробуем получить шрифт через несколько путей
            Font font = null;
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            if (font != null)
            {
                _bubbleText.font = font;
                _bubbleText.fontSize = _fontSize;
                _bubbleText.color = _textColor;
                _bubbleText.alignment = TextAnchor.MiddleCenter;
                _bubbleText.raycastTarget = false;
                _bubbleText.text = "TEST"; // ← временно, чтобы убедиться, что текст отображается
            }
            else
            {
                Debug.LogError("[CatThoughts] НЕ ВОЗМОЖНО загрузить шрифт! Текст будет невидимым.");
                // Создаём dummy-шрифт программно (если очень нужно)
                _bubbleText.text = "??";
            }

            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);

            // 5. Рамка
            var outline = bubbleGO.AddComponent<Outline>();
            outline.effectColor = _borderColor;
            outline.effectDistance = new Vector2(2, -2);

            bubbleGO.SetActive(false);
        }

        private Sprite CreateRoundedRectSprite()
        {
            int w = 256, h = 80;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            float r = 20f; // Радиус скругления в пикселях текстуры
            
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool inCorner = false;
                    float dx = 0, dy = 0;
                    
                    if (x < r && y < r) { dx = x - r; dy = y - r; inCorner = true; }
                    else if (x >= w-r && y < r) { dx = x - (w-r); dy = y - r; inCorner = true; }
                    else if (x < r && y >= h-r) { dx = x - r; dy = y - (h-r); inCorner = true; }
                    else if (x >= w-r && y >= h-r) { dx = x - (w-r); dy = y - (h-r); inCorner = true; }

                    tex.SetPixel(x, y, (inCorner && Mathf.Sqrt(dx*dx + dy*dy) > r) ? Color.clear : _backgroundColor);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,w,h), new Vector2(0.5f,0.5f), 100f, 0, SpriteMeshType.Tight, new Vector4(r,r,r,r));
        }

        private void OnDestroy()
        {
            if (_bubbleRect != null) Destroy(_bubbleRect.gameObject);
        }
    }
}