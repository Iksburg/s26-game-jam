using System;
using UnityEngine;
using UnityEngine.UI;

namespace CatWorld.Cats
{
    /// <summary>
    /// Карточка кота во вкладке продажи: спрайт кота (берётся автоматически
    /// из его визуала и тонируется цветом шерсти), имя и цена продажи.
    /// Создаётся клонированием шаблона из сцены.
    /// </summary>
    public class ShopCatCard : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Text _nameLabel;
        [SerializeField] private Text _priceLabel;
        [SerializeField] private Button _sellButton;

        private Cat _cat;
        private Action<Cat> _onSell;

        /// <summary>Заполняется билдером сцены (editor wiring).</summary>
        public void Configure(Image icon, Text nameLabel, Text priceLabel, Button sellButton)
        {
            _icon = icon;
            _nameLabel = nameLabel;
            _priceLabel = priceLabel;
            _sellButton = sellButton;
        }

        /// <summary>Наполняет карточку данными конкретного кота.</summary>
        public void Bind(Cat cat, int price, Action<Cat> onSell)
        {
            _cat = cat;
            _onSell = onSell;

            if (_nameLabel != null)
                _nameLabel.text = cat.Name;
            if (_priceLabel != null)
                _priceLabel.text = $"{price} MC";

            if (_icon != null)
            {
                var view = cat.GetComponent<CatView>();
                if (view != null)
                    _icon.sprite = view.CurrentSprite;
                _icon.color = cat.FurColor; // карточка в цвете конкретного кота
            }

            if (_sellButton != null)
            {
                // Карточки переиспользуются между обновлениями списка —
                // старые подписки надо снимать, иначе продадим не того кота.
                _sellButton.onClick.RemoveAllListeners();
                _sellButton.onClick.AddListener(HandleSell);
            }
        }

        private void HandleSell()
        {
            if (_cat != null)
                _onSell?.Invoke(_cat);
        }
    }
}
