using System;
using System.Collections.Generic;
using Cats.Genome.Abstract;
using CatWorld.Cats;
using UnityEngine;

namespace Cats.Genome
{
    public abstract class CatGenome : ICatGenome
    {
        public Sex Sex { get; }
        public Color Color { get; }
        
        // Инкапсуляция: списки закрыты на чтение извне через IReadOnlyList
        private readonly List<ICatGenome> _children = new();
        private readonly ICatGenome[] _parents;

        public IReadOnlyList<ICatGenome> Parents => _parents;
        public IReadOnlyList<ICatGenome> Children => _children;

        // Конструктор для создания котёнка от родителей
        protected CatGenome(Sex sex, CatGenomeMale father, CatGenomeFemale mother, float fatherColorStrength, float motherColorStrength)
        {
            Sex = sex;
            _parents = new ICatGenome[] { father, mother };
            
            // Бизнес-логика: цвет рассчитывается автоматически при рождении
            Color = CalculateChildColor(father.Color, fatherColorStrength, mother.Color, motherColorStrength);
            
            // Сразу регистрируем связь у родителей
            father.AddChild(this);
            mother.AddChild(this);
        }

        // Конструктор для "базовых" котов (генерация на старте игры)
        protected CatGenome(Sex sex, Color color)
        {
            Sex = sex;
            Color = color;
            _parents = Array.Empty<ICatGenome>();
        }

        public void AddChild(ICatGenome child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            if (!_children.Contains(child))
            {
                _children.Add(child);
            }
        }

        #region Генетические утилиты (Чистая математика)

        public static Sex CalculateChildSex(float maleStrength, float femaleStrength)
        {
            var sum = maleStrength + femaleStrength;

            if (sum <= 0)
            {
                return UnityEngine.Random.Range(0, 2) == 0 ? Sex.Male : Sex.Female;
            }

            var randomValue = UnityEngine.Random.Range(0f, sum);
            return randomValue < maleStrength ? Sex.Male : Sex.Female;
        }
        
        private static Color CalculateChildColor(Color fatherColor, float fatherStrength, Color motherColor, float motherStrength)
        {
            var sum = fatherStrength + motherStrength;

            if (sum <= 0)
            {
                return Color.Lerp(fatherColor, motherColor, 0.5f);
            }

            var t = motherStrength / sum;
            return Color.Lerp(fatherColor, motherColor, t);
        }

        #endregion
    }
}
