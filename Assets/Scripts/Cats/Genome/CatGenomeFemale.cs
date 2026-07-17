using CatWorld.Cats;
using UnityEngine;

namespace Cats.Genome
{
    public class CatGenomeFemale : CatGenome
    {
        public float FemaleStrength { get; }

        // Конструктор для котёнка-девочки
        public CatGenomeFemale(CatGenomeMale father, CatGenomeFemale mother, float femaleStrength) 
            : base(Sex.Female, father, mother, father.MaleStrength, mother.FemaleStrength)
        {
            FemaleStrength = femaleStrength;
        }

        // Конструктор для начальной кошки-самки
        public CatGenomeFemale(Color color, float femaleStrength) : base(Sex.Female, color)
        {
            FemaleStrength = femaleStrength;
        }
    }
}