using System.Collections.Generic;
using PTT.Tutorial;
using PTT.Tutorial.Editor;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.UI;

/// <summary>
/// Builds the demo scene that follows the "Tutorial Mask Demo – Main Menu" design:
/// a main menu (Play, Shop, Inventory, Daily, Settings) plus a 4-step tutorial
/// (welcome → Daily → Shop → Play) using UITutorialManager.
/// Menu: PTT > Build Main Menu Tutorial Demo
/// </summary>
public static class MainMenuDemoBuilder
{
    const string SceneName = "MainMenuTutorial_Built.unity";
    const string TooltipPrefabPath = "Packages/com.ptt.tutorial/Runtime/Prefabs/tooltipUI.prefab";
    const string RoundSpritePath = "Packages/com.ptt.tutorial/Runtime/Prefabs/Sprites/UI_SquareRound.png";

    // Palette from the design
    static readonly Color Ground = Hex("#0E1320");
    static readonly Color Surface = Hex("#1A2133");
    static readonly Color Raised = Hex("#2A3550");
    static readonly Color Hill = Hex("#3A4766");
    static readonly Color Accent = Hex("#FFB547");
    static readonly Color Ink = Hex("#141A28");
    static readonly Color Muted = Hex("#A3ADC2");
    static readonly Color Dim = new Color(0.024f, 0.035f, 0.07f, 0.78f);

    static Sprite round;
    static Sprite knob;

    [MenuItem("PTT/Build Main Menu Tutorial Demo")]
    public static void Build()
    {
        if (Resources.Load<TMP_Settings>("TMP Settings") == null)
        {
            EditorUtility.DisplayDialog("TextMeshPro is not set up",
                "Import TMP Essential Resources first:\nWindow > TextMeshPro > Import TMP Essential Resources\nthen run this menu again.", "OK");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        // Register the demo IDs in the project's TutorialIDDatabase (created if missing).
        foreach (var id in new[] { "btn_play", "btn_shop", "btn_inventory", "btn_daily", "btn_settings" })
            TutorialIDDatabaseLocator.EnsureID(TutorialIDKind.Button, id);
        TutorialIDDatabaseLocator.EnsureID(TutorialIDKind.Sequence, "seq_main_menu");
        AssetDatabase.SaveAssets();

        // Save next to this sample (the folder above Editor/).
        string builderPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("MainMenuDemoBuilder t:Script")[0]);
        string ScenePath = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(builderPath)).Replace('\\', '/') + "/" + SceneName;

        round = AssetDatabase.LoadAssetAtPath<Sprite>(RoundSpritePath);
        knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        var tooltipPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TooltipPrefabPath);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Ground;
        cam.orthographic = true;

        // EventSystem (project uses the new Input System)
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        es.AddComponent<InputSystemUIInputModule>();
#else
        es.AddComponent<StandaloneInputModule>();
#endif

        // Canvas
        var canvasGo = new GameObject("MenuCanvas", typeof(RectTransform));
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();
        var root = canvasGo.transform;

        var bg = NewImage("Background", root, Ground, null);
        Stretch(bg.rectTransform);

        // Controller (created early so buttons can wire to it)
        var managerGo = new GameObject("TutorialManager");
        var manager = managerGo.AddComponent<UITutorialManager>();
        var controller = managerGo.AddComponent<MainMenuTutorialController>();
        controller.tutorialManager = manager;

        // ---- Top bar ----
        var avatar = NewImage("Avatar", root, Raised, round);
        PlaceTop(avatar.rectTransform, 56, 60, 122, 122);
        NewText("Label", avatar.transform, "P1", 48, Accent, TextAlignmentOptions.Center, FontStyles.Bold, stretch: true);

        var nameText = NewText("PlayerName", root, "Player One", 42, Color.white, TextAlignmentOptions.Left, FontStyles.Bold);
        PlaceTop(nameText.rectTransform, 210, 72, 420, 56);
        var levelText = NewText("PlayerLevel", root, "Level 1", 32, Muted, TextAlignmentOptions.Left, FontStyles.Normal);
        PlaceTop(levelText.rectTransform, 210, 128, 420, 44);

        var coins = NewImage("Coins", root, Surface, round);
        PlaceTop(coins.rectTransform, 640, 76, 220, 90);
        NewText("Value", coins.transform, "1,250", 38, Color.white, TextAlignmentOptions.Center, FontStyles.Bold, stretch: true);

        var settings = NewButton("Button_Settings", root, Surface, "btn_settings", controller);
        PlaceTop(settings.GetComponent<RectTransform>(), 902, 60, 122, 122);
        NewText("Label", settings.transform, "SET", 34, Color.white, TextAlignmentOptions.Center, FontStyles.Bold, stretch: true);

        // ---- Title ----
        var season = NewText("Season", root, "SEASON 1", 36, Accent, TextAlignmentOptions.Left, FontStyles.Bold);
        season.characterSpacing = 12;
        PlaceTop(season.rectTransform, 56, 300, 968, 50);
        var title = NewText("Title", root, "Starfall Run", 120, Color.white, TextAlignmentOptions.Left, FontStyles.Bold);
        PlaceTop(title.rectTransform, 56, 350, 968, 140);

        // ---- Hero ----
        var hero = NewImage("Hero", root, Surface, round);
        PlaceTop(hero.rectTransform, 56, 520, 968, 620);
        hero.gameObject.AddComponent<RectMask2D>();
        var sun = NewImage("Sun", hero.transform, Accent, knob);
        PlaceTopIn(sun.rectTransform, 650, 90, 210, 210);
        var hillBack = NewImage("HillBack", hero.transform, Raised, null);
        PlaceTopIn(hillBack.rectTransform, -100, 380, 1200, 600);
        hillBack.rectTransform.localEulerAngles = new Vector3(0, 0, 6);
        var hillFront = NewImage("HillFront", hero.transform, Hill, null);
        PlaceTopIn(hillFront.rectTransform, -100, 500, 1200, 600);
        hillFront.rectTransform.localEulerAngles = new Vector3(0, 0, -4);

        // ---- Play ----
        var play = NewButton("Button_Play", root, Accent, "btn_play", controller);
        PlaceTop(play.GetComponent<RectTransform>(), 56, 1200, 968, 200);
        NewText("Label", play.transform, "PLAY", 72, Ink, TextAlignmentOptions.Center, FontStyles.Bold, stretch: true);

        // ---- Tiles ----
        float tileW = 298, gap = 37, tileY = 1460, tileH = 266;
        var shop = NewTile("Button_Shop", root, "Shop", "btn_shop", controller, 56, tileY, tileW, tileH);
        var inventory = NewTile("Button_Inventory", root, "Inventory", "btn_inventory", controller, 56 + tileW + gap, tileY, tileW, tileH);
        var daily = NewTile("Button_Daily", root, "Daily", "btn_daily", controller, 56 + 2 * (tileW + gap), tileY, tileW, tileH);

        // ---- Footer: replay tutorial ----
        var replay = NewImage("Button_ReplayTutorial", root, new Color(0, 0, 0, 0), null);
        PlaceTop(replay.rectTransform, 290, 1780, 500, 90);
        var replayBtn = replay.gameObject.AddComponent<Button>();
        UnityEventTools.AddPersistentListener(replayBtn.onClick, controller.ReplayTutorial);
        NewText("Label", replay.transform, "Replay tutorial", 34, Muted, TextAlignmentOptions.Center, FontStyles.Underline, stretch: true);

        // ---- Overlay (must be the last child so it draws on top) ----
        var overlay = NewImage("OverlayPanel", root, Dim, null);
        Stretch(overlay.rectTransform);
        overlay.raycastTarget = true;
        overlay.gameObject.SetActive(false);

        // ---- Tutorial setup ----
        manager.overlayPanel = overlay.gameObject;
        manager.tooltipPrefab = tooltipPrefab;
        manager.testSequenceID = "seq_main_menu";
        manager.testButtonID = "btn_play";
        manager.TutorialSequences = new List<TutorialSequence>
        {
            new TutorialSequence
            {
                sequenceName = "seq_main_menu",
                steps = new List<UITutorialStep>
                {
                    Step("",     "Welcome! Let's take a quick tour of the main menu.\n<size=70%>Tap anywhere to continue</size>"),
                    Step("btn_daily", "Claim a free reward here every day."),
                    Step("btn_shop",  "Spend coins on new skins and boosts in the Shop."),
                    Step("btn_play",  "Ready? Tap Play to start your first run!"),
                }
            }
        };

        if (tooltipPrefab == null)
            Debug.LogWarning($"Tooltip prefab not found at {TooltipPrefabPath}. Assign it on TutorialManager.");

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettings(ScenePath);
        Debug.Log($"Main menu tutorial demo built: {ScenePath}. Press Play to run the tutorial.");
    }

    // ---------- helpers ----------

    static UITutorialStep Step(string id, string message) =>
        new UITutorialStep { focusButtonID = id, message = message, OnStepCompleted = new UnityEvent() };

    static GameObject NewTile(string name, Transform parent, string label, string id,
        MainMenuTutorialController controller, float x, float y, float w, float h)
    {
        var go = NewButton(name, parent, Surface, id, controller);
        PlaceTop(go.GetComponent<RectTransform>(), x, y, w, h);
        var dot = NewImage("Icon", go.transform, Accent, knob);
        PlaceTopIn(dot.rectTransform, (w - 72) / 2, 56, 72, 72);
        var text = NewText("Label", go.transform, label, 38, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
        PlaceTopIn(text.rectTransform, 0, 150, w, 60);
        return go;
    }

    static GameObject NewButton(string name, Transform parent, Color color, string id, MainMenuTutorialController controller)
    {
        var img = NewImage(name, parent, color, round);
        var button = img.gameObject.AddComponent<Button>();
        var colors = button.colors;
        colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
        button.colors = colors;
        UnityEventTools.AddStringPersistentListener(button.onClick, controller.OnMenuButton, id);
        var uiButton = img.gameObject.AddComponent<UIButton>();
        uiButton.buttonID = id;
        return img.gameObject;
    }

    static Image NewImage(string name, Transform parent, Color color, Sprite sprite)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        img.sprite = sprite;
        if (sprite != null && sprite == round)
        {
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 1.2f;
        }
        img.raycastTarget = go.name.StartsWith("Button") || go.name == "OverlayPanel";
        return img;
    }

    static TextMeshProUGUI NewText(string name, Transform parent, string text, float size, Color color,
        TextAlignmentOptions align, FontStyles style, bool stretch = false)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = align;
        tmp.fontStyle = style;
        tmp.raycastTarget = false;
        if (stretch) Stretch(tmp.rectTransform);
        return tmp;
    }

    // Position in canvas reference pixels, measured from the top-left of a 1080-wide screen.
    // Anchored top-center so it stays centred on wider/narrower screens; pivot is centre
    // because UITutorialManager positions the tooltip from the button's pivot.
    static void PlaceTop(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(x + w / 2 - 540f, -(y + h / 2));
    }

    // Position inside a parent, measured from the parent's top-left.
    static void PlaceTopIn(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(x + w / 2, -(y + h / 2));
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void AddToBuildSettings(string path)
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (scenes.Exists(s => s.path == path)) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }
}
