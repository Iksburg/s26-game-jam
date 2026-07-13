using System.Collections.Generic;
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
        [SerializeField] private Sex _sex;
        [SerializeField] private LifeStage _stage;

        [Header("Внешний вид")]
        [SerializeField] private Color _furColor = Color.white;

        [Header("Черты характера")]
        [SerializeField] private List<CatTrait> _innateTraits = new List<CatTrait>();   // врождённые, до 3
        [SerializeField] private List<CatTrait> _acquiredTraits = new List<CatTrait>();  // приобретённые, до 2

        [Header("Потребности (0-100 %)")]
        [SerializeField, Range(MinNeed, MaxNeed)] private float _satiety = MaxNeed;      // сытость
        [SerializeField, Range(MinNeed, MaxNeed)] private float _water = MaxNeed;        // вода
        [SerializeField, Range(MinNeed, MaxNeed)] private float _cleanliness = MaxNeed;  // чистота

        [Header("Родословная")]
        [SerializeField] private string _parentId1;
        [SerializeField] private string _parentId2;
        [SerializeField] private List<string> _childrenIds = new List<string>();

        [Header("Состояние")]
        [SerializeField] private CatActivity _currentActivity = CatActivity.Idle;
        [SerializeField] private FarmStatus _farmStatus = FarmStatus.OnFarm;

        // ---- Публичные геттеры ----
        public string Id => _id;
        public string Name => _name;
        public Sex Sex => _sex;
        public LifeStage Stage => _stage;
        public Color FurColor => _furColor;
        public IReadOnlyList<CatTrait> InnateTraits => _innateTraits;
        public IReadOnlyList<CatTrait> AcquiredTraits => _acquiredTraits;
        public float Satiety => _satiety;
        public float Water => _water;
        public float Cleanliness => _cleanliness;
        public string ParentId1 => _parentId1;
        public string ParentId2 => _parentId2;
        public IReadOnlyList<string> ChildrenIds => _childrenIds;
        public CatActivity CurrentActivity => _currentActivity;
        public FarmStatus FarmStatus => _farmStatus;

        /// <summary>
        /// Инициализирует кота стартовыми характеристиками. Генерирует уникальный ID,
        /// если он ещё не задан. Потребности по умолчанию — 100%.
        /// </summary>
        public void Initialize(string catName, Sex sex, LifeStage stage, Color furColor,
            string parentId1 = null, string parentId2 = null)
        {
            if (string.IsNullOrEmpty(_id))
                _id = System.Guid.NewGuid().ToString();

            _name = catName;
            _sex = sex;
            _stage = stage;
            _furColor = furColor;
            _parentId1 = parentId1;
            _parentId2 = parentId2;

            _satiety = MaxNeed;
            _water = MaxNeed;
            _cleanliness = MaxNeed;
            _currentActivity = CatActivity.Idle;
            _farmStatus = FarmStatus.OnFarm;
        }

        /// <summary>Создаёт GameObject с компонентом Cat и инициализирует его.</summary>
        public static Cat Create(string catName, Sex sex, LifeStage stage, Color furColor,
            string parentId1 = null, string parentId2 = null)
        {
            var go = new GameObject($"Cat_{catName}");
            var cat = go.AddComponent<Cat>();
            cat.Initialize(catName, sex, stage, furColor, parentId1, parentId2);
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

        /// <summary>Регистрирует ID ребёнка в родословной (без дубликатов).</summary>
        public void AddChild(string childId)
        {
            if (string.IsNullOrEmpty(childId) || _childrenIds.Contains(childId))
                return;
            _childrenIds.Add(childId);
        }

        // ---- Сеттеры состояния с валидацией ----
        public void SetActivity(CatActivity activity) => _currentActivity = activity;
        public void SetFarmStatus(FarmStatus status) => _farmStatus = status;

        public void SetSatiety(float value) => _satiety = Mathf.Clamp(value, MinNeed, MaxNeed);
        public void SetWater(float value) => _water = Mathf.Clamp(value, MinNeed, MaxNeed);
        public void SetCleanliness(float value) => _cleanliness = Mathf.Clamp(value, MinNeed, MaxNeed);
    }
}
