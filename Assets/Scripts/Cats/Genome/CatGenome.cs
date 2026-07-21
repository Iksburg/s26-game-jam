using System;
using System.Collections.Generic;
using Cats.Genome.Abstract;
using CatWorld.Cats;
using UnityEngine;

namespace Cats.Genome
{
    public abstract class CatGenome : ICatGenome
    {
        public string Id { get; }
        public string Name { get; }
        public Sex Sex { get; }
        public Color Color { get; }
        
        // Инкапсуляция: списки закрыты на чтение извне через IReadOnlyList
        private readonly List<ICatGenome> _children = new();
        private readonly List<ICatGenome> _parents = new();

        public IReadOnlyList<ICatGenome> Parents => _parents;
        public IReadOnlyList<ICatGenome> Children => _children;

        // Конструктор для создания котёнка от родителей
        protected CatGenome(string name, Sex sex, CatGenomeMale father, CatGenomeFemale mother, float fatherColorStrength, float motherColorStrength)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            Sex = sex;
            _parents.Add(father);
            _parents.Add(mother);

            Color = CalculateChildColor(father.Color, fatherColorStrength, mother.Color, motherColorStrength);

            father.AddChild(this);
            mother.AddChild(this);
        }

        // Конструктор для "базовых" котов (генерация на старте игры)
        protected CatGenome(string name, Sex sex, Color color)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            Sex = sex;
            Color = color;
        }

        /// <summary>
        /// Конструктор восстановления из сохранения: Id и цвет берутся из сейва,
        /// а не генерируются, иначе после загрузки развалятся связи родословной.
        /// Родители подключаются отдельно через RestoreParent — на момент создания
        /// генома они могут быть ещё не восстановлены.
        /// </summary>
        protected CatGenome(string id, string name, Sex sex, Color color)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Пустой Id при восстановлении генома", nameof(id));
            Id = id;
            Name = name;
            Sex = sex;
            Color = color;
        }

        /// <summary>
        /// Восстанавливает связь с родителем при загрузке сейва (без обратного
        /// AddChild — список детей восстанавливается отдельно по своим Id).
        /// </summary>
        public void RestoreParent(ICatGenome parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (!_parents.Contains(parent))
            {
                _parents.Add(parent);
            }
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
