# ARM64 migration status

Status date: **2026-08-25** (America/Los_Angeles).

This is the historical migration report. For the September 8 release-mode APK
(`20071`), current native payload and emulator measurements, see
[HUD_REFRESH_STATUS.md](HUD_REFRESH_STATUS.md). For the later driving pass,
see [DRIVING_PARITY.md](DRIVING_PARITY.md). Hashes and phone results below refer
to the August artifact, not the newest build.

This directory is a reconstruction of the Unity player contained in
`com.aim.racing_4.17.6-20070_minAPI16(armeabi-v7a)(nodpi)_apkmirror.com.apk`.
It is not the developer's original Unity project.

## Current result

A genuine ARM64-only Unity 6/IL2CPP development APK now builds successfully. The
latest repaired APK was installed on the 64-bit-only `Pixel_8_API_31` emulator,
cold-launched, and left as the resumed activity with `primaryCpuAbi=arm64-v8a`.
The emulator reports only `arm64-v8a` in its ABI list.

The exact APK was uninstalled/reinstalled with clean app data and exercised twice
from a fresh profile. The tested path is now a credible initial City driving smoke
test:

- Consent, the reconstructed menu, its city backdrop, floor/ramp geometry, and
  player-car preview render correctly instead of exposing the combined
  static-batch mesh.
- Visible-center taps opened Drive, then the mode selector, and selected **Free
  Mode**.
- The game entered the initial City slice with the player car upright and
  stationary, the HUD visible, and roads, buildings, trees, and terrain present.
  The 2-, 8-, and 30-second captures show the same intact player car rather than
  the previous launch/roll/deformation failure.
- Reset realigned and settled the vehicle upright. The pre-fix baseline reached
  only 69 km/h after 15 seconds at full throttle and remained in display gear 1.
  The final drivetrain reached 120.0 km/h/display gear 2 after about three
  seconds of throttle, 152.5 km/h/display gear 3 after about four seconds, and
  178.3 km/h in gear 3 after about five seconds.
- A second cleared-profile run independently remained upright and stationary at
  its 8-second checkpoint.
- Logcat contains only the two expected initial adjacent-sector starts,
  `2Game_Zone3` and `2Game_Zone1`. It contains no `UnsatisfiedLinkError`, dlopen
  or JNI failure, native crash, fatal Android exception, or managed exception in
  the captured run.

Current visual evidence is preserved at:

```text
../../evidence/emulator-frictionfix-clean-consent.png
../../evidence/emulator-frictionfix-drive-menu.png
../../evidence/emulator-frictionfix-clean-gameplay-2s.png
../../evidence/emulator-frictionfix-clean-gameplay-8s.png
../../evidence/emulator-frictionfix-clean-gameplay-30s.png
../../evidence/emulator-frictionfix-reset-2s.png
../../evidence/emulator-frictionfix-reset-8s.png
../../evidence/emulator-frictionfix-accelerate-3s.png
../../evidence/emulator-frictionfix-after-accelerate.png
../../evidence/emulator-frictionfix-clean-repeat2-8s.png
../../evidence/emulator-frictionfix-clean-gameplay.log
../../evidence/emulator-frictionfix-clean-repeat2.log
../../evidence/emulator-baseline-gear-15s.png
../../evidence/emulator-drivetrain-final-05s.png
../../evidence/emulator-drivetrain-final.log
../../evidence/physical-drivetrain-final-launch.png
../../evidence/physical-drivetrain-final-launch.log
../../evidence/physical-drivetrain-final-install.txt
```

## Completed locally

- Recovered 28 enabled scenes, 1,077 meshes, 360 textures, 422 materials, 136
  prefabs, animation/audio, and the four managed game assemblies.
- Preserved the 2,172-file decompilation baseline. The active migration tree has
  1,118 C# files after 1,060 obsolete SDK sources were moved, intact and
  reversible, to `../AuxiliaryFiles/QuarantinedLegacySdk`.
- Compiled the active assembly graph with zero errors against an API reference
  built from Unity's exact official `6000.3.22f1` source tag, then completed the
  real Unity 6 import, IL2CPP conversion, and Android player build.
- Removed all 355 confirmed hard-obsolete Unity API diagnostics, routed all 55
  old scene-loading calls through `LegacySceneManager`, and retained the
  converted UnityScript assemblies in their original compilation phases.
- Replaced the old native SDK path with deterministic offline facades for
  wallet/credits, ads, analytics, Google Play, Facebook, AWS configuration,
  time, notifications, and cross-promotion. The active `Assets` tree contains
  no `DllImport` declarations and no vendor `.so`, `.aar`, or `.jar` payloads.
- Installed Unity `6000.3.22f1` with Android Build Support, OpenJDK 17,
  SDK/Build Tools 36, NDK r27c, CMake, Gradle, and IL2CPP.

## Final visual and runtime repairs

- Replaced the main NGUI, AIM day/night, terrain, skybox, car-paint,
  flare/projector, mobile, and particle shader placeholders while preserving
  their serialized GUIDs and property contracts. The current inventory is **65
  shader assets, 13 `DummyShaderTextExporter` placeholders, and 422 materials**.
  Only **3 materials** directly reference one of the remaining placeholders;
  the old claim of 52 placeholder shaders referenced by 388 materials described
  the pre-repair export, not this project state.
- Corrected the recovered baked-lightmap textures from Sprite/sRGB import to
  lightmap/linear import semantics.
- Added Unity 6-compatible NGUI `Physics2D.SyncTransforms()` calls before 2D
  hit testing, restoring taps at the visible Drive and Free Mode controls.
- Kept `CarExternalInputRandom` disabled for the human player car and reset
  player inputs during car replacement. The car no longer accelerates and
  deforms itself without user input.
- Restricted streaming-zone triggers to the local player's `BasicCollider`
  attached to its root rigidbody while preserving the Trial-camera path. This
  removed the false Zone0/Zone2 loads caused by workshop-gate rigidbodies.
- Removed a clean-profile spawn race by holding the complete player-car rigidbody
  hierarchy until the exact `2Game_City` scene is loaded, synchronizing transforms,
  and accepting initial wheel-ground hits only from that scene. All four wheels
  now align to `Road_2x30` at -0.663 m; Reset and replacement share the same path.
- Migrated EVP4's Unity 4/PhysX 2 wheel response in two axes: lateral friction
  retains the proven per-wheel load normalization, while longitudinal friction
  is reconstructed from the measured lookup and `motorPerformancePeak`. Coche1
  now uses peak slip 1.778 and extremum coefficient 0.8155; motor/brake demand is
  mapped through average chassis normal load to keep left/right drive torque
  balanced. This preserves the stable spawn while restoring acceleration.
- Migrated Unity 4 legacy static batches into renderer-local standalone meshes.
  Raw scene backups and manifests preserve the original subset indices, source
  mesh GUID/fileID, materials, LOD references, and lightmap metadata:

| Scene | Migrated renderers | Selected submeshes | Combined meshes |
|---|---:|---:|---:|
| `Assets/Scenes/Common/1Menu.unity` | 275 | 299 | 1 |
| `Assets/Scenes/Common/2Game.unity` | 270 | 364 | 1 |
| `2Game_City.unity` | 437 | 461 | 3 |
| `2Game_Zone0.unity` | 207 | 237 | 1 |
| `2Game_Zone1.unity` | 76 | 88 | 1 |
| `2Game_Zone2.unity` | 75 | 101 | 1 |
| `2Game_Zone3.unity` | 41 | 57 | 1 |
| **Total** | **1,381** | **1,607** | **9** |

Unity reopened and validated every migrated renderer. The final player build
also reapplied all 1,381 preserved lightmap bindings to its temporary build
scenes without modifying the source scenes.

## Target configuration

`Assets/Editor/Arm64Build.cs` configures Unity `6000.3.22f1`, Built-in Render
Pipeline, Android IL2CPP, ARM64 only, API 25 minimum/API 36 target, OpenGL ES 3,
Minimal managed stripping, direct development APK output, and test application
ID `com.aim.racing.arm64test`. It uses Unity's development/debug signing and
never attempts to reproduce the publisher certificate.

Unity imported and rewrote the project for `6000.3.22f1`. A pre-import backup
is preserved at:

```text
../../evidence/ExportedProject-pre-unity6.zip
```

## Remaining limitations

- Thirteen shaders still contain `DummyShaderTextExporter`: `CarPaintReflective`,
  `Mobile/VertexLit`, the three Normal/Bumped/Specular shaders, and eight legacy
  image-effect shaders (`DLAA`, `FXAA II`, `FXAA Preset 2`, `FXAA Preset 3`,
  `FXAA III (Console)`, `FastBloom`, `NFAA`, and `SSAA`). The three direct
  material bindings are the two legacy truck body materials and truck wheels.
  Reconstructed AIM shaders are also compatibility implementations, not recovered
  original source, so exact lighting fidelity is not claimed.
- Static-batch recovery was deliberately limited to `1Menu` and the six scenes
  required by the initial City/Free Mode path. A current raw scan finds **3,065
  non-startup renderers selecting 3,635 submeshes across 12 still-unmigrated
  scenes**: Workshop, Airport and its three zones, City_2, Forest, Radar_04, and
  Trial_01 through Trial_04. Store, the other Radar scenes, Crashable scenes,
  and other game paths also remain visually/runtime unvalidated.
- Nine data-empty, optional promo/popup components remain unresolved in
  `2Game.unity`: four `InfoGame` and four `GUIMoreGameButton` components on the
  four `btDownload` objects, plus one `PopupLogic` component on inactive
  `PnPopup`. They generate warnings but are not car, streaming, geometry, or
  material logic.
- Collision particle lookup remains unavailable on the tested City start and is
  guarded as a cosmetic failure. Six negative-scale large-zone unload colliders
  also produce warnings and should be normalized before cross-region driving is
  treated as validated.
- Only the initial City Free Mode path has been smoke-tested. Reset and a
  straight-line launch through display gear 3 work; steering, braking, every vehicle, damage,
  traffic/checkpoint modes, saves, store/workshop, other regions, multiplayer,
  and production-release behavior are not exhaustively tested.
- The exact final hash is installed and cold-launches on a physical ARM64-only
  Pixel 9 Pro XL. Its installed `base.apk` hash matches the local artifact,
  Package Manager selected `arm64-v8a`, Unity reports IL2CPP/ARM64, and the
  physical launch log has no native-loader, JNI, managed, shader, or fatal error.

## Reproduce the build and verification

1. Run `MigrationTools/build_arm64.sh`.
2. Run
   `MigrationTools/verify_arm64_apk.sh Builds/Android/ExtremeCarDrivingSimulator-arm64-test.apk`.
3. Install on a 64-bit-only ARM64 target and inspect Logcat for managed errors,
   JNI/dlopen failures, missing symbols, shader failures, and native crashes.

## Current artifact state

Final repaired APK:

```text
Builds/Android/ExtremeCarDrivingSimulator-arm64-test.apk
```

- APK modification time: `2026-08-25 23:01:00 -0700`
- Size: `466,078,323` bytes
- SHA-256: `2673e507628ab78932435c7c1bee63882b4009d102c057bfe06461e0edea45e4`
- Package/version: `com.aim.racing.arm64test`, `4.17.6-reconstructed` (`20070`)
- SDK levels: minimum API 25, target/compile API 36
- Backend/ABI: Unity 6 IL2CPP, `arm64-v8a` only
- Native files: `libmain.so`, `libunity.so`, and `libil2cpp.so`; `file` and
  `readelf` report `ELF64`, shared object type `DYN`, and `Machine: AArch64` for
  all three. The verifier found no `armeabi-v7a` payload.
- Signing: one new Android debug signer (`CN=Android Debug`), not the publisher
  certificate
- Emulator result: the package began from a strict clean install at
  `2026-08-25 22:18:03 -0700`; the exact final revision was installed at 23:01:42,
  menu-to-Drive-to-Free-Mode-to-City completed, idle/Reset passed, and the
  drivetrain reached 178.3 km/h/display gear 3 after about five seconds of throttle
- Physical phone result for this exact APK: **installed and cold-launched on the
  ARM64-only Pixel 9 Pro XL; exact on-device hash matched, and the visible run
  reached 155 km/h/display gear 3**

True ARM64 packaging and launch on both the emulator and physical phone are
proven. This remains a reconstruction rather than the original project, and full
visual/content parity is not yet proven.
