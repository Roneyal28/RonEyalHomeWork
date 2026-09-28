using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AmbientLightController : MonoBehaviour
{
    public Light mainDirectionalLight;
    public CharacterSelectionController selection;
    public Slider simulatedLuxSlider;
    public Text sensorLabel;

    [Header("Lux Mapping")]
    public float maximumLux = 500f;
    public float minimumDirectionalIntensity = 0.15f;
    public float maximumDirectionalIntensity = 1.2f;
    public float flashlightOnLux = 15f;
    public float flashlightOffLux = 25f;
    public float smoothingSpeed = 5f;

    public float CurrentLux { get; private set; } = 100f;
    public bool UsingRealSensor { get; private set; }
    public bool FlashlightEnabled { get; private set; }

    private float targetLux = 100f;
    private LightSensor sensor;

    private void OnEnable()
    {
        TryEnableSensor();
        if (simulatedLuxSlider != null)
        {
            simulatedLuxSlider.minValue = 0f;
            simulatedLuxSlider.maxValue = maximumLux;
            simulatedLuxSlider.value = targetLux;
            simulatedLuxSlider.onValueChanged.AddListener(SetSimulatedLux);
        }
    }

    private void OnDisable()
    {
        if (simulatedLuxSlider != null) simulatedLuxSlider.onValueChanged.RemoveListener(SetSimulatedLux);
        if (sensor != null && sensor.enabled) InputSystem.DisableDevice(sensor);
    }

    private void TryEnableSensor()
    {
        sensor = LightSensor.current;
        UsingRealSensor = sensor != null;
        if (UsingRealSensor)
        {
            InputSystem.EnableDevice(sensor);
            sensor.samplingFrequency = 10f;
        }
        UpdateSensorUi();
    }

    private void Update()
    {
        if (!UsingRealSensor && LightSensor.current != null) TryEnableSensor();
        if (UsingRealSensor && sensor != null) targetLux = Mathf.Max(0f, sensor.lightLevel.ReadValue());

        float blend = 1f - Mathf.Exp(-smoothingSpeed * Time.unscaledDeltaTime);
        CurrentLux = Mathf.Lerp(CurrentLux, targetLux, blend);
        ApplyLighting(CurrentLux);
    }

    public void SetSimulatedLux(float lux)
    {
        if (UsingRealSensor) return;
        targetLux = Mathf.Clamp(lux, 0f, maximumLux);
        UpdateSensorUi();
    }

    private void ApplyLighting(float lux)
    {
        float t = Mathf.InverseLerp(0f, maximumLux, lux);
        if (mainDirectionalLight != null)
            mainDirectionalLight.intensity = Mathf.Lerp(minimumDirectionalIntensity, maximumDirectionalIntensity, t);
        RenderSettings.ambientIntensity = Mathf.Lerp(0.25f, 1f, t);

        if (!FlashlightEnabled && lux <= flashlightOnLux) FlashlightEnabled = true;
        else if (FlashlightEnabled && lux >= flashlightOffLux) FlashlightEnabled = false;

        if (selection != null && selection.ActiveFlashlight != null)
            selection.ActiveFlashlight.enabled = FlashlightEnabled;
        UpdateSensorUi();
    }

    private void UpdateSensorUi()
    {
        if (simulatedLuxSlider != null) simulatedLuxSlider.transform.parent.gameObject.SetActive(!UsingRealSensor);
        if (sensorLabel != null)
            sensorLabel.text = (UsingRealSensor ? "Room light sensor" : "Editor light simulation") + "  " + CurrentLux.ToString("0") + " lux" + (FlashlightEnabled ? "  |  lamp ON" : "");
    }
}
