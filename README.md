# Extreme Car Driving Simulator — ARM64 reconstruction

This is a reconstructed Unity `6000.3.22f1` project targeting Android ARM64
with IL2CPP. It was migrated from the Unity 4.7.2f1 game data and includes
offline replacements for retired third-party service integrations.

The project is a reconstruction, not the publisher's original Unity project.
The downloadable APK is signed with an Android debug certificate and is meant
for sideloaded testing, not as an in-place update to the publisher build.

## Build

Install Unity `6000.3.22f1` with Android Build Support, then run:

```sh
MigrationTools/build_arm64.sh
```

The script builds `Builds/Android/ExtremeCarDrivingSimulator-arm64-test.apk`
and verifies that its native payload is AArch64-only. An existing APK can be
checked independently with:

```sh
MigrationTools/verify_arm64_apk.sh path/to/application.apk
```

See [MIGRATION_STATUS.md](MIGRATION_STATUS.md) for reconstruction details and
known limitations. The newest packaged APK is available from GitHub Releases.

