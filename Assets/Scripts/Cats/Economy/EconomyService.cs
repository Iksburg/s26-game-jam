using Cats.Spawning;
using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// Операции экономики: покупка котов, корма и воды, продажа котов.
    /// Единая точка входа для будущего UI магазина — он должен вызывать
    /// только эти методы, не трогая кошелёк и запасы напрямую.
    /// </summary>
    public class EconomyService : MonoBehaviour
    {
        [SerializeField] private EconomySettings _settings;
        [SerializeField] private PlayerWallet _wallet;
        [SerializeField] private FarmResources _resources;
        [SerializeField] private CatSpawner _spawner;

        public EconomySettings Settings => _settings;
        public PlayerWallet Wallet => _wallet;

        private void Awake()
        {
            if (_wallet == null)
                _wallet = FindFirstObjectByType<PlayerWallet>();
            if (_resources == null)
                _resources = FindFirstObjectByType<FarmResources>();
            if (_spawner == null)
                _spawner = FindFirstObjectByType<CatSpawner>();
        }

        /// <summary>Заполняется билдером сцены (editor wiring).</summary>
        public void Configure(EconomySettings settings, PlayerWallet wallet,
            FarmResources resources, CatSpawner spawner)
        {
            _settings = settings;
            _wallet = wallet;
            _resources = resources;
            _spawner = spawner;
        }

        // ---------- Покупка ----------

        /// <summary>Хватает ли денег на кота.</summary>
        public bool CanBuyCat() => _wallet != null && _settings != null && _wallet.CanAfford(_settings.CatPrice);

        /// <summary>
        /// Покупает кота: списывает цену и создаёт котёнка со случайным цветом.
        /// Возвращает null, если не хватило мяукоинов.
        /// </summary>
        public Cat BuyCat(Sex sex, string catName)
        {
            if (_settings == null || _wallet == null || _spawner == null)
            {
                Debug.LogError("[Economy] Покупка кота невозможна: сервис не настроен.");
                return null;
            }

            if (!_wallet.TrySpend(_settings.CatPrice))
                return null;

            var cat = _spawner.SpawnPurchasedCat(sex, catName);
            Debug.Log($"[Economy] Куплен котёнок «{catName}» за {_settings.CatPrice} MC.");
            return cat;
        }

        public bool CanBuyFood() => _wallet != null && _settings != null && _wallet.CanAfford(_settings.FoodPrice);

        /// <summary>Покупает порцию корма. false — не хватило мяукоинов.</summary>
        public bool BuyFood()
        {
            if (_settings == null || _wallet == null || _resources == null)
                return false;
            if (!_wallet.TrySpend(_settings.FoodPrice))
                return false;

            _resources.AddFood(_settings.FoodAmountPerPurchase);
            Debug.Log($"[Economy] Куплен корм: +{_settings.FoodAmountPerPurchase} " +
                      $"за {_settings.FoodPrice} MC (всего {_resources.Food}).");
            return true;
        }

        public bool CanBuyWater() => _wallet != null && _settings != null && _wallet.CanAfford(_settings.WaterPrice);

        /// <summary>Покупает порцию воды. false — не хватило мяукоинов.</summary>
        public bool BuyWater()
        {
            if (_settings == null || _wallet == null || _resources == null)
                return false;
            if (!_wallet.TrySpend(_settings.WaterPrice))
                return false;

            _resources.AddWater(_settings.WaterAmountPerPurchase);
            Debug.Log($"[Economy] Куплена вода: +{_settings.WaterAmountPerPurchase} " +
                      $"за {_settings.WaterPrice} MC (всего {_resources.Water}).");
            return true;
        }

        // ---------- Продажа ----------

        /// <summary>Сколько дадут за кота на его текущей стадии.</summary>
        public int GetSellPrice(Cat cat)
        {
            if (cat == null || _settings == null)
                return 0;
            return _settings.GetSellPrice(cat.Stage);
        }

        /// <summary>
        /// Продаёт кота: начисляет мяукоины по стадии и убирает его с фермы.
        /// Пожилой уходит бесплатно (0 MC) — по концепту это «отдать новым хозяевам».
        /// Возвращает начисленную сумму.
        /// </summary>
        public int SellCat(Cat cat)
        {
            if (cat == null || _wallet == null || _settings == null)
                return 0;

            int price = _settings.GetSellPrice(cat.Stage);
            _wallet.Add(price);

            // Статус меняем до удаления: родословная должна помнить, что кот
            // ушёл в новую семью, а не просто исчез.
            cat.SetFarmStatus(FarmStatus.InNewFamily);
            Debug.Log($"[Economy] Продан кот «{cat.Name}» ({cat.Stage}) за {price} MC.");

            Destroy(cat.gameObject);
            return price;
        }
    }
}
