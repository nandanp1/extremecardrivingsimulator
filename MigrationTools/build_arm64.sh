#!/bin/sh
set -eu

UNITY_VERSION="6000.3.22f1"
UNITY_EXECUTABLE=${UNITY_EXECUTABLE:-"/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity"}
SCRIPT_DIRECTORY=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
PROJECT_DIRECTORY=$(CDPATH= cd -- "${SCRIPT_DIRECTORY}/.." && pwd)
LOG_PATH="${PROJECT_DIRECTORY}/Builds/Android/unity-arm64-build.log"
APK_PATH="${PROJECT_DIRECTORY}/Builds/Android/ExtremeCarDrivingSimulator-arm64-test.apk"
VERIFY_SCRIPT="${SCRIPT_DIRECTORY}/verify_arm64_apk.sh"

if [ ! -x "${UNITY_EXECUTABLE}" ]; then
    echo "Unity ${UNITY_VERSION} is not installed at ${UNITY_EXECUTABLE}" >&2
    echo "Install Unity with Android Build Support, SDK/NDK tools, and OpenJDK first." >&2
    exit 2
fi

mkdir -p "${PROJECT_DIRECTORY}/Builds/Android"

"${UNITY_EXECUTABLE}" \
    -batchmode \
    -accept-apiupdate \
    -quit \
    -buildTarget Android \
    -projectPath "${PROJECT_DIRECTORY}" \
    -executeMethod RacingConversion.Editor.Arm64Build.BuildFromCommandLine \
    -logFile "${LOG_PATH}"

if [ ! -f "${APK_PATH}" ]; then
    echo "Unity exited without creating the expected APK: ${APK_PATH}" >&2
    echo "Inspect ${LOG_PATH}." >&2
    exit 1
fi

"${VERIFY_SCRIPT}" "${APK_PATH}"

echo "Build and static ARM64 verification completed. Inspect ${LOG_PATH}, then test the APK on a 64-bit-only ARM64 device."
