# Off-road terrain repair

The Forest/off-road scene still used recovered Unity 4 static-batch meshes.
Unity 6 did not preserve the original renderer subset selection, so the
rendered terrain and scenery could be misplaced or duplicated even though the
scene loaded. City and Airport had already received the corresponding repair.

The Forest migration reconstructs 925 renderer-local meshes from 1,259 selected
submeshes across four combined source meshes. It preserves the original scene
GUID, transforms, material assignments, colliders, and lightmap bindings. The
player-build processor now restores Forest lightmap bindings before batching.
The original Forest YAML and a subset manifest are retained for reproducibility.

The nine terrain collision meshes already match the intended geometry; this
change does not alter their shape or the driving controller.

## Reproduce

Use Unity 6000.3.22f1 with this project and `-buildTarget Android`:

1. Run `-batchmode -nographics -quit -executeMethod RacingConversion.Editor.GameplayLegacyStaticBatchMigration.MigrateForestSceneFromCommandLine`.
2. Run `-batchmode -executeMethod OffroadLoadingDiagnostic.Run`, without `-quit`.
   Set `RACING_OFFROAD_DIAGNOSTIC_OUTPUT` to the JSON report filename.
3. Run `MigrationTools/build_arm64.sh`.

The diagnostic runs real startup, menu, and Free Mode code. It dispatches
the existing zone handlers using the actual player collider to exercise first
entry, unloading, repeat async entry, loading-boundary pause, HUD recovery,
geometry bounds, and required ground raycasts. This is not an automated drive
through the tunnel. Android visual/device verification is separate.

## Verification status

- Migration executed: all 925 renderer records validated.
- First-entry and repeat-entry PlayMode checks passed; eight targeted ground
  probes passed on each visit, with no Forest-transition errors.
- The actual player car passed a five-second natural physics drop after Reset
  at the authored Forest spawn. It remained dynamic with gravity, settled with
  at least three wheels contacting Forest colliders, and did not fall through
  the ground. The camera capture shows visible terrain and scenery.
- Release APK built successfully, version `4.17.6-reconstructed.3` / code `20072`.
  All five native libraries are ARM64; APK v2 signature and 16 KB ZIP alignment
  verification pass. It installs as an update and cold-launches on the ARM64-only
  Android 12 emulator. City loads, drives, and reverses with the final APK.
- The manual emulator route reached the Offroad sign/intersection but did not
  reliably cross the Forest loading trigger. Therefore actual Android Forest
  traversal is not claimed; first/repeat Forest entry and vehicle support are
  verified by the Unity PlayMode diagnostic described above.

Artifact in the workspace root:
`ExtremeCarDrivingSimulator-arm64-offroad-fixed.apk` (262.60 MiB).

SHA-256: `4864edc8a4b0620d0ac53e810f4d4a02e7d1f797be1c54fa42bbd7b280b17c21`.

The installed emulator APK hash matches this artifact. No physical-phone test
has been performed for this version. Existing legacy startup/serialization and
lightmap-UV warnings are not a claim of a warning-free project; the Forest
transition and settling assertions completed without errors.

Local logs and reports: `../../evidence/offroad-fix-20261016/`.
