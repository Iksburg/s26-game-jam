using System.Collections.Generic;
using Cats.Genome;
using Cats.Genome.Abstract;
using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// Базовая сущность кота. Хранит все характеристики одного кота.
    /// Вешается на GameObject кота. Поведение, движение и визуал добавляются
    /// отдельными компонентами в последующих задачах.
    /// </summary>
    public class Cat : MonoBehaviour
    {
        public const int MaxInnateTraits = 3;
        public const int MaxAcquiredTraits = 2;
        public const float MinNeed = 0f;
        public const float MaxNeed = 100f;

        [Header("Идентификация")]
        [SerializeField] private string _id;
        [SerializeField] private string _name;
        [SerializeField] private LifeStage _stage;

        [Header("Черты характера")]
        [SerializeField] private List<CatTrait> _innateTraits = new List<CatTrait>();   // врождённые, до 3
        [SerializeField] private List<CatTrait> _acquiredTraits = new List<CatTrait>();  // приобретённые, до 2

        [Header("Потребности (0-100 %)")]
        [SerializeField, Range(MinNeed, MaxNeed)] private float _satiety = MaxNeed;      // сытость
        [SerializeField, Range(MinNeed, MaxNeed)] private float _water = MaxNeed;        // вода
        [SerializeField, Range(MinNeed, MaxNeed)] private float _cleanliness = MaxNeed;  // чистота

        [Header("Состояние")]
        [SerializeField] private CatActivity _currentActivity = CatActivity.Idle;
        [SerializeField] private FarmStatus _farmStatus = FarmStatus.OnFarm;
        [SerializeField] private bool _canBreed; // задаётся CatAgeController по стадии

        // ---- Публичные геттеры ----
        public string Id => _id;
        public string Name => _name;
        public LifeStage Stage => _stage;
        public IReadOnlyList<CatTrait> InnateTraits => _innateTraits;
        public IReadOnlyList<CatTrait> AcquiredTraits => _acquiredTraits;
        public float Satiety => _satiety;
        public float Water => _water;
        public float Cleanliness => _cleanliness;
        public CatActivity CurrentActivity => _currentActivity;
        public FarmStatus FarmStatus => _farmStatus;
        public bool CanBreed => _canBreed;

        // ---- Геттеры, делегированные в бизнес-логику генома ----
        [field: Header("Бизнес-логика генома")]
        public ICatGenome Genome { get; private set; }

        public Sex Sex => Genome?.Sex ?? Sex.Male; // Значение по умолчанию, если геном не задан
        public Color FurColor => Genome?.Color ?? Color.white;

        /// <summary>
        /// Инициализирует кота стартовыми характеристиками. Генерирует уникальный ID,
        /// если он ещё не задан. Потребности по умолчанию — 100%.
        /// </summary>
        public void Initialize(string catName, LifeStage stage, ICatGenome genome)
        {
            if (string.IsNullOrEmpty(_id))
                _id = System.Guid.NewGuid().ToString();

            _name = catName;
            _stage = stage;
            
            // Внедряем чистую C# модель генома
            Genome = genome;

            _satiety = MaxNeed;
            _water = MaxNeed;
            _cleanliness = MaxNeed;
            _currentActivity = CatActivity.Idle;
            _farmStatus = FarmStatus.OnFarm;

            // Применяем цвет из генома к визуальному компоненту, если он есть
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Genome.Color;
            }
        }

        /// <summary>Создаёт GameObject с компонентом Cat и инициализирует его.</summary>
        public static Cat Create(string catName, LifeStage stage, ICatGenome genome)
        {
            var go = new GameObject($"Cat_{catName}");
            var cat = go.AddComponent<Cat>();
            cat.Initialize(catName, stage, genome);
            return cat;
        }

        /// <summary>Добавляет врождённую черту, если не превышен лимит (3) и черта уникальна.</summary>
        public bool TryAddInnateTrait(CatTrait trait)
        {
            return TryAddTrait(_innateTraits, trait, MaxInnateTraits);
        }

        /// <summary>Добавляет приобретённую черту, если не превышен лимит (2) и черта уникальна.</summary>
        public bool TryAddAcquiredTrait(CatTrait trait)
        {
            return TryAddTrait(_acquiredTraits, trait, MaxAcquiredTraits);
        }

        private bool TryAddTrait(List<CatTrait> traits, CatTrait trait, int limit)
        {
            if (traits.Count >= limit)
                return false;
            if (traits.Contains(trait))
                return false;

            traits.Add(trait);
            return true;
        }

        /// <summary>Метод перенесён в доменную модель генома. Оставлен для совместимости с внешними вызовами.</summary>
        public void AddChild(string childId)
        {
            // Метод оставлен пустым, так как связи детей теперь автоматически регистрируются 
            // в чистом C# конструкторе генома при его создании через CatBreedingService.
        }

        // ---- Сеттеры состояния с валидацией ----
        public void SetActivity(CatActivity activity) => _currentActivity = activity;
        public void SetFarmStatus(FarmStatus status) => _farmStatus = status;
        public void SetStage(LifeStage stage) => _stage = stage;
        public void SetCanBreed(bool canBreed) => _canBreed = canBreed;

        public void SetSatiety(float value) => _satiety = Mathf.Clamp(value, MinNeed, MaxNeed);
        public void SetWater(float value) => _water = Mathf.Clamp(value, MinNeed, MaxNeed);
        public void SetCleanliness(float value) => _cleanliness = Mathf.Clamp(value, MinNeed, MaxNeed);
    }
}
