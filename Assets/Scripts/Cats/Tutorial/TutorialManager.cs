using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CatWorld.Cats.Tutorial
{
    /// <summary>
    /// Главный менеджер системы обучения: управляет последовательностью шагов,
    /// показывает подсказки и подсвечивает элементы.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        public TutorialHintPanel _hintPanel;
        public TutorialHighlight _highlighter;
        [SerializeField] private bool _startTutorialOnAwake = true;

        private List<TutorialStep> _steps = new List<TutorialStep>();
        private int _currentStepIndex = 0;
        private bool _isRunning = false;
        private bool _tutorialCompleted = false;

        private void Awake()
        {
            if (_highlighter == null)
            {
                var highlightObj = new GameObject("TutorialHighlight");
                highlightObj.transform.SetParent(transform);
                _highlighter = highlightObj.AddComponent<TutorialHighlight>();
            }

            if (_hintPanel == null)
            {
                Debug.LogWarning("TutorialHintPanel не назначена в инспекторе!");
            }
        }

        private void Start()
        {
            if (_startTutorialOnAwake && !_tutorialCompleted && !PlayerPrefs.HasKey("TutorialCompleted"))
                StartTutorial();
        }

        public void SetHintPanel(TutorialHintPanel hintPanel)
        {
            _hintPanel = hintPanel;
        }

        public void DisableAutoStart()
        {
            _startTutorialOnAwake = false;
        }

        /// <summary>Добавить шаг обучения.</summary>
        public void AddStep(string hintText, Button targetButton = null, bool waitForClick = false, float duration = 0f)
        {
            _steps.Add(new TutorialStep(hintText, targetButton, waitForClick, duration));
        }

        /// <summary>Добавить шаг обучения с целевым изображением.</summary>
        public void AddStep(string hintText, Image targetImage, float duration = 0f)
        {
            _steps.Add(new TutorialStep(hintText, targetImage, duration));
        }

        /// <summary>Запустить обучение.</summary>
        public void StartTutorial()
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _currentStepIndex = 0;

            if (_steps.Count == 0)
            {
                Debug.LogWarning("Нет шагов обучения!");
                _isRunning = false;
                return;
            }

            ShowStep(_currentStepIndex);
        }

        /// <summary>Остановить обучение.</summary>
        public void StopTutorial()
        {
            _isRunning = false;
            _highlighter.RemoveHighlight();
            if (_hintPanel != null)
                _hintPanel.HideHint();
            StopAllCoroutines();
        }

        /// <summary>Пропустить обучение и отметить как завершённое.</summary>
        public void SkipTutorial()
        {
            StopTutorial();
            _tutorialCompleted = true;
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();
        }

        /// <summary>Перейти к следующему шагу.</summary>
        public void NextStep()
        {
            if (!_isRunning)
                return;

            _currentStepIndex++;

            if (_currentStepIndex >= _steps.Count)
            {
                CompleteTutorial();
                return;
            }

            ShowStep(_currentStepIndex);
        }

        private void ShowStep(int stepIndex)
        {
            if (stepIndex < 0 || stepIndex >= _steps.Count)
                return;

            TutorialStep step = _steps[stepIndex];

            // Подсвечиваем целевой элемент
            if (step.TargetImage != null)
                _highlighter.HighlightElement(step.TargetImage);

            // Показываем подсказку
            if (_hintPanel != null)
            {
                RectTransform targetRect = null;
                if (step.TargetButton != null)
                    targetRect = step.TargetButton.GetComponent<RectTransform>();
                else if (step.TargetImage != null)
                    targetRect = step.TargetImage.GetComponent<RectTransform>();

                _hintPanel.ShowHint(step.HintText, targetRect, !step.WaitForButtonClick);
            }

            // Если нужно ждать клика по кнопке
            if (step.WaitForButtonClick && step.TargetButton != null)
            {
                // Используем временный делегат для отслеживания
                System.Action onClickListener = null;
                onClickListener = () =>
                {
                    step.TargetButton.onClick.RemoveListener(onClickListener);
                    NextStep();
                };
                
                step.TargetButton.onClick.AddListener(onClickListener);
                StartCoroutine(WaitForButtonClickTimeout(step.TargetButton, onClickListener));
            }
            // Если установлена длительность отображения
            else if (step.DisplayDuration > 0f)
            {
                StartCoroutine(WaitForDuration(step.DisplayDuration));
            }
        }

        private IEnumerator WaitForButtonClickTimeout(Button button, System.Action listener)
        {
            // Ждём нажатия кнопки (максимум 30 секунд)
            float elapsed = 0f;
            while (elapsed < 30f && _isRunning)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (button != null)
                button.onClick.RemoveListener(listener);

            if (_isRunning && elapsed >= 30f)
            {
                // Если времени истекло, переходим к следующему шагу автоматически
                NextStep();
            }
        }

        private IEnumerator WaitForDuration(float duration)
        {
            yield return new WaitForSeconds(duration);
            if (_isRunning)
                NextStep();
        }

        private void CompleteTutorial()
        {
            StopTutorial();
            _tutorialCompleted = true;
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();

            Debug.Log("Обучение завершено!");
        }

        /// <summary>Проверить, уже ли было обучение.</summary>
        public static bool HasCompletedTutorial => PlayerPrefs.HasKey("TutorialCompleted");

        /// <summary>Сбросить статус обучения для переиграния.</summary>
        public static void ResetTutorial()
        {
            PlayerPrefs.DeleteKey("TutorialCompleted");
            PlayerPrefs.Save();
        }
    }
}
