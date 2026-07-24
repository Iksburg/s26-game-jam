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
        
        public bool Completed => _tutorialCompleted;

        // === НОВОЕ: Храним активный слушатель и кнопку для гарантированной очистки ===
        private UnityAction _activeClickListener;
        private Button _currentActiveButton;
        
        private List<Button> _disabledButtons = new List<Button>();
        private Image _currentDropTarget; 
        private int _requiredDrops = 0; // Сколько раз нужно успешно дропнуть
        private int _currentDrops = 0;  // Сколько раз уже дропнули

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

        public void AddStep(string hintText, SpriteRenderer targetImage, float duration = 0f, bool waitForDrop = false)
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

        private void ShowStep(int stepIndex)
        {
            if (stepIndex < 0 || stepIndex >= _steps.Count) return;
            
            TutorialStep step = _steps[stepIndex];
            _currentDropTarget = null;
            _currentDropTargetSprite = null; // Сброс целевого спрайта

            // === ПОДСВЕТКА ===
            if (step.TargetImage != null)
                _highlighter.HighlightElement(step.TargetImage);
            else if (step.TargetSprite != null)
                _highlighter.HighlightSprite(step.TargetSprite); // Новый метод
            else
                _highlighter.RemoveHighlight();

            // === ПОДСКАЗКА ===
            if (_hintPanel != null)
            {
                RectTransform targetRect = step.GetTargetRect();
                
                // Если цель - SpriteRenderer, конвертируем мировые координаты в экранные для Canvas
                if (targetRect == null && step.TargetSprite != null)
                {
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(step.TargetSprite.bounds.center);
                    // Создаем временный Rect для позиционирования
                    // Примечание: для точного позиционирования лучше использовать отдельный UI элемент-якорь, 
                    // но для простоты используем центр спрайта
                }
                
                bool showNextButton = !step.WaitForButtonClick && !step.WaitForDropEvent;
                
                // Передаем либо Rect, либо сам Transform для сложной логики позиционирования
                // В текущей реализации HintPanel ждет RectTransform. 
                // Если у нас Sprite, нам нужно либо создать dummy Rect, либо доработать HintPanel.
                // Для простоты пока передадим null, если это Sprite, и пусть HintPanel висит в центре или использует свою логику,
                // ЛИБО доработаем ShowHint ниже.
                
                 _hintPanel.ShowHint(step.HintText, targetRect, showNextButton);
                 
                 // Если это Sprite, попробуем вручную подвинуть панель
                 if (targetRect == null && step.TargetSprite != null && _hintPanel.gameObject.activeSelf)
                 {
                     Vector3 screenPos = Camera.main.WorldToScreenPoint(step.TargetSprite.bounds.center);
                     // Преобразуем в локальные координаты Canvas (предполагается Screen Space Overlay или Camera)
                     // Это упрощенная логика, может потребовать доработки под ваш Canvas
                     _hintPanel.transform.position = new Vector3(screenPos.x, screenPos.y + 100, screenPos.z);
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
            // Логика ожидания ДРОПА (UI Image)
            else if (step.WaitForDropEvent && step.TargetImage != null)
            {
                _currentDropTarget = step.TargetImage;
            }
            // Логика ожидания ДРОПА (SpriteRenderer)
            else if (step.WaitForDropEvent && step.TargetSprite != null)
            {
                _currentDropTargetSprite = step.TargetSprite;
            }
            // Логика ожидания ВРЕМЕНИ
            else if (step.DisplayDuration > 0f)
            {
                StartCoroutine(WaitForDuration(step.DisplayDuration));
            }
        }

        // Добавляем поле для целевого спрайта
        private SpriteRenderer _currentDropTargetSprite;

        /// <summary>
        /// Гарантированно удаляет активный слушатель клика и разблокирует кнопки.
        /// Вызывается при каждом переходе между шагами.
        /// </summary>
        private void CleanupActiveListener()
        {
            if (_currentActiveButton != null && _activeClickListener != null)
            {
                // Удаляем конкретный слушатель
                _currentActiveButton.onClick.RemoveListener(_activeClickListener);
            }
            
            // Сбрасываем ссылки
            _currentActiveButton = null;
            _activeClickListener = null;
            
            // Разблокируем кнопки, так как следующий шаг может иметь другую цель
            UnlockAllButtons();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isRunning) return;

            // Проверка для UI Image
            if (_currentDropTarget != null)
            {
                if (eventData.pointerEnter == _currentDropTarget.gameObject)
                {
                    Debug.Log("Tutorial Drop detected on UI!");
                    _currentDropTarget = null;
                    NextStep();
                    return;
                }
            }

            // Проверка для SpriteRenderer (Мировые координаты)
            if (_currentDropTargetSprite != null)
            {
                // Проверяем, попал ли курсор в bounds спрайта
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(eventData.position);
                // Используем Physics2D.OverlapPoint или простую проверку границ
                if (_currentDropTargetSprite.bounds.Contains(mousePos))
                {
                    Debug.Log("Tutorial Drop detected on Sprite!");
                    _currentDropTargetSprite = null;
                    NextStep();
                }
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