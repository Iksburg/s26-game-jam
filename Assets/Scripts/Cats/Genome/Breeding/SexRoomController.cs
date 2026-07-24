using System;
using System.Collections;
using System.Collections.Generic;
using Cats.Genome.Abstract;
using CatWorld.Cats;
using Cats.Spawning;
using UnityEngine;
using UnityEngine.Serialization;
using Random = Unity.Mathematics.Random;

namespace Cats.Genome.Breeding
{
    /// <summary>
    /// Контроллер комнаты для разведения котов
    /// </summary>
    /// <remarks>Контроллер секса</remarks>
    public class SexRoomController : MonoBehaviour
    {
        [FormerlySerializedAs("_spawnCatPanel")]
        [Header("Ссылки на компоненты")]
        [SerializeField] private SpawnCatPanel spawnCatPanel;
        [FormerlySerializedAs("_catSpawner")] [SerializeField] private CatSpawner catSpawner; // Ссылка на спавнер для корректной настройки визуала и ИИ
        
        [FormerlySerializedAs("_breedingDuration")]
        [Header("Настройки времени")]
        [Tooltip("Время в секундах, через которое родится котёнок после сбора пары.")]
        [SerializeField] private float breedingDuration = 5f;

        // Храним ссылки на самих котов, так как они физически находятся в комнате
        private Cat _parentMale;
        private Cat _parentFemale;

        // Ссылка на текущий запущенный таймер
        private Coroutine _breedingCoroutine;

        // Метод CreateNewChild больше не нужен, так как вся логика инкапсулирована в корутине и колбэке панели

        public void AddCat(Cat cat)
        {
            if (!cat) throw new ArgumentNullException(nameof(cat));

            // Размножаться могут только взрослые коты: флаг CanBreed выставляет
            // CatAgeController по настройкам стадии (у котят и пожилых он выключен).
            if (!cat.CanBreed)
            {
                Debug.Log($"[SexRoom] Кот «{cat.Name}» ({cat.Stage}) не может размножаться — не добавлен в комнату.");
                return;
            }

            // Проверяем пол кота через его геном
            switch (cat.Genome)
            {
                case CatGenomeMale:
                    _parentMale = cat;
                    Debug.Log($"Кот добавлен в комнату секса как мужчина [{cat.Id}]");
                    break;
                case CatGenomeFemale:
                    _parentFemale = cat;
                    Debug.Log($"Кот добавлен в комнату секса как женщина [{cat.Id}]");
                    break;
                default:
                    throw new GenomeException($"Невозможно добавить кота в комнату - неизвестный пол в геноме");
            }

            // Проверяем, собралась ли пара, чтобы запустить таймер
            TryStartBreeding();
        }

        private void TryStartBreeding()
        {
            // Если оба родителя на месте и таймер еще не запущен — начинаем процесс
            if (!_parentMale || !_parentFemale || _breedingCoroutine != null) return;
            
            Debug.Log($"[SexRoom] Пара собрана! Процесс разведения запущен. Котёнок появится через {breedingDuration} сек.");
            _breedingCoroutine = StartCoroutine(BreedingTimerRoutine());
        }

        private IEnumerator BreedingTimerRoutine()
        {
            yield return new WaitForSeconds(breedingDuration);
            if (!catSpawner)
            {
                Debug.LogError("[SexRoom] Невозможно создать котенка: Ссылка на CatSpawner не задана в инспекторе!");
                ClearRoom();
                yield break;
            }

            // 1. Заранее рассчитываем пол и гены ребенка через бизнес-логику (до открытия UI)
            var fatherGenome = (CatGenomeMale)_parentMale.Genome;
            var motherGenome = (CatGenomeFemale)_parentFemale.Genome;
            var preCalculatedSex = CatGenome.CalculateChildSex(fatherGenome.MaleStrength, motherGenome.FemaleStrength);

            // 2. Открываем панель именования, скрывая выбор пола, и передаем Callback
            spawnCatPanel.OpenForBreeding(preCalculatedSex, (chosenName) =>
            {
                // Генерируем геном уже с финальным именем, которое ввел игрок
                var childGenome = CatBreedingService.Breed(chosenName, fatherGenome, motherGenome);
             
                // Создаем котенка
                var child = catSpawner.SpawnChildCat(chosenName, childGenome);
             
                // Наследуем черты от родителей
                InheritTraits(child, _parentMale, _parentFemale);
             
                ClearRoom();
            });
            _breedingCoroutine = null;
        }
        
        /// <summary>
        /// Рассчитывает и применяет унаследованные черты от родителей к котенку.
        /// Каждая черта родителя имеет шанс быть унаследованной.
        /// </summary>
        private void InheritTraits(Cat child, Cat father, Cat mother)
        {
            var inheritedTraits = new HashSet<CatTrait>();
         
            // Шанс наследования каждой черты (можно вынести в константу или настройку)
            const float inheritChance = 0.5f;
         
            // Проверяем все врожденные черты отца
            foreach (var trait in father.InnateTraits)
            {
                if (UnityEngine.Random.value < inheritChance && !inheritedTraits.Contains(trait))
                {
                    inheritedTraits.Add(trait);
                }
            }
         
            // Проверяем все врожденные черты матери
            foreach (var trait in mother.InnateTraits)
            {
                if (UnityEngine.Random.value < inheritChance && !inheritedTraits.Contains(trait))
                {
                    inheritedTraits.Add(trait);
                }
            }
         
            // Применяем унаследованные черты к котенку (с учетом лимита MaxInnateTraits)
            int appliedCount = 0;
            foreach (var trait in inheritedTraits)
            {
                if (appliedCount >= Cat.MaxInnateTraits)
                    break;
                 
                if (child.TryAddInnateTrait(trait))
                {
                    appliedCount++;
                    Debug.Log($"[Наследование] Котенок {child.Name} унаследовал черту: {trait}");
                }
            }
         
            Debug.Log($"[Наследование] Всего унаследовано черт: {appliedCount} из {inheritedTraits.Count} возможных");
        }

        /// <summary> Сбрасывает состояние комнаты и подготавливает её к новой паре </summary>
        private void ClearRoom()
        {
            // Если процесс шёл, но комнату принудительно очистили — останавливаем таймер
            if (_breedingCoroutine != null)
            {
                StopCoroutine(_breedingCoroutine);
                _breedingCoroutine = null;
                Debug.Log("[SexRoom] Процесс разведения прерван.");
            }

            _parentMale = null;
            _parentFemale = null;
            Debug.Log("[SexRoom] Комната разведения пуста и готова к новым котам.");
        }
    }
}
