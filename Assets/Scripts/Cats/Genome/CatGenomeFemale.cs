using CatWorld.Cats;
using UnityEngine;

namespace Cats.Genome
{
    public class CatGenomeFemale : CatGenome
    {
        public float FemaleStrength { get; }

        // Конструктор для котёнка-девочки
        public CatGenomeFemale(string name, CatGenomeMale father, CatGenomeFemale mother, float femaleStrength) 
            : base(name, Sex.Female, father, mother, father.MaleStrength, mother.FemaleStrength)
        {
            FemaleStrength = femaleStrength;
        }

        // Конструктор для начальной кошки-самки
        public CatGenomeFemale(string name, Color color, float femaleStrength) : base(name, Sex.Female, color)
        {
            FemaleStrength = femaleStrength;
        }
    }
}