using UnityEngine;
using UnityEngine.UI;

namespace CatWorld.Cats.Tutorial
{
    /// <summary>
    /// Один шаг обучения: содержит текст подсказки, целевую кнопку/элемент и условие завершения.
    /// </summary>
    public class TutorialStep
    {
        public string HintText { get; set; }
        public Button TargetButton { get; set; }
        public Image TargetImage { get; set; }
        public bool WaitForButtonClick { get; set; }
        public float DisplayDuration { get; set; }

        public TutorialStep(string hintText, Button targetButton = null, bool waitForClick = false, float duration = 0f)
        {
            HintText = hintText;
            TargetButton = targetButton;
            TargetImage = targetButton != null ? targetButton.GetComponent<Image>() : null;
            WaitForButtonClick = waitForClick;
            DisplayDuration = duration;
        }

        public TutorialStep(string hintText, Image targetImage, float duration = 0f)
        {
            HintText = hintText;
            TargetImage = targetImage;
            TargetButton = null;
            WaitForButtonClick = false;
            DisplayDuration = duration;
        }
    }
}
