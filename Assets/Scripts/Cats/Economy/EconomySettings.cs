using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// Цены и доходы экономики в мяукоинах (MeowCoin/MC).
    /// Значения из постановки задачи; правятся в ассете без изменения кода.
    /// Внимание: в концепте покупка кота стоит 120 MC и приходит взрослый,
    /// здесь — 200 MC и котёнок (актуальная версия по задаче).
    /// </summary>
    [CreateAssetMenu(fileName = "EconomySettings", menuName = "CatWorld/Economy Settings")]
    public class EconomySettings : ScriptableObject
    {
        [Header("Стартовый баланс")]
        [SerializeField, Min(0)] private int _startingCoins = 200;

        [Header("Покупка")]
        [Tooltip("Цена нового кота. Приходит котёнок со случайным цветом из палитры.")]
        [SerializeField, Min(0)] private int _catPrice = 200;
        [Tooltip("Цена одной покупки корма.")]
        [SerializeField, Min(0)] private int _foodPrice = 10;
        [Tooltip("Сколько единиц корма даёт одна покупка.")]
        [SerializeField, Min(1)] private int _foodAmountPerPurchase = 10;
        [Tooltip("Цена одной покупки воды.")]
        [SerializeField, Min(0)] private int _waterPrice = 5;
        [Tooltip("Сколько единиц воды даёт одна покупка.")]
        [SerializeField, Min(1)] private int _waterAmountPerPurchase = 10;

        [Header("Продажа котов")]
        [SerializeField, Min(0)] private int _kittenSellPrice = 150;
        [SerializeField, Min(0)] private int _adultSellPrice = 100;
        [Tooltip("Пожилых котов отдают новым хозяевам бесплатно (по концепту).")]
        [SerializeField, Min(0)] private int _seniorSellPrice;

        public int StartingCoins => _startingCoins;
        public int CatPrice => _catPrice;
        public int FoodPrice => _foodPrice;
        public int FoodAmountPerPurchase => _foodAmountPerPurchase;
        public int WaterPrice => _waterPrice;
        public int WaterAmountPerPurchase => _waterAmountPerPurchase;

        /// <summary>Сколько мяукоинов принесёт продажа кота на данной стадии.</summary>
        public int GetSellPrice(LifeStage stage)
        {
            switch (stage)
            {
                case LifeStage.Kitten: return _kittenSellPrice;
                case LifeStage.Adult: return _adultSellPrice;
                default: return _seniorSellPrice;
            }
        }
    }
}
