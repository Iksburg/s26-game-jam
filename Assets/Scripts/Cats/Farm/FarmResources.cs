using System;
using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// Запасы корма и воды на ферме. Дефолт 100/100, редактируется в инспекторе.
    /// Коты расходуют по 1 за приём; UI подписывается на Changed.
    /// Пополнение (покупка за мяукоины) — задача экономики.
    /// </summary>
    public class FarmResources : MonoBehaviour
    {
        [SerializeField, Min(0)] private int _food = 100;
        [SerializeField, Min(0)] private int _water = 100;

        /// <summary>Вызывается при любом изменении запасов.</summary>
        public event Action Changed;

        public int Food => _food;
        public int Water => _water;

        /// <summary>Списывает 1 корм. false — корм закончился.</summary>
        public bool TryConsumeFood()
        {
            if (_food <= 0)
                return false;
            _food--;
            Debug.Log($"[FarmResources] Корм съеден, остаток: {_food}");
            Changed?.Invoke();
            return true;
        }

        /// <summary>Списывает 1 воду. false — вода закончилась.</summary>
        public bool TryConsumeWater()
        {
            if (_water <= 0)
                return false;
            _water--;
            Debug.Log($"[FarmResources] Вода выпита, остаток: {_water}");
            Changed?.Invoke();
            return true;
        }
    }
}
