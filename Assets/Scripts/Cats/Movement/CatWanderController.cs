using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// Автономное перемещение кота по ферме: кот идёт к случайной точке внутри
    /// FarmBounds, по прибытии останавливается на случайное время, затем выбирает
    /// новую точку. Меняет активность (Idle/Walking) и разворачивает спрайт по
    /// направлению движения. За границы зоны не выходит.
    /// </summary>
    [RequireComponent(typeof(Cat))]
    public class CatWanderController : MonoBehaviour
    {
        private enum State { Paused, Walking }

        [Header("Скорость")]
        [SerializeField] private float _moveSpeed = 2.5f;

        [Header("Остановки")]
        [SerializeField] private float _pauseMin = 1f;
        [SerializeField] private float _pauseMax = 3f;
        [SerializeField] private float _arrivalThreshold = 0.1f;

        [SerializeField] private FarmBounds _bounds;

        private Cat _cat;
        private CatView _view;
        private State _state;
        private Vector2 _target;
        private float _pauseTimer;
        private float _speedMultiplier = 1f; // задаётся CatAgeController по стадии

        private void Awake()
        {
            _cat = GetComponent<Cat>();
            _view = GetComponent<CatView>();
            if (_bounds == null)
                _bounds = FindFirstObjectByType<FarmBounds>();
        }

        private void Start()
        {
            EnterPause();
        }

        /// <summary>Инъекция зоны спавнером (editor/runtime wiring).</summary>
        public void SetBounds(FarmBounds bounds)
        {
            _bounds = bounds;
        }

        /// <summary>Множитель скорости от стадии жизни (задаёт CatAgeController).</summary>
        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = multiplier;
        }

        private void Update()
        {
            if (_bounds == null)
                return;

            switch (_state)
            {
                case State.Paused:
                    TickPause();
                    break;
                case State.Walking:
                    TickWalk();
                    break;
            }
        }

        private void TickPause()
        {
            _pauseTimer -= Time.deltaTime;
            if (_pauseTimer <= 0f)
                EnterWalk();
        }

        private void TickWalk()
        {
            Vector2 position = transform.position;
            float speed = _moveSpeed * _speedMultiplier;
            Vector2 next = Vector2.MoveTowards(position, _target, speed * Time.deltaTime);

            // Страж границы: не даём срезать вогнутые участки полигона.
            if (!_bounds.Contains(next))
            {
                PickNewTarget();
                return;
            }

            UpdateFacing(next.x - position.x);
            transform.position = new Vector3(next.x, next.y, transform.position.z);

            if (Vector2.Distance(next, _target) < _arrivalThreshold)
                EnterPause();
        }

        private void EnterPause()
        {
            _state = State.Paused;
            _pauseTimer = Random.Range(_pauseMin, _pauseMax);
            if (_cat != null)
                _cat.SetActivity(CatActivity.Idle);
        }

        private void EnterWalk()
        {
            PickNewTarget();
            _state = State.Walking;
            if (_cat != null)
                _cat.SetActivity(CatActivity.Walking);
        }

        private void PickNewTarget()
        {
            _target = _bounds.GetRandomPointInside();
        }

        private void UpdateFacing(float deltaX)
        {
            if (_view == null || Mathf.Abs(deltaX) < 0.0001f)
                return;
            _view.SetFacingRight(deltaX > 0f);
        }
    }
}
