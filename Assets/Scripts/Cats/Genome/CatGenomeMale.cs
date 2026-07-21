using CatWorld.Cats;
using UnityEngine;

namespace Cats.Genome
{
    public class CatGenomeMale : CatGenome
    {
        public float MaleStrength { get; }

        // Конструктор для котёнка-мальчика
        public CatGenomeMale(string name, CatGenomeMale father, CatGenomeFemale mother, float maleStrength) 
            : base(name, Sex.Male, father, mother, father.MaleStrength, mother.FemaleStrength)
        {
            MaleStrength = maleStrength; // Можем передать мутировавшую силу или среднее родителей
        }

        // Конструктор для начального кота-самца
        public CatGenomeMale(string name, Color color, float maleStrength) : base(name, Sex.Male, color)
        {
            MaleStrength = maleStrength;
        }

        // Конструктор восстановления из сохранения (Id берётся из сейва)
        public CatGenomeMale(string id, string name, Color color, float maleStrength)
            : base(id, name, Sex.Male, color)
        {
            MaleStrength = maleStrength;
        }
    }
}