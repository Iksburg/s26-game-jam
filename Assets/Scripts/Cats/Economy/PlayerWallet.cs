using System;
using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// Кошелёк игрока: баланс мяукоинов (MC). Все траты идут через TrySpend,
    /// который не даёт уйти в минус. UI подписывается на Changed.
    /// </summary>
    public class PlayerWallet : MonoBehaviour
    {
        [SerializeField] private EconomySettings _settings;
        [SerializeField, Min(0)] private int _coins;

        private bool _initialized;

        /// <summary>Вызывается при любом изменении баланса.</summary>
        public event Action Changed;

        public int Coins => _coins;

        private void Awake()
        {
            // Стартовый баланс ставится только для новой игры; загрузка сейва
            // перезапишет его через RestoreCoins.
            if (!_initialized)
            {
                _coins = _settings != null ? _settings.StartingCoins : 0;
                _initialized = true;
            }
        }

        /// <summary>Заполняется билдером сцены (editor wiring).</summary>
        public void Configure(EconomySettings settings)
        {
            _settings = settings;
        }

        /// <summary>Хватает ли денег на покупку.</summary>
        public bool CanAfford(int price) => price >= 0 && _coins >= price;

        /// <summary>Списывает сумму. false — не хватает мяукоинов.</summary>
        public bool TrySpend(int price)
        {
            if (!CanAfford(price))
            {
                Debug.Log($"[Wallet] Недостаточно мяукоинов: нужно {price}, есть {_coins}.");
                return false;
            }

            _coins -= price;
            Changed?.Invoke();
            Debug.Log($"[Wallet] Потрачено {price} MC, остаток {_coins} MC.");
            return true;
        }

        /// <summary>Начисляет доход (например, за продажу кота).</summary>
        public void Add(int amount)
        {
            if (amount <= 0)
                return;

            _coins += amount;
            Changed?.Invoke();
            Debug.Log($"[Wallet] Получено {amount} MC, баланс {_coins} MC.");
        }

        /// <summary>Восстанавливает баланс из сохранения.</summary>
        public void RestoreCoins(int coins)
        {
            _initialized = true;
            _coins = Mathf.Max(0, coins);
            Changed?.Invoke();
        }
    }
}
