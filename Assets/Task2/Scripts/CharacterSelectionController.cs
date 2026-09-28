using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using ETouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class CharacterSelectionController : MonoBehaviour
{
    [Header("Character ScriptableObjects")]
    public CharacterData[] characters;
    public Transform characterContainer;
    public Camera viewCamera;

    [Header("Simple UI")]
    public Text selectedLabel;
    public Text confirmationLabel;
    public RectTransform[] ignoredTouchAreas;

    [Header("Gesture Tuning")]
    [Range(0.02f, 0.25f)] public float swipeScreenFraction = 0.08f;
    [Range(0.001f, 0.03f)] public float pinchScreenFraction = 0.004f;
    public float rotationDegreesPerPixel = 0.22f;
    public float minimumScale = 0.6f;
    public float maximumScale = 1.6f;
    public float doubleTapSeconds = 0.32f;

    public int SelectedIndex { get; private set; }
    public CharacterData ActiveData => characters != null && characters.Length > 0 ? characters[SelectedIndex] : null;
    public Transform ActiveTransform { get; private set; }
    public Light ActiveFlashlight { get; private set; }
    public bool IsConfirmed { get; private set; }
    public event Action<Transform> ActiveCharacterChanged;

    private int primaryTouchId = -1;
    private bool trackingOneFinger;
    private bool oneFingerStartedOnModel;
    private bool oneFingerStartedOverUi;
    private bool twoFingerGesture;
    private bool suppressUntilAllReleased;
    private Vector2 oneFingerStart;
    private Vector2 oneFingerLast;
    private float previousPinchDistance;
    private float scaleMultiplier = 1f;
    private float lastMouseClickTime = -10f;
    private Vector2 mouseStart;
    private Vector2 mouseLast;
    private bool mouseStartedOnModel;
    private bool mouseStartedOverUi;
    private bool mouseDragging;

    private void OnEnable() => EnhancedTouchSupport.Enable();
    private void OnDisable() => EnhancedTouchSupport.Disable();

    private void Start()
    {
        if (characters == null || characters.Length == 0)
        {
            Debug.LogError("Task 2 needs at least one CharacterData asset.");
            enabled = false;
            return;
        }
        SelectCharacter(0);
    }

    private void Update()
    {
        if (ETouch.activeTouches.Count > 0 || trackingOneFinger || twoFingerGesture)
            HandleTouches();
        else
            HandleEditorMouse();
    }

    public void SelectCharacter(int index)
    {
        if (characters == null || characters.Length == 0) return;
        SelectedIndex = (index % characters.Length + characters.Length) % characters.Length;
        IsConfirmed = false;
        if (confirmationLabel != null) confirmationLabel.text = "Double-tap the character to confirm";

        if (ActiveTransform != null)
        {
            ActiveTransform.gameObject.SetActive(false);
            Destroy(ActiveTransform.gameObject);
        }

        var root = new GameObject("Active Character - " + ActiveData.displayName).transform;
        root.SetParent(characterContainer, false);
        root.localPosition = Vector3.zero;
        root.localRotation = Quaternion.Euler(0f, 180f, 0f);
        root.localScale = Vector3.one;

        var model = Instantiate(ActiveData.prefab, root);
        model.name = ActiveData.prefab.name;
        NormalizeModel(root, model.transform);

        ActiveFlashlight = root.gameObject.AddComponent<Light>();
        ActiveFlashlight.name = "Character Low-Light Lamp";
        ActiveFlashlight.type = LightType.Point;
        ActiveFlashlight.range = 5f;
        ActiveFlashlight.intensity = 2.5f;
        ActiveFlashlight.color = new Color(1f, 0.88f, 0.65f);
        ActiveFlashlight.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        ActiveFlashlight.enabled = false;

        ActiveTransform = root;
        scaleMultiplier = 1f;
        if (selectedLabel != null) selectedLabel.text = ActiveData.displayName;
        ActiveCharacterChanged?.Invoke(ActiveTransform);
    }

    private static void NormalizeModel(Transform root, Transform model)
    {
        var renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        if (bounds.size.y > 0.001f) model.localScale = Vector3.one * (2.2f / bounds.size.y);

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        model.position += root.position - new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        var box = root.gameObject.AddComponent<BoxCollider>();
        box.center = root.InverseTransformPoint(bounds.center);
        box.size = bounds.size;
    }

    public void CycleCharacter(int direction) => SelectCharacter(SelectedIndex + (direction >= 0 ? 1 : -1));

    public void ApplyPinch(float screenDistanceDelta)
    {
        if (ActiveTransform == null) return;
        float minEdge = Mathf.Max(1f, Mathf.Min(Screen.width, Screen.height));
        scaleMultiplier = Mathf.Clamp(scaleMultiplier + screenDistanceDelta / minEdge * 2f, minimumScale, maximumScale);
        ActiveTransform.localScale = Vector3.one * scaleMultiplier;
    }

    public void ApplyRotation(float horizontalPixels)
    {
        if (ActiveTransform != null)
            ActiveTransform.Rotate(0f, -horizontalPixels * rotationDegreesPerPixel, 0f, Space.World);
    }

    public void MoveByScreenDelta(Vector2 screenDelta)
    {
        if (ActiveTransform == null || viewCamera == null) return;
        float unitsPerPixel = 4f / Mathf.Max(1f, Mathf.Min(Screen.width, Screen.height));
        Vector3 delta = (viewCamera.transform.right * screenDelta.x + viewCamera.transform.up * screenDelta.y) * unitsPerPixel;
        MoveWorld(delta);
    }

    public void MoveWorld(Vector3 delta)
    {
        if (ActiveTransform == null) return;
        Vector3 p = ActiveTransform.position + delta;
        p.x = Mathf.Clamp(p.x, -4f, 4f);
        p.y = Mathf.Clamp(p.y, 0f, 2.5f);
        p.z = Mathf.Clamp(p.z, -2.5f, 3f);
        ActiveTransform.position = p;
    }

    public void ConfirmSelection()
    {
        if (ActiveData == null) return;
        IsConfirmed = true;
        if (confirmationLabel != null) confirmationLabel.text = "Selected: " + ActiveData.displayName;
    }

    private void HandleTouches()
    {
        var touches = ETouch.activeTouches;
        int liveCount = 0;
        ETouch first = default;
        ETouch second = default;
        ETouch ended = default;
        bool hasEnded = false;

        foreach (var touch in touches)
        {
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                if (touch.touchId == primaryTouchId) { ended = touch; hasEnded = true; }
                continue;
            }
            if (liveCount == 0) first = touch;
            else if (liveCount == 1) second = touch;
            liveCount++;
        }

        if (liveCount >= 2)
        {
            HandleTwoTouches(first, second);
            return;
        }

        if (twoFingerGesture)
        {
            if (liveCount == 0)
            {
                twoFingerGesture = false;
                suppressUntilAllReleased = false;
                trackingOneFinger = false;
                primaryTouchId = -1;
            }
            else suppressUntilAllReleased = true;
            return;
        }

        if (suppressUntilAllReleased)
        {
            if (liveCount == 0) suppressUntilAllReleased = false;
            return;
        }

        if (liveCount == 1)
        {
            if (!trackingOneFinger || first.phase == TouchPhase.Began)
                BeginOneFinger(first.touchId, first.screenPosition);
            else if (first.touchId == primaryTouchId && first.phase == TouchPhase.Moved)
            {
                Vector2 delta = first.screenPosition - oneFingerLast;
                if (oneFingerStartedOnModel && !oneFingerStartedOverUi) MoveByScreenDelta(delta);
                oneFingerLast = first.screenPosition;
            }
        }

        if (hasEnded && trackingOneFinger) EndOneFinger(ended.screenPosition, ended.tapCount);
    }

    private void HandleTwoTouches(ETouch a, ETouch b)
    {
        if (!twoFingerGesture)
        {
            twoFingerGesture = true;
            trackingOneFinger = false;
            previousPinchDistance = Vector2.Distance(a.screenPosition, b.screenPosition);
        }

        float distance = Vector2.Distance(a.screenPosition, b.screenPosition);
        float minEdge = Mathf.Max(1f, Mathf.Min(Screen.width, Screen.height));
        bool bothMoved = a.phase == TouchPhase.Moved && b.phase == TouchPhase.Moved;
        float distanceDelta = distance - previousPinchDistance;

        // The assignment requires both fingers to move before scale logic executes.
        if (bothMoved && Mathf.Abs(distanceDelta) >= minEdge * pinchScreenFraction)
        {
            ApplyPinch(distanceDelta);
        }
        else
        {
            ETouch moving = a;
            ETouch held = b;
            if (b.delta.sqrMagnitude > a.delta.sqrMagnitude) { moving = b; held = a; }
            bool heldStill = held.phase == TouchPhase.Stationary || held.delta.magnitude < minEdge * 0.004f;
            bool horizontalMove = moving.phase == TouchPhase.Moved && Mathf.Abs(moving.delta.x) > Mathf.Abs(moving.delta.y);
            if (heldStill && horizontalMove) ApplyRotation(moving.delta.x);
        }
        previousPinchDistance = distance;
    }

    private void BeginOneFinger(int touchId, Vector2 position)
    {
        primaryTouchId = touchId;
        trackingOneFinger = true;
        oneFingerStart = oneFingerLast = position;
        oneFingerStartedOverUi = IsIgnoredPosition(position);
        oneFingerStartedOnModel = !oneFingerStartedOverUi && IsPointOnModel(position);
    }

    private void EndOneFinger(Vector2 position, int tapCount)
    {
        Vector2 travel = position - oneFingerStart;
        float minEdge = Mathf.Max(1f, Mathf.Min(Screen.width, Screen.height));
        if (!oneFingerStartedOverUi)
        {
            if (oneFingerStartedOnModel && tapCount >= 2 && travel.magnitude < minEdge * 0.025f)
                ConfirmSelection();
            else if (!oneFingerStartedOnModel && Mathf.Abs(travel.x) >= Screen.width * swipeScreenFraction && Mathf.Abs(travel.x) > Mathf.Abs(travel.y))
                CycleCharacter(travel.x < 0f ? 1 : -1);
        }
        trackingOneFinger = false;
        primaryTouchId = -1;
    }

    private void HandleEditorMouse()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;
        Vector2 position = mouse.position.ReadValue();
        Vector2 scroll = mouse.scroll.ReadValue();
        if (Mathf.Abs(scroll.y) > 0.01f) ApplyPinch(scroll.y * 0.08f);

        if (mouse.leftButton.wasPressedThisFrame)
        {
            mouseStart = mouseLast = position;
            mouseStartedOverUi = IsIgnoredPosition(position) || (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject());
            mouseStartedOnModel = !mouseStartedOverUi && IsPointOnModel(position);
            mouseDragging = true;
        }
        if (mouseDragging && mouse.leftButton.isPressed)
        {
            Vector2 delta = position - mouseLast;
            if (mouseStartedOnModel && !mouseStartedOverUi)
            {
                if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed) ApplyRotation(delta.x);
                else MoveByScreenDelta(delta);
            }
            mouseLast = position;
        }
        if (mouseDragging && mouse.leftButton.wasReleasedThisFrame)
        {
            Vector2 travel = position - mouseStart;
            if (!mouseStartedOverUi)
            {
                if (mouseStartedOnModel && travel.magnitude < 12f)
                {
                    float now = Time.unscaledTime;
                    if (now - lastMouseClickTime <= doubleTapSeconds) ConfirmSelection();
                    lastMouseClickTime = now;
                }
                else if (!mouseStartedOnModel && Mathf.Abs(travel.x) >= Screen.width * swipeScreenFraction && Mathf.Abs(travel.x) > Mathf.Abs(travel.y))
                    CycleCharacter(travel.x < 0f ? 1 : -1);
            }
            mouseDragging = false;
        }
    }

    private bool IsPointOnModel(Vector2 screenPosition)
    {
        if (ActiveTransform == null || viewCamera == null) return false;
        Ray ray = viewCamera.ScreenPointToRay(screenPosition);
        return Physics.Raycast(ray, out RaycastHit hit, 100f) && hit.transform.IsChildOf(ActiveTransform);
    }

    private bool IsIgnoredPosition(Vector2 screenPosition)
    {
        if (ignoredTouchAreas == null) return false;
        foreach (var area in ignoredTouchAreas)
            if (area != null && area.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(area, screenPosition))
                return true;
        return false;
    }
}
