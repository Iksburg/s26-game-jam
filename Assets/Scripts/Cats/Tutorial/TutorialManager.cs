// TutorialManager.cs
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CatWorld.Cats.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        public TutorialHintPanel _hintPanel;
        public TutorialHighlight _highlighter;
        [SerializeField] private bool _startTutorialOnAwake = true;

        private List<TutorialStep> _steps = new List<TutorialStep>();
        private int _currentStepIndex = 0;
        private bool _isRunning = false;
        private bool _tutorialCompleted = false;
        public bool Completed => _tutorialCompleted;

        private UnityAction _activeClickListener;
        private Button _currentActiveButton;
        private List<Button> _disabledButtons = new List<Button>();

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

        public void AddStep(string hintText, Button targetButton = null, bool waitForClick = false, float duration = 0f)
        {
            _steps.Add(new TutorialStep(hintText, targetButton, waitForClick, duration));
        }

        public void AddStep(string hintText, Image targetImage, float duration = 0f)
        {
            _steps.Add(new TutorialStep(hintText, targetImage, duration));
        }

        public void AddStep(string hintText, SpriteRenderer targetSprite, float duration = 0f)
        {
            _steps.Add(new TutorialStep(hintText, targetSprite, duration));
        }

        public void StartTutorial()
        {
            if (_isRunning) return;
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

        public void StopTutorial()
        {
            _isRunning = false;
            CleanupActiveListener();
            UnlockAllButtons();
            _highlighter.RemoveHighlight();
            
            if (_hintPanel != null) 
                Destroy(_hintPanel);
                
            StopAllCoroutines();
        }

        public void SkipTutorial()
        {
            StopTutorial();
            _tutorialCompleted = true;
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();
        }

        public void NextStep()
        {
            if (!_isRunning) return;

            CleanupActiveListener();
            _currentStepIndex++;

            if (_currentStepIndex >= _steps.Count)
            {
                CompleteTutorial();
                return;
            }

            ShowStep(_currentStepIndex);
        }

        private void CleanupActiveListener()
        {
            if (_currentActiveButton != null && _activeClickListener != null)
            {
                _currentActiveButton.onClick.RemoveListener(_activeClickListener);
            }

            _currentActiveButton = null;
            _activeClickListener = null;
            UnlockAllButtons();
        }

        private void ShowStep(int stepIndex)
        {
            if (stepIndex < 0 || stepIndex >= _steps.Count) return;

            TutorialStep step = _steps[stepIndex];

            // === ПРОВЕРКА АКТИВНОСТИ ЦЕЛИ ===
            // Если цель - кнопка и она неактивна, ждем
            if (step.TargetButton != null && !step.TargetButton.interactable)
            {
                StartCoroutine(WaitForTargetActive(step));
                return;
            }
            
            // Если цель - Image и она скрыта/неактивна, ждем
            if (step.TargetImage != null && !step.TargetImage.gameObject.activeInHierarchy)
            {
                 // Упрощенная проверка для Image. Можно расширить под CanvasGroup
                 StartCoroutine(WaitForTargetActive(step));
                 return;
            }

            // === ПОДСВЕТКА ===
            if (step.TargetImage != null)
                _highlighter.HighlightElement(step.TargetImage);
            else if (step.TargetSprite != null)
                _highlighter.HighlightSprite(step.TargetSprite);
            else
                _highlighter.RemoveHighlight();

            // === ПОДСКАЗКА ===
            if (_hintPanel != null)
            {
                RectTransform targetRect = step.GetTargetRect();

                if (targetRect == null && step.TargetSprite != null)
                {
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(step.TargetSprite.bounds.center);
                    _hintPanel.ShowHint(step.HintText, null, false);
                    _hintPanel.transform.position = new Vector3(screenPos.x, screenPos.y + 150, 0);
                }
                else
                {
                    bool showNextButton = !step.WaitForButtonClick;
                    _hintPanel.ShowHint(step.HintText, targetRect, showNextButton);
                }
            }

            LockAllButtonsExcept(step.TargetButton);

            // === ЛОГИКА ОЖИДАНИЯ КЛИКА ===
            if (step.WaitForButtonClick && step.TargetButton != null)
            {
                UnityAction onClickListener = null;
                onClickListener = () =>
                {
                    CleanupActiveListener();
                    NextStep();
                };

                _activeClickListener = onClickListener;
                _currentActiveButton = step.TargetButton;
                step.TargetButton.onClick.AddListener(onClickListener);
                StartCoroutine(WaitForButtonClickTimeout(step.TargetButton, onClickListener));
            }
            // === ЛОГИКА ОЖИДАНИЯ ВРЕМЕНИ ===
            else if (step.DisplayDuration > 0f)
            {
                StartCoroutine(WaitForDuration(step.DisplayDuration));
            }
        }

        /// <summary>
        /// Ждет, пока целевой элемент станет активным/доступным
        /// </summary>
        private IEnumerator WaitForTargetActive(TutorialStep step)
        {
            while (_isRunning)
            {
                bool isReady = false;

                if (step.TargetButton != null)
                {
                    isReady = step.TargetButton.interactable && step.TargetButton.gameObject.activeInHierarchy;
                }
                else if (step.TargetImage != null)
                {
                    isReady = step.TargetImage.gameObject.activeInHierarchy;
                    // Если есть CanvasGroup, можно проверить alpha и interactable
                    var cg = step.TargetImage.GetComponent<CanvasGroup>();
                    if (cg != null) isReady = isReady && cg.alpha > 0 && cg.interactable;
                }
                else if (step.TargetSprite != null)
                {
                    isReady = step.TargetSprite.gameObject.activeInHierarchy;
                }
                else
                {
                    // Если цели нет (просто текст), сразу переходим
                    isReady = true;
                }

                if (isReady)
                {
                    ShowStep(_currentStepIndex); // Повторяем попытку показа того же шага
                    yield break;
                }

                yield return null;
            }
        }

        private IEnumerator WaitForButtonClickTimeout(Button button, UnityAction listener)
        {
            float elapsed = 0f;
            while (elapsed < 30f && _isRunning)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (button != null) 
                button.onClick.RemoveListener(listener);

            if (_isRunning && elapsed >= 30f) 
                NextStep();
        }

        private IEnumerator WaitForDuration(float duration)
        {
            yield return new WaitForSeconds(duration);
            if (_isRunning) NextStep();
        }

        private void CompleteTutorial()
        {
            StopTutorial();
            _tutorialCompleted = true;
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();
            Debug.Log("Обучение завершено!");
        }

        private void LockAllButtonsExcept([CanBeNull] Button targetButton)
        {
            UnlockAllButtons();
            var allButtons = FindObjectsOfType<Button>(true);

            foreach (var btn in allButtons)
            {
                if (btn == targetButton ||
                    (_hintPanel != null && btn.transform.IsChildOf(_hintPanel.transform)))
                    continue;

                if (btn.interactable)
                {
                    btn.interactable = false;
                    _disabledButtons.Add(btn);
                }
            }
        }

        private void UnlockAllButtons()
        {
            foreach (var btn in _disabledButtons)
            {
                if (btn != null)
                    btn.interactable = true;
            }
            _disabledButtons.Clear();
        }

        public static bool HasCompletedTutorial => PlayerPrefs.HasKey("TutorialCompleted");

        public static void ResetTutorial()
        {
            PlayerPrefs.DeleteKey("TutorialCompleted");
            PlayerPrefs.Save();
        }
    }
}