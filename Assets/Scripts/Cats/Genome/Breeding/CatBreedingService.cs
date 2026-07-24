using System;
using System.Collections.Generic;
using System.Linq;
using Cats.Genome.Abstract;
using CatWorld.Cats;
using UnityEngine;

namespace Cats.Genome.Breeding
{
    public static class CatBreedingService
    {
        private const float InheritTraitChance = 0.5f; // 50% шанс унаследовать каждую черту

        public static ICatGenome Breed(string childName, CatGenomeMale father, CatGenomeFemale mother)
        {
            var childSex = CatGenome.CalculateChildSex(father.MaleStrength, mother.FemaleStrength);
            var inheritedStrength = (father.MaleStrength + mother.FemaleStrength) / 2f;

            return childSex switch
            {
                Sex.Male => new CatGenomeMale(childName, father, mother, inheritedStrength),
                Sex.Female => new CatGenomeFemale(childName, father, mother, inheritedStrength),
                _ => throw new ArgumentOutOfRangeException(nameof(childSex), "Неизвестный пол котенка")
            };
        }

        /// <summary>
        /// Рассчитывает врождённые черты котёнка на основе черт родителей.
        /// Каждая черта родителя имеет шанс быть унаследованной.
        /// </summary>
        public static List<CatTrait> CalculateInheritedTraits(
            IReadOnlyList<CatTrait> fatherTraits, 
            IReadOnlyList<CatTrait> motherTraits)
        {
            var inheritedTraits = new HashSet<CatTrait>();

            // Наследуем черты отца
            foreach (var trait in fatherTraits)
            {
                if (UnityEngine.Random.value < InheritTraitChance)
                {
                    inheritedTraits.Add(trait);
                }
            }

            // Наследуем черты матери
            foreach (var trait in motherTraits)
            {
                if (UnityEngine.Random.value < InheritTraitChance)
                {
                    inheritedTraits.Add(trait);
                }
            }

            // Ограничиваем максимальным количеством врождённых черт (3)
            return inheritedTraits.Take(Cat.MaxInnateTraits).ToList();
        }
    }
}