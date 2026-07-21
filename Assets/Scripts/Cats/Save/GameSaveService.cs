using System.Collections.Generic;
using Cats.Genome;
using Cats.Genome.Abstract;
using Cats.Spawning;
using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// Собирает состояние фермы в сейв и восстанавливает его при загрузке.
    /// Живёт в сцене CatSpawn. Если при входе в сцену выставлен
    /// SaveSystem.LoadRequested (кнопка «Продолжить»), состояние применяется на старте.
    /// </summary>
    public class GameSaveService : MonoBehaviour
    {
        [SerializeField] private CatSpawner _spawner;
        [SerializeField] private FarmResources _resources;

        private void Start()
        {
            if (_spawner == null)
                _spawner = FindFirstObjectByType<CatSpawner>();
            if (_resources == null)
                _resources = FindFirstObjectByType<FarmResources>();

            if (!SaveSystem.LoadRequested)
                return;

            // Флаг одноразовый: «Новая игра» после этого не должна грузить сейв.
            SaveSystem.LoadRequested = false;
            LoadGame();
        }

        /// <summary>Заполняется билдером сцены (editor wiring).</summary>
        public void Configure(CatSpawner spawner, FarmResources resources)
        {
            _spawner = spawner;
            _resources = resources;
        }

        /// <summary>Сохраняет текущее состояние фермы.</summary>
        public void SaveGame()
        {
            var data = new GameSaveData
            {
                food = _resources != null ? _resources.Food : 0,
                water = _resources != null ? _resources.Water : 0,
                meowCoins = 0 // экономика ещё не реализована — поле-задел
            };

            foreach (var cat in FindObjectsByType<Cat>(FindObjectsSortMode.None))
            {
                data.cats.Add(CollectCat(cat));
            }

            SaveSystem.Save(data);
        }

        private static CatSaveData CollectCat(Cat cat)
        {
            var position = cat.transform.position;
            var age = cat.GetComponent<CatAgeController>();

            var data = new CatSaveData
            {
                catId = cat.Id,
                name = cat.Name,
                stage = cat.Stage,
                activity = cat.CurrentActivity,
                farmStatus = cat.FarmStatus,
                satiety = cat.Satiety,
                water = cat.Water,
                cleanliness = cat.Cleanliness,
                stageTimer = age != null ? age.StageTimer : 0f,
                leaveTimer = age != null ? age.LeaveTimer : 0f,
                positionX = position.x,
                positionY = position.y,
                innateTraits = new List<CatTrait>(cat.InnateTraits),
                acquiredTraits = new List<CatTrait>(cat.AcquiredTraits)
            };

            var genome = cat.Genome;
            if (genome != null)
            {
                data.genomeId = genome.Id;
                data.sex = genome.Sex;
                data.SetColor(genome.Color);
                data.sexStrength = GetSexStrength(genome);

                foreach (var parent in genome.Parents)
                {
                    if (parent != null)
                        data.parentGenomeIds.Add(parent.Id);
                }
                foreach (var child in genome.Children)
                {
                    if (child != null)
                        data.childGenomeIds.Add(child.Id);
                }
            }

            return data;
        }

        /// <summary>Восстанавливает ферму из сейва.</summary>
        public void LoadGame()
        {
            var data = SaveSystem.Load();
            if (data == null)
            {
                Debug.LogWarning("[GameSaveService] Сохранение не найдено — начинаем новую игру.");
                return;
            }

            if (_spawner == null)
            {
                Debug.LogError("[GameSaveService] Нет CatSpawner — коты не восстановлены.");
                return;
            }

            // Убираем котов, которые могли оказаться в сцене до загрузки.
            foreach (var existing in FindObjectsByType<Cat>(FindObjectsSortMode.None))
            {
                Destroy(existing.gameObject);
            }

            if (_resources != null)
                _resources.RestoreResources(data.food, data.water);

            // Проход 1: создаём геномы без связей — родителя может ещё не быть.
            var genomesById = new Dictionary<string, CatGenome>();
            foreach (var catData in data.cats)
            {
                var genome = CreateGenome(catData);
                if (genome != null)
                    genomesById[catData.genomeId] = genome;
            }

            // Проход 2: связываем родителей и детей по Id.
            foreach (var catData in data.cats)
            {
                if (!genomesById.TryGetValue(catData.genomeId, out var genome))
                    continue;

                foreach (var parentId in catData.parentGenomeIds)
                {
                    if (genomesById.TryGetValue(parentId, out var parent))
                        genome.RestoreParent(parent);
                }
                foreach (var childId in catData.childGenomeIds)
                {
                    if (genomesById.TryGetValue(childId, out var child))
                        genome.AddChild(child);
                }
            }

            // Проход 3: создаём котов на сцене с восстановленными геномами.
            foreach (var catData in data.cats)
            {
                if (genomesById.TryGetValue(catData.genomeId, out var genome))
                    _spawner.SpawnRestoredCat(catData, genome);
            }

            Debug.Log($"[GameSaveService] Игра загружена: котов {data.cats.Count}, " +
                      $"корм {data.food}, вода {data.water} (сохранено {data.savedAt}).");
        }

        private static CatGenome CreateGenome(CatSaveData data)
        {
            if (string.IsNullOrEmpty(data.genomeId))
            {
                Debug.LogWarning($"[GameSaveService] У кота «{data.name}» нет генома в сейве — пропускаем.");
                return null;
            }

            return data.sex == Sex.Male
                ? new CatGenomeMale(data.genomeId, data.name, data.GetColor(), data.sexStrength)
                : (CatGenome)new CatGenomeFemale(data.genomeId, data.name, data.GetColor(), data.sexStrength);
        }

        /// <summary>Сила пола хранится в подклассах генома — достаём под конкретный пол.</summary>
        private static float GetSexStrength(ICatGenome genome)
        {
            switch (genome)
            {
                case CatGenomeMale male: return male.MaleStrength;
                case CatGenomeFemale female: return female.FemaleStrength;
                default: return 0f;
            }
        }
    }
}
