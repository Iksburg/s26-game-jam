// TutorialStep.cs
using UnityEngine;
using UnityEngine.UI;

namespace CatWorld.Cats.Tutorial
{
    public class TutorialStep
    {
        public string HintText { get; set; }
        public Button TargetButton { get; set; }
        public Image TargetImage { get; set; }
        
        public bool WaitForButtonClick { get; set; }
        public bool WaitForDropEvent { get; set; } // Новое поле
        
        public float DisplayDuration { get; set; }

        // Конструктор для кнопки
        public TutorialStep(string hintText, Button targetButton = null, bool waitForClick = false, float duration = 0f)
        {
            HintText = hintText;
            TargetButton = targetButton;
            TargetImage = targetButton != null ? targetButton.GetComponent<Image>() : null;
            WaitForButtonClick = waitForClick;
            WaitForDropEvent = false;
            DisplayDuration = duration;
        }

        // Обновленный конструктор для изображения с поддержкой Drop
        public TutorialStep(string hintText, Image targetImage, float duration = 0f, bool waitForDrop = false)
        {
            HintText = hintText;
            TargetImage = targetImage;
            TargetButton = null;
            WaitForButtonClick = false;
            WaitForDropEvent = waitForDrop; // Присваиваем значение
            DisplayDuration = duration;
        }
    }
}