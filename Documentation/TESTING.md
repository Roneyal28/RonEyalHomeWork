# Verification

Unity 6000.5.3f1 compiles the project. Automated Play mode checks cover:

- Pool prewarming, expansion, instance reuse, duplicate returns, and multiple prefab types.
- Pistol cooldown, 3D hit damage, target knockdown, and target reset.
- Shotgun projectile count and spread angle.
- Bullet return on hit and at maximum range.
- Automatic firing cadence and warm-pool reuse.
- Synthetic mouse hold/release behavior and the weapon button's callback.

The headless editor discards queued mouse UI events, so button selection is checked by invoking the actual button callback. A physical mouse click on the UI still needs a quick manual check in the Game view. An unrelated Unity editor search-index exception is excluded from the gameplay checks. Android is not tested for this mouse-only version.

To rerun, close the project in Unity, then invoke your Unity executable with:

```text
-batchmode -projectPath /path/to/UnityMobile2026Summer -executeMethod HomeworkChecks.Run -logFile /tmp/unity-homework-test.log
```

The runner exits with code 0 on success and 1 on failure. It writes the check report to `/tmp/unity-homework-checks.txt` and renders `Documentation/Screenshot.png`. The normal startup pool contains 30 bullets.
