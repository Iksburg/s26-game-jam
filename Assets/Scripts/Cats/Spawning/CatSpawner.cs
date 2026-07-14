using UnityEngine;

namespace CatWorld.Cats
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

        /// <summary>Создаёт кота с заданными полом и именем; остальное — по правилам спавна.</summary>
        public Cat SpawnCat(Sex sex, string catName)
        {
            Vector3 spawnPoint = _bounds != null ? (Vector3)_bounds.GetRandomPointInside() : Vector3.zero;
            var cat = Instantiate(_catPrefab, spawnPoint, Quaternion.identity);
            var furColor = _palette != null ? _palette.GetRandomColor() : Color.white;

            cat.Initialize(catName, sex, _initialStage, furColor);
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

        /// <summary>Заполняется билдером сцены (editor-time wiring).</summary>
        public void Configure(CatColorPalette palette, Cat catPrefab, FarmBounds bounds)
        {
            _palette = palette;
            _catPrefab = catPrefab;
            _bounds = bounds;
        }
    }
}
