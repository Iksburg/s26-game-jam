using System;
using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// Настройки стадий жизни кота (тюнинг гейм-дизайнера, без кода).
    /// Таймеры предварительные: котёнок 2 мин, взрослый 15 мин (концепт: 5/30).
    /// Пожилой живёт без таймера, но с шансом уходит с карты
    /// (leaveChance = 0 — никогда не уходит, поведение концепта).
    /// Множители еды/воды/сна хранятся здесь и применятся в задачах
    /// потребностей и сна.
    /// </summary>
    [CreateAssetMenu(fileName = "CatLifeStageSettings", menuName = "CatWorld/Cat Life Stage Settings")]
    public class CatLifeStageSettings : ScriptableObject
    {
        [Serializable]
        public class StageConfig
        {
            [Tooltip("Длительность стадии в секундах (у пожилого не используется).")]
            public float duration = 60f;
            [Tooltip("Масштаб спрайта кота.")]
            public float scale = 1f;
            [Tooltip("Множитель скорости перемещения.")]
            public float speedMultiplier = 1f;
            [Tooltip("Множитель скорости расхода сытости (задача потребностей).")]
            public float hungerRateMultiplier = 1f;
            [Tooltip("Множитель скорости расхода воды (задача потребностей).")]
            public float thirstRateMultiplier = 1f;
            [Tooltip("Может ли кот размножаться на этой стадии.")]
            public bool canBreed;
            [Tooltip("Множитель частоты сна (задача сна).")]
            public float sleepFrequencyMultiplier = 1f;
        }

        [SerializeField] private StageConfig _kitten = new StageConfig
        {
            duration = 120f,
            scale = 0.5f,
            speedMultiplier = 1.2f,
            hungerRateMultiplier = 1.2f,
            thirstRateMultiplier = 1.2f,
            canBreed = false,
            sleepFrequencyMultiplier = 1f
        };

        [SerializeField] private StageConfig _adult = new StageConfig
        {
            duration = 900f,
            scale = 1f,
            speedMultiplier = 1f,
            hungerRateMultiplier = 1f,
            thirstRateMultiplier = 1f,
            canBreed = true,
            sleepFrequencyMultiplier = 1f
        };

        [SerializeField] private StageConfig _senior = new StageConfig
        {
            duration = 0f, // бесконечно
            scale = 0.9f,
            speedMultiplier = 0.6f,
            hungerRateMultiplier = 0.8f,
            thirstRateMultiplier = 0.8f,
            canBreed = false,
            sleepFrequencyMultiplier = 1.5f
        };

        [Header("Уход пожилого кота с карты")]
        [Tooltip("Как часто (в секундах) пожилой кот бросает шанс ухода.")]
        [SerializeField] private float _seniorLeaveCheckInterval = 60f;
        [Tooltip("Шанс ухода за одну проверку (0–1). 0 — никогда не уходит (как в концепте).")]
        [SerializeField, Range(0f, 1f)] private float _seniorLeaveChance = 0.1f;

        public float SeniorLeaveCheckInterval => _seniorLeaveCheckInterval;
        public float SeniorLeaveChance => _seniorLeaveChance;

        public StageConfig GetConfig(LifeStage stage)
        {
            switch (stage)
            {
                case LifeStage.Kitten: return _kitten;
                case LifeStage.Adult: return _adult;
                default: return _senior;
            }
        }
    }
}
