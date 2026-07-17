using Cats.Genome;
using Cats.Genome.Abstract;
using CatWorld.Cats;
using UnityEngine;

namespace Cats.Spawning
{
    /// <summary>
    /// Создаёт нового кота на поле фермы: случайный цвет из палитры,
    /// стадия Adult (по концепту покупается взрослый кот), потребности 100%,
    /// уникальный ID, случайная позиция внутри проходимой зоны FarmBounds.
    /// </summary>
    public class CatSpawner : MonoBehaviour
    {
        [SerializeField] private CatColorPalette _palette;
        [SerializeField] private Cat _catPrefab;
        [SerializeField] private FarmBounds _bounds;
        [Tooltip("Стадия нового кота. По концепту покупной кот — Adult; Kitten удобен для теста стадий.")]
        [SerializeField] private LifeStage _initialStage = LifeStage.Adult;

        [Header("Генетические настройки по умолчанию")]
        [Tooltip("Сила пола по умолчанию для покупных котов.")]
        [SerializeField] private float _defaultSexStrength = 10f;

        /// <summary>Создаёт кота с заданными полом и именем; остальное — по правилам спавна.</summary>
        public Cat SpawnCat(Sex sex, string catName)
        {
            var spawnPoint = _bounds != null ? (Vector3)_bounds.GetRandomPointInside() : Vector3.zero;
            var cat = Instantiate(_catPrefab, spawnPoint, Quaternion.identity);
            var furColor = _palette != null ? _palette.GetRandomColor() : Color.white;

            // 1. Создаем чистую C# модель генома для базового (первого) кота
            ICatGenome initialGenome = sex switch
            {
                Sex.Male => new CatGenomeMale(furColor, _defaultSexStrength),
                Sex.Female => new CatGenomeFemale(furColor, _defaultSexStrength),
                _ => throw new System.ArgumentOutOfRangeException(nameof(sex), "Неизвестный пол при спавне")
            };

            // 2. Инициализируем кота, передавая обновленные параметры и готовую модель генома
            cat.Initialize(catName, _initialStage, initialGenome);
            cat.name = $"Cat_{catName}";

            var view = cat.GetComponent<CatView>();
            if (view != null)
                view.ApplyColor(furColor);

            var wander = cat.GetComponent<CatWanderController>();
            if (wander != null)
                wander.SetBounds(_bounds);

            Debug.Log($"[CatSpawner] Появился кот: {catName} ({sex}), ID {cat.Id}, " +
                      $"цвет {furColor}, сытость {cat.Satiety}%, вода {cat.Water}%, чистота {cat.Cleanliness}%");
            return cat;
        }
        
        /// <summary>Создаёт котёнка на основе готового генома (результата скрещивания).</summary>
        public Cat SpawnChildCat(string catName, ICatGenome childGenome)
        {
            // Находим случайную точку на ферме для появления котёнка
            var spawnPoint = _bounds != null ? (Vector3)_bounds.GetRandomPointInside() : Vector3.zero;
            var cat = Instantiate(_catPrefab, spawnPoint, Quaternion.identity);

            // Инициализируем кота готовым геномом и стадией Kitten
            cat.Initialize(catName, LifeStage.Kitten, childGenome);
            cat.name = $"Cat_{catName}";

            // Применяем цвет из генома к визуальному представлению кота
            var view = cat.GetComponent<CatView>();
            if (view != null)
                view.ApplyColor(childGenome.Color);

            // Настраиваем ИИ перемещения котёнка в границах фермы
            var wander = cat.GetComponent<CatWanderController>();
            if (wander != null)
                wander.SetBounds(_bounds);

            Debug.Log($"[CatSpawner] Из генетики родился котёнок: {catName} ({childGenome.Sex}), ID {cat.Id}, " +
                      $"цвет {childGenome.Color}, потребности 100%");
              
            return cat;
        }

        /// <summary>Заполняется билдером сцены (editor-time wiring).</summary>
        public void Configure(CatColorPalette palette, Cat catPrefab, FarmBounds bounds)
        {
            _palette = palette;
            _catPrefab = catPrefab;
            _bounds = bounds;
        }
    }
}
