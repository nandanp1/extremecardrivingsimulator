#!/bin/sh
set -eu
LC_ALL=C
export LC_ALL

if [ "$#" -ne 1 ]; then
    echo "Usage: $0 path/to/application.apk" >&2
    exit 2
fi

APK_PATH=$1
if [ ! -f "${APK_PATH}" ]; then
    echo "APK does not exist: ${APK_PATH}" >&2
    exit 2
fi

for REQUIRED_TOOL in unzip file awk find grep; do
    if ! command -v "${REQUIRED_TOOL}" >/dev/null 2>&1; then
        echo "${REQUIRED_TOOL} is required." >&2
        exit 2
    fi
done

READELF_TOOL=${READELF_TOOL:-}
if [ -z "${READELF_TOOL}" ]; then
    READELF_TOOL=$(command -v readelf || command -v greadelf || command -v llvm-readelf || true)
fi
if [ -z "${READELF_TOOL}" ] && [ -x /usr/local/opt/binutils/bin/greadelf ]; then
    READELF_TOOL=/usr/local/opt/binutils/bin/greadelf
fi
if [ -z "${READELF_TOOL}" ] && [ -x /opt/homebrew/opt/binutils/bin/greadelf ]; then
    READELF_TOOL=/opt/homebrew/opt/binutils/bin/greadelf
fi
if [ -z "${READELF_TOOL}" ]; then
    for SDK_ROOT_CANDIDATE in \
        "${ANDROID_SDK_ROOT:-}" \
        "${ANDROID_HOME:-}" \
        "${HOME:+${HOME}/Library/Android/sdk}"
    do
        [ -n "${SDK_ROOT_CANDIDATE}" ] || continue
        for NDK_READELF in \
            "${SDK_ROOT_CANDIDATE}"/ndk/*/toolchains/llvm/prebuilt/*/bin/llvm-readelf
        do
            if [ -x "${NDK_READELF}" ]; then
                READELF_TOOL=${NDK_READELF}
            fi
        done
    done
fi
if [ -z "${READELF_TOOL}" ]; then
    echo "readelf, greadelf, or llvm-readelf is required." >&2
    exit 2
fi

TEMP_DIRECTORY=$(mktemp -d "${TMPDIR:-/tmp}/racing-arm64-verify.XXXXXX")
trap 'rm -rf -- "${TEMP_DIRECTORY}"' EXIT HUP INT TERM
EXTRACT_DIRECTORY="${TEMP_DIRECTORY}/apk"
FILE_LIST="${TEMP_DIRECTORY}/all-files.list"
VERIFICATION_LOG="${TEMP_DIRECTORY}/elf-verification.log"

mkdir -p "${EXTRACT_DIRECTORY}"
# Release aapt2 may emit resource paths that differ only by case. macOS uses a
# case-insensitive temporary filesystem, so overwrite those extraction aliases
# non-interactively; native payload paths are unique and validated below.
unzip -oq "${APK_PATH}" -d "${EXTRACT_DIRECTORY}"

if find "${EXTRACT_DIRECTORY}/lib" -type f -path '*/armeabi-v7a/*' 2>/dev/null | grep -q .; then
    echo "FAIL: APK still contains armeabi-v7a libraries." >&2
    exit 1
fi

ARM64_LIBRARY_COUNT=0
ELF_COUNT=0
find "${EXTRACT_DIRECTORY}" -type f > "${FILE_LIST}"
: > "${VERIFICATION_LOG}"

while IFS= read -r CANDIDATE; do
    RELATIVE_PATH=${CANDIDATE#"${EXTRACT_DIRECTORY}/"}
    IS_ARM64_LIBRARY=0
    case "${RELATIVE_PATH}" in
        lib/arm64-v8a/*.so)
            LIBRARY_NAME=${RELATIVE_PATH#lib/arm64-v8a/}
            case "${LIBRARY_NAME}" in
                */*)
                    echo "FAIL: native library is not directly under lib/arm64-v8a: ${RELATIVE_PATH}" >&2
                    exit 1
                    ;;
            esac
            IS_ARM64_LIBRARY=1
            ;;
        *.so)
            echo "FAIL: .so is outside lib/arm64-v8a: ${RELATIVE_PATH}" >&2
            exit 1
            ;;
    esac

    FILE_DESCRIPTION=$(file -b "${CANDIDATE}")
    case "${FILE_DESCRIPTION}" in
        *ELF*) ;;
        *)
            if [ "${IS_ARM64_LIBRARY}" -eq 1 ]; then
                echo "FAIL: packaged .so is not an ELF file: ${RELATIVE_PATH}" >&2
                exit 1
            fi
            continue
            ;;
    esac

    ELF_COUNT=$((ELF_COUNT + 1))
    ELF_HEADER=$(${READELF_TOOL} -h "${CANDIDATE}")
    ELF_CLASS=$(printf '%s\n' "${ELF_HEADER}" | awk -F: '/Class:/{sub(/^[[:space:]]+/, "", $2); print $2; exit}')
    ELF_DATA=$(printf '%s\n' "${ELF_HEADER}" | awk -F: '/Data:/{sub(/^[[:space:]]+/, "", $2); print $2; exit}')
    ELF_TYPE=$(printf '%s\n' "${ELF_HEADER}" | awk -F: '/Type:/{sub(/^[[:space:]]+/, "", $2); print $2; exit}')
    MACHINE=$(printf '%s\n' "${ELF_HEADER}" | awk -F: '/Machine:/{sub(/^[[:space:]]+/, "", $2); print $2; exit}')

    printf '%s: %s\n' "${RELATIVE_PATH}" "${FILE_DESCRIPTION}" >> "${VERIFICATION_LOG}"
    printf '  ELF Class: %s\n' "${ELF_CLASS}" >> "${VERIFICATION_LOG}"
    printf '  ELF Data: %s\n' "${ELF_DATA}" >> "${VERIFICATION_LOG}"
    printf '  ELF Type: %s\n' "${ELF_TYPE}" >> "${VERIFICATION_LOG}"
    printf '  ELF Machine: %s\n' "${MACHINE}" >> "${VERIFICATION_LOG}"

    case "${ELF_CLASS}" in
        ELF64) ;;
        *)
            echo "FAIL: non-ELF64 payload: ${RELATIVE_PATH} (${ELF_CLASS})" >&2
            exit 1
            ;;
    esac

    case "${ELF_DATA}" in
        *little\ endian*) ;;
        *)
            echo "FAIL: ELF payload is not little-endian: ${RELATIVE_PATH} (${ELF_DATA})" >&2
            exit 1
            ;;
    esac

    case "${MACHINE}" in
        *AArch64*) ;;
        *)
            echo "FAIL: non-AArch64 ELF payload: ${RELATIVE_PATH} (${MACHINE})" >&2
            exit 1
            ;;
    esac

    if [ "${IS_ARM64_LIBRARY}" -eq 1 ]; then
        case "${ELF_TYPE}" in
            DYN*) ;;
            *)
                echo "FAIL: packaged .so is not an ELF shared object: ${RELATIVE_PATH} (${ELF_TYPE})" >&2
                exit 1
                ;;
        esac

        if ! PROGRAM_HEADERS=$(${READELF_TOOL} -l "${CANDIDATE}" 2>&1); then
            echo "FAIL: readelf could not parse program headers: ${RELATIVE_PATH}" >&2
            exit 1
        fi
        case "${PROGRAM_HEADERS}" in
            *Error:*)
                echo "FAIL: malformed ELF program headers: ${RELATIVE_PATH}" >&2
                exit 1
                ;;
        esac
        if ! printf '%s\n' "${PROGRAM_HEADERS}" |
            awk '$1 == "LOAD" { load = 1 } $1 == "DYNAMIC" { dynamic = 1 } END { exit !(load && dynamic) }'
        then
            echo "FAIL: shared object lacks valid LOAD and DYNAMIC program segments: ${RELATIVE_PATH}" >&2
            exit 1
        fi

        if ! DYNAMIC_SECTION=$(${READELF_TOOL} -d "${CANDIDATE}" 2>&1); then
            echo "FAIL: readelf could not parse the dynamic section: ${RELATIVE_PATH}" >&2
            exit 1
        fi
        case "${DYNAMIC_SECTION}" in
            *Error:*|*'There is no dynamic section'*)
                echo "FAIL: shared object has no valid dynamic section: ${RELATIVE_PATH}" >&2
                exit 1
                ;;
            *'Dynamic section at offset'*) ;;
            *)
                echo "FAIL: shared object dynamic section was not recognized: ${RELATIVE_PATH}" >&2
                exit 1
                ;;
        esac

        ARM64_LIBRARY_COUNT=$((ARM64_LIBRARY_COUNT + 1))
    fi
done < "${FILE_LIST}"

cat "${VERIFICATION_LOG}"

if [ "${ARM64_LIBRARY_COUNT}" -eq 0 ]; then
    echo "FAIL: no lib/arm64-v8a/*.so files were found." >&2
    exit 1
fi

echo "PASS: verified ${ELF_COUNT} AArch64 ELF payloads, including ${ARM64_LIBRARY_COUNT} arm64-v8a libraries, and no armeabi-v7a payload."
