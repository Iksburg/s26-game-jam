using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// Связывает состояние кота с Animator: подставляет контроллер под стадию
    /// роста и отдаёт в параметр Speed фактическую скорость перемещения —
    /// по ней контроллер переключает Idle ↔ Walk.
    /// Во время еды/питья Animator выключается: анимация не проигрывается.
    /// </summary>
    [RequireComponent(typeof(Cat))]
    [RequireComponent(typeof(Animator))]
    public class CatAnimationController : MonoBehaviour
    {
        /// <summary>Имя float-параметра в Animator-контроллерах.</summary>
        public const string SpeedParameter = "Speed";

        /// <summary>Ниже этого значения скорость считается нулевой (защита от дрожания float).</summary>
        public const float SpeedThreshold = 0.01f;

        [SerializeField] private Animator _animator;

        [Header("Контроллеры по стадиям (назначаются в инспекторе)")]
        [SerializeField] private RuntimeAnimatorController _kittenController;
        [SerializeField] private RuntimeAnimatorController _adultController;
        [SerializeField] private RuntimeAnimatorController _seniorController;

        private static readonly int SpeedHash = Animator.StringToHash(SpeedParameter);

        private Cat _cat;
        private bool? _isPlaying;
        private bool _hasSpeedParameter;
        private Vector3 _lastPosition;

        private void Awake()
        {
            _cat = GetComponent<Cat>();
            if (_animator == null)
                _animator = GetComponent<Animator>();
            _lastPosition = transform.position;
            RefreshSpeedParameter();
        }

        /// <summary>
        /// Считаем в LateUpdate: к этому моменту перемещение за кадр уже выполнено
        /// системой движения, поэтому скорость получается корректной.
        /// </summary>
        private void LateUpdate()
        {
            float speed = MeasureSpeed();

            // Пока кот ест/пьёт, анимация не проигрывается (Animator выключен).
            // При ходьбе Animator остаётся включённым — играет Walk-анимация.
            bool shouldPlay = _cat.CurrentActivity != CatActivity.Eating;
            if (_isPlaying != shouldPlay)
            {
                _isPlaying = shouldPlay;
                _animator.enabled = shouldPlay;
            }

            // Скорость отдаём всегда: контроллер сам решает, когда Idle, когда Walk.
            if (_hasSpeedParameter)
                _animator.SetFloat(SpeedHash, speed);
        }

        /// <summary>Фактическая скорость по смещению за кадр (в юнитах/сек).</summary>
        private float MeasureSpeed()
        {
            Vector3 position = transform.position;
            float deltaTime = Time.deltaTime;
            float speed = deltaTime > 0f
                ? (position - _lastPosition).magnitude / deltaTime
                : 0f;
            _lastPosition = position;
            return speed;
        }

        /// <summary>
        /// Меняет Animator-контроллер под стадию роста (котёнок/взрослый/пожилой).
        /// Вызывается из CatAgeController при смене стадии. Если контроллер для
        /// стадии не назначен — остаётся текущий (безопасный fallback).
        /// </summary>
        public void ApplyStage(LifeStage stage)
        {
            if (_animator == null)
                _animator = GetComponent<Animator>();

            RuntimeAnimatorController controller;
            switch (stage)
            {
                case LifeStage.Kitten: controller = _kittenController; break;
                case LifeStage.Adult: controller = _adultController; break;
                default: controller = _seniorController; break;
            }

            if (controller == null || _animator.runtimeAnimatorController == controller)
                return;

            _animator.runtimeAnimatorController = controller;
            RefreshSpeedParameter(); // у нового контроллера свой набор параметров
        }

        /// <summary>
        /// Проверяет, есть ли в текущем контроллере параметр Speed. Без этого
        /// SetFloat на контроллере без параметра сыпал бы предупреждения
        /// (например, если у взрослого кота ещё нет Walk-анимации).
        /// </summary>
        private void RefreshSpeedParameter()
        {
            _hasSpeedParameter = false;
            if (_animator == null || _animator.runtimeAnimatorController == null)
                return;

            foreach (var parameter in _animator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Float &&
                    parameter.nameHash == SpeedHash)
                {
                    _hasSpeedParameter = true;
                    return;
                }
            }
        }
    }
}
