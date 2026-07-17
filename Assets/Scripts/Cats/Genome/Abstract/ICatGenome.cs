using System.Collections.Generic;
using CatWorld.Cats;
using UnityEngine; // Оставляем только для структуры Color

namespace Cats.Genome.Abstract
{
    public interface ICatGenome
    {
        public string Id { get; }
        public string Name { get; }
        Sex Sex { get; }
        Color Color { get; }
        IReadOnlyList<ICatGenome> Parents { get; }
        IReadOnlyList<ICatGenome> Children { get; }
        
        void AddChild(ICatGenome child);
    }
}