using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace CatWorld.Cats.Editor
{
    /// <summary>
    /// Собирает UI магазина в сцене CatSpawn: кнопка «Магазин» под кнопкой «Меню»,
    /// окно с двумя вкладками (покупка и продажа) и шаблон карточки кота.
    /// Камера и FarmBounds не изменяются.
    /// </summary>
    public static class ShopSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Dima/CatSpawn.unity";

        private static readonly Color TextColor = new Color(0.25f, 0.2f, 0.15f);
        private static readonly Color CardColor = new Color(0.90f, 0.86f, 0.76f);

        [MenuItem("Tools/CatWorld/Upgrade CatSpawn Scene (Shop)")]
        public static void Build()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[ShopSceneBuilder] В сцене нет Canvas.");
                return;
            }

            if (canvas.GetComponent<ShopPanel>() != null)
            {
                Debug.Log("[ShopSceneBuilder] Магазин уже собран.");
                return;
            }

            BuildShop(canvas);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[ShopSceneBuilder] Готово: UI магазина добавлен в сцену CatSpawn.");
        }

        private static void BuildShop(Canvas canvas)
        {
            // --- Кнопка «Магазин» под кнопкой «Меню» ---
            var openButton = CatSpawnSceneBuilder.CreateButton(canvas.transform, "ShopButton",
                "Магазин", new Vector2(160f, 64f));
            var openRect = openButton.GetComponent<RectTransform>();
            openRect.anchorMin = openRect.anchorMax = new Vector2(1f, 1f);
            openRect.pivot = new Vector2(1f, 1f);
            openRect.anchoredPosition = new Vector2(-20f, -94f); // «Меню» стоит на -20

            // Счётчик запасов уезжает ниже, чтобы не пересекаться с двумя кнопками.
            var resourcesLabel = GameObject.Find("ResourcesLabel");
            if (resourcesLabel != null)
            {
                var labelRect = resourcesLabel.GetComponent<RectTransform>();
                if (labelRect != null)
                    labelRect.anchoredPosition = new Vector2(-20f, -172f);
            }

            // --- Затемнение и окно ---
            var dim = CatSpawnSceneBuilder.CreateUiObject("ShopPanel", canvas.transform);
            CatSpawnSceneBuilder.StretchFull(dim.GetComponent<RectTransform>());
            dim.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

            var window = CatSpawnSceneBuilder.CreateUiObject("Window", dim.transform);
            var windowRect = window.GetComponent<RectTransform>();
            windowRect.sizeDelta = new Vector2(940f, 620f);
            var windowImage = window.AddComponent<Image>();
            windowImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            windowImage.type = Image.Type.Sliced;
            windowImage.color = new Color(0.96f, 0.93f, 0.85f);

            // Заголовок сверху по середине
            var title = CatSpawnSceneBuilder.CreateText(window.transform, "Title", "Магазин", 48, TextColor);
            AnchorTop(title.rectTransform, new Vector2(0f, -26f), new Vector2(400f, 60f));

            // Баланс — слева сверху, чтобы игрок видел, на что хватает
            var coins = CatSpawnSceneBuilder.CreateText(window.transform, "CoinsLabel",
                "Мяукоины: 0 MC", 28, TextColor);
            coins.alignment = TextAnchor.MiddleLeft;
            AnchorTopLeft(coins.rectTransform, new Vector2(30f, -30f), new Vector2(340f, 40f));

            // Крестик закрытия
            var closeButton = CatSpawnSceneBuilder.CreateButton(window.transform, "CloseButton",
                "X", new Vector2(48f, 48f));
            var closeRect = closeButton.GetComponent<RectTransform>();
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-10f, -10f);

            // --- Вкладки ---
            var buyTab = CatSpawnSceneBuilder.CreateUiObject("BuyTab", window.transform);
            StretchContent(buyTab.GetComponent<RectTransform>());
            var sellTab = CatSpawnSceneBuilder.CreateUiObject("SellTab", window.transform);
            StretchContent(sellTab.GetComponent<RectTransform>());

            BuildBuyTab(buyTab.transform,
                out Button buyCat, out Button buyFood, out Button buyWater,
                out Text catPrice, out Text foodPrice, out Text waterPrice,
                out Image catIcon, out Image foodIcon, out Image waterIcon);

            BuildSellTab(sellTab.transform, out RectTransform sellContent,
                out ShopCatCard template, out Text emptyLabel);

            // --- Кнопки вкладок в правом нижнем углу окна ---
            var buyTabButton = CreateTabButton(window.transform, "BuyTabButton", "Покупка", -250f);
            var sellTabButton = CreateTabButton(window.transform, "SellTabButton", "Продажа", -30f);

            // Статус — слева снизу, рядом с кнопками вкладок
            var status = CatSpawnSceneBuilder.CreateText(window.transform, "StatusLabel",
                string.Empty, 26, new Color(0.45f, 0.4f, 0.32f));
            status.alignment = TextAnchor.MiddleLeft;
            var statusRect = status.rectTransform;
            statusRect.anchorMin = statusRect.anchorMax = new Vector2(0f, 0f);
            statusRect.pivot = new Vector2(0f, 0f);
            statusRect.sizeDelta = new Vector2(480f, 40f);
            statusRect.anchoredPosition = new Vector2(30f, 30f);

            var shop = canvas.gameObject.AddComponent<ShopPanel>();
            shop.Configure(dim, openButton, closeButton, buyTabButton, sellTabButton,
                buyTab, sellTab, buyCat, buyFood, buyWater,
                catPrice, foodPrice, waterPrice, catIcon, foodIcon, waterIcon,
                sellContent, template, emptyLabel, coins, status,
                Object.FindFirstObjectByType<EconomyService>(),
                Object.FindFirstObjectByType<SpawnCatPanel>());

            sellTab.SetActive(false);
            dim.SetActive(false);
        }

        // ---------- Вкладка покупки ----------

        private static void BuildBuyTab(Transform parent,
            out Button buyCat, out Button buyFood, out Button buyWater,
            out Text catPrice, out Text foodPrice, out Text waterPrice,
            out Image catIcon, out Image foodIcon, out Image waterIcon)
        {
            // Три карточки в ряд по центру
            buyCat = CreateGoodsCard(parent, "CatCard", "Котёнок", -280f, out catPrice, out catIcon);
            buyFood = CreateGoodsCard(parent, "FoodCard", "Корм", 0f, out foodPrice, out foodIcon);
            buyWater = CreateGoodsCard(parent, "WaterCard", "Вода", 280f, out waterPrice, out waterIcon);
        }

        /// <summary>
        /// Карточка товара: сверху изображение, под ним название, ниже цена.
        /// Вся карточка — кнопка покупки.
        /// </summary>
        private static Button CreateGoodsCard(Transform parent, string name, string title,
            float x, out Text priceLabel, out Image icon)
        {
            var card = CatSpawnSceneBuilder.CreateUiObject(name, parent);
            var cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(240f, 330f);
            cardRect.anchoredPosition = new Vector2(x, 10f);

            var cardImage = card.AddComponent<Image>();
            cardImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            cardImage.type = Image.Type.Sliced;
            cardImage.color = CardColor;

            var button = card.AddComponent<Button>();
            button.targetGraphic = cardImage;

            // Изображение товара — слот под спрайт дизайнера
            var iconGo = CatSpawnSceneBuilder.CreateUiObject("Icon", card.transform);
            var iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.pivot = new Vector2(0.5f, 1f);
            iconRect.sizeDelta = new Vector2(180f, 180f);
            iconRect.anchoredPosition = new Vector2(0f, -20f);
            icon = iconGo.AddComponent<Image>();
            icon.color = new Color(1f, 1f, 1f, 0.35f); // видимый пустой слот до спрайта
            icon.raycastTarget = false;

            var nameLabel = CatSpawnSceneBuilder.CreateText(card.transform, "NameLabel", title, 32, TextColor);
            AnchorTop(nameLabel.rectTransform, new Vector2(0f, -215f), new Vector2(220f, 44f));
            nameLabel.raycastTarget = false;

            priceLabel = CatSpawnSceneBuilder.CreateText(card.transform, "PriceLabel", "0 MC", 30,
                new Color(0.35f, 0.30f, 0.12f));
            AnchorTop(priceLabel.rectTransform, new Vector2(0f, -265f), new Vector2(220f, 40f));
            priceLabel.raycastTarget = false;

            return button;
        }

        // ---------- Вкладка продажи ----------

        private static void BuildSellTab(Transform parent, out RectTransform content,
            out ShopCatCard template, out Text emptyLabel)
        {
            // Область прокрутки
            var scrollGo = CatSpawnSceneBuilder.CreateUiObject("SellScroll", parent);
            var scrollRect = scrollGo.GetComponent<RectTransform>();
            CatSpawnSceneBuilder.StretchFull(scrollRect);
            scrollRect.offsetMin = new Vector2(20f, 20f);
            scrollRect.offsetMax = new Vector2(-20f, -20f);
            var scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;

            // Viewport с маской — без него карточки вылезали бы за окно
            var viewport = CatSpawnSceneBuilder.CreateUiObject("Viewport", scrollGo.transform);
            var viewportRect = viewport.GetComponent<RectTransform>();
            CatSpawnSceneBuilder.StretchFull(viewportRect);
            var viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.01f); // маске нужен Graphic
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            scroll.viewport = viewportRect;

            var contentGo = CatSpawnSceneBuilder.CreateUiObject("Content", viewport.transform);
            content = contentGo.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = new Vector2(0f, 0f);
            scroll.content = content;

            // Сетка сама раскладывает карточки, фиттер тянет высоту под их количество
            var grid = contentGo.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(190f, 250f);
            grid.spacing = new Vector2(16f, 16f);
            grid.padding = new RectOffset(12, 12, 12, 12);
            grid.childAlignment = TextAnchor.UpperLeft;
            var fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            template = CreateCatCardTemplate(contentGo.transform);

            emptyLabel = CatSpawnSceneBuilder.CreateText(parent, "SellEmptyLabel",
                "На ферме нет котов", 30, new Color(0.45f, 0.4f, 0.32f));
            CatSpawnSceneBuilder.StretchFull(emptyLabel.rectTransform);
            emptyLabel.gameObject.SetActive(false);
        }

        /// <summary>Шаблон карточки кота: спрайт, имя, цена. Клонируется в рантайме.</summary>
        private static ShopCatCard CreateCatCardTemplate(Transform parent)
        {
            var card = CatSpawnSceneBuilder.CreateUiObject("CatCardTemplate", parent);
            var cardImage = card.AddComponent<Image>();
            cardImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            cardImage.type = Image.Type.Sliced;
            cardImage.color = CardColor;

            var button = card.AddComponent<Button>();
            button.targetGraphic = cardImage;

            var iconGo = CatSpawnSceneBuilder.CreateUiObject("Icon", card.transform);
            var iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.pivot = new Vector2(0.5f, 1f);
            iconRect.sizeDelta = new Vector2(120f, 120f);
            iconRect.anchoredPosition = new Vector2(0f, -14f);
            var icon = iconGo.AddComponent<Image>();
            icon.raycastTarget = false;

            var nameLabel = CatSpawnSceneBuilder.CreateText(card.transform, "NameLabel", "Имя", 26, TextColor);
            AnchorTop(nameLabel.rectTransform, new Vector2(0f, -142f), new Vector2(180f, 36f));
            nameLabel.raycastTarget = false;

            var priceLabel = CatSpawnSceneBuilder.CreateText(card.transform, "PriceLabel", "0 MC", 26,
                new Color(0.35f, 0.30f, 0.12f));
            AnchorTop(priceLabel.rectTransform, new Vector2(0f, -182f), new Vector2(180f, 36f));
            priceLabel.raycastTarget = false;

            var cardComponent = card.AddComponent<ShopCatCard>();
            cardComponent.Configure(icon, nameLabel, priceLabel, button);
            card.SetActive(false); // шаблон не показываем
            return cardComponent;
        }

        // ---------- Хелперы ----------

        /// <summary>Кнопка вкладки в правом нижнем углу окна.</summary>
        private static Button CreateTabButton(Transform parent, string name, string label, float x)
        {
            var button = CatSpawnSceneBuilder.CreateButton(parent, name, label, new Vector2(200f, 64f));
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(x, 24f);
            return button;
        }

        /// <summary>Контентная область вкладки: под заголовком и над кнопками вкладок.</summary>
        private static void StretchContent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(20f, 100f);
            rect.offsetMax = new Vector2(-20f, -80f);
        }

        private static void AnchorTop(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
        }

        private static void AnchorTopLeft(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
        }
    }
}
