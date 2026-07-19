using System.Collections.Generic;
using UnityEngine;

namespace Cats.Spawning
{
    /// <summary>
    /// Заданный список цветов шерсти, из которого случайно выбирается окрас
    /// нового кота. Редактируется в инспекторе ассета (Assets/Data/CatColorPalette).
    /// </summary>
    [CreateAssetMenu(fileName = "CatColorPalette", menuName = "CatWorld/Cat Color Palette")]
    public class CatColorPalette : ScriptableObject
    {
        [SerializeField] private List<Color> _colors = new List<Color>
        {
            new Color(1.00f, 1.00f, 1.00f), // белый
            new Color(0.15f, 0.15f, 0.15f), // чёрный
            new Color(0.91f, 0.52f, 0.23f), // рыжий
            new Color(0.55f, 0.55f, 0.55f), // серый
            new Color(0.95f, 0.89f, 0.77f), // кремовый
            new Color(0.42f, 0.26f, 0.15f), // коричневый
            new Color(0.36f, 0.36f, 0.43f), // дымчатый
            new Color(0.85f, 0.71f, 0.56f)  // бежевый
        };

        public IReadOnlyList<Color> Colors => _colors;

        /// <summary>Возвращает случайный цвет из списка (белый, если список пуст).</summary>
        public Color GetRandomColor()
        {
            if (_colors == null || _colors.Count == 0)
                return Color.white;
            return _colors[Random.Range(0, _colors.Count)];
        }
    }
}
