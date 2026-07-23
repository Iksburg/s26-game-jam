using UnityEngine;
using UnityEngine.UI;

namespace CatWorld.Cats.Tutorial
{
    /// <summary>
    /// Компонент подсветки UI элемента: создает светящуюся рамку или эффект вокруг целевого элемента.
    /// </summary>
    public class TutorialHighlight : MonoBehaviour
    {
        private Image _highlightImage;
        private Outline _outline;
        private Color _originalOutlineColor;
        private float _originalOutlineWidth;

        public void HighlightElement(Image targetImage)
        {
            if (targetImage == null)
                return;

            RemoveHighlight();

            _outline = targetImage.GetComponent<Outline>();
            if (_outline == null)
                _outline = targetImage.gameObject.AddComponent<Outline>();

            _originalOutlineColor = _outline.effectColor;
            _originalOutlineWidth = _outline.effectDistance.x;

            _outline.effectColor = new Color(1f, 1f, 0f, 1f); // Желтый
            _outline.effectDistance = new Vector2(5, 5);
            _outline.enabled = true;

            // Добавляем мигание
            StartCoroutine(BlinkEffect(_outline));
        }

        public void RemoveHighlight()
        {
            if (_outline != null)
            {
                _outline.effectColor = _originalOutlineColor;
                _outline.effectDistance = new Vector2(_originalOutlineWidth, _originalOutlineWidth);
                _outline.enabled = false;
                StopAllCoroutines();
                _outline = null;
            }
        }

        private System.Collections.IEnumerator BlinkEffect(Outline outline)
        {
            while (outline != null && outline.enabled)
            {
                // Мигание каждые 0.5 секунды
                outline.enabled = false;
                yield return new WaitForSeconds(0.3f);
                outline.enabled = true;
                yield return new WaitForSeconds(0.3f);
            }
        }
    }
}
