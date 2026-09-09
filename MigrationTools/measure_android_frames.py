#!/usr/bin/env python3
"""Read-only, app-scoped SurfaceFlinger presentation sampling (Python 3 stdlib).

No app launches, input events, settings changes, log clears or latency clears.
Run parser tests: python3 -m unittest discover -s MigrationTools -p 'test_measure_android_frames.py'
"""

import argparse
import csv
import json
import math
import os
from pathlib import Path
import re
import shlex
import shutil
import statistics
import subprocess
import sys
import time

MAX_TIMESTAMP = (1 << 63) - 1
PACKAGE = "com.aim.racing.arm64test"
FRAME_SOURCE = "https://android.googlesource.com/platform/frameworks/native/+/51de481/services/surfaceflinger/FrameTracker.cpp"


def parse_latency(text):
    """AOSP columns: desiredPresentTime, actualPresentTime, frameReadyTime (ns)."""
    lines = [line.strip() for line in text.splitlines() if line.strip()]
    if not lines or not re.fullmatch(r"[0-9]+", lines[0]):
        raise ValueError("SurfaceFlinger did not return a numeric refresh-period header")
    period = int(lines[0])
    if not 0 < period < MAX_TIMESTAMP:
        raise ValueError("Invalid SurfaceFlinger refresh period")
    frames, invalid = [], 0
    for line in lines[1:]:
        fields = line.split()
        if len(fields) != 3 or not all(re.fullmatch(r"-?[0-9]+", field) for field in fields):
            raise ValueError("Unexpected SurfaceFlinger latency row: " + line[:160])
        row = tuple(int(field) for field in fields)
        if 0 < row[1] < MAX_TIMESTAMP:
            frames.append(row)
        else:
            invalid += 1  # Empty, pending (INT64_MAX), or invalid presentation fence.
    return {"refresh_period_ns": period, "frames": frames, "invalid_rows": invalid}


def candidate_layers(text, package):
    return sorted(set(line.strip() for line in text.splitlines()
                      if package + "/" in line and "SurfaceView" in line
                      and not line.strip().startswith(("Background for ", "Bounds for "))))


class FrameAccumulator:
    def __init__(self, baseline):
        valid = baseline["frames"]
        if not valid:
            raise ValueError("Selected layer has no valid baseline presentation timestamps")
        self.baseline_max_ns = max(row[1] for row in valid)
        self.previous = set(row[1] for row in valid)
        self.frames = {}
        self.possible_history_overruns = 0
        self.duplicate_observations = 0
        self.excluded_history_observations = 0

    def add(self, sample):
        times = set(row[1] for row in sample["frames"])
        if self.previous and times and max(times) > max(self.previous) and not times.intersection(self.previous):
            self.possible_history_overruns += 1
        if times:
            self.previous = times
        for row in sample["frames"]:
            present = row[1]
            if present <= self.baseline_max_ns:
                self.excluded_history_observations += 1
            elif present in self.frames:
                self.duplicate_observations += 1
            else:
                self.frames[present] = row


def percentile(values, fraction):
    if not values:
        return None
    values = sorted(values)
    index = (len(values) - 1) * fraction
    lo, hi = math.floor(index), math.ceil(index)
    return values[lo] + (values[hi] - values[lo]) * (index - lo)


def presentation_metrics(rows, elapsed):
    timestamps = sorted(set(row[1] for row in rows))
    intervals = [(end - start) / 1e6 for start, end in zip(timestamps, timestamps[1:])]
    span = (timestamps[-1] - timestamps[0]) / 1e9 if len(timestamps) >= 2 else 0.0
    return {
        "unique_new_presentations": len(timestamps),
        "presentation_span_seconds": span,
        "presented_fps_over_timestamp_span": (len(timestamps) - 1) / span if span > 0 else None,
        "new_presentations_per_host_collection_second": len(timestamps) / elapsed if elapsed > 0 else None,
        "interval_ms": {
            "mean": statistics.mean(intervals) if intervals else None,
            "p50": percentile(intervals, 0.50), "p90": percentile(intervals, 0.90),
            "p95": percentile(intervals, 0.95), "p99": percentile(intervals, 0.99),
            "max": max(intervals) if intervals else None,
        },
    }


def display_rate_lines(text):
    return list(dict.fromkeys(line for line in text.splitlines() if "[DisplayRate]" in line))


def parse_display_rate(line):
    result = {"line": line}
    for key, value in re.findall(r"([A-Za-z][A-Za-z0-9]*)=(-?[0-9]+(?:\.[0-9]+)?)", line):
        result[key] = float(value) if "." in value else int(value)
    return result


class Adb:
    def __init__(self, executable, serial):
        self.prefix = [executable, "-s", serial]

    def run(self, *arguments):
        result = subprocess.run(self.prefix + list(arguments), text=True, stdout=subprocess.PIPE,
                                stderr=subprocess.PIPE, timeout=10, check=False)
        if result.returncode:
            raise RuntimeError("ADB read failed: " + result.stderr.strip()[:500])
        return result.stdout

    def shell(self, *arguments):
        # adb shell joins arguments into a remote shell command; quote EVERY token.
        return self.run("shell", " ".join(shlex.quote(str(argument)) for argument in arguments))

    def layers(self):
        return self.shell("dumpsys", "SurfaceFlinger", "--list")

    def latency(self, layer):
        return self.shell("dumpsys", "SurfaceFlinger", "--latency", layer)

    def pid(self, package):
        output = self.shell("pidof", package).split()
        if len(output) != 1 or not output[0].isdigit():
            raise RuntimeError("Expected one running app process; start the app separately before measuring")
        return output[0]

    def display_logs(self, pid):
        # Read only the app process's Unity tag; preserve just DisplayRate messages.
        return display_rate_lines(self.run("logcat", "-d", "-v", "epoch", "--pid=" + pid, "-s", "Unity:I", "*:S"))


def resolve_adb(value):
    candidates = [value, os.environ.get("ADB"), shutil.which("adb"),
                  str(Path.home() / "Library/Android/sdk/platform-tools/adb")]
    for candidate in candidates:
        if candidate and Path(candidate).is_file() and os.access(candidate, os.X_OK):
            return candidate
    raise ValueError("ADB not found; provide --adb /absolute/path/to/adb")


def select_layer(adb, package, override, output):
    names = candidate_layers(adb.layers(), package)
    (output / "app-layers.txt").write_text("\n".join(names) + "\n")
    if override:
        if override not in names:
            raise ValueError("--layer must exactly name one of this app's SurfaceView layers")
        names = [override]
    if not names:
        raise ValueError("No app SurfaceView layer found; collector will not launch or navigate the app")
    candidates, failures = [], []
    for index, name in enumerate(names):
        try:
            raw = adb.latency(name)
            (output / ("discovery-%02d.txt" % index)).write_text(raw)
            sample = parse_latency(raw)
            if sample["frames"]:
                score = (max(row[1] for row in sample["frames"]), "BLAST" in name, len(sample["frames"]))
                candidates.append((score, name, sample))
        except (RuntimeError, ValueError, subprocess.TimeoutExpired) as error:
            failures.append({"layer": name, "error": str(error)})
    if not candidates:
        raise ValueError("No app SurfaceView layer exposes valid actual-present timestamps; " + str(failures))
    candidates.sort(reverse=True)
    return candidates[0][1], names, failures


def collect(args):
    output = Path(args.output).expanduser().resolve()
    output.mkdir(parents=True, exist_ok=True)
    if any(output.iterdir()):
        raise ValueError("Output directory must be empty; existing evidence will not be overwritten")
    report = {
        "status": "invalid", "serial": args.serial, "package": args.package,
        "requested_seconds": args.seconds, "poll_interval_seconds": args.interval,
        "emulator_guest_measurement": args.serial.startswith("emulator-"),
        "method": "Deduplicated second-column actual-present timestamps from app SurfaceView --latency; initial history excluded, not cleared",
        "source": FRAME_SOURCE,
        "limitations": [
            "SurfaceFlinger timing is compositor/guest presentation evidence, not physical panel photon timing.",
            "On an emulator this does not prove the host monitor or a physical phone is displaying 120 Hz.",
            "Refresh-period header is display configuration, not the app's measured FPS.",
            "Unity measuredUnityFps is Update frequency, separately reported; its log sample may straddle this collection window.",
            "Finite SurfaceFlinger history can overrun if polling stalls; detected non-overlap invalidates rate verification.",
            "No app launches, input events, settings changes, buffer clears, root commands or global stats enables are performed.",
        ], "warnings": [],
    }
    exit_code = 2
    try:
        adb = Adb(resolve_adb(args.adb), args.serial)
        pid = adb.pid(args.package)
        report["pid_start"] = pid
        try:
            initial_logs = adb.display_logs(pid)
        except (RuntimeError, subprocess.TimeoutExpired) as error:
            initial_logs = []
            report["warnings"].append("Initial DisplayRate log read unavailable: " + str(error))
        layer, candidates, discovery_errors = select_layer(adb, args.package, args.layer, output)
        report.update(layer=layer, candidate_layers=candidates, discovery_errors=discovery_errors)
        # Fresh baseline AFTER discovery, so candidate probing never counts as measured gameplay.
        baseline_raw = adb.latency(layer)
        baseline = parse_latency(baseline_raw)
        (output / "baseline-latency.txt").write_text(baseline_raw)
        accumulator = FrameAccumulator(baseline)
        periods = {baseline["refresh_period_ns"]}
        started = time.monotonic()
        deadline = started + args.seconds
        polls, poll_durations, errors = 0, [], []
        with (output / "latency-samples.jsonl").open("w") as raw_file:
            while time.monotonic() < deadline:
                time.sleep(min(args.interval, max(0.0, deadline - time.monotonic())))
                before = time.monotonic()
                raw = adb.latency(layer)
                after = time.monotonic()
                sample = parse_latency(raw)
                accumulator.add(sample)
                periods.add(sample["refresh_period_ns"])
                poll_durations.append(after - before)
                raw_file.write(json.dumps({"poll": polls, "host_elapsed_seconds": after - started,
                                           "adb_seconds": after - before, "raw": raw}) + "\n")
                raw_file.flush()
                polls += 1
        elapsed = time.monotonic() - started
        report.update(host_collection_seconds=elapsed, polls=polls,
                      max_poll_command_seconds=max(poll_durations, default=0),
                      refresh_period_ns_observed=sorted(periods),
                      header_refresh_hz_observed=[1e9 / period for period in sorted(periods)],
                      baseline_max_present_ns=accumulator.baseline_max_ns,
                      possible_history_overruns=accumulator.possible_history_overruns,
                      duplicate_observations=accumulator.duplicate_observations,
                      excluded_initial_history_observations=accumulator.excluded_history_observations)
        rows = [accumulator.frames[key] for key in sorted(accumulator.frames)]
        metrics = presentation_metrics(rows, elapsed)
        report["presentation"] = metrics
        with (output / "presented-frames.csv").open("w", newline="") as handle:
            writer = csv.writer(handle)
            writer.writerow(["desired_present_ns", "actual_present_ns", "frame_ready_ns", "since_first_present_seconds", "interval_ms"])
            first = rows[0][1] if rows else 0
            previous = None
            for row in rows:
                writer.writerow(list(row) + [(row[1] - first) / 1e9, (row[1] - previous) / 1e6 if previous is not None else ""])
                previous = row[1]
        if accumulator.possible_history_overruns:
            errors.append("Polling snapshots did not overlap; frames may have fallen out of the finite history")
        if metrics["presentation_span_seconds"] < args.seconds * 0.75:
            errors.append("Insufficient fresh presentation coverage (less than 75% of requested duration)")
        report["pid_end"] = adb.pid(args.package)
        if report["pid_end"] != pid:
            errors.append("App process changed during collection")
        if layer not in candidate_layers(adb.layers(), args.package):
            errors.append("Selected app SurfaceView layer disappeared during collection")
        try:
            final_logs = adb.display_logs(pid)
            before_set = set(initial_logs)
            logs = [line for line in final_logs if line not in before_set]
            (output / "display-rate.log").write_text("\n".join(logs) + ("\n" if logs else ""))
            report["unity_display_rate_new_messages"] = [parse_display_rate(line) for line in logs]
            report["unity_display_rate_preexisting_context"] = [parse_display_rate(line) for line in initial_logs[-5:]]
            if not logs:
                report["warnings"].append("No new app DisplayRate messages; old APKs may not contain this instrumentation")
        except (RuntimeError, subprocess.TimeoutExpired) as error:
            report["warnings"].append("Final DisplayRate log read unavailable: " + str(error))
        report["measurement_errors"] = errors
        if not errors:
            report["status"] = "measured"
            exit_code = 0
        if args.expect_fps is not None:
            fps = metrics["presented_fps_over_timestamp_span"]
            passed = not errors and fps is not None and fps >= args.expect_fps * args.min_ratio
            report["expectation"] = {"fps": args.expect_fps, "minimum_ratio": args.min_ratio,
                                     "minimum_presented_fps": args.expect_fps * args.min_ratio,
                                     "passed": passed,
                                     "meaning": "Average guest/app presentation rate over this sample, not a sustained physical-device guarantee"}
            if not errors and not passed:
                exit_code = 1
    except (OSError, RuntimeError, ValueError, subprocess.TimeoutExpired, KeyboardInterrupt) as error:
        report["error"] = str(error) or "Interrupted; partial raw evidence retained"
    (output / "summary.json").write_text(json.dumps(report, indent=2) + "\n")
    print(json.dumps(report, indent=2))
    print("Evidence: " + str(output), file=sys.stderr)
    return exit_code


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--serial", required=True, help="Explicit adb device serial")
    parser.add_argument("--seconds", type=float, default=20.0)
    parser.add_argument("--output", required=True, help="New or empty local evidence directory")
    parser.add_argument("--package", default=PACKAGE)
    parser.add_argument("--adb", help="ADB executable path; otherwise ADB env, PATH, macOS SDK")
    parser.add_argument("--interval", type=float, default=0.25, help="Seconds between read-only polls (default .25)")
    parser.add_argument("--layer", help="Optional exact app SurfaceView layer name")
    parser.add_argument("--expect-fps", type=float, choices=(60.0, 120.0))
    parser.add_argument("--min-ratio", type=float, default=0.95, help="Minimum average FPS / expected FPS (default .95)")
    args = parser.parse_args()
    if not 1 <= args.seconds <= 3600 or not 0.05 <= args.interval <= 0.5 or not 0 < args.min_ratio <= 1:
        parser.error("seconds must be 1..3600, interval .05...5, and min-ratio >0..1")
    if not re.fullmatch(r"[A-Za-z0-9_]+(?:\.[A-Za-z0-9_]+)+", args.package):
        parser.error("invalid package name")
    try:
        return collect(args)
    except (OSError, ValueError) as error:
        print("Measurement not started: " + str(error), file=sys.stderr)
        return 2


if __name__ == "__main__":
    sys.exit(main())
