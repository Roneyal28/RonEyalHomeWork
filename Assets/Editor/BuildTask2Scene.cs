using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class BuildTask2Scene
{
    private const string ScenePath = "Assets/Scenes/Character Selection.unity";
    private const string TaskRoot = "Assets/Task2";
    private static Font font;

    static BuildTask2Scene()
    {
        EditorApplication.delayCall += BuildOnce;
    }

    [MenuItem("Tools/Task 2/Rebuild Character Selection Scene")]
    public static void Build()
    {
        Directory.CreateDirectory(TaskRoot + "/Data");
        Directory.CreateDirectory(TaskRoot + "/Icons");
        Directory.CreateDirectory(TaskRoot + "/Materials");
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Scene scene = File.Exists(ScenePath) ? EditorSceneManager.OpenScene(ScenePath) : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        foreach (GameObject root in scene.GetRootGameObjects()) Object.DestroyImmediate(root);

        CharacterData[] data =
        {
            MakeCharacter("Male A", "Assets/ToonyTinyPeople/TT_demo/prefabs/TT_demo_male_A.prefab", new Color(0.2f, 0.7f, 0.95f)),
            MakeCharacter("Female", "Assets/ToonyTinyPeople/TT_demo/prefabs/TT_demo_female.prefab", new Color(0.95f, 0.45f, 0.75f)),
            MakeCharacter("Zombie", "Assets/ToonyTinyPeople/TT_demo/prefabs/TT_demo_zombie.prefab", new Color(0.45f, 0.85f, 0.4f))
        };

        Material floorMaterial = MakeMaterial("Floor", new Color(0.08f, 0.12f, 0.17f));
        Material pedestalMaterial = MakeMaterial("Pedestal", new Color(0.15f, 0.42f, 0.52f));

        var systems = new GameObject("Task 2 Systems");
        var characterContainer = new GameObject("Character Display").transform;
        characterContainer.position = Vector3.zero;

        var cameraRig = new GameObject("Camera Orbit Rig").transform;
        cameraRig.position = new Vector3(0f, 1.1f, 0f);
        cameraRig.rotation = Quaternion.Euler(12f, 0f, 0f);
        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(cameraRig, false);
        cameraObject.transform.localPosition = new Vector3(0f, 0f, -6.2f);
        var camera = cameraObject.GetComponent<Camera>();
        camera.fieldOfView = 42f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.025f, 0.045f, 0.075f);

        var lightObject = new GameObject("Main Directional Light", typeof(Light));
        lightObject.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
        var directional = lightObject.GetComponent<Light>();
        directional.type = LightType.Directional;
        directional.intensity = 1f;
        directional.color = new Color(1f, 0.94f, 0.86f);

        MakeCube("Floor", new Vector3(0f, -0.22f, 1f), new Vector3(14f, 0.3f, 12f), floorMaterial);
        MakeCube("Display Pedestal", new Vector3(0f, -0.08f, 0f), new Vector3(2.8f, 0.22f, 2.8f), pedestalMaterial);

        Canvas canvas = new GameObject("Task 2 HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Text title = MakeText(canvas.transform, "Title", "TASK 2  |  CHARACTER LAB", 34, TextAnchor.UpperCenter,
            new Vector2(0.25f, 0.91f), new Vector2(0.75f, 0.99f), new Color(0.84f, 0.95f, 1f));
        Text selected = MakeText(canvas.transform, "Selected Character", "", 42, TextAnchor.MiddleCenter,
            new Vector2(0.30f, 0.82f), new Vector2(0.70f, 0.91f), Color.white);
        Text instructions = MakeText(canvas.transform, "Instructions",
            "Swipe empty space: switch   |   Drag model: move   |   Pinch: scale\nHold one finger + move second: rotate   |   Double-tap: confirm",
            21, TextAnchor.UpperCenter, new Vector2(0.20f, 0.72f), new Vector2(0.80f, 0.82f), new Color(0.72f, 0.82f, 0.9f));
        Text confirmation = MakeText(canvas.transform, "Confirmation", "Double-tap the character to confirm", 25, TextAnchor.MiddleCenter,
            new Vector2(0.28f, 0.10f), new Vector2(0.72f, 0.18f), new Color(1f, 0.8f, 0.35f));
        MakeText(canvas.transform, "Editor Help", "EDITOR: mouse drag | wheel scale | Shift-drag rotate | double-click confirm", 17,
            TextAnchor.LowerCenter, new Vector2(0.22f, 0.015f), new Vector2(0.78f, 0.07f), new Color(0.52f, 0.63f, 0.72f));

        RectTransform sensorPanel = MakePanel(canvas.transform, "Light Sensor Panel", new Vector2(0.72f, 0.83f), new Vector2(0.985f, 0.985f), new Color(0.035f, 0.09f, 0.14f, 0.9f));
        Text sensorLabel = MakeText(sensorPanel, "Sensor Status", "Editor light simulation", 18, TextAnchor.UpperCenter,
            new Vector2(0.04f, 0.56f), new Vector2(0.96f, 0.96f), Color.white);
        RectTransform sliderRow = MakePanel(sensorPanel, "Simulation Slider", new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.52f), new Color(0f, 0f, 0f, 0f));
        Slider slider = MakeSlider(sliderRow);

        RectTransform leftZone = MakeJoystick(canvas.transform, "Left Joystick - Move", new Vector2(0.03f, 0.08f), new Vector2(0.20f, 0.38f), "<Gamepad>/leftStick", new Color(0.15f, 0.62f, 0.8f, 0.25f));
        RectTransform rightZone = MakeJoystick(canvas.transform, "Right Joystick - Orbit", new Vector2(0.80f, 0.08f), new Vector2(0.97f, 0.38f), "<Gamepad>/rightStick", new Color(0.8f, 0.42f, 0.18f, 0.25f));
        MakeText(leftZone, "Label", "MOVE", 18, TextAnchor.LowerCenter, new Vector2(0f, 0f), new Vector2(1f, 0.22f), Color.white);
        MakeText(rightZone, "Label", "CAMERA", 18, TextAnchor.LowerCenter, new Vector2(0f, 0f), new Vector2(1f, 0.22f), Color.white);

        var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();

        var selection = systems.AddComponent<CharacterSelectionController>();
        selection.characters = data;
        selection.characterContainer = characterContainer;
        selection.viewCamera = camera;
        selection.selectedLabel = selected;
        selection.confirmationLabel = confirmation;
        selection.ignoredTouchAreas = new[] { leftZone, rightZone, sensorPanel };

        var environment = systems.AddComponent<AmbientLightController>();
        environment.mainDirectionalLight = directional;
        environment.selection = selection;
        environment.simulatedLuxSlider = slider;
        environment.sensorLabel = sensorLabel;

        var joysticks = systems.AddComponent<BonusJoystickController>();
        joysticks.selection = selection;
        joysticks.cameraRig = cameraRig;
        joysticks.viewCamera = camera;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.32f, 0.36f, 0.42f);
        RenderSettings.ambientIntensity = 0.6f;
        RenderSettings.fog = false;

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true),
            new EditorBuildSettingsScene("Assets/Scenes/ArsenalArmory.unity", true)
        };
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = systems;
        Debug.Log("TASK2_SCENE_CREATED");
    }

    private static void BuildOnce()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path == ScenePath && GameObject.Find("Task 2 Systems") != null) return;
        Build();
    }

    private static CharacterData MakeCharacter(string displayName, string prefabPath, Color iconColor)
    {
        string safeName = displayName.Replace(" ", "");
        string iconPath = TaskRoot + "/Icons/" + safeName + ".png";
        if (!File.Exists(iconPath))
        {
            var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[64 * 64];
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                {
                    float dx = x - 31.5f;
                    float dy = y - 31.5f;
                    pixels[y * 64 + x] = dx * dx + dy * dy < 850f ? iconColor : new Color(0f, 0f, 0f, 0f);
                }
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(iconPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(iconPath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(iconPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }

        string assetPath = TaskRoot + "/Data/" + safeName + ".asset";
        CharacterData data = AssetDatabase.LoadAssetAtPath<CharacterData>(assetPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<CharacterData>();
            AssetDatabase.CreateAsset(data, assetPath);
        }
        data.displayName = displayName;
        data.prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        data.icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
        EditorUtility.SetDirty(data);
        return data;
    }

    private static Material MakeMaterial(string name, Color color)
    {
        string path = TaskRoot + "/Materials/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.color = color;
        material.SetFloat("_Glossiness", 0.15f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static GameObject MakeCube(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().sharedMaterial = material;
        return cube;
    }

    private static RectTransform MakePanel(Transform parent, string name, Vector2 min, Vector2 max, Color color)
    {
        var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = color;
        return rect;
    }

    private static Text MakeText(Transform parent, string name, string value, int size, TextAnchor alignment, Vector2 min, Vector2 max, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var text = go.GetComponent<Text>();
        text.text = value;
        text.font = font;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    private static Slider MakeSlider(RectTransform parent)
    {
        var resources = new DefaultControls.Resources
        {
            standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
            background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
            inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
            knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
            checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd"),
            dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd"),
            mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd")
        };
        GameObject go = DefaultControls.CreateSlider(resources);
        go.name = "Simulated Lux";
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.15f);
        rect.anchorMax = new Vector2(1f, 0.85f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return go.GetComponent<Slider>();
    }

    private static RectTransform MakeJoystick(Transform parent, string name, Vector2 min, Vector2 max, string controlPath, Color color)
    {
        RectTransform zone = MakePanel(parent, name, min, max, color);
        var knob = new GameObject("Stick", typeof(RectTransform), typeof(Image), typeof(OnScreenStick));
        knob.transform.SetParent(zone, false);
        RectTransform rect = knob.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.55f);
        rect.sizeDelta = new Vector2(105f, 105f);
        rect.anchoredPosition = Vector2.zero;
        knob.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 0.75f);
        OnScreenStick stick = knob.GetComponent<OnScreenStick>();
        stick.controlPath = controlPath;
        stick.movementRange = 90f;
        stick.behaviour = OnScreenStick.Behaviour.RelativePositionWithStaticOrigin;
        stick.useIsolatedInputActions = true;
        return zone;
    }
}
