using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Editor-only setup. The saved scene and assets work without running this again.
public static class BuildHomeworkScene
{
    private static Sprite square;
    private static Font font;

    public static void Create()
    {
        if (File.Exists("Assets/Scenes/ArsenalArmory.unity"))
            throw new System.InvalidOperationException("Scene already exists; refusing to overwrite your work.");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSettings.serializationMode = SerializationMode.ForceText;
        PlayerSettings.productName = "Arsenal Armory";
        PlayerSettings.companyName = "Unity Mobile Class";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.unitymobileclass.arsenalarmory");
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        settings.FindProperty("activeInputHandler").intValue = 1;
        settings.ApplyModifiedPropertiesWithoutUndo();
        var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        tags.FindProperty("layers").GetArrayElementAtIndex(6).stringValue = "Target";
        tags.ApplyModifiedPropertiesWithoutUndo();

        var texture = new Texture2D(8, 8);
        var pixels = new Color[64];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        texture.SetPixels(pixels);
        texture.Apply();
        File.WriteAllBytes("Assets/Art/Placeholder.png", texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset("Assets/Art/Placeholder.png");
        var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Art/Placeholder.png");
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 8;
        importer.filterMode = FilterMode.Point;
        importer.SaveAndReimport();
        square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Placeholder.png");
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(0, 13, -16);
        camera.transform.LookAt(new Vector3(0, 0, 4));
        camera.fieldOfView = 55;
        var light = new GameObject("Directional Light").AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.3f;
        light.shadows = LightShadows.Soft;
        light.transform.rotation = Quaternion.Euler(50, -30, 0);
        RenderSettings.ambientLight = new Color(0.5f, 0.55f, 0.65f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.075f, 0.105f, 0.14f);

        var pool = new GameObject("Pool Manager").AddComponent<PoolManager>();
        var bulletObject = new GameObject("Bullet", typeof(Bullet));
        var bulletVisual = MakeSprite("Bullet Image", Vector3.zero, Vector2.one, Color.white, 5);
        bulletVisual.transform.SetParent(bulletObject.transform, false);
        bulletVisual.transform.localRotation = Quaternion.Euler(90, 0, 90);
        bulletObject.GetComponent<Bullet>().visual = bulletVisual;
        bulletObject.SetActive(false);
        var prefab = PrefabUtility.SaveAsPrefabAsset(bulletObject, "Assets/Prefabs/Bullet.prefab").GetComponent<Bullet>();
        Object.DestroyImmediate(bulletObject);

        var single = MakeType("Single", FireMode.Single, 1, 0);
        var automatic = MakeType("Automatic", FireMode.Automatic, 1, 0);
        var spread = MakeType("Spread", FireMode.Spread, 5, 24);
        var round = MakeBullet("PistolRound", prefab, 34, 14, 18, new Color(1, 0.8f, 0.3f), new Vector2(0.3f, 0.12f));
        var fastRound = MakeBullet("FastRound", prefab, 12, 20, 18, new Color(0.35f, 0.85f, 1), new Vector2(0.38f, 0.1f));
        var pellet = MakeBullet("Pellet", prefab, 20, 12, 15, new Color(1, 0.55f, 0.3f), new Vector2(0.17f, 0.17f));
        var weapons = new[] {
            MakeWeapon("Pistol", single, round, 0.35f),
            MakeWeapon("Automatic", automatic, fastRound, 0.1f),
            MakeWeapon("Shotgun", spread, pellet, 0.65f)
        };

        var gun = new GameObject("Player Gun").AddComponent<WeaponController>();
        gun.transform.position = new Vector3(0, 1, -3.4f);
        gun.weapons = weapons;
        gun.pool = pool;
        gun.aimCamera = camera;
        gun.muzzle = new GameObject("Muzzle").transform;
        gun.muzzle.SetParent(gun.transform, false);
        gun.muzzle.localPosition = new Vector3(0, 0, 0.7f);

        MakeCube("Range Floor", new Vector3(0, -0.25f, 4), new Vector3(18, 0.4f, 23), new Color(0.18f, 0.24f, 0.29f));
        MakeCube("Back Wall", new Vector3(0, 0.75f, 15.5f), new Vector3(18, 2, 0.3f), new Color(0.25f, 0.32f, 0.38f));
        for (int i = -8; i <= 8; i += 2)
            MakeCube("Lane Marking", new Vector3(i, -0.035f, 4), new Vector3(0.025f, 0.015f, 22), new Color(0.28f, 0.36f, 0.4f));
        MakeCube("Gun Stand", new Vector3(0, 0.4f, -3.4f), new Vector3(0.5f, 0.9f, 0.5f), new Color(0.3f, 0.37f, 0.42f));

        var canvas = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        MakePanel("Header", canvas.transform, new Vector2(0, 0.81f), Vector2.one, new Color(0.055f, 0.08f, 0.11f));
        MakeText("Title", canvas.transform, "ARSENAL ARMORY", 34, new Vector2(0.05f, 0.89f), new Vector2(0.6f, 0.98f), TextAnchor.MiddleLeft);
        MakeText("Instructions", canvas.transform, "Move mouse to aim. Left-click to shoot. Hold for automatic.", 20, new Vector2(0.05f, 0.815f), new Vector2(0.91f, 0.89f), TextAnchor.MiddleLeft);
        MakePanel("Controls", canvas.transform, Vector2.zero, new Vector2(1, 0.24f), new Color(0.055f, 0.08f, 0.11f));
        var weaponLabel = MakeText("Selected Weapon", canvas.transform, "Pistol / Single", 23, new Vector2(0.05f, 0.17f), new Vector2(0.55f, 0.235f), TextAnchor.MiddleLeft);
        var buttons = new Button[3];
        for (int i = 0; i < 3; i++)
        {
            float x = 0.05f + i * 0.305f;
            var panel = MakePanel(weapons[i].displayName + " Button", canvas.transform, new Vector2(x, 0.065f), new Vector2(x + 0.285f, 0.16f), new Color(0.19f, 0.25f, 0.31f));
            buttons[i] = panel.AddComponent<Button>();
            buttons[i].targetGraphic = panel.GetComponent<Image>();
            var navigation = buttons[i].navigation;
            navigation.mode = Navigation.Mode.None;
            buttons[i].navigation = navigation;
            MakeText("Label", panel.transform, weapons[i].displayName, 23, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter);
        }
        MakeText("Mouse Hint", canvas.transform, "Click a weapon above to switch. Targets reset automatically.", 16, new Vector2(0.05f, 0), new Vector2(0.95f, 0.065f), TextAnchor.MiddleCenter);

        var targets = new Target[3];
        var positions = new[] { new Vector3(-4, 1, 4), new Vector3(0, 1, 7), new Vector3(4, 1, 10) };
        for (int i = 0; i < targets.Length; i++)
        {
            var visual = MakeCube("Target " + (i + 1), positions[i], new Vector3(1.4f, 2, 0.6f), new Color(0.8f, 0.34f, 0.27f));
            visual.gameObject.layer = 6;
            var target = visual.gameObject.AddComponent<Target>();
            target.visual = visual;
            MakeCube("Target Stripe", positions[i] + new Vector3(0, 0, -0.32f), new Vector3(0.55f, 0.7f, 0.05f), new Color(1f, 0.79f, 0.47f));
            var healthCanvas = new GameObject("Target Health", typeof(Canvas)).GetComponent<Canvas>();
            healthCanvas.renderMode = RenderMode.WorldSpace;
            healthCanvas.transform.position = positions[i] + new Vector3(0, 1.65f, 0);
            healthCanvas.transform.rotation = camera.transform.rotation;
            healthCanvas.transform.localScale = Vector3.one * 0.02f;
            healthCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 50);
            healthCanvas.sortingOrder = 6;
            target.healthLabel = MakeText("Health", healthCanvas.transform, "100 / 100", 28, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter);
            targets[i] = target;
        }
        var hud = canvas.gameObject.AddComponent<ArmoryHUD>();
        hud.weapon = gun;
        hud.weaponLabel = weaponLabel; hud.weaponButtons = buttons;
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/ArsenalArmory.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/ArsenalArmory.unity", true) };
        AssetDatabase.SaveAssets();
        Debug.Log("HOMEWORK_SCENE_CREATED");
    }

    public static void MakeSimplePov()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/ArsenalArmory.unity");
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var camera = Camera.main;
        camera.transform.position = new Vector3(0f, 1.7f, -5.5f);
        camera.transform.rotation = Quaternion.identity;
        camera.fieldOfView = 60f;

        var gun = Object.FindFirstObjectByType<WeaponController>();
        gun.transform.SetParent(camera.transform, false);
        gun.transform.localPosition = new Vector3(0.65f, -0.62f, 1.5f);
        gun.transform.localRotation = Quaternion.identity;

        var stand = GameObject.Find("Gun Stand");
        if (stand != null) Object.DestroyImmediate(stand);

        SetTarget("Target 1", new Vector3(-3.2f, 1.1f, 5f));
        SetTarget("Target 2", new Vector3(0f, 1.1f, 8f));
        SetTarget("Target 3", new Vector3(3.2f, 1.1f, 11f));

        DestroyUi("Header");
        DestroyUi("Title");
        DestroyUi("Instructions");
        DestroyUi("Controls");
        DestroyUi("Pool Counts");
        DestroyUi("Mouse Hint");
        DestroyUi("Score");

        var canvas = GameObject.Find("HUD").GetComponent<Canvas>();
        var hud = canvas.GetComponent<ArmoryHUD>();
        SetTextLayout("Selected Weapon", "Pistol", 20,
            new Vector2(0.03f, 0.92f), new Vector2(0.2f, 0.98f), TextAnchor.MiddleLeft);

        for (int i = 0; i < hud.weaponButtons.Length; i++)
        {
            var rect = hud.weaponButtons[i].GetComponent<RectTransform>();
            float width = 0.12f;
            float gap = 0.012f;
            float start = 0.5f - (3f * width + 2f * gap) / 2f;
            rect.anchorMin = new Vector2(start + i * (width + gap), 0.035f);
            rect.anchorMax = new Vector2(start + i * (width + gap) + width, 0.105f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            hud.weaponButtons[i].GetComponentInChildren<Text>().fontSize = 17;
        }

        var crosshair = GameObject.Find("Crosshair");
        if (crosshair == null)
            crosshair = MakeText("Crosshair", canvas.transform, "+", 28,
                new Vector2(0.48f, 0.46f), new Vector2(0.52f, 0.54f), TextAnchor.MiddleCenter).gameObject;
        crosshair.GetComponent<Text>().font = font;
        crosshair.GetComponent<Text>().color = new Color(1f, 1f, 1f, 0.85f);

        foreach (var target in Object.FindObjectsByType<Target>(FindObjectsSortMode.None))
            target.healthLabel.transform.parent.gameObject.SetActive(false);

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("SIMPLE_POV_SCENE_SAVED");
    }

    [MenuItem("Tools/Apply Pool and ScriptableObject Update %#p")]
    public static void MakePoolAndScriptableObjectsObvious()
    {
        string moveError = AssetDatabase.MoveAsset("Assets/Data", "Assets/ScriptableObjects");
        if (!string.IsNullOrEmpty(moveError) && !AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            throw new System.Exception(moveError);

        EditorSceneManager.OpenScene("Assets/Scenes/ArsenalArmory.unity");
        DestroyUi("Score");
        var pool = Object.FindFirstObjectByType<PoolManager>();
        pool.name = "Pool Manager - 30 Bullets";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("POOL_AND_SCRIPTABLE_OBJECTS_UPDATED");
    }

    private static void SetTarget(string name, Vector3 position)
    {
        var target = GameObject.Find(name);
        Vector3 oldPosition = target.transform.position;
        // Each stripe is parentless, so match it to the target at its old position.
        Renderer nearest = null;
        float best = float.MaxValue;
        foreach (var renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (renderer.name != "Target Stripe") continue;
            float distance = Vector3.Distance(renderer.transform.position, oldPosition);
            if (distance < best) { best = distance; nearest = renderer; }
        }
        target.transform.position = position;
        if (nearest != null) nearest.transform.position = position + new Vector3(0f, 0f, -0.32f);
        var health = target.GetComponent<Target>().healthLabel.transform.parent;
        health.position = position + new Vector3(0f, 1.65f, 0f);
        health.rotation = Camera.main.transform.rotation;
    }

    private static void DestroyUi(string name)
    {
        var item = GameObject.Find(name);
        if (item != null) Object.DestroyImmediate(item);
    }

    private static void SetTextLayout(string name, string value, int size, Vector2 min, Vector2 max, TextAnchor alignment)
    {
        var text = GameObject.Find(name).GetComponent<Text>();
        text.text = value;
        text.fontSize = size;
        text.alignment = alignment;
        text.rectTransform.anchorMin = min;
        text.rectTransform.anchorMax = max;
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;
    }

    private static T Save<T>(T asset, string folder, string name) where T : ScriptableObject
    {
        AssetDatabase.CreateAsset(asset, "Assets/ScriptableObjects/" + folder + "/" + name + ".asset");
        return asset;
    }
    private static WeaponTypeData MakeType(string name, FireMode mode, int count, float angle)
    {
        var data = ScriptableObject.CreateInstance<WeaponTypeData>();
        data.fireMode = mode; data.projectilesPerShot = count; data.spreadAngle = angle;
        return Save(data, "WeaponTypes", name);
    }
    private static BulletData MakeBullet(string name, Bullet prefab, int damage, float speed, float range, Color color, Vector2 size)
    {
        var data = ScriptableObject.CreateInstance<BulletData>();
        data.prefab = prefab; data.damage = damage; data.speed = speed; data.range = range; data.color = color; data.size = size;
        return Save(data, "Bullets", name);
    }
    private static WeaponData MakeWeapon(string name, WeaponTypeData type, BulletData bullet, float delay)
    {
        var data = ScriptableObject.CreateInstance<WeaponData>();
        data.displayName = name; data.weaponType = type; data.bullet = bullet; data.shotDelay = delay;
        return Save(data, "Weapons", name);
    }
    private static SpriteRenderer MakeSprite(string name, Vector3 position, Vector2 size, Color color, int order)
    {
        var sprite = new GameObject(name).AddComponent<SpriteRenderer>();
        sprite.sprite = square; sprite.color = color; sprite.sortingOrder = order;
        sprite.transform.position = position;
        sprite.transform.localScale = new Vector3(size.x, size.y, 1);
        return sprite;
    }
    private static Renderer MakeCube(string name, Vector3 position, Vector3 size, Color color)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = size;
        var renderer = cube.GetComponent<Renderer>();
        string path = "Assets/Art/Material-" + ColorUtility.ToHtmlStringRGB(color) + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard"));
            material.color = color;
            AssetDatabase.CreateAsset(material, path);
        }
        renderer.sharedMaterial = material;
        return renderer;
    }
    private static GameObject MakePanel(string name, Transform parent, Vector2 min, Vector2 max, Color color)
    {
        var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        Place(panel.GetComponent<RectTransform>(), parent, min, max);
        panel.GetComponent<Image>().color = color;
        return panel;
    }
    private static Text MakeText(string name, Transform parent, string value, int size, Vector2 min, Vector2 max, TextAnchor alignment)
    {
        var text = new GameObject(name, typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        Place(text.rectTransform, parent, min, max);
        text.font = font; text.fontSize = size; text.text = value;
        text.color = new Color(0.88f, 0.93f, 0.96f); text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }
    private static void Place(RectTransform rect, Transform parent, Vector2 min, Vector2 max)
    {
        rect.SetParent(parent, false); rect.anchorMin = min; rect.anchorMax = max;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
    }
}
