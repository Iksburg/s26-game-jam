using System;
using Cats.Genome.Abstract;
using CatWorld.Cats;

namespace Cats.Genome.Breeding
{
    public static class CatBreedingService
    {
        public static ICatGenome Breed(CatGenomeMale father, CatGenomeFemale mother)
        {
            // 1. Рассчитываем пол будущего ребенка на основе сил родителей
            var childSex = CatGenome.CalculateChildSex(father.MaleStrength, mother.FemaleStrength);
            
            // 2. Рассчитываем "силу пола" для нового поколения (например, берем среднее + небольшой рандом)
            var inheritedStrength = (father.MaleStrength + mother.FemaleStrength) / 2f; 

            // 3. Возвращаем строго типизированный C# объект через стандартный new
            return childSex switch
            {
                Sex.Male => new CatGenomeMale(father, mother, inheritedStrength),
                Sex.Female => new CatGenomeFemale(father, mother, inheritedStrength),
                _ => throw new ArgumentOutOfRangeException(nameof(childSex), "Неизвестный пол котенка")
            };
        }
    }
}