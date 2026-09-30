using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class Task2Checks
{
    private const string RunningKey = "Task2ChecksRunning";
    private static readonly List<string> results = new();
    private static double deadline;

    static Task2Checks()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    [MenuItem("Tools/Task 2/Run Verification")]
    public static void Run()
    {
        results.Clear();
        EditorSceneManager.OpenScene("Assets/Scenes/Character Selection.unity");
        SessionState.SetBool(RunningKey, true);
        EditorApplication.EnterPlaymode();
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(RunningKey, false) || state != PlayModeStateChange.EnteredPlayMode) return;
        deadline = EditorApplication.timeSinceStartup + 60;
        var runner = new GameObject("Task 2 Verification").AddComponent<HomeworkSimulationRunner>();
        runner.steps = CheckScene();
        runner.finish = Finish;
        Application.logMessageReceived += OnLog;
        EditorApplication.update += Tick;
    }

    private static void OnLog(string message, string stack, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error) Finish(false, message + "\n" + stack);
    }

    private static void Tick()
    {
        EditorApplication.Step();
        if (EditorApplication.timeSinceStartup > deadline) Finish(false, "Task 2 verification timed out");
    }

    private static IEnumerator CheckScene()
    {
        yield return 0.5f;
        var selection = Object.FindAnyObjectByType<CharacterSelectionController>();
        var environment = Object.FindAnyObjectByType<AmbientLightController>();
        var joystick = Object.FindAnyObjectByType<BonusJoystickController>();
        Check(selection != null && environment != null && joystick != null, "Task 2 controllers exist");
        Check(selection.characters.Length == 3 && selection.ActiveTransform != null, "Three ScriptableObject characters load");
        Check(selection.ActiveTransform.GetComponent<BoxCollider>() != null, "Active imported model gets a raycast collider");

        selection.SelectCharacter(2);
        Check(selection.ActiveData.displayName == "Zombie", "Character selection reads ScriptableObject data");
        selection.CycleCharacter(1);
        Check(selection.SelectedIndex == 0, "Horizontal cycling wraps at the end");

        selection.ApplyPinch(100000f);
        Check(Mathf.Abs(selection.ActiveTransform.localScale.x - selection.maximumScale) < 0.001f, "Pinch scale clamps at maximum");
        selection.ApplyPinch(-100000f);
        Check(Mathf.Abs(selection.ActiveTransform.localScale.x - selection.minimumScale) < 0.001f, "Pinch scale clamps at minimum");
        float oldYaw = selection.ActiveTransform.eulerAngles.y;
        selection.ApplyRotation(100f);
        Check(Mathf.Abs(Mathf.DeltaAngle(oldYaw, selection.ActiveTransform.eulerAngles.y)) > 1f, "Horizontal delta rotates around Y");
        selection.ConfirmSelection();
        Check(selection.IsConfirmed, "Double-tap confirmation action records selection");

        environment.SetSimulatedLux(0f);
        yield return 1.2f;
        Check(environment.FlashlightEnabled && selection.ActiveFlashlight.enabled, "Low light enables character lamp");
        float darkIntensity = environment.mainDirectionalLight.intensity;
        environment.SetSimulatedLux(500f);
        yield return 1.2f;
        Check(!environment.FlashlightEnabled && environment.mainDirectionalLight.intensity > darkIntensity, "Bright room disables lamp and brightens scene");

        Vector3 oldPosition = selection.ActiveTransform.position;
        Vector3 oldCameraPosition = joystick.cameraRig.position;
        Quaternion oldCameraRotation = joystick.cameraRig.rotation;
        joystick.ApplyInput(new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), 0.5f);
        Check(selection.ActiveTransform.position != oldPosition, "Left virtual stick moves active character");
        Check(joystick.cameraRig.position == oldCameraPosition, "Left virtual stick does not move camera rig");
        Check(joystick.cameraRig.rotation != oldCameraRotation, "Right virtual stick orbits camera");
        var sticks = Object.FindObjectsByType<OnScreenStick>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Check(sticks.Length == 2, "Two OnScreenStick controls exist");

        selection.SelectCharacter(0);
        environment.SetSimulatedLux(120f);
        yield return 0.4f;
        Capture("Documentation/Task2-Screenshot.png", 1280, 720);
        Check(File.Exists("Documentation/Task2-Screenshot.png"), "Task 2 screenshot captured");
    }

    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception(name);
        results.Add("PASS: " + name);
        Debug.Log("PASS: " + name);
    }

    private static void Capture(string path, int width, int height)
    {
        Camera camera = Camera.main;
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        var render = new RenderTexture(width, height, 24);
        camera.targetTexture = render;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1f;
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
        if (!SessionState.GetBool(RunningKey, false)) return;
        SessionState.SetBool(RunningKey, false);
        EditorApplication.update -= Tick;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        Application.logMessageReceived -= OnLog;
        File.WriteAllText("/tmp/unity-task2-checks.txt", string.Join("\n", results) + "\n" + message);
        if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
    }
}
