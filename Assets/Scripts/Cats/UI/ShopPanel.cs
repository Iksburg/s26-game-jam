using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CatWorld.Cats
{
    /// <summary>
    /// Окно магазина: вкладка «Покупка» с карточками котёнка, корма и воды
    /// и вкладка «Продажа» со списком котов на ферме. Все операции идут через
    /// EconomyService — кошелёк и запасы напрямую не трогаем.
    /// </summary>
    public class ShopPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;

        [Header("Вкладки")]
        [SerializeField] private Button _buyTabButton;
        [SerializeField] private Button _sellTabButton;
        [SerializeField] private GameObject _buyTab;
        [SerializeField] private GameObject _sellTab;

        [Header("Карточки покупки")]
        [SerializeField] private Button _buyCatButton;
        [SerializeField] private Button _buyFoodButton;
        [SerializeField] private Button _buyWaterButton;
        [SerializeField] private Text _catPriceLabel;
        [SerializeField] private Text _foodPriceLabel;
        [SerializeField] private Text _waterPriceLabel;

        [Header("Спрайты товаров (назначаются в инспекторе)")]
        [SerializeField] private Image _catIcon;
        [SerializeField] private Image _foodIcon;
        [SerializeField] private Image _waterIcon;

        [Header("Список продажи")]
        [SerializeField] private RectTransform _sellContent;
        [SerializeField] private ShopCatCard _catCardTemplate;
        [SerializeField] private Text _sellEmptyLabel;

        [Header("Прочее")]
        [SerializeField] private Text _coinsLabel;
        [SerializeField] private Text _statusLabel;
        [SerializeField] private EconomyService _economy;
        [SerializeField] private SpawnCatPanel _spawnCatPanel;

        private readonly List<ShopCatCard> _spawnedCards = new List<ShopCatCard>();

        private void Awake()
        {
            // Компонент висит на активном Canvas, окно сохранено выключенным,
            // поэтому SetActive(false) здесь не нужен.
            _openButton.onClick.AddListener(Open);
            _closeButton.onClick.AddListener(Close);
            _buyTabButton.onClick.AddListener(ShowBuyTab);
            _sellTabButton.onClick.AddListener(ShowSellTab);

            _buyCatButton.onClick.AddListener(BuyCat);
            _buyFoodButton.onClick.AddListener(BuyFood);
            _buyWaterButton.onClick.AddListener(BuyWater);
        }

        private void Start()
        {
            if (_economy == null)
                _economy = FindFirstObjectByType<EconomyService>();
            if (_spawnCatPanel == null)
                _spawnCatPanel = FindFirstObjectByType<SpawnCatPanel>();

            if (_economy != null && _economy.Wallet != null)
                _economy.Wallet.Changed += RefreshWallet;

            if (_catCardTemplate != null)
                _catCardTemplate.gameObject.SetActive(false);

            RefreshPrices();
        }

        private void OnDestroy()
        {
            if (_economy != null && _economy.Wallet != null)
                _economy.Wallet.Changed -= RefreshWallet;
        }

        /// <summary>Заполняется билдером сцены (editor wiring).</summary>
        public void Configure(GameObject root, Button openButton, Button closeButton,
            Button buyTabButton, Button sellTabButton, GameObject buyTab, GameObject sellTab,
            Button buyCatButton, Button buyFoodButton, Button buyWaterButton,
            Text catPriceLabel, Text foodPriceLabel, Text waterPriceLabel,
            Image catIcon, Image foodIcon, Image waterIcon,
            RectTransform sellContent, ShopCatCard catCardTemplate, Text sellEmptyLabel,
            Text coinsLabel, Text statusLabel, EconomyService economy, SpawnCatPanel spawnCatPanel)
        {
            _root = root;
            _openButton = openButton;
            _closeButton = closeButton;
            _buyTabButton = buyTabButton;
            _sellTabButton = sellTabButton;
            _buyTab = buyTab;
            _sellTab = sellTab;
            _buyCatButton = buyCatButton;
            _buyFoodButton = buyFoodButton;
            _buyWaterButton = buyWaterButton;
            _catPriceLabel = catPriceLabel;
            _foodPriceLabel = foodPriceLabel;
            _waterPriceLabel = waterPriceLabel;
            _catIcon = catIcon;
            _foodIcon = foodIcon;
            _waterIcon = waterIcon;
            _sellContent = sellContent;
            _catCardTemplate = catCardTemplate;
            _sellEmptyLabel = sellEmptyLabel;
            _coinsLabel = coinsLabel;
            _statusLabel = statusLabel;
            _economy = economy;
            _spawnCatPanel = spawnCatPanel;
        }

        public void Open()
        {
            SetStatus(string.Empty);
            _root.SetActive(true);
            RefreshPrices();
            RefreshWallet();
            ShowBuyTab(); // по умолчанию открыта вкладка покупки
        }

        public void Close()
        {
            _root.SetActive(false);
        }

        // ---------- Вкладки ----------

        private void ShowBuyTab()
        {
            _buyTab.SetActive(true);
            _sellTab.SetActive(false);
            RefreshTabHighlight(buyActive: true);
        }

        private void ShowSellTab()
        {
            _buyTab.SetActive(false);
            _sellTab.SetActive(true);
            RefreshTabHighlight(buyActive: false);
            RebuildSellList();
        }

        /// <summary>Активная вкладка подсвечивается, её кнопка недоступна для нажатия.</summary>
        private void RefreshTabHighlight(bool buyActive)
        {
            _buyTabButton.interactable = !buyActive;
            _sellTabButton.interactable = buyActive;
        }

        // ---------- Покупка ----------

        private void BuyCat()
        {
            if (_economy == null)
                return;

            // Деньги проверяем до окна имени, чтобы не звать игрока зря.
            if (!_economy.CanBuyCat())
            {
                SetStatus("Недостаточно мяукоинов");
                return;
            }

            if (_spawnCatPanel == null)
            {
                SetStatus("Не удалось открыть окно выбора имени");
                return;
            }

            // Имя и пол выбирает игрок; списание — после подтверждения.
            _spawnCatPanel.OpenForShopPurchase((sex, catName) =>
            {
                var cat = _economy.BuyCat(sex, catName);
                SetStatus(cat != null ? $"Куплен котёнок «{catName}»" : "Недостаточно мяукоинов");
                RefreshWallet();
            });
        }

        private void BuyFood()
        {
            if (_economy == null)
                return;
            SetStatus(_economy.BuyFood() ? "Корм куплен" : "Недостаточно мяукоинов");
        }

        private void BuyWater()
        {
            if (_economy == null)
                return;
            SetStatus(_economy.BuyWater() ? "Вода куплена" : "Недостаточно мяукоинов");
        }

        // ---------- Продажа ----------

        /// <summary>Пересобирает список котов на ферме.</summary>
        private void RebuildSellList()
        {
            if (_sellContent == null || _catCardTemplate == null)
                return;

            foreach (var card in _spawnedCards)
            {
                if (card != null)
                    Destroy(card.gameObject);
            }
            _spawnedCards.Clear();

            var cats = FindObjectsByType<Cat>(FindObjectsSortMode.None);
            foreach (var cat in cats)
            {
                var card = Instantiate(_catCardTemplate, _sellContent);
                card.gameObject.SetActive(true);
                card.Bind(cat, _economy != null ? _economy.GetSellPrice(cat) : 0, SellCat);
                _spawnedCards.Add(card);
            }

            if (_sellEmptyLabel != null)
                _sellEmptyLabel.gameObject.SetActive(cats.Length == 0);
        }

        private void SellCat(Cat cat)
        {
            if (_economy == null || cat == null)
                return;

            string catName = cat.Name;
            int price = _economy.SellCat(cat);
            SetStatus(price > 0
                ? $"«{catName}» продан за {price} MC"
                : $"«{catName}» отдан новым хозяевам");

            RefreshWallet();
            RebuildSellList(); // кот удалён — список надо пересобрать
        }

        // ---------- Обновление подписей ----------

        private void RefreshPrices()
        {
            if (_economy == null || _economy.Settings == null)
                return;

            var settings = _economy.Settings;
            if (_catPriceLabel != null)
                _catPriceLabel.text = $"{settings.CatPrice} MC";
            if (_foodPriceLabel != null)
                _foodPriceLabel.text = $"{settings.FoodPrice} MC";
            if (_waterPriceLabel != null)
                _waterPriceLabel.text = $"{settings.WaterPrice} MC";
        }

        private void RefreshWallet()
        {
            if (_coinsLabel != null && _economy != null && _economy.Wallet != null)
                _coinsLabel.text = $"Мяукоины: {_economy.Wallet.Coins} MC";
        }

        private void SetStatus(string message)
        {
            if (_statusLabel != null)
                _statusLabel.text = message;
        }
    }
}
