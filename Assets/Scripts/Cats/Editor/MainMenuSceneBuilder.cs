using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace CatWorld.Cats.Editor
{
    /// <summary>
    /// Собирает сцену главного меню: фон-заглушка под будущий арт, заголовок,
    /// колонка кнопок (Новая игра/Продолжить/Настройки/Выход) и модальное окно
    /// настроек (громкость музыки и звуков, полноэкранный режим).
    /// Также добавляет обе сцены в Build Settings, чтобы работали переходы.
    /// </summary>
    public static class MainMenuSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Main/MainMenu.unity";
        private const string GameScenePath = "Assets/Scenes/Dima/CatSpawn.unity";

        [MenuItem("Tools/CatWorld/Build Main Menu Scene")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // --- Камера (только заливка фона за Canvas) ---
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.93f, 0.89f, 0.80f);

            // --- EventSystem (новый Input System) ---
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<InputSystemUIInputModule>();

            // --- Canvas ---
            var canvasGo = CatSpawnSceneBuilder.CreateUiObject("Canvas", null);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // --- Фон: полноэкранный слот под арт художника ---
            var background = CatSpawnSceneBuilder.CreateUiObject("BackgroundImage", canvasGo.transform);
            CatSpawnSceneBuilder.StretchFull(background.GetComponent<RectTransform>());
            var backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = new Color(0.86f, 0.90f, 0.78f); // заглушка до арта
            // Когда появится арт: назначить спрайт в этот Image и вернуть цвет в белый.

            // --- Заголовок ---
            var title = CatSpawnSceneBuilder.CreateText(canvasGo.transform, "Title", "CatWorld",
                96, new Color(0.25f, 0.2f, 0.12f));
            var titleRect = title.rectTransform;
            titleRect.anchorMin = titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(800f, 120f);
            titleRect.anchoredPosition = new Vector2(0f, -60f);

            // --- Колонка кнопок по центру ---
            var newGame = CreateMenuButton(canvasGo.transform, "NewGameButton", "Новая игра", 100f);
            var continueBtn = CreateMenuButton(canvasGo.transform, "ContinueButton", "Продолжить", -4f);
            var settingsBtn = CreateMenuButton(canvasGo.transform, "SettingsButton", "Настройки", -108f);
            var quitBtn = CreateMenuButton(canvasGo.transform, "QuitButton", "Выход", -212f);

            // --- Окно настроек ---
            var settingsPanel = BuildSettingsPanel(canvasGo);

            // --- Контроллер меню (на активном Canvas — Awake отработает на старте) ---
            var menu = canvasGo.AddComponent<MainMenuController>();
            menu.Configure(newGame, continueBtn, settingsBtn, quitBtn, settingsPanel);

            CatSpawnSceneBuilder.EnsureFolder("Assets/Scenes/Main");
            EditorSceneManager.SaveScene(scene, ScenePath);

            AddScenesToBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log($"[MainMenuSceneBuilder] Готово: {ScenePath} (+ сцены в Build Settings).");
        }

        /// <summary>
        /// Пересобирает панель настроек в обеих сценах, добавляя блок
        /// автосохранения, и заводит AutoSaveService в игровой сцене.
        /// Пересобирается только сама панель — остальная сцена (фон, камера,
        /// FarmBounds, ручные правки) не трогается.
        /// </summary>
        [MenuItem("Tools/CatWorld/Upgrade Settings (Autosave)")]
        public static void UpgradeSettingsForAutoSave()
        {
            // --- Главное меню ---
            var menuScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var menuCanvas = Object.FindFirstObjectByType<Canvas>();
            if (menuCanvas != null)
            {
                var panel = RebuildSettingsPanel(menuCanvas.gameObject);
                var controller = menuCanvas.GetComponent<MainMenuController>();
                if (controller != null)
                    SetReference(controller, "_settingsPanel", panel);
            }
            EditorSceneManager.MarkSceneDirty(menuScene);
            EditorSceneManager.SaveScene(menuScene);

            // --- Игровая сцена ---
            var gameScene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            var gameCanvas = Object.FindFirstObjectByType<Canvas>();
            if (gameCanvas != null)
            {
                var panel = RebuildSettingsPanel(gameCanvas.gameObject);
                var inGameMenu = gameCanvas.GetComponent<InGameMenuPanel>();
                if (inGameMenu != null)
                    SetReference(inGameMenu, "_settingsPanel", panel);
            }

            var autoSave = Object.FindFirstObjectByType<AutoSaveService>();
            if (autoSave == null)
            {
                var go = new GameObject("AutoSaveService");
                autoSave = go.AddComponent<AutoSaveService>();
            }
            autoSave.Configure(Object.FindFirstObjectByType<GameSaveService>());

            EditorSceneManager.MarkSceneDirty(gameScene);
            EditorSceneManager.SaveScene(gameScene);
            AssetDatabase.SaveAssets();
            Debug.Log("[MainMenuSceneBuilder] Готово: настройки автосохранения добавлены в обе сцены.");
        }

        /// <summary>
        /// Заменяет панель настроек новой. Старая удаляется целиком (компонент
        /// с Canvas и дочерний объект окна), новая создаётся последней — поэтому
        /// рисуется поверх остального UI.
        /// </summary>
        private static SettingsPanel RebuildSettingsPanel(GameObject canvasGo)
        {
            var oldPanel = canvasGo.GetComponent<SettingsPanel>();
            if (oldPanel != null)
                Object.DestroyImmediate(oldPanel);

            var oldRoot = canvasGo.transform.Find("SettingsPanel");
            if (oldRoot != null)
                Object.DestroyImmediate(oldRoot.gameObject);

            return BuildSettingsPanel(canvasGo);
        }

        private static void SetReference(Object target, string propertyName, Object value)
        {
            var so = new SerializedObject(target);
            var property = so.FindProperty(propertyName);
            if (property == null)
                return;
            property.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Button CreateMenuButton(Transform parent, string name, string label, float y)
        {
            var button = CatSpawnSceneBuilder.CreateButton(parent, name, label, new Vector2(420f, 84f));
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, y);
            return button;
        }

        internal static SettingsPanel BuildSettingsPanel(GameObject canvasGo)
        {
            // Затемнение поверх меню
            var dim = CatSpawnSceneBuilder.CreateUiObject("SettingsPanel", canvasGo.transform);
            CatSpawnSceneBuilder.StretchFull(dim.GetComponent<RectTransform>());
            var dimImage = dim.AddComponent<Image>();
            dimImage.color = new Color(0f, 0f, 0f, 0.5f);

            var window = CatSpawnSceneBuilder.CreateUiObject("Window", dim.transform);
            var windowRect = window.GetComponent<RectTransform>();
            windowRect.sizeDelta = new Vector2(640f, 680f);
            var windowImage = window.AddComponent<Image>();
            windowImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            windowImage.type = Image.Type.Sliced;
            windowImage.color = new Color(0.96f, 0.93f, 0.85f);

            var title = CatSpawnSceneBuilder.CreateText(window.transform, "Title", "Настройки",
                44, new Color(0.25f, 0.2f, 0.15f));
            var titleRect = title.rectTransform;
            titleRect.anchorMin = titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(400f, 60f);
            titleRect.anchoredPosition = new Vector2(0f, -30f);

            // --- Секция автосохранения (над громкостью) ---
            var autoSaveToggle = CatSpawnSceneBuilder.CreateToggle(window.transform, "AutoSaveToggle",
                "Автосохранение", Vector2.zero, null);
            PlaceLeft(autoSaveToggle.GetComponent<RectTransform>(), new Vector2(40f, -105f),
                new Vector2(360f, 44f));

            var intervalLabel = CatSpawnSceneBuilder.CreateText(window.transform, "IntervalLabel",
                "Интервал:", 30, new Color(0.28f, 0.24f, 0.18f));
            intervalLabel.alignment = TextAnchor.MiddleLeft;
            PlaceLeft(intervalLabel.rectTransform, new Vector2(40f, -165f), new Vector2(150f, 40f));

            // Радиокнопки: ToggleGroup гарантирует, что активен ровно один интервал.
            var intervalGroup = window.AddComponent<ToggleGroup>();
            intervalGroup.allowSwitchOff = false;
            var interval1 = CreateIntervalToggle(window.transform, "Interval1Toggle", "1 мин", 190f, intervalGroup);
            var interval5 = CreateIntervalToggle(window.transform, "Interval5Toggle", "5 мин", 330f, intervalGroup);
            var interval15 = CreateIntervalToggle(window.transform, "Interval15Toggle", "15 мин", 470f, intervalGroup);

            // Разделитель отбивает новую секцию от блока звука
            var separator = CatSpawnSceneBuilder.CreateUiObject("Separator", window.transform);
            var separatorRect = separator.GetComponent<RectTransform>();
            separatorRect.anchorMin = new Vector2(0f, 1f);
            separatorRect.anchorMax = new Vector2(1f, 1f);
            separatorRect.pivot = new Vector2(0.5f, 0.5f);
            separatorRect.offsetMin = new Vector2(40f, 0f);
            separatorRect.offsetMax = new Vector2(-40f, 0f);
            separatorRect.sizeDelta = new Vector2(separatorRect.sizeDelta.x, 2f);
            separatorRect.anchoredPosition = new Vector2(0f, -215f);
            separator.AddComponent<Image>().color = new Color(0.75f, 0.70f, 0.60f);

            // --- Громкость ---
            CreateSettingsRow(window.transform, "Music", "Музыка", -275f,
                out Slider musicSlider, out Text musicValue);
            CreateSettingsRow(window.transform, "Sound", "Звуки", -350f,
                out Slider soundSlider, out Text soundValue);

            var fullscreen = CatSpawnSceneBuilder.CreateToggle(window.transform, "FullscreenToggle",
                "Во весь экран", Vector2.zero, null);
            var fullscreenRect = fullscreen.GetComponent<RectTransform>();
            fullscreenRect.anchorMin = fullscreenRect.anchorMax = new Vector2(0.5f, 1f);
            fullscreenRect.pivot = new Vector2(0.5f, 0.5f);
            fullscreenRect.sizeDelta = new Vector2(300f, 44f);
            fullscreenRect.anchoredPosition = new Vector2(0f, -440f);

            var back = CatSpawnSceneBuilder.CreateButton(window.transform, "BackButton", "Назад",
                new Vector2(240f, 70f));
            var backRect = back.GetComponent<RectTransform>();
            backRect.anchorMin = backRect.anchorMax = new Vector2(0.5f, 0f);
            backRect.pivot = new Vector2(0.5f, 0f);
            backRect.anchoredPosition = new Vector2(0f, 26f);

            // Компонент на активном Canvas, окно (dim) — выключенный root.
            var panel = canvasGo.AddComponent<SettingsPanel>();
            panel.Configure(dim, musicSlider, soundSlider, musicValue, soundValue, fullscreen, back,
                autoSaveToggle, interval1, interval5, interval15, intervalLabel);
            dim.SetActive(false);
            return panel;
        }

        /// <summary>Компактный переключатель интервала автосохранения.</summary>
        private static Toggle CreateIntervalToggle(Transform parent, string name, string label,
            float x, ToggleGroup group)
        {
            var toggle = CatSpawnSceneBuilder.CreateToggle(parent, name, label, Vector2.zero, group);
            PlaceLeft(toggle.GetComponent<RectTransform>(), new Vector2(x, -165f), new Vector2(135f, 40f));
            return toggle;
        }

        /// <summary>Ставит элемент от левого верхнего угла окна.</summary>
        private static void PlaceLeft(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
        }

        private static void CreateSettingsRow(Transform parent, string name, string label, float y,
            out Slider slider, out Text valueLabel)
        {
            var rowLabel = CatSpawnSceneBuilder.CreateText(parent, name + "Label", label,
                32, new Color(0.28f, 0.24f, 0.18f));
            rowLabel.alignment = TextAnchor.MiddleLeft;
            var labelRect = rowLabel.rectTransform;
            labelRect.anchorMin = labelRect.anchorMax = new Vector2(0f, 1f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.sizeDelta = new Vector2(150f, 44f);
            labelRect.anchoredPosition = new Vector2(40f, y);

            slider = CreateSlider(parent, name + "Slider", new Vector2(210f, y));

            valueLabel = CatSpawnSceneBuilder.CreateText(parent, name + "Value", "100%",
                30, new Color(0.28f, 0.24f, 0.18f));
            valueLabel.alignment = TextAnchor.MiddleRight;
            var valueRect = valueLabel.rectTransform;
            valueRect.anchorMin = valueRect.anchorMax = new Vector2(0f, 1f);
            valueRect.pivot = new Vector2(0f, 0.5f);
            valueRect.sizeDelta = new Vector2(90f, 44f);
            valueRect.anchoredPosition = new Vector2(510f, y);
        }

        /// <summary>Стандартный uGUI-слайдер 0–1 из встроенных спрайтов.</summary>
        private static Slider CreateSlider(Transform parent, string name, Vector2 anchoredPosition)
        {
            var root = CatSpawnSceneBuilder.CreateUiObject(name, parent);
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = rootRect.anchorMax = new Vector2(0f, 1f);
            rootRect.pivot = new Vector2(0f, 0.5f);
            rootRect.sizeDelta = new Vector2(280f, 36f);
            rootRect.anchoredPosition = anchoredPosition;

            var background = CatSpawnSceneBuilder.CreateUiObject("Background", root.transform);
            var bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 0.5f);
            bgRect.anchorMax = new Vector2(1f, 0.5f);
            bgRect.sizeDelta = new Vector2(0f, 14f);
            var bgImage = background.AddComponent<Image>();
            bgImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            bgImage.type = Image.Type.Sliced;
            bgImage.color = new Color(0.75f, 0.70f, 0.60f);

            var fillArea = CatSpawnSceneBuilder.CreateUiObject("Fill Area", root.transform);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.5f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.5f);
            fillAreaRect.offsetMin = new Vector2(10f, -7f);
            fillAreaRect.offsetMax = new Vector2(-10f, 7f);

            var fill = CatSpawnSceneBuilder.CreateUiObject("Fill", fillArea.transform);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.sizeDelta = new Vector2(10f, 0f);
            var fillImage = fill.AddComponent<Image>();
            fillImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            fillImage.type = Image.Type.Sliced;
            fillImage.color = new Color(0.55f, 0.70f, 0.40f);

            var handleArea = CatSpawnSceneBuilder.CreateUiObject("Handle Slide Area", root.transform);
            var handleAreaRect = handleArea.GetComponent<RectTransform>();
            CatSpawnSceneBuilder.StretchFull(handleAreaRect);
            handleAreaRect.offsetMin = new Vector2(14f, 0f);
            handleAreaRect.offsetMax = new Vector2(-14f, 0f);

            var handle = CatSpawnSceneBuilder.CreateUiObject("Handle", handleArea.transform);
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(28f, 0f);
            var handleImage = handle.AddComponent<Image>();
            handleImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            var slider = root.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            return slider;
        }

        /// <summary>Добавляет сцены меню и игры в Build Settings (существующие не трёт).</summary>
        private static void AddScenesToBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            AddSceneIfMissing(scenes, ScenePath, insertFirst: true); // меню — стартовая сцена
            AddSceneIfMissing(scenes, GameScenePath, insertFirst: false);
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void AddSceneIfMissing(List<EditorBuildSettingsScene> scenes, string path, bool insertFirst)
        {
            foreach (var scene in scenes)
            {
                if (scene.path == path)
                    return;
            }
            var entry = new EditorBuildSettingsScene(path, true);
            if (insertFirst)
                scenes.Insert(0, entry);
            else
                scenes.Add(entry);
        }
    }
}
