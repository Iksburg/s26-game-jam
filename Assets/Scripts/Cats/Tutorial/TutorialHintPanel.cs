using UnityEngine;
using UnityEngine.UI;

namespace CatWorld.Cats.Tutorial
{
    /// <summary>
    /// Всплывающая панель с подсказкой для обучения.
    /// Отслеживает целевой элемент и остается рядом с ним.
    /// </summary>
    public class TutorialHintPanel : MonoBehaviour
    {
        [SerializeField] private Text _hintText;
        [SerializeField] private Button _nextButton;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _panelRect;
        [SerializeField] private float _offsetDistance = 20f;

        private RectTransform _targetRect;
        private bool _showNextButton;

        private void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            if (_panelRect == null)
                _panelRect = GetComponent<RectTransform>();

            if (_nextButton != null)
                _nextButton.onClick.AddListener(() => gameObject.SetActive(false));

            gameObject.SetActive(false);
        }

        public void SetTextComponent(Text textComponent)
        {
            _hintText = textComponent;
        }

        public void SetNextButton(Button nextButton)
        {
            _nextButton = nextButton;
            if (_nextButton != null)
                _nextButton.onClick.AddListener(() => gameObject.SetActive(false));
        }

        private void LateUpdate()
        {
            if (_targetRect != null && gameObject.activeSelf)
                FollowTarget();
        }

        public void ShowHint(string hintText, RectTransform targetRect, bool showNextButton = true)
        {
            if (_hintText != null)
                _hintText.text = hintText;

            _targetRect = targetRect;
            _showNextButton = showNextButton;

            if (_nextButton != null)
                _nextButton.gameObject.SetActive(showNextButton);

            gameObject.SetActive(true);
            PositionPanel();
            FadeIn();
        }

        public void HideHint()
        {
            FadeOut();
            _targetRect = null;
        }

        private void PositionPanel()
        {
            if (_targetRect == null || _panelRect == null)
                return;

            // Получаем размеры экрана в пикселях
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // Позиция цели
            Vector3 targetPos = _targetRect.position;
        
            // Размеры панели
            float panelWidth = _panelRect.rect.width;
            float panelHeight = _panelRect.rect.height;

            // Начальная позиция: над целью + отступ
            float posX = targetPos.x;
            float posY = targetPos.y + panelHeight / 2 + _offsetDistance;

            // Проверка горизонтальных границ
            // Левая граница: posX - половина ширины панели не должна быть меньше 0
            if (posX - panelWidth / 2 < 0)
                posX = panelWidth / 2;
        
            // Правая граница: posX + половина ширины панели не должна быть больше ширины экрана
            if (posX + panelWidth / 2 > screenWidth)
                posX = screenWidth - panelWidth / 2;

            // Проверка вертикальных границ
            // Если панель вылезает за верхний край, пробуем разместить её ПОД целью
            if (posY + panelHeight / 2 > screenHeight)
            {
                posY = targetPos.y - panelHeight / 2 - _offsetDistance;
            
                // Если и так вылезает за низ (редко, но бывает на маленьких экранах), 
                // прижимаем к нижнему краю
                if (posY - panelHeight / 2 < 0)
                    posY = panelHeight / 2;
            }
        
            // Дополнительная проверка нижнего края для стандартного случая (над целью)
            if (posY - panelHeight / 2 < 0 && posY > targetPos.y) 
            {
                // Если мы всё еще над целью, но уперлись в низ (странно, но для надежности)
                // Можно оставить как есть или тоже перевернуть вниз
            }

            _panelRect.position = new Vector3(posX, posY, targetPos.z);
        }

        private void FollowTarget()
        {
            PositionPanel();
        }

        private void FadeIn()
        {
            if (_canvasGroup != null)
            {
                StopAllCoroutines();
                StartCoroutine(FadeRoutine(1f));
            }
        }

        private void FadeOut()
        {
            if (_canvasGroup != null)
            {
                StopAllCoroutines();
                StartCoroutine(FadeRoutine(0f));
            }
        }

        private System.Collections.IEnumerator FadeRoutine(float targetAlpha)
        {
            float elapsedTime = 0f;
            float duration = 0.3f;
            float startAlpha = _canvasGroup.alpha;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;

            if (targetAlpha == 0f)
                gameObject.SetActive(false);
        }
    }
}
