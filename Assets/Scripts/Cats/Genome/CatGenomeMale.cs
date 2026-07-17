using CatWorld.Cats;
using UnityEngine;

namespace Cats.Genome
{
    public class CatGenomeMale : CatGenome
    {
        public float MaleStrength { get; }

        // Конструктор для котёнка-мальчика
        public CatGenomeMale(CatGenomeMale father, CatGenomeFemale mother, float maleStrength) 
            : base(Sex.Male, father, mother, father.MaleStrength, mother.FemaleStrength)
        {
            MaleStrength = maleStrength; // Можем передать мутировавшую силу или среднее родителей
        }

        // Конструктор для начального кота-самца
        public CatGenomeMale(Color color, float maleStrength) : base(Sex.Male, color)
        {
            MaleStrength = maleStrength;
        }
    }
}