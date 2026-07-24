// TutorialManager.cs
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CatWorld.Cats.Tutorial
{
    public class TutorialManager : MonoBehaviour, IEndDragHandler 
    {
        public TutorialHintPanel _hintPanel;
        public TutorialHighlight _highlighter;
        [SerializeField] private bool _startTutorialOnAwake = true;

        private List<TutorialStep> _steps = new List<TutorialStep>();
        private int _currentStepIndex = 0;
        private bool _isRunning = false;
        private bool _tutorialCompleted = false;

        // === НОВОЕ: Храним активный слушатель и кнопку для гарантированной очистки ===
        private UnityAction _activeClickListener;
        private Button _currentActiveButton;
        
        private List<Button> _disabledButtons = new List<Button>();
        private Image _currentDropTarget; 

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
            _steps.Add(new TutorialStep(hintText, targetImage, duration, false));
        }

        public void AddStep(string hintText, Image targetImage, float duration = 0f, bool waitForDrop = false)
        {
            _steps.Add(new TutorialStep(hintText, targetImage, duration, waitForDrop));
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
            _currentDropTarget = null;
            
            // === ОБЯЗАТЕЛЬНО очищаем активный слушатель ===
            CleanupActiveListener();
            
            UnlockAllButtons();
            _highlighter.RemoveHighlight();
            if (_hintPanel != null) _hintPanel.HideHint();
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
            
            // Очищаем слушатель ТЕКУЩЕГО шага ПЕРЕД переходом
            CleanupActiveListener();
            
            _currentStepIndex++;
            if (_currentStepIndex >= _steps.Count)
            {
                CompleteTutorial();
                return;
            }
            ShowStep(_currentStepIndex);
        }

        /// <summary>
        /// Гарантированно удаляет активный слушатель клика и разблокирует кнопки.
        /// Вызывается при каждом переходе между шагами.
        /// </summary>
        private void CleanupActiveListener()
        {
            if (_currentActiveButton != null && _activeClickListener != null)
            {
                _currentActiveButton.onClick.RemoveListener(_activeClickListener);
                _currentActiveButton = null;
                _activeClickListener = null;
            }
            UnlockAllButtons();
        }

        private void ShowStep(int stepIndex)
        {
            if (stepIndex < 0 || stepIndex >= _steps.Count) return;

            TutorialStep step = _steps[stepIndex];
            _currentDropTarget = null;

            // Подсвечиваем элемент
            if (step.TargetImage != null)
                _highlighter.HighlightElement(step.TargetImage);

            // Показываем подсказку
            if (_hintPanel != null)
            {
                RectTransform targetRect = null;
                if (step.TargetButton != null) targetRect = step.TargetButton.GetComponent<RectTransform>();
                else if (step.TargetImage != null) targetRect = step.TargetImage.GetComponent<RectTransform>();
        
                bool showNextButton = !step.WaitForButtonClick && !step.WaitForDropEvent;
                _hintPanel.ShowHint(step.HintText, targetRect, showNextButton);
            }

            // === БЛОКИРОВКА ТОЛЬКО ДЛЯ ШАГОВ С WaitForButtonClick ===
            if (step.WaitForButtonClick && step.TargetButton != null)
            {
                LockAllButtonsExcept(step.TargetButton);
                
                UnityAction onClickListener = null;
                onClickListener = () =>
                {
                    // Удаляем слушатель сразу при клике, чтобы не сработал дважды
                    CleanupActiveListener();
                    NextStep();
                };
                
                // Сохраняем ссылки для последующей очистки
                _activeClickListener = onClickListener;
                _currentActiveButton = step.TargetButton;
                
                step.TargetButton.onClick.AddListener(onClickListener);
                StartCoroutine(WaitForButtonClickTimeout(step.TargetButton, onClickListener));
            }
            // Логика ожидания ДРОПА
            else if (step.WaitForDropEvent && step.TargetImage != null)
            {
                _currentDropTarget = step.TargetImage;
            }
            // Логика ожидания ВРЕМЕНИ
            else if (step.DisplayDuration > 0f)
            {
                StartCoroutine(WaitForDuration(step.DisplayDuration));
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isRunning || _currentDropTarget == null) return;

            if (eventData.pointerEnter == _currentDropTarget.gameObject)
            {
                Debug.Log("Tutorial Drop detected!");
                _currentDropTarget = null;
                NextStep();
            }
        }

        private IEnumerator WaitForButtonClickTimeout(Button button, UnityAction listener)
        {
            float elapsed = 0f;
            while (elapsed < 30f && _isRunning)
            {
                // === ИСПОЛЬЗУЕМ unscaledDeltaTime НА СЛУЧАЙ ПАУЗЫ ИГРЫ ===
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            if (button != null) button.onClick.RemoveListener(listener);
            if (_isRunning && elapsed >= 30f) NextStep();
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