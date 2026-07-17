using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// Проигрывает Idle-анимацию (Animator/CatController) только когда кот
    /// бездействует. Пока кот идёт или ест/пьёт — Animator выключается, и
    /// анимация не воспроизводится. Работает с контроллером, где есть лишь
    /// зациклённое Idle-состояние, без правки графа Animator.
    /// </summary>
    [RequireComponent(typeof(Cat))]
    [RequireComponent(typeof(Animator))]
    public class CatAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        [Header("Контроллеры по стадиям")]
        [SerializeField] private RuntimeAnimatorController _kittenController;
        [SerializeField] private RuntimeAnimatorController _adultController;
        [SerializeField] private RuntimeAnimatorController _seniorController;

        private Cat _cat;
        private bool? _isPlaying;

        private void Awake()
        {
            _cat = GetComponent<Cat>();
            if (_animator == null)
                _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            // Анимация не должна проигрываться при движении и приёме еды/воды.
            var activity = _cat.CurrentActivity;
            bool shouldPlay = activity != CatActivity.Walking && activity != CatActivity.Eating;

            if (_isPlaying == shouldPlay)
                return;

            _isPlaying = shouldPlay;
            // Выключенный Animator замирает на текущем кадре и ничего не проигрывает;
            // при возврате к бездействию воспроизведение возобновляется.
            _animator.enabled = shouldPlay;
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
        }
    }
}
