using System;
using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// Автоматическое прохождение стадий жизни: котёнок → взрослый → пожилой.
    /// По таймеру из CatLifeStageSettings меняет стадию и применяет её эффекты:
    /// масштаб/спрайт (CatView), скорость (CatWanderController), возможность
    /// размножения (Cat). Пожилой кот периодически бросает шанс ухода с карты.
    /// </summary>
    [RequireComponent(typeof(Cat))]
    public class CatAgeController : MonoBehaviour
    {
        [SerializeField] private CatLifeStageSettings _settings;

        /// <summary>Вызывается после смены стадии (новая стадия).</summary>
        public event Action<LifeStage> StageChanged;

        private Cat _cat;
        private CatView _view;
        private CatWanderController _wander;
        private float _stageTimer;
        private float _leaveTimer;

        private void Awake()
        {
            _cat = GetComponent<Cat>();
            _view = GetComponent<CatView>();
            _wander = GetComponent<CatWanderController>();
        }

        private void Start()
        {
            // Применяем эффекты стартовой стадии (кот мог заспавниться любой).
            EnterStage(_cat.Stage, invokeEvent: false);
        }

        /// <summary>Заполняется билдером префаба (editor wiring).</summary>
        public void Configure(CatLifeStageSettings settings)
        {
            _settings = settings;
        }

        private void Update()
        {
            if (_settings == null)
                return;

            switch (_cat.Stage)
            {
                case LifeStage.Kitten:
                case LifeStage.Adult:
                    TickGrowth();
                    break;
                case LifeStage.Senior:
                    TickSeniorLeave();
                    break;
            }
        }

        private void TickGrowth()
        {
            _stageTimer -= Time.deltaTime;
            if (_stageTimer > 0f)
                return;

            LifeStage next = _cat.Stage == LifeStage.Kitten ? LifeStage.Adult : LifeStage.Senior;
            EnterStage(next, invokeEvent: true);
        }

        private void TickSeniorLeave()
        {
            if (_settings.SeniorLeaveChance <= 0f)
                return;

            _leaveTimer -= Time.deltaTime;
            if (_leaveTimer > 0f)
                return;

            _leaveTimer = _settings.SeniorLeaveCheckInterval;
            if (UnityEngine.Random.value < _settings.SeniorLeaveChance)
                LeaveFarm();
        }

        private void EnterStage(LifeStage stage, bool invokeEvent)
        {
            if (_settings == null)
                return;

            var config = _settings.GetConfig(stage);

            if (invokeEvent)
                _cat.SetStage(stage);
            _cat.SetCanBreed(config.canBreed);

            if (_view != null)
                _view.ApplyStage(stage, config.scale);
            if (_wander != null)
                _wander.SetSpeedMultiplier(config.speedMultiplier);

            _stageTimer = config.duration;
            _leaveTimer = _settings.SeniorLeaveCheckInterval;

            if (invokeEvent)
            {
                Debug.Log($"[CatAgeController] {_cat.Name}: новая стадия — {stage}");
                StageChanged?.Invoke(stage);
            }
        }

        private void LeaveFarm()
        {
            _cat.SetFarmStatus(FarmStatus.InNewFamily);
            Debug.Log($"[CatAgeController] {_cat.Name} (пожилой) ушёл с фермы к новым хозяевам.");
            Destroy(gameObject);
        }
    }
}
