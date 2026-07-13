using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// Создаёт нового кота на поле фермы: случайный цвет из палитры,
    /// стадия Adult (по концепту покупается взрослый кот), потребности 100%,
    /// уникальный ID, случайная позиция в зоне спавна.
    /// </summary>
    public class CatSpawner : MonoBehaviour
    {
        [SerializeField] private CatColorPalette _palette;
        [SerializeField] private Cat _catPrefab;
        [SerializeField] private Transform _spawnAreaCenter;
        [SerializeField] private Vector2 _spawnAreaSize = new Vector2(12f, 6f);

        /// <summary>Создаёт кота с заданными полом и именем; остальное — по правилам спавна.</summary>
        public Cat SpawnCat(Sex sex, string catName)
        {
            var cat = Instantiate(_catPrefab, GetRandomSpawnPoint(), Quaternion.identity);
            var furColor = _palette != null ? _palette.GetRandomColor() : Color.white;

            cat.Initialize(catName, sex, LifeStage.Adult, furColor);
            cat.name = $"Cat_{catName}";

            var view = cat.GetComponent<CatView>();
            if (view != null)
                view.ApplyColor(furColor);

            Debug.Log($"[CatSpawner] Появился кот: {catName} ({sex}), ID {cat.Id}, " +
                      $"цвет {furColor}, сытость {cat.Satiety}%, вода {cat.Water}%, чистота {cat.Cleanliness}%");
            return cat;
        }

        /// <summary>Заполняется билдером сцены (editor-time wiring).</summary>
        public void Configure(CatColorPalette palette, Cat catPrefab, Transform spawnAreaCenter, Vector2 spawnAreaSize)
        {
            _palette = palette;
            _catPrefab = catPrefab;
            _spawnAreaCenter = spawnAreaCenter;
            _spawnAreaSize = spawnAreaSize;
        }

        private Vector3 GetRandomSpawnPoint()
        {
            Vector3 center = _spawnAreaCenter != null ? _spawnAreaCenter.position : Vector3.zero;
            float x = Random.Range(-_spawnAreaSize.x * 0.5f, _spawnAreaSize.x * 0.5f);
            float y = Random.Range(-_spawnAreaSize.y * 0.5f, _spawnAreaSize.y * 0.5f);
            return center + new Vector3(x, y, 0f);
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = _spawnAreaCenter != null ? _spawnAreaCenter.position : transform.position;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(center, new Vector3(_spawnAreaSize.x, _spawnAreaSize.y, 0f));
        }
    }
}
