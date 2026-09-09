# Read-only Android frame measurement

Start the app and enter the desired gameplay area yourself, then run:

```sh
python3 MigrationTools/measure_android_frames.py --serial emulator-5554 --seconds 20 --output /absolute/new/evidence-directory --expect-fps 120
```

Use `--expect-fps 60` for the fallback test, or omit it to collect without a rate gate. `--adb /path/to/adb` selects an SDK. The output directory must be new or empty. No app launches, input events, settings writes, latency clears, log clears, or global statistics enables are performed. Only the specified app's SurfaceView timings and Unity DisplayRate messages are retained.

The collector discovers exact app SurfaceView names and probes them for valid timestamps. Modern Android often places real data on the `(BLAST)` child while the parent returns zero rows. An initial baseline excludes old frames without clearing history. Polls every 0.25 seconds deduplicate actual-present timestamps; nonoverlapping snapshots flag possible history loss and invalidate verification.

Outputs are `summary.json`, deduplicated `presented-frames.csv`, raw `latency-samples.jsonl`, baseline/discovery samples and new `display-rate.log` messages. The [AOSP FrameTracker implementation](https://android.googlesource.com/platform/frameworks/native/+/51de481/services/surfaceflinger/FrameTracker.cpp) defines columns as desired presentation, actual presentation, and frame-ready timestamps in nanoseconds. Zero/invalid timestamps and pending `INT64_MAX` presentation fences are excluded.

Read the metrics separately:

- `presented_fps_over_timestamp_span` measures distinct app-buffer presentation timestamps, not the display's declared mode.
- The latency header describes display refresh configuration; 120 Hz there alone does not prove 120 FPS.
- `measuredUnityFps` in app logs measures Unity Update frequency, not compositor presentation. Its ten-second sample may overlap the collection boundary.
- Emulator results concern the Android guest. They do not establish that the host monitor or a physical phone actually displays 120 Hz.
- Average FPS is not a smoothness guarantee: examine interval p95/p99/max and repeat representative driving/airport scenarios after warm-up.

Exit `0` means usable measurement and any requested average-rate gate passed; `1` means usable measurement below the requested rate; `2` means invalid/incomplete measurement. The optional gate requires at least 95% of the requested average FPS by default, fresh presentation coverage for at least 75% of the requested duration, no detected history overruns, and a stable app PID/layer. It never establishes sustained physical-device performance by itself.

Parser/unit tests (no device required):

```sh
python3 -m unittest discover -s MigrationTools -p 'test_measure_android_frames.py'
```
