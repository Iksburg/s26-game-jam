using UnityEngine;
using UnityEngine.UI;

namespace CatWorld.Cats.Tutorial
{
    /// <summary>
    /// Оверлей-затемнение для обучения: затемняет фон, оставляя видимой целевую кнопку.
    /// </summary>
    public class TutorialOverlay : MonoBehaviour
    {
        [SerializeField] private Image _overlayImage;
        [SerializeField] private Color _overlayColor = new Color(0, 0, 0, 0.7f);
        [SerializeField] private float _fadeSpeed = 2f;

        private CanvasGroup _canvasGroup;
        private bool _isVisible = false;

        private void Awake()
        {
            if (_overlayImage == null)
                _overlayImage = GetComponent<Image>();

            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            _canvasGroup.alpha = 0f;
        }

        /// <summary>Показать оверлей с затемнением.</summary>
        public void Show()
        {
            if (_isVisible)
                return;

            _isVisible = true;
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(FadeRoutine(1f));
        }

        /// <summary>Скрыть оверлей.</summary>
        public void Hide()
        {
            if (!_isVisible)
                return;

            _isVisible = false;
            StopAllCoroutines();
            StartCoroutine(FadeRoutine(0f));
        }

        private System.Collections.IEnumerator FadeRoutine(float targetAlpha)
        {
            float elapsedTime = 0f;
            float startAlpha = _canvasGroup.alpha;

            while (elapsedTime < 1f / _fadeSpeed)
            {
                elapsedTime += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime * _fadeSpeed);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;

            if (targetAlpha == 0f)
                gameObject.SetActive(false);
        }
    }
}
