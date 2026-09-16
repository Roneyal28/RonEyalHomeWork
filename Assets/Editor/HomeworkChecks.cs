using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object = UnityEngine.Object;

// Optional editor-only smoke checks. Not part of the game's runtime code.
[InitializeOnLoad]
public static class HomeworkChecks
{
    private const string RunningKey = "ArmoryChecksRunning";
    private static double deadline;
    private static readonly List<string> results = new();

    static HomeworkChecks()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/ArsenalArmory.unity");
        SessionState.SetBool(RunningKey, true);
        EditorApplication.EnterPlaymode();
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(RunningKey, false) || state != PlayModeStateChange.EnteredPlayMode) return;
        deadline = EditorApplication.timeSinceStartup + 90;
        var runner = new GameObject("Verification").AddComponent<HomeworkSimulationRunner>();
        runner.steps = CheckGameplay();
        runner.finish = Finish;
        Application.logMessageReceived += OnLog;
        EditorApplication.update += Tick;
    }

    private static void OnLog(string message, string stack, LogType type)
    {
        // Unity 6.5 can fail to open its optional search index in batch mode.
        // This editor indexing issue is unrelated to the running scene.
        if (stack.Contains("UnityEditor.Search.SearchDatabase")) { EditorApplication.isPaused = false; return; }
        if (type == LogType.Exception || type == LogType.Error) Finish(false, message + "\n" + stack);
    }

    private static void Tick()
    {
        // Batch mode has no focused Game view, so explicitly advance each frame.
        EditorApplication.Step();
        if (EditorApplication.timeSinceStartup > deadline) { Finish(false, "Check timed out"); return; }
    }

    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception(name);
        results.Add("PASS: " + name);
        Debug.Log("PASS: " + name);
    }

    private static IEnumerator CheckGameplay()
    {
        yield return 0.3f;
        var gun = Object.FindFirstObjectByType<WeaponController>();
        var pool = Object.FindFirstObjectByType<PoolManager>();
        var target = GameObject.Find("Target 2").GetComponent<Target>();
        gun.enabled = false; // Isolate checks from the user's real mouse.
        Check(pool.CreatedCount == 30 && pool.ActiveCount == 0, "Scene prewarms 30 reusable bullets");
        var borrowed = new List<PooledObject>();
        for (int i = 0; i < 40; i++) borrowed.Add(pool.Get(gun.CurrentWeapon.bullet.prefab, Vector3.zero, Quaternion.identity));
        Check(pool.CreatedCount == 40 && pool.ActiveCount == 40, "Empty pool expands correctly");
        foreach (var item in borrowed) item.ReturnToPool();
        borrowed[0].ReturnToPool();
        Check(pool.ActiveCount == 0, "Return and duplicate-return guard keep correct counts");
        var reused = pool.Get(gun.CurrentWeapon.bullet.prefab, Vector3.zero, Quaternion.identity);
        Check(reused == borrowed[39] && pool.CreatedCount == 40, "Returned instance is reused without creating another bullet");
        reused.ReturnToPool();
        var otherPrefab = new GameObject("Other pooled type").AddComponent<PooledObject>();
        otherPrefab.gameObject.SetActive(false);
        pool.Prewarm(otherPrefab, 2);
        var other = pool.Get(otherPrefab, Vector3.zero, Quaternion.identity);
        Check(other.Prefab == otherPrefab && pool.CreatedCount == 42, "One pool manager supports another prefab type");
        other.ReturnToPool();

        gun.SelectWeapon(0);
        gun.transform.LookAt(target.transform.position);
        Check(gun.TryFire() && !gun.TryFire(), "Pistol fires one shot and respects cooldown");
        yield return new WaitUntil(() => pool.ActiveCount == 0);
        Check(target.CurrentHealth == 66 && pool.ActiveCount == 0, "3D bullet hit applies configured damage and returns to pool (health=" + target.CurrentHealth + ", active=" + pool.ActiveCount + ")");
        gun.TryFire();
        yield return new WaitUntil(() => pool.ActiveCount == 0);
        gun.TryFire();
        yield return new WaitUntil(() => pool.ActiveCount == 0);
        Check(target.CurrentHealth == 0 && target.Knockdowns == 1, "Target counts a knockdown at zero health");
        yield return new WaitUntil(() => target.CurrentHealth == 100);
        Check(target.CurrentHealth == 100 && target.GetComponent<Collider>().enabled, "Target resets without being destroyed or recreated");

        gun.SelectWeapon(2);
        gun.transform.rotation = Quaternion.Euler(0, 65, 0);
        Check(gun.TryFire() && pool.ActiveCount == 5, "Shotgun creates five active pooled projectiles");
        var bullets = Object.FindObjectsByType<Bullet>(FindObjectsSortMode.None);
        float min = 180, max = -180;
        foreach (var bullet in bullets) { float angle = bullet.transform.eulerAngles.y; min = Mathf.Min(min, angle); max = Mathf.Max(max, angle); }
        Check(Mathf.Abs(max - min - 24) < 0.1f, "Shotgun uses configured 24 degree spread");
        yield return new WaitUntil(() => pool.ActiveCount == 0);
        Check(pool.ActiveCount == 0, "Missed bullets return at maximum range");
        gun.SelectWeapon(1);
        Check(gun.TryFire(), "Automatic weapon fires");
        yield return 0.13f;
        Check(gun.TryFire(), "Automatic weapon permits another shot after 0.1 seconds");
        yield return new WaitUntil(() => pool.ActiveCount == 0);
        Check(pool.CreatedCount == 42 && pool.ActiveCount == 0, "Repeated firing reuses the warm pool");

        InputSystem.settings = Object.Instantiate(InputSystem.settings);
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        var hardwareMouse = Mouse.current;
        if (hardwareMouse != null) InputSystem.DisableDevice(hardwareMouse);
        var mouse = InputSystem.AddDevice<Mouse>();
        gun.SelectWeapon(0);
        gun.enabled = true;
        Vector2 aim = gun.aimCamera.WorldToScreenPoint(target.transform.position);
        SetMouse(mouse, new MouseState { position = aim });
        yield return 0.15f;
        int shots = gun.ShotsFired;
        SetMouse(mouse, new MouseState { position = aim }.WithButton(MouseButton.Left));
        yield return 0.65f;
        Check(gun.ShotsFired == shots + 1, "Holding mouse fires only once with the pistol (shots=" + (gun.ShotsFired - shots) + ")");
        SetMouse(mouse, new MouseState { position = aim });
        yield return 0.15f;
        var hud = Object.FindFirstObjectByType<ArmoryHUD>();
        shots = gun.ShotsFired;
        hud.weaponButtons[1].onClick.Invoke();
        Check(gun.SelectedIndex == 1 && gun.ShotsFired == shots, "Weapon button callback selects automatic without firing");
        SetMouse(mouse, new MouseState { position = aim });
        yield return 0.1f;
        SetMouse(mouse, new MouseState { position = aim }.WithButton(MouseButton.Left));
        yield return new WaitUntil(() => gun.ShotsFired >= shots + 4);
        Check(gun.ShotsFired >= shots + 4, "Holding mouse continuously fires the automatic weapon");
        SetMouse(mouse, new MouseState { position = aim });
        yield return 0.15f;
        shots = gun.ShotsFired;
        yield return 0.25f;
        Check(gun.ShotsFired == shots, "Releasing mouse stops automatic fire");
        gun.enabled = false;
        InputSystem.RemoveDevice(mouse);
        if (hardwareMouse != null) InputSystem.EnableDevice(hardwareMouse);
        yield return new WaitUntil(() => pool.ActiveCount == 0);
        gun.SelectWeapon(0);
        gun.transform.LookAt(target.transform.position);
        yield return 0.1f;
        Capture("Documentation/Screenshot.png", 1280, 720);
        Capture("/tmp/unity-homework-wide.png", 1920, 1080);
    }

    private static void SetMouse(Mouse mouse, MouseState state)
    {
        InputState.Change(mouse, state);
    }

    private static void Capture(string path, int width, int height)
    {
        var camera = Camera.main;
        var canvas = GameObject.Find("HUD").GetComponent<Canvas>();
        var render = new RenderTexture(width, height, 24);
        camera.targetTexture = render;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1;
        Canvas.ForceUpdateCanvases();
        camera.Render();
        RenderTexture.active = render;
        var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        RenderTexture.active = null;
        camera.targetTexture = null;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        Object.Destroy(texture);
        Object.Destroy(render);
    }

    private static void Finish(bool success, string message)
    {
        SessionState.SetBool(RunningKey, false);
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= OnLog;
        File.WriteAllText("/tmp/unity-homework-checks.txt", string.Join("\n", results) + "\n" + message);
        EditorApplication.Exit(success ? 0 : 1);
    }
}
