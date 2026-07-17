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
    }
}
