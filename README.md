# Unity Mobile Homework - Ron Eyal

Unity 6000.5.3f1 using Input System 1.19.0 and the Built-in Render Pipeline.

## Task 2 - Mobile Input and Sensors

Open `Assets/Scenes/Character Selection.unity` and press Play. The scene uses three `CharacterData` ScriptableObjects: Male A, Female, and Zombie.

### Mobile controls

- Swipe horizontally on empty viewport space to switch character.
- Drag directly on the character with one finger to move it.
- Move two fingers together in a pinch to scale the character. Scaling runs only while both touches are moving.
- Hold one finger and move the second left or right to rotate around Y. Vertical movement is ignored.
- Double-tap the character to confirm it.
- Left virtual joystick moves the character across the ground.
- Right virtual joystick orbits the camera.

Touches that begin on the joysticks or light controls are ignored by the character gesture reader.

### Editor controls

The mouse controls make the project demonstrable without an Android phone:

- Drag the character to move it.
- Drag empty space horizontally to switch.
- Use the mouse wheel to scale.
- Hold Shift while dragging the character to rotate.
- Double-click the character to confirm.

### Ambient-light sensor

`AmbientLightController` checks `LightSensor.current`, enables it at 10 Hz, reads `lightLevel` in lux, and disables it cleanly. Room brightness controls the main directional light and ambient intensity. At 15 lux or below, the character lamp turns on; at 25 lux or above, it turns off.

Unity Remote does not forward Android ambient-light readings. When the sensor is unavailable, the top-right **Editor light simulation** slider supplies 0-500 lux so both the dynamic lighting and threshold reaction can be demonstrated.

No Android permission is needed for the ambient-light sensor. A real Android device may not contain this sensor, so the code always checks for `null` and uses the simulator when absent.

### Android build

1. Open **File > Build Profiles** and select Android.
2. Switch Platform if needed.
3. Confirm `Character Selection` is the first enabled scene.
4. Choose **Build** and save the APK outside the Unity project.
5. Install the APK on an Android phone to validate real light-sensor data.

The project is currently verified in the Editor because no Android test device was available.

### Verification

Use **Tools > Task 2 > Run Verification**. It checks the three data assets, character creation, collider, cycling, scale limits, rotation, confirmation, simulated lux, low-light lamp, both virtual sticks, and screenshot capture.

The latest result is saved at `Documentation/Task2-Screenshot.png`.

## Task 1

The original shooting-range homework remains in `Assets/Scenes/ArsenalArmory.unity`. Task 2 does not change its gameplay.

## Submission

Task 2 belongs on branch `Task2` in the same repository. Commit Unity `.meta` files together with every asset, then push the branch:

```bash
git add Assets Packages ProjectSettings Documentation README.md .gitignore
git commit -m "Complete Task 2 mobile input and sensors"
git push -u origin Task2
```

Send the GitHub link for the `Task2` branch to `adigaash@gmail.com`.
