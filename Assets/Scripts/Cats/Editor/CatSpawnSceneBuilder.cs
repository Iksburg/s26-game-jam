using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace CatWorld.Cats.Editor
{
    /// <summary>
    /// Воспроизводимо собирает всё для механики появления кота:
    /// палитру цветов, префаб кота и сцену CatSpawn с кнопкой и окном
    /// выбора пола/имени. Запуск: Tools → CatWorld → Build Cat Spawn Scene
    /// или batchmode -executeMethod CatWorld.Cats.Editor.CatSpawnSceneBuilder.Build.
    /// </summary>
    public static class CatSpawnSceneBuilder
    {
        private const string PalettePath = "Assets/Data/CatColorPalette.asset";
        private const string LifeStageSettingsPath = "Assets/Data/CatLifeStageSettings.asset";
        private const string CatPrefabPath = "Assets/Prefabs/Cat.prefab";
        private const string ScenePath = "Assets/Scenes/Dima/CatSpawn.unity";

        private static Font _font;

        [MenuItem("Tools/CatWorld/Build Cat Spawn Scene")]
        public static void Build()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            CatColorPalette palette = BuildPalette();
            CatLifeStageSettings lifeStageSettings = BuildLifeStageSettings();
            Cat catPrefab = BuildCatPrefab(lifeStageSettings);
            BuildScene(palette, catPrefab);

            AssetDatabase.SaveAssets();
            Debug.Log($"[CatSpawnSceneBuilder] Готово: {PalettePath}, {CatPrefabPath}, {ScenePath}");
        }

        private static CatColorPalette BuildPalette()
        {
            var palette = AssetDatabase.LoadAssetAtPath<CatColorPalette>(PalettePath);
            if (palette != null)
                return palette; // не перетираем настроенные вручную цвета

            EnsureFolder("Assets/Data");
            palette = ScriptableObject.CreateInstance<CatColorPalette>();
            AssetDatabase.CreateAsset(palette, PalettePath);
            return palette;
        }

        private static CatLifeStageSettings BuildLifeStageSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<CatLifeStageSettings>(LifeStageSettingsPath);
            if (settings != null)
                return settings; // не перетираем тюнинг гейм-дизайнера

            EnsureFolder("Assets/Data");
            settings = ScriptableObject.CreateInstance<CatLifeStageSettings>();
            AssetDatabase.CreateAsset(settings, LifeStageSettingsPath);
            return settings;
        }

        private static Cat BuildCatPrefab(CatLifeStageSettings lifeStageSettings)
        {
            EnsureFolder("Assets/Prefabs");

            var temp = new GameObject("Cat");
            temp.AddComponent<SpriteRenderer>(); // спрайт-заглушку CatView создаст в рантайме
            temp.AddComponent<CatView>();
            temp.AddComponent<Cat>();
            temp.AddComponent<CatWanderController>(); // автономное перемещение
            temp.AddComponent<CatAgeController>().Configure(lifeStageSettings); // стадии возраста
            temp.AddComponent<CatNeedsController>(); // потребности (еда/вода/чистота)

            var prefab = PrefabUtility.SaveAsPrefabAsset(temp, CatPrefabPath);
            Object.DestroyImmediate(temp);
            return prefab.GetComponent<Cat>();
        }

        private static void BuildScene(CatColorPalette palette, Cat catPrefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // --- Камера ---
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.36f, 0.55f, 0.29f); // лужайка фермы

            // --- Поле фермы, границы и спавнер ---
            var farmRoot = new GameObject("Farm").transform;

            // Фон — мировой спрайт позади котов. Дизайнер назначит спрайт забора
            // в SpriteRenderer.Sprite; коты (order 10) рисуются поверх (order -100).
            var backgroundGo = new GameObject("FarmBackground");
            backgroundGo.transform.SetParent(farmRoot, false);
            var backgroundRenderer = backgroundGo.AddComponent<SpriteRenderer>();
            backgroundRenderer.sortingOrder = -100;

            var boundsGo = new GameObject("FarmBounds");
            boundsGo.transform.SetParent(farmRoot, false);
            var polygon = boundsGo.AddComponent<PolygonCollider2D>();
            polygon.isTrigger = true;
            // Дефолтный прямоугольник по вью камеры; дизайнер обведёт им забор на фоне.
            polygon.points = new[]
            {
                new Vector2(-7f, -4f),
                new Vector2(7f, -4f),
                new Vector2(7f, 4f),
                new Vector2(-7f, 4f)
            };
            var bounds = boundsGo.AddComponent<FarmBounds>();

            var spawnerGo = new GameObject("CatSpawner");
            var spawner = spawnerGo.AddComponent<CatSpawner>();
            spawner.Configure(palette, catPrefab, bounds);

            // --- EventSystem (проект на новом Input System) ---
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<InputSystemUIInputModule>();

            // --- Canvas ---
            var canvasGo = CreateUiObject("Canvas", null);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // --- Кнопка «Добавить кота» (низ экрана) ---
            var openButton = CreateButton(canvasGo.transform, "AddCatButton", "Добавить кота",
                new Vector2(320f, 90f));
            var openRect = openButton.GetComponent<RectTransform>();
            openRect.anchorMin = openRect.anchorMax = new Vector2(0.5f, 0f);
            openRect.pivot = new Vector2(0.5f, 0f);
            openRect.anchoredPosition = new Vector2(0f, 40f);

            // --- Окно появления кота ---
            var panelRoot = BuildSpawnPanel(canvasGo.transform,
                out InputField nameInput, out Toggle maleToggle, out Toggle femaleToggle,
                out Button confirmButton, out Button cancelButton);

            var panel = canvasGo.AddComponent<SpawnCatPanel>();
            panel.Configure(spawner, openButton, panelRoot, nameInput,
                maleToggle, femaleToggle, confirmButton, cancelButton);
            panelRoot.SetActive(false);

            // --- Потребности: ресурсы, миски, UI запасов ---
            EnsureNeedsObjects();

            EnsureFolder("Assets/Scenes/Dima");
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        /// <summary>
        /// Дополняет СУЩЕСТВУЮЩУЮ сцену CatSpawn объектами задачи потребностей:
        /// FarmResources, места мисок (пустые объекты) и число запасов в UI.
        /// Камера и FarmBounds не изменяются. Также добавляет CatNeedsController
        /// на префаб кота, не пересоздавая ассет.
        /// </summary>
        [MenuItem("Tools/CatWorld/Upgrade CatSpawn Scene (Needs)")]
        public static void UpgradeForNeeds()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 1. Префаб: добавляем компонент, сохраняя всё настроенное вручную.
            var prefabRoot = PrefabUtility.LoadPrefabContents(CatPrefabPath);
            if (prefabRoot.GetComponent<CatNeedsController>() == null)
            {
                prefabRoot.AddComponent<CatNeedsController>();
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, CatPrefabPath);
            }
            PrefabUtility.UnloadPrefabContents(prefabRoot);

            // 2. Сцена: только дополняем.
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            EnsureNeedsObjects();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[CatSpawnSceneBuilder] Готово: FarmResources, миски и UI запасов добавлены в сцену и префаб.");
        }

        /// <summary>
        /// Дополняет СУЩЕСТВУЮЩУЮ сцену CatSpawn карточкой кота, окном семейного
        /// древа и мини-меню по ПКМ. Добавляет коллайдер на префаб кота для клика.
        /// Камера и FarmBounds не изменяются.
        /// </summary>
        [MenuItem("Tools/CatWorld/Upgrade CatSpawn Scene (Cat Card)")]
        public static void UpgradeForCards()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 1. Префаб: коллайдер нужен для ПКМ-рейкаста.
            var prefabRoot = PrefabUtility.LoadPrefabContents(CatPrefabPath);
            if (prefabRoot.GetComponent<Collider2D>() == null)
            {
                var circle = prefabRoot.AddComponent<CircleCollider2D>();
                circle.radius = 0.75f; // радиус спрайта-заглушки; масштабируется стадией
                circle.isTrigger = true;
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, CatPrefabPath);
            }
            PrefabUtility.UnloadPrefabContents(prefabRoot);

            // 2. Сцена: только дополняем.
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            EnsureCardObjects();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[CatSpawnSceneBuilder] Готово: карточка кота, семейное древо и мини-меню добавлены.");
        }

        /// <summary>Создаёт UI карточки в активной сцене (идемпотентно).</summary>
        private static void EnsureCardObjects()
        {
            if (GameObject.Find("CatCardPanel") != null)
                return; // уже собрано

            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
                return;

            // --- Мини-меню по ПКМ (создаём первым: карточка должна перекрывать) ---
            var menuGo = CreateUiObject("CatContextMenu", canvas.transform);
            var menuRect = menuGo.GetComponent<RectTransform>();
            menuRect.pivot = new Vector2(0f, 0.5f); // растёт вправо от кота
            menuRect.sizeDelta = new Vector2(190f, 60f);
            var menuImage = menuGo.AddComponent<Image>();
            menuImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            menuImage.type = Image.Type.Sliced;
            menuImage.color = new Color(0.96f, 0.93f, 0.85f);

            var inspectButton = CreateButton(menuGo.transform, "InspectButton", "Осмотреть",
                new Vector2(170f, 46f));
            inspectButton.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            var contextMenu = menuGo.AddComponent<CatContextMenu>();

            // --- Карточка кота ---
            var cardDim = CreateUiObject("CatCardPanel", canvas.transform);
            StretchFull(cardDim.GetComponent<RectTransform>());
            var cardDimImage = cardDim.AddComponent<Image>();
            cardDimImage.color = new Color(0f, 0f, 0f, 0.5f);

            var card = CreateUiObject("Window", cardDim.transform);
            var cardRect = card.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(720f, 560f);
            var cardImage = card.AddComponent<Image>();
            cardImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            cardImage.type = Image.Type.Sliced;
            cardImage.color = new Color(0.96f, 0.93f, 0.85f);

            // Иконка кота — левый верхний угол
            var icon = CreateImageObject(card.transform, "CatIcon", new Vector2(130f, 130f));
            AnchorTopLeft(icon.rectTransform, new Vector2(24f, -24f));

            // Имя — под иконкой
            var nameLabel = CreateText(card.transform, "NameLabel", "Имя", 34,
                new Color(0.2f, 0.16f, 0.1f));
            nameLabel.alignment = TextAnchor.MiddleLeft;
            nameLabel.rectTransform.sizeDelta = new Vector2(200f, 44f);
            AnchorTopLeft(nameLabel.rectTransform, new Vector2(24f, -162f));

            // Пол — изображение под именем (спрайты назначает дизайнер)
            var sexIcon = CreateImageObject(card.transform, "SexIcon", new Vector2(56f, 56f));
            AnchorTopLeft(sexIcon.rectTransform, new Vector2(24f, -212f));

            // Стадия роста — справа от иконки
            var stageLabel = CreateText(card.transform, "StageLabel", "Взрослый", 34,
                new Color(0.2f, 0.16f, 0.1f));
            stageLabel.alignment = TextAnchor.MiddleLeft;
            stageLabel.rectTransform.sizeDelta = new Vector2(380f, 44f);
            AnchorTopLeft(stageLabel.rectTransform, new Vector2(180f, -28f));

            // Показатели — под стадией
            var satiety = CreateNeedLabel(card.transform, "SatietyLabel", "Сытость: 100%", -86f);
            var water = CreateNeedLabel(card.transform, "WaterLabel", "Жажда: 100%", -128f);
            var cleanliness = CreateNeedLabel(card.transform, "CleanlinessLabel", "Чистота: 100%", -170f);

            // Блок черт — под показателями (заготовка)
            var traitsBlock = CreateUiObject("TraitsBlock", card.transform);
            var traitsRect = traitsBlock.GetComponent<RectTransform>();
            traitsRect.sizeDelta = new Vector2(660f, 150f);
            AnchorTopLeft(traitsRect, new Vector2(24f, -290f));
            var traitsImage = traitsBlock.AddComponent<Image>();
            traitsImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            traitsImage.type = Image.Type.Sliced;
            traitsImage.color = new Color(0.90f, 0.86f, 0.76f);

            var traitsPlaceholder = CreateText(traitsBlock.transform, "TraitsPlaceholder",
                "Черты характера: пока нет", 28, new Color(0.4f, 0.35f, 0.28f));
            StretchWithPadding(traitsPlaceholder.rectTransform, 16f, 8f);
            traitsPlaceholder.alignment = TextAnchor.UpperLeft;

            // Кнопка семейного древа — снизу по центру
            var treeButton = CreateButton(card.transform, "FamilyTreeButton", "Семейное древо",
                new Vector2(300f, 70f));
            var treeRect = treeButton.GetComponent<RectTransform>();
            treeRect.anchorMin = treeRect.anchorMax = new Vector2(0.5f, 0f);
            treeRect.pivot = new Vector2(0.5f, 0f);
            treeRect.anchoredPosition = new Vector2(0f, 22f);

            // Кнопка закрытия — максимально в правом верхнем углу
            var cardClose = CreateButton(card.transform, "CloseButton", "X", new Vector2(56f, 56f));
            var cardCloseRect = cardClose.GetComponent<RectTransform>();
            cardCloseRect.anchorMin = cardCloseRect.anchorMax = new Vector2(1f, 1f);
            cardCloseRect.pivot = new Vector2(1f, 1f);
            cardCloseRect.anchoredPosition = Vector2.zero;

            // --- Семейное древо (последним: рисуется поверх карточки) ---
            var treeDim = CreateUiObject("FamilyTreePanel", canvas.transform);
            StretchFull(treeDim.GetComponent<RectTransform>());
            var treeDimImage = treeDim.AddComponent<Image>();
            treeDimImage.color = new Color(0f, 0f, 0f, 0.5f);

            var treeWindow = CreateUiObject("Window", treeDim.transform);
            var treeWindowRect = treeWindow.GetComponent<RectTransform>();
            treeWindowRect.sizeDelta = new Vector2(900f, 620f);
            var treeWindowImage = treeWindow.AddComponent<Image>();
            treeWindowImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            treeWindowImage.type = Image.Type.Sliced;
            treeWindowImage.color = new Color(0.96f, 0.93f, 0.85f);

            var treeTitle = CreateText(treeWindow.transform, "Title", "Семейное древо", 40,
                new Color(0.25f, 0.2f, 0.15f));
            var treeTitleRect = treeTitle.rectTransform;
            treeTitleRect.anchorMin = treeTitleRect.anchorMax = new Vector2(0.5f, 1f);
            treeTitleRect.pivot = new Vector2(0.5f, 1f);
            treeTitleRect.sizeDelta = new Vector2(600f, 60f);
            treeTitleRect.anchoredPosition = new Vector2(0f, -24f);

            var treeClose = CreateButton(treeWindow.transform, "CloseButton", "X", new Vector2(56f, 56f));
            var treeCloseRect = treeClose.GetComponent<RectTransform>();
            treeCloseRect.anchorMin = treeCloseRect.anchorMax = new Vector2(1f, 1f);
            treeCloseRect.pivot = new Vector2(1f, 1f);
            treeCloseRect.anchoredPosition = Vector2.zero;

            var familyTree = treeDim.AddComponent<FamilyTreePanel>();
            familyTree.Configure(treeDim, treeClose);
            treeDim.SetActive(false);

            // --- Связывание ---
            var cardPanel = cardDim.AddComponent<CatCardPanel>();
            cardPanel.Configure(cardDim, icon, nameLabel, sexIcon, stageLabel,
                satiety, water, cleanliness, traitsBlock.transform, traitsPlaceholder,
                treeButton, cardClose, familyTree);
            cardDim.SetActive(false);

            contextMenu.Configure(menuGo, menuRect, inspectButton, cardPanel);
            menuGo.SetActive(false);

            // Детектор ПКМ по котам
            var detectorGo = GameObject.Find("CatClickDetector");
            if (detectorGo == null)
                detectorGo = new GameObject("CatClickDetector");
            var detector = detectorGo.GetComponent<CatClickDetector>();
            if (detector == null)
                detector = detectorGo.AddComponent<CatClickDetector>();
            detector.Configure(contextMenu);
        }

        private static Image CreateImageObject(Transform parent, string name, Vector2 size)
        {
            var go = CreateUiObject(name, parent);
            go.GetComponent<RectTransform>().sizeDelta = size;
            return go.AddComponent<Image>();
        }

        private static Text CreateNeedLabel(Transform parent, string name, string value, float y)
        {
            var label = CreateText(parent, name, value, 30, new Color(0.28f, 0.24f, 0.18f));
            label.alignment = TextAnchor.MiddleLeft;
            label.rectTransform.sizeDelta = new Vector2(380f, 38f);
            AnchorTopLeft(label.rectTransform, new Vector2(180f, y));
            return label;
        }

        private static void AnchorTopLeft(RectTransform rect, Vector2 anchoredPosition)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        /// <summary>Создаёт недостающие объекты потребностей в активной сцене (идемпотентно).</summary>
        private static void EnsureNeedsObjects()
        {
            // Запасы фермы (дефолт 100/100 задан в компоненте).
            var resourcesGo = GameObject.Find("FarmResources");
            if (resourcesGo == null)
                resourcesGo = new GameObject("FarmResources");
            var resources = resourcesGo.GetComponent<FarmResources>();
            if (resources == null)
                resources = resourcesGo.AddComponent<FarmResources>();

            // Места мисок — пустые объекты внутри границ фермы.
            var farm = GameObject.Find("Farm");
            Transform farmRoot = farm != null ? farm.transform : null;
            GetBowlPositions(out Vector2 foodPos, out Vector2 waterPos);
            EnsureStation("FoodBowlSpot", NeedType.Food, foodPos, farmRoot);
            EnsureStation("WaterBowlSpot", NeedType.Water, waterPos, farmRoot);

            // Число запасов в правом верхнем углу UI.
            if (GameObject.Find("ResourcesLabel") == null)
            {
                var canvas = Object.FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    var label = CreateText(canvas.transform, "ResourcesLabel",
                        "Корм: 100   Вода: 100", 34, new Color(0.15f, 0.12f, 0.08f));
                    label.alignment = TextAnchor.MiddleRight;
                    var rect = label.rectTransform;
                    rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
                    rect.pivot = new Vector2(1f, 1f);
                    rect.sizeDelta = new Vector2(460f, 50f);
                    rect.anchoredPosition = new Vector2(-20f, -20f);

                    var panel = label.gameObject.AddComponent<FarmResourcesPanel>();
                    panel.Configure(resources, label);
                }
            }
        }

        private static void EnsureStation(string name, NeedType type, Vector2 position, Transform parent)
        {
            var go = GameObject.Find(name);
            if (go == null)
            {
                go = new GameObject(name);
                if (parent != null)
                    go.transform.SetParent(parent, false);
                go.transform.position = new Vector3(position.x, position.y, 0f);
            }

            // Компоненты навешиваем идемпотентно; позицию существующего объекта не трогаем.
            var station = go.GetComponent<NeedStation>();
            if (station == null)
                station = go.AddComponent<NeedStation>();
            station.SetType(type);

            if (go.GetComponent<NeedStationView>() == null)
                go.AddComponent<NeedStationView>(); // спрайты полной/пустой миски назначит дизайнер
        }

        /// <summary>Точки мисок внутри полигона FarmBounds (не меняя сам полигон).</summary>
        private static void GetBowlPositions(out Vector2 foodPos, out Vector2 waterPos)
        {
            foodPos = new Vector2(-3f, 2f);
            waterPos = new Vector2(3f, 2f);

            var bounds = Object.FindFirstObjectByType<FarmBounds>();
            if (bounds == null)
                return;
            var polygon = bounds.GetComponent<PolygonCollider2D>();
            if (polygon == null || polygon.points.Length < 3)
                return;

            Vector2 center = polygon.bounds.center;
            float offsetX = polygon.bounds.extents.x * 0.4f;
            Vector2 food = center + new Vector2(-offsetX, 0f);
            Vector2 water = center + new Vector2(offsetX, 0f);

            Vector2[] worldPoints = new Vector2[polygon.points.Length];
            for (int i = 0; i < worldPoints.Length; i++)
                worldPoints[i] = polygon.transform.TransformPoint(polygon.points[i]);

            foodPos = PointInPolygon(food, worldPoints) ? food : center;
            waterPos = PointInPolygon(water, worldPoints) ? water : center;
        }

        private static bool PointInPolygon(Vector2 point, Vector2[] polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if (polygon[i].y > point.y != polygon[j].y > point.y &&
                    point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) /
                    (polygon[j].y - polygon[i].y) + polygon[i].x)
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        private static GameObject BuildSpawnPanel(Transform canvas,
            out InputField nameInput, out Toggle maleToggle, out Toggle femaleToggle,
            out Button confirmButton, out Button cancelButton)
        {
            // Затемнение на весь экран (блокирует клики по полю)
            var dim = CreateUiObject("SpawnCatPanel", canvas);
            var dimRect = dim.GetComponent<RectTransform>();
            dimRect.anchorMin = Vector2.zero;
            dimRect.anchorMax = Vector2.one;
            dimRect.offsetMin = dimRect.offsetMax = Vector2.zero;
            var dimImage = dim.AddComponent<Image>();
            dimImage.color = new Color(0f, 0f, 0f, 0.5f);

            // Окно
            var window = CreateUiObject("Window", dim.transform);
            var windowRect = window.GetComponent<RectTransform>();
            windowRect.sizeDelta = new Vector2(640f, 460f);
            var windowImage = window.AddComponent<Image>();
            windowImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            windowImage.type = Image.Type.Sliced;
            windowImage.color = new Color(0.96f, 0.93f, 0.85f);

            // Заголовок
            var title = CreateText(window.transform, "Title", "Новый кот", 42,
                new Color(0.25f, 0.2f, 0.15f));
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(560f, 60f);
            titleRect.anchoredPosition = new Vector2(0f, -55f);

            // Поле имени
            nameInput = CreateInputField(window.transform, new Vector2(0f, 40f), new Vector2(480f, 64f));

            // Переключатели пола
            var togglesRow = CreateUiObject("SexToggles", window.transform);
            var togglesRect = togglesRow.GetComponent<RectTransform>();
            togglesRect.sizeDelta = new Vector2(480f, 50f);
            togglesRect.anchoredPosition = new Vector2(0f, 130f);
            var group = togglesRow.AddComponent<ToggleGroup>();
            group.allowSwitchOff = false;

            maleToggle = CreateToggle(togglesRow.transform, "MaleToggle", "Мальчик",
                new Vector2(-120f, 0f), group);
            femaleToggle = CreateToggle(togglesRow.transform, "FemaleToggle", "Девочка",
                new Vector2(80f, 0f), group);
            maleToggle.isOn = true;
            femaleToggle.isOn = false;

            // Кнопки подтверждения/отмены
            confirmButton = CreateButton(window.transform, "ConfirmButton", "Подтвердить",
                new Vector2(240f, 70f));
            var confirmRect = confirmButton.GetComponent<RectTransform>();
            confirmRect.anchoredPosition = new Vector2(-135f, -150f);

            cancelButton = CreateButton(window.transform, "CancelButton", "Отмена",
                new Vector2(240f, 70f));
            var cancelRect = cancelButton.GetComponent<RectTransform>();
            cancelRect.anchoredPosition = new Vector2(135f, -150f);

            return dim;
        }

        // ---------- UI-хелперы ----------

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            if (parent != null)
                go.transform.SetParent(parent, false);
            return go;
        }

        private static Text CreateText(Transform parent, string name, string value,
            int fontSize, Color color)
        {
            var go = CreateUiObject(name, parent);
            var text = go.AddComponent<Text>();
            text.font = _font;
            text.text = value;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 size)
        {
            var go = CreateUiObject(name, parent);
            go.GetComponent<RectTransform>().sizeDelta = size;

            var image = go.AddComponent<Image>();
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = Image.Type.Sliced;

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;

            var text = CreateText(go.transform, "Text", label, 32, new Color(0.2f, 0.2f, 0.2f));
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = textRect.offsetMax = Vector2.zero;

            return button;
        }

        private static InputField CreateInputField(Transform parent, Vector2 anchoredPos, Vector2 size)
        {
            var go = CreateUiObject("NameInput", parent);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPos;

            var image = go.AddComponent<Image>();
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
            image.type = Image.Type.Sliced;

            var input = go.AddComponent<InputField>();
            input.targetGraphic = image;

            var placeholder = CreateText(go.transform, "Placeholder", "Имя кота…", 30,
                new Color(0.5f, 0.5f, 0.5f));
            placeholder.fontStyle = FontStyle.Italic;
            placeholder.alignment = TextAnchor.MiddleLeft;
            StretchWithPadding(placeholder.rectTransform, 16f, 8f);

            var text = CreateText(go.transform, "Text", string.Empty, 30, new Color(0.15f, 0.15f, 0.15f));
            text.alignment = TextAnchor.MiddleLeft;
            text.supportRichText = false;
            StretchWithPadding(text.rectTransform, 16f, 8f);

            input.textComponent = text;
            input.placeholder = placeholder;
            return input;
        }

        private static Toggle CreateToggle(Transform parent, string name, string label,
            Vector2 anchoredPos, ToggleGroup group)
        {
            var go = CreateUiObject(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200f, 44f);
            rect.anchoredPosition = anchoredPos;

            var background = CreateUiObject("Background", go.transform);
            var bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 0.5f);
            bgRect.anchorMax = new Vector2(0f, 0.5f);
            bgRect.sizeDelta = new Vector2(36f, 36f);
            bgRect.anchoredPosition = new Vector2(18f, 0f);
            var bgImage = background.AddComponent<Image>();
            bgImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            bgImage.type = Image.Type.Sliced;

            var checkmark = CreateUiObject("Checkmark", background.transform);
            var checkRect = checkmark.GetComponent<RectTransform>();
            checkRect.sizeDelta = new Vector2(28f, 28f);
            var checkImage = checkmark.AddComponent<Image>();
            checkImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");

            var text = CreateText(go.transform, "Label", label, 30, new Color(0.2f, 0.2f, 0.2f));
            text.alignment = TextAnchor.MiddleLeft;
            var textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(44f, 0f);
            textRect.offsetMax = Vector2.zero;

            var toggle = go.AddComponent<Toggle>();
            toggle.targetGraphic = bgImage;
            toggle.graphic = checkImage;
            toggle.group = group;
            return toggle;
        }

        private static void StretchWithPadding(RectTransform rect, float horizontal, float vertical)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(horizontal, vertical);
            rect.offsetMax = new Vector2(-horizontal, -vertical);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            string leaf = System.IO.Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
