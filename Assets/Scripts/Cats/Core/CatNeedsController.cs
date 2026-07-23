using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// Потребности кота: сытость, вода и чистота автоматически снижаются со
    /// временем (скорости настраиваются в инспекторе, множители стадии задаёт
    /// CatAgeController). При падении сытости ниже 60% / воды ниже 75% кот
    /// получает состояние Hungry и, если ресурс есть, сам идёт к миске:
    /// ест/пьёт, ресурс фермы уменьшается на 1, показатель восстанавливается
    /// до 100%, кот возвращается к обычному блужданию.
    /// Побег при нуле (концепт) — отдельная задача.
    /// </summary>
    [RequireComponent(typeof(Cat))]
    public class CatNeedsController : MonoBehaviour
    {
        private enum ConsumeState { None, GoingToStation, Consuming }

        [Header("Скорость снижения (ед./сек)")]
        [SerializeField] private float _satietyDecay = 1f;
        [SerializeField] private float _waterDecay = 1.2f;
        [SerializeField] private float _cleanlinessDecay = 0.3f;

        [Header("Пороги (%)")]
        [SerializeField, Range(0f, 100f)] private float _hungerThreshold = 60f;
        [SerializeField, Range(0f, 100f)] private float _thirstThreshold = 75f;

        [Header("Приём пищи/воды")]
        [SerializeField] private float _consumeDuration = 2f;

        [Header("Отладка")]
        [SerializeField] private float _logInterval = 10f;

        /// <summary>Кот успешно поел (только еда, не вода). Слушает CatPoopController.</summary>
        public event System.Action Ate;

        private Cat _cat;
        private CatWanderController _wander;
        private FarmResources _resources;
        private NeedStation[] _stations;

        private ConsumeState _state;
        private NeedType _consumingType;
        private float _consumeTimer;
        private float _logTimer;
        private float _hungerRateMultiplier = 1f;
        private float _thirstRateMultiplier = 1f;

        private void Awake()
        {
            _cat = GetComponent<Cat>();
            _wander = GetComponent<CatWanderController>();
        }

        private void Start()
        {
            _resources = FindFirstObjectByType<FarmResources>();
            _stations = FindObjectsByType<NeedStation>(FindObjectsSortMode.None);
            _logTimer = _logInterval;
        }

        /// <summary>Множители расхода еды/воды от стадии жизни (задаёт CatAgeController).</summary>
        public void SetRateMultipliers(float hunger, float thirst)
        {
            _hungerRateMultiplier = hunger;
            _thirstRateMultiplier = thirst;
        }

        private void Update()
        {
            TickDecay();
            TickLog();

            switch (_state)
            {
                case ConsumeState.None:
                    TrySeekStation();
                    break;
                case ConsumeState.GoingToStation:
                    // Ресурс мог закончиться, пока кот шёл — прерываем поход.
                    if (!IsResourceAvailable(_consumingType))
                        AbortSeek();
                    break;
                case ConsumeState.Consuming:
                    TickConsume();
                    break;
            }
        }

        private void LateUpdate()
        {
            // После Update() блуждания: еда/питьё и голод имеют приоритет активности.
            if (_state == ConsumeState.Consuming)
                _cat.SetActivity(CatActivity.Eating);
            else if (_state != ConsumeState.GoingToStation && (IsHungry() || IsThirsty()))
                _cat.SetActivity(CatActivity.Hungry);
        }

        private void TickDecay()
        {
            float dt = Time.deltaTime;
            _cat.SetSatiety(_cat.Satiety - _satietyDecay * _hungerRateMultiplier * dt);
            _cat.SetWater(_cat.Water - _waterDecay * _thirstRateMultiplier * dt);
            _cat.SetCleanliness(_cat.Cleanliness - _cleanlinessDecay * dt);
        }

        private void TickLog()
        {
            if (_logInterval <= 0f)
                return;
            _logTimer -= Time.deltaTime;
            if (_logTimer > 0f)
                return;
            _logTimer = _logInterval;
            Debug.Log($"[CatNeeds] {_cat.Name}: сытость {_cat.Satiety:F0}%, " +
                      $"вода {_cat.Water:F0}%, чистота {_cat.Cleanliness:F0}%");
        }

        private bool IsHungry() => _cat.Satiety < _hungerThreshold;
        private bool IsThirsty() => _cat.Water < _thirstThreshold;

        private void TrySeekStation()
        {
            if (_wander == null)
                return;

            // Выбираем более срочную потребность из доступных.
            bool wantFood = IsHungry() && IsResourceAvailable(NeedType.Food);
            bool wantWater = IsThirsty() && IsResourceAvailable(NeedType.Water);
            if (!wantFood && !wantWater)
                return;

            NeedType type;
            if (wantFood && wantWater)
                type = _cat.Satiety <= _cat.Water ? NeedType.Food : NeedType.Water;
            else
                type = wantFood ? NeedType.Food : NeedType.Water;

            NeedStation station = FindNearestStation(type);
            if (station == null)
                return;

            _consumingType = type;
            _state = ConsumeState.GoingToStation;
            Debug.Log($"[CatNeeds] {_cat.Name} проголодался ({type}) и идёт к миске.");
            _wander.SetExternalTarget(station.Position, OnArrivedAtStation);
        }

        private void OnArrivedAtStation()
        {
            _state = ConsumeState.Consuming;
            _consumeTimer = _consumeDuration;
        }

        private void TickConsume()
        {
            _consumeTimer -= Time.deltaTime;
            if (_consumeTimer > 0f)
                return;

            bool consumed = _consumingType == NeedType.Food
                ? _resources != null && _resources.TryConsumeFood()
                : _resources != null && _resources.TryConsumeWater();

            if (consumed)
            {
                if (_consumingType == NeedType.Food)
                {
                    _cat.SetSatiety(Cat.MaxNeed);
                    Ate?.Invoke(); // еда → позже появится какашка
                }
                else
                {
                    _cat.SetWater(Cat.MaxNeed);
                }
                Debug.Log($"[CatNeeds] {_cat.Name} поел/попил ({_consumingType}), показатель восстановлен до 100%.");
            }

            _state = ConsumeState.None;
            _wander.ClearExternalTarget();
        }

        private void AbortSeek()
        {
            Debug.Log($"[CatNeeds] {_cat.Name}: ресурс {_consumingType} закончился по пути — возвращаюсь к прогулке.");
            _state = ConsumeState.None;
            _wander.ClearExternalTarget();
        }

        private bool IsResourceAvailable(NeedType type)
        {
            if (_resources == null)
                return false;
            return type == NeedType.Food ? _resources.Food > 0 : _resources.Water > 0;
        }

        private NeedStation FindNearestStation(NeedType type)
        {
            NeedStation nearest = null;
            float bestSqr = float.MaxValue;
            Vector2 position = transform.position;

            foreach (var station in _stations)
            {
                if (station == null || station.Type != type)
                    continue;
                float sqr = ((Vector2)station.Position - position).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearest = station;
                }
            }

            return nearest;
        }
    }
}
