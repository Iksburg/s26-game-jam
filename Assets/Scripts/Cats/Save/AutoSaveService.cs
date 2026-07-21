using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// Автосохранение: раз в выбранный интервал перезаписывает тот же сейв,
    /// что и ручное сохранение (отдельных слотов нет). Включается и настраивается
    /// в настройках; по умолчанию выключено.
    /// </summary>
    public class AutoSaveService : MonoBehaviour
    {
        [SerializeField] private GameSaveService _saveService;

        private float _timer;
        private int _activeInterval;

        private void Start()
        {
            if (_saveService == null)
                _saveService = FindFirstObjectByType<GameSaveService>();
            ResetTimer();
        }

        /// <summary>Заполняется билдером сцены (editor wiring).</summary>
        public void Configure(GameSaveService saveService)
        {
            _saveService = saveService;
        }

        private void Update()
        {
            if (!GameSettings.AutoSaveEnabled)
            {
                // Выключено — держим таймер взведённым, чтобы после включения
                // отсчёт шёл с полного интервала, а не срабатывал сразу.
                ResetTimer();
                return;
            }

            // Игрок мог сменить интервал в настройках — пересчитываем отсчёт.
            if (_activeInterval != GameSettings.AutoSaveIntervalMinutes)
                ResetTimer();

            // Масштабированное время: пока игра на паузе в меню, автосохранение
            // не тикает — интервал считается по игровому времени.
            _timer -= Time.deltaTime;
            if (_timer > 0f)
                return;

            ResetTimer();
            PerformAutoSave();
        }

        private void PerformAutoSave()
        {
            if (_saveService == null)
            {
                Debug.LogWarning("[AutoSave] Нет GameSaveService — автосохранение пропущено.");
                return;
            }

            _saveService.SaveGame();
            Debug.Log($"[AutoSave] Автосохранение выполнено (интервал {_activeInterval} мин).");
        }

        private void ResetTimer()
        {
            _activeInterval = GameSettings.AutoSaveIntervalMinutes;
            _timer = _activeInterval * 60f;
        }
    }
}
