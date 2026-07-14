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

            EnsureFolder("Assets/Scenes/Dima");
            EditorSceneManager.SaveScene(scene, ScenePath);
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
