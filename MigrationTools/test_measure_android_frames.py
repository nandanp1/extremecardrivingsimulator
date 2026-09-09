import unittest

from measure_android_frames import (
    MAX_TIMESTAMP, FrameAccumulator, candidate_layers, display_rate_lines,
    parse_display_rate, parse_latency, presentation_metrics,
)


def sample(times):
    return parse_latency("8333333\n" + "\n".join("%d %d %d" % (t - 1, t, t - 2) for t in times))


class FrameMeasurementTests(unittest.TestCase):
    def test_columns_and_invalid_fences(self):
        result = parse_latency("8333333\n11 22 33\n0 0 0\n1 %d 2\n1 -1 2\n" % MAX_TIMESTAMP)
        self.assertEqual(result["frames"], [(11, 22, 33)])
        self.assertEqual(result["invalid_rows"], 3)

    def test_integer_nanoseconds_are_not_rounded(self):
        stamp = 9007199254740993
        self.assertEqual(sample([stamp])["frames"][0][1], stamp)

    def test_no_data_header_is_not_successful_measurement(self):
        self.assertEqual(parse_latency("16666666\n")["frames"], [])
        with self.assertRaises(ValueError):
            FrameAccumulator(parse_latency("16666666\n0 0 0\n"))

    def test_rejects_non_latency_output(self):
        for text in ("Permission Denial", "0\n1 2 3", "8333333\n1 2", "8333333\na b c"):
            with self.assertRaises(ValueError):
                parse_latency(text)

    def test_dedupe_and_exclude_history(self):
        accumulator = FrameAccumulator(sample([100, 200, 300]))
        accumulator.add(sample([200, 300, 400]))
        accumulator.add(sample([300, 400, 500]))
        self.assertEqual(sorted(accumulator.frames), [400, 500])
        self.assertEqual(accumulator.duplicate_observations, 1)
        self.assertEqual(accumulator.excluded_history_observations, 3)
        self.assertEqual(accumulator.possible_history_overruns, 0)

    def test_history_overrun_is_reported(self):
        accumulator = FrameAccumulator(sample([100, 200]))
        accumulator.add(sample([500, 600]))
        self.assertEqual(accumulator.possible_history_overruns, 1)

    def test_exact_120_fps_intervals(self):
        rows = [(0, i * 25000000, 0) for i in range(4)]
        metrics = presentation_metrics(rows, 0.1)
        self.assertEqual(metrics["presented_fps_over_timestamp_span"], 40.0)
        rows = [(0, round(1000000000 + i * 1000000000 / 120), 0) for i in range(121)]
        metrics = presentation_metrics(rows, 1)
        self.assertEqual(metrics["presented_fps_over_timestamp_span"], 120)
        self.assertAlmostEqual(metrics["interval_ms"]["mean"], 1000 / 120)

    def test_empty_metrics_are_unknown_not_zero_proof(self):
        metrics = presentation_metrics([], 20)
        self.assertIsNone(metrics["presented_fps_over_timestamp_span"])
        self.assertIsNone(metrics["interval_ms"]["p95"])

    def test_only_exact_package_surface_layers(self):
        text = "\n".join([
            "SurfaceView[com.aim.racing.arm64test/A](BLAST)#1",
            "SurfaceView[com.aim.racing.arm64test.extra/A]#1",
            "Background for SurfaceView[com.aim.racing.arm64test/A]#1",
            "com.aim.racing.arm64test/A#1",
            "SurfaceView[com.other.app/A]#1",
        ])
        self.assertEqual(candidate_layers(text, "com.aim.racing.arm64test"),
                         ["SurfaceView[com.aim.racing.arm64test/A](BLAST)#1"])

    def test_unity_update_fps_remains_separate(self):
        line = "123.000 I Unity: [DisplayRate] targetFps=120, activeHz=120.00, measuredUnityFps=82.4, sampleSeconds=10.0"
        self.assertEqual(display_rate_lines("irrelevant\n" + line + "\n" + line), [line])
        result = parse_display_rate(line)
        self.assertEqual(result["targetFps"], 120)
        self.assertEqual(result["activeHz"], 120.0)
        self.assertEqual(result["measuredUnityFps"], 82.4)


if __name__ == "__main__":
    unittest.main()
