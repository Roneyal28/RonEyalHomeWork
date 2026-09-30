using UnityEngine;
using UnityEngine.InputSystem;

public class BonusJoystickController : MonoBehaviour
{
    public CharacterSelectionController selection;
    public Transform cameraRig;
    public Camera viewCamera;
    public float moveSpeed = 2f;
    public float orbitSpeed = 80f;
    public float minimumPitch = -5f;
    public float maximumPitch = 55f;

    private float yaw;
    private float pitch = 12f;

    private void Start()
    {
        if (cameraRig != null)
        {
            Vector3 e = cameraRig.eulerAngles;
            yaw = e.y;
            pitch = NormalizeAngle(e.x);
        }
    }

    private void Update()
    {
        Vector2 move = Vector2.zero;
        Vector2 look = Vector2.zero;
        if (Gamepad.current != null)
        {
            move = Gamepad.current.leftStick.ReadValue();
            look = Gamepad.current.rightStick.ReadValue();
        }
        ApplyInput(move, look, Time.deltaTime);
    }

    public void ApplyInput(Vector2 move, Vector2 look, float deltaTime)
    {
        if (selection == null || selection.ActiveTransform == null || cameraRig == null) return;

        Vector3 forward = viewCamera != null ? viewCamera.transform.forward : Vector3.forward;
        Vector3 right = viewCamera != null ? viewCamera.transform.right : Vector3.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        selection.MoveWorld((right * move.x + forward * move.y) * moveSpeed * deltaTime);

        yaw += look.x * orbitSpeed * deltaTime;
        pitch = Mathf.Clamp(pitch - look.y * orbitSpeed * 0.65f * deltaTime, minimumPitch, maximumPitch);
        cameraRig.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private static float NormalizeAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }
}
