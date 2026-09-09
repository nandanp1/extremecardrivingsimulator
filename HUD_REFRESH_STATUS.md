# Handbrake layout and high-refresh presentation

Version `4.17.6-reconstructed.2` / Android version code `20071`.

## Handbrake fix

The recovered `BtHandBrake` sprite mixed a panel-bottom anchor with a
panel-center anchor. On wide screens the HUD panel is subsequently scaled to
0.68. Airport loading calls `HideHUD()` / `ShowHUD()`, causing NGUI's on-enable
layout to recalculate the inconsistent anchors: a 102-pixel-high button became
271 pixels high. Both vertical anchors now use the panel center, preserving
the original 102-by-102 local size and initial center Y=57. This does not enlarge
the touch collider, change input handling, or apply a per-frame scale override.

`HandbrakeHudScaleDiagnostic.Run` passed against the real NGUI layout classes
and anchors read from the shipping scene: 16:9, 20:9 and 21:9 viewports, 15
additive-scene/HUD-enable cycles and 30 press/release events. The old-anchor
negative control reproduced the 271-pixel height. These are isolated layout
fixtures, not automated traversal of the entire airport scene.

## Refresh-rate policy

`Manager` now owns a persistent `DisplayFrameRateController` instead of
unconditionally setting a 60 FPS cap. On Android it:

- Queries the actual display's supported physical modes on the Android UI thread.
- Prefers a same-resolution 120 Hz mode, accepting fractional rates within
  0.5 Hz. Otherwise targets 60 FPS and requests a matching 60 Hz mode if available.
- Uses 60 FPS while Android battery saver is active, and rechecks on resume,
  focus and at two-second intervals. It does not repeatedly override an
  unchanged mode preference or change device-wide refresh settings.
- Enables Unity's optimized Android frame pacing. Unity API changes happen on
  the Unity thread, not in asynchronous Java callbacks.
- Logs selected target, requested mode, reported active refresh rate and a
  separate ten-second Unity Update-rate measurement under `[DisplayRate]`.

The recovered physics timestep remains 0.02 seconds (50 Hz); the vehicle uses
rigidbody interpolation. A higher render target does not speed up simulation.

A 120 FPS target is not a sustained-performance guarantee. Android can impose
battery, thermal or user-policy limits. Displays without a matching 60 Hz mode
receive a 60 FPS target without inventing an unsupported mode; Unity may round
that target down to a divisor of the active refresh rate (for example 45 FPS
on a fixed 90 Hz display). Unity Update frequency and compositor-presented FPS
are different measurements.

`DisplayFrameRateDiagnostic.Run` passed 35 mode-selection and lifecycle checks,
including fractional rates, invalid modes, resolution mismatch, battery saver,
stable tie selection, resume and unchanged physics timing. The 41 existing
driving scenarios also completed without an error after these changes.

## Repeatable checks

Run each Unity entry point separately with `-batchmode -nographics -buildTarget
Android -projectPath <project>`; the diagnostic entry points exit themselves.

```sh
RACING_HUD_DIAGNOSTIC_OUTPUT=/tmp/racing-hud.json Unity ... -executeMethod HandbrakeHudScaleDiagnostic.Run
RACING_DISPLAY_OUTPUT=/tmp/racing-display Unity ... -executeMethod DisplayFrameRateDiagnostic.Run
RACING_PARITY_OUTPUT=/tmp/racing-driving Unity ... -executeMethod DrivingParityDiagnostic.Run
MigrationTools/build_arm64.sh
```

## API references

- [Unity Android frame-rate targeting and divisor behavior](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Application-targetFrameRate.html)
- [Android window display-mode preference](https://developer.android.com/reference/android/view/WindowManager.LayoutParams#preferredDisplayModeId)
- [Unity optimized Android frame pacing](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/PlayerSettings.Android-optimizedFramePacing.html)
- [Android game refresh-rate policy](https://developer.android.com/games/optimize/display-refresh-rate-change)

## Final APK and emulator measurements

The same release-mode APK installed and launched successfully on two Android
12 / API 31 emulators whose ABI list contains only `arm64-v8a`:

| Emulator configuration | Sample | Presented FPS average | Present interval p95 |
| --- | --- | ---: | ---: |
| Pixel_8_API_31, 60 Hz | Menu, 20 seconds | 59.37 | 20.18 ms |
| Pixel_8_API_31, 60 Hz | Free Mode / short driving sample, 20 seconds | 59.45 | 20.03 ms |
| Racing_120Hz_API31_20260908, 120 Hz | Menu, 20 seconds | 96.57 | 17.38 ms |
| Racing_120Hz_API31_20260908, 120 Hz | Free Mode / acceleration and braking, 30 seconds | 67.59 | 24.17 ms |

These are deduplicated SurfaceFlinger actual-present timestamps from the app's
BLAST SurfaceView, collected with `MigrationTools/measure_android_frames.py`.
Both 60 FPS samples passed the collector's 95%-of-target average-rate gate;
the 120 FPS samples **did not**. The 120 Hz mode and 120 FPS target are verified,
but this emulator did not sustain 120 presented FPS. Do not describe this as
a locked-120-FPS build or a physical-phone performance result. The first 120 Hz
sample ran with the other emulator at Home; the 60 Hz emulator was stopped
before the 120 Hz gameplay sample. Other user applications were left untouched.

The separate 120 Hz AVD uses the existing ARM64 Android image and was launched
with `-no-snapshot -vsync-rate 120 -gpu host -no-audio -no-boot-anim -port 5556`.
Android reports a supported/active 120.00001 Hz mode and an 8,333,333 ns display
period. The ordinary AVD reports 60.000004 Hz / 16,666,666 ns. No global Android
refresh setting was changed or unsupported display mode fabricated.

The handbrake stayed circular and the same size after three real pause/Continue
cycles on **each** installed emulator. That exercises the same HUD disable/enable
path as zone loading; it is distinct from the isolated additive-scene tests.
The attempted touch-driven airport route did not reach the airport load boundary,
so an actual airport transition on this exact APK remains unverified. No teleport
or production-code diagnostic shortcut was added to claim that coverage.
Bringing the 120 Hz app back from Android Home also preserved its 120 FPS target;
post-resume logs continued reporting the active 120 Hz display without JNI errors.

APK: `Builds/Android/ExtremeCarDrivingSimulator-arm64-hud-120hz.apk`

Package: `com.aim.racing.arm64test`; installed `primaryCpuAbi=arm64-v8a`,
`secondaryCpuAbi=null`, version code 20071, not marked DEBUGGABLE.

SHA-256: `5550fd112eca8db0f29b85d3d3f2f3cb1c764310fb5dcd5261d3b9fc543589da`

All five native libraries under `lib/arm64-v8a/` report ELF64 / DYN / AArch64:
`libmain.so`, `libunity.so`, `libil2cpp.so`, `libswappywrapper.so`, and
`libc++_shared.so`. Scanning every extracted file found no ARM32 ELF payload.
The last two libraries are the genuine ARM64 frame-pacing wrapper and its C++
runtime supplied/built by the installed Unity Android toolchain.

APK v2 signature verification and `zipalign -c -P 16 4` pass. The certificate
is Android Debug, not a forged publisher certificate. Existing reconstruction
installs with the same test key can update; the publisher's package is separate.

Both app logs confirm `SwappyWrapperInit() succeeded` and successful display
selection. No managed/JNI exception or fatal native crash occurred in these
smoke tests. The 60 Hz emulator logged an initial unsuccessful
`liblibswappywrapper.so` name probe immediately followed by loading the actual
`libswappywrapper.so` successfully. EGL's ES 3.2/3.1 probes likewise fall back
successfully to ES 3.0. These probes are not unresolved loading failures.
Legacy nonfatal scene-lifecycle warnings remain.

Raw logs, JSON, frame CSVs and screenshots are preserved locally under
`audit/reconstruction/evidence/hud-refresh-20260908` (outside the Git repository).
See [frame measurement instructions](MigrationTools/FRAME_MEASUREMENT.md) to
repeat the tests. Audio output was disabled; physical-device thermals, Android
15+ game policies, and the full collectible route were not tested in this pass.
