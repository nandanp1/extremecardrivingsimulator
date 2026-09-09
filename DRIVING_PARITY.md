# Reference gameplay driving pass — 2026-09-08

This is a reconstruction of the recovered Unity 4.7.2f1 game in Unity
6000.3.22f1, not the original project's physics runtime. The objective is to
match observable behavior without disguising ARM32 code or inventing an exact
frame-for-frame match.

## Reference and measurement

Reference: [Auto Matic — Extreme Car Racing Simulator All collectibles Part 2](https://www.youtube.com/watch?v=mE7MzWxPoRs).
The exact video was downloaded and inspected across its duration using sampled
frames, with denser, presentation-timestamp-labeled samples of the relevant
maneuvers. This is visual evidence, not access to the original simulation's
input or physics telemetry.

Evidence directory, relative to the top-level `racingconversion` workspace:
`audit/reconstruction/evidence/gameplay-parity-20260908/`.
It contains the reference video, timestamped contact sheets, two observation
reports, and before/after native Unity physics telemetry.

The recovered HUD deliberately displays forward speed in m/s multiplied by
7.2. All comparison speeds below use that same HUD scale, not physical km/h.

| Maneuver | Measured reference |
| --- | --- |
| Continuous acceleration | 49 → 248 in 5.805 s, PTS 421.612–427.417 |
| Independent acceleration run | Approximately 95 → 276 in 7 s, around 512–519 s |
| Low-speed acceleration | 24 → 99 in 1.421 s, PTS 53.016–54.437 |
| High-speed lift-off | 253 → 177 in 2.273 s, PTS 306.609–308.882 |
| Low-speed lift-off | 110 → 94 in 1.420 s, PTS 433.710–435.130 |
| Service braking | 233 → 126 in 1.421 s while brake is visibly held, PTS 427.823–429.244 |
| Red airport loop | Entry around 278; climb, inversion, and descent visible around 23–27 s |

The top of the loop is obscured by the camera and the exit includes a sideways
scrub. Those frames do not establish exact wheel contacts or a clean exit
speed. High-speed pivots around 169–171 and 230–232 seconds use the circular
handbrake; they are not examples of ordinary steering. Large dish/ramp jumps
allow free airborne rotation. A reset-like upright snap is visible, but its
button press is not captured.

## Runtime changes

- Steering uses a normalized fraction of the available, speed-limited wheel
  angle. It no longer accumulates hidden full-lock input while capped at high
  speed. Release and countersteer respond promptly, retaining a progressive
  low-speed sweep and the recovered Ackermann geometry.
- Wheel steering is applied on the physics step. Vehicle suspension/tire
  substeps reduce contact-force instability without changing the global 50 Hz
  simulation step.
- A minimum suspension damping ratio of 0.30 eliminates persistent rear-axle
  hopping after disturbed initial conditions. Spring rates, travel, target
  positions, geometry, and anti-roll settings are retained; suspension remains
  underdamped and visibly compliant.
- Modern longitudinal friction uses a dimensionless slip-ratio axis, while
  the recovered motor/brake demand API keeps its original slip-speed units.
  Mixing those units previously encouraged excessive wheelspin.
- The TC switch now also controls actual overspin feedback, including unloaded
  wheels. It cuts motor torque, not brake torque or chassis velocity. TC-off
  remains available, and torque opposing a backwards roll is not incorrectly
  classified as acceleration wheelspin.
- Near-zero-speed slip is regularized so numerical wheel-RPM chatter does not
  delay standing starts. Service/handbrake force is scaled by 1.15 to bring the
  measured braking interval closer to the video; this is separate from coasting.
- Lift-off uses the recovered continuous aerodynamic drag instead of the
  earlier 0.35 multiplier. It does not reintroduce full-speed wheel braking.
- Reset clears accelerator, service brake, handbrake, steering, and their
  mapped vehicle inputs; the handbrake could previously remain latched.

Airport-loop assistance remains restricted to its identified loop collider
and location. A new uphill-only throttle assist compensates gravity and drag
while real tires touch that loop, fading before the ceiling and speed limit.
This is an explicit reconstruction choice, not a claim to have recovered an
identical original-engine force law. It does not set velocity, add airborne
propulsion, affect other ramps, or automatically upright the car. Existing
graphics repairs, the NANDAN plate, and original collectible content are
preserved.

## Reproduce the physics checks

Final native-physics run: `final-physics/summary.json` and `telemetry.csv` in
the evidence directory. All 41 default behavioral/numerical checks passed.

| Check | Reference | Final reconstruction |
| --- | --- | --- |
| Low-speed acceleration, 24 → 99 | 1.421 s | Approximately 1.476 s, interpolated from telemetry |
| Acceleration over 5.8 s, starting near 49 | 248 HUD endpoint | 243 |
| Acceleration over 7 s, starting near 95 | Approximately 276 | 267 |
| Coast from 253 over 2.28 s | 177 | 179 |
| Coast from 110 over 1.42 s | 94 | 96 |
| Service brake, 233 → 126 | 1.421 s | 1.489 s |
| Ordinary steering release at high speed | Prompt visible response | Wheels centered within 0.5 s; short taps within 0.4 s |
| Reset | Upright recovery visible; exact button input unknown | All pedal/steering inputs verified clear |

The initial reconstructed baseline reached only 242 after the seeded 95 + 7 s
acceleration test; the final version reaches 267. Natural acceleration and
seeded acceleration now agree, rather than the seeded car remaining trapped
in a rear-wheel hopping/torque-cut cycle. A natural 0.25 m drop recovers rear
contact and settles; forward/reverse transitions and TC-off are also exercised.

All three loop scenarios establish inverted contact and grounded exit. Their
names (278/300/324) are **initial approach speeds**, not speed at the ramp foot:
the zero-RPM seeded start includes an approach before reaching the incline.
Final measured incline-entry speeds are approximately 226/241/264; incline
entry to grounded exit takes 4.28/4.14/3.94 s. The comparable 278 approach case
took approximately 5.94 s from incline to exit before the uphill assist.
This is a substantial reduction in bogging, but it is not identical to the
reference's climb toward 306–322 HUD or its sideways exit. Exact loop speed
progression remains a known approximation.

An attempted straight natural run-up from x=900 hit the loop's solid back wall
at x≈752; this is an invalid approach route, not a successful driving test.
That failed attempt is preserved in `loop-drive/`. The optional
`RACING_PARITY_NATURAL_LOOP=1` fixture now checks the entire approach and reports
blocked routes separately. Default results explicitly mark natural loop
approach as unverified. The complete collectible route has not been replayed.

From the Unity project directory, with no other Editor process using it:

```sh
RACING_PARITY_OUTPUT="$PWD/Builds/Diagnostics/driving-parity" \
  /Applications/Unity/Hub/Editor/6000.3.22f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -buildTarget Android -projectPath "$PWD" \
  -executeMethod DrivingParityDiagnostic.Run \
  -logFile "$PWD/Builds/Diagnostics/driving-parity-unity.log"
```

The Editor-only diagnostic instantiates the real Coche1 prefab and invokes its
actual input filters, vehicle controller, wheels, and anti-roll components
against Unity's native `Physics.Simulate`. It also loads the actual airport
loop geometry. It does not save or modify scenes/prefabs. Outputs are
`summary.json` and per-wheel `telemetry.csv`; a failed gate produces a nonzero
exit code.

Manual EditMode callback ordering, a synthetic flat test surface, and seeded
initial conditions are limitations. These checks are not substitutes for
Android launch, rendered gameplay, touch input, or physical-device testing.

## Earlier gameplay-parity artifact (20070)

This section preserves the earlier driving-pass artifact and its test results.
The current release is version code **20071**, includes the same driving
changes plus HUD/refresh fixes, and has a different filename/hash/native payload.
See [HUD_REFRESH_STATUS.md](HUD_REFRESH_STATUS.md) for that exact APK.

Earlier release-mode build:
`Builds/Android/ExtremeCarDrivingSimulator-arm64-gameplay-parity.apk` (about 263 MiB).

SHA-256:
`97695de969acf01dfc59e2145b14e985857bf9f66180e3b447248c7a7c9357d3`

- `libmain.so`, `libunity.so`, and `libil2cpp.so` each report `ELF64`, shared
  object (`DYN`), little-endian, `AArch64` under `lib/arm64-v8a/`. All APK files
  were inspected for ELF payloads; no ARM32 payload was found. Full output is
  saved in `arm64-verification.txt` in the evidence directory.
- APK signature verification passes (v2, Android Debug test certificate);
  `zipalign -c -P 16 4` passes. The publisher's certificate was not reproduced.
- Installed successfully on cold-booted `Pixel_8_API_31`, Android 12,
  `sdk_gphone64_arm64`, serial `emulator-5554`. Its complete ABI list is only
  `arm64-v8a`; the installed package reports `primaryCpuAbi=arm64-v8a` and
  `secondaryCpuAbi=null`. This is not ARM32 emulation.
- Package `com.aim.racing.arm64test`, version code 20070, version name
  `4.17.6-reconstructed`, min SDK 25, target SDK 36. The installed release
  package is not marked `DEBUGGABLE`.
- Main menu and Free Mode launch. Touch accelerator, steering, service brake,
  handbrake, reset, and movement after reset were exercised. The car drives,
  gears advance, the city renders, and the NANDAN plate remains visible.
- App-process Logcat from this smoke test contains no managed exceptions,
  native-load failures, or fatal crashes. Existing nonfatal initialization
  warnings remain. `JNI_OnLoad` is informational, not a JNI failure.
- Screenshots, `emulator-smoke.mp4`, and `emulator-app-logcat.txt` are in the
  evidence directory. The emulator was launched with audio output disabled;
  audio parity was not tested.

The prior emulator snapshot had crashing Android system services before APK
installation. A cold boot without wiping user data resolved that environment
failure. Interrupted development/release mode switching also left stale Unity
generated objects; the build cache was moved to
`Library/Bee-interrupted-20260908` and the successful release built from a fresh
cache. Source/assets and older delivered APKs were preserved. This generated
cache backup is not needed to run the APK.

This is a successful ARM64 emulator installation and gameplay smoke test, not
a new physical-phone test or proof of full-game parity. No physical phone was
connected. Camera clipping around nearby walls/signs remains visible, as do
the documented loop-trajectory differences; the entire collectible route and
physical-device frame pacing have not been validated.
