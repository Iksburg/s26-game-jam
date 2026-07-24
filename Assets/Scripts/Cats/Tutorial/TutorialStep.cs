// В файле TutorialStep.cs
using UnityEngine;
using UnityEngine.UI;

namespace CatWorld.Cats.Tutorial
{
    public class TutorialStep
    {
        public string HintText { get; set; }
        public Button TargetButton { get; set; }
        public Image TargetImage { get; set; }
        public SpriteRenderer TargetSprite { get; set; } // Новое поле
        
        public bool WaitForButtonClick { get; set; }
        public bool WaitForDropEvent { get; set; }
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

        // Конструктор для UI Image
        public TutorialStep(string hintText, Image targetImage, float duration = 0f, bool waitForDrop = false)
        {
            HintText = hintText;
            TargetImage = targetImage;
            TargetButton = null;
            TargetSprite = null;
            WaitForButtonClick = false;
            WaitForDropEvent = waitForDrop;
            DisplayDuration = duration;
        }

        // Новый конструктор для SpriteRenderer
        public TutorialStep(string hintText, SpriteRenderer targetSprite, float duration = 0f, bool waitForDrop = false)
        {
            HintText = hintText;
            TargetSprite = targetSprite;
            TargetImage = null;
            TargetButton = null;
            WaitForButtonClick = false;
            WaitForDropEvent = waitForDrop;
            DisplayDuration = duration;
        }
        
        // Помощник для получения RectTransform (для UI) или позиции (для World)
        public RectTransform GetTargetRect()
        {
            if (TargetButton != null) return TargetButton.GetComponent<RectTransform>();
            if (TargetImage != null) return TargetImage.GetComponent<RectTransform>();
            return null;
        }
        
        public Transform GetTargetTransform()
        {
            if (TargetSprite != null) return TargetSprite.transform;
            if (TargetImage != null) return TargetImage.transform;
            if (TargetButton != null) return TargetButton.transform;
            return null;
        }
    }
}