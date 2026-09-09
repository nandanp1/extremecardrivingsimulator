using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Editor-only policy/lifecycle checks. No Android bridge or real display is
// emulated: JNI selection, panel switching and presented FPS need device tests.
public static class DisplayFrameRateDiagnostic
{
    private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.NonPublic;
    private static readonly List<Result> Results = new List<Result>();

    [Serializable]
    private sealed class Result
    {
        public string name;
        public bool passed;
        public string detail;
    }

    [Serializable]
    private sealed class Report
    {
        public string unityVersion;
        public string method = "Pure SelectMode plus isolated, inactive-component lifecycle calls; no scene saves or Android JNI calls";
        public string limitations = "Does not prove Android window requests, mode switching, frame pacing, compositor-presented FPS, thermal behavior or native callback scheduling.";
        public bool completed;
        public bool passed;
        public int testCount;
        public string error;
        public Result[] tests;
    }

    [MenuItem("Racing Conversion/Diagnostics/Display Frame Rate Policy")]
    public static void Run()
    {
        Results.Clear();
        var report = new Report { unityVersion = Application.unityVersion };
        try
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Run in EditMode, not while entering or running PlayMode.");
            RunSelectionTests();
            RunLifecycleTests();
            report.completed = true;
        }
        catch (Exception exception)
        {
            report.error = exception.ToString();
        }
        report.tests = Results.ToArray();
        report.testCount = Results.Count;
        report.passed = report.completed;
        foreach (Result result in Results) report.passed &= result.passed;
        string directory = Environment.GetEnvironmentVariable("RACING_DISPLAY_OUTPUT");
        if (string.IsNullOrEmpty(directory))
            directory = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Temp", "DisplayFrameRateDiagnostic");
        try
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "summary.json"), JsonUtility.ToJson(report, true));
        }
        catch (Exception exception)
        {
            report.passed = false;
            Debug.LogError("[DisplayDiagnostic] Could not write evidence: " + exception);
        }
        Debug.Log("[DisplayDiagnostic] completed=" + report.completed + ", passed=" + report.passed +
            ", tests=" + report.testCount + ", output=" + directory);
        if (report.error != null) Debug.LogError("[DisplayDiagnostic] " + report.error);
        if (Application.isBatchMode) EditorApplication.Exit(report.passed ? 0 : 2);
    }

    private static DisplayFrameRateController.Mode Mode(int id, float hz, int width = 1080, int height = 2400)
    {
        return new DisplayFrameRateController.Mode(id, width, height, hz);
    }

    private static void RunSelectionTests()
    {
        var sixty = Mode(1, 60f);
        var ninety = Mode(2, 90f);
        var high = Mode(3, 120f);
        Check("60_only", sixty, new[] { sixty }, false, 60, 1, 60f);
        Check("90_only_no_exact_60_mode", ninety, new[] { ninety }, false, 60, 0, 0f);
        Check("60_90_uses_60", ninety, new[] { sixty, ninety }, false, 60, 1, 60f);
        Check("60_120_uses_120", sixty, new[] { sixty, high }, false, 120, 3, 120f);
        Check("60_144_is_not_120", sixty, new[] { sixty, Mode(4, 144f) }, false, 60, 1, 60f);
        Check("120_only", high, new[] { high }, false, 120, 3, 120f);
        Check("other_resolution_120_excluded", sixty, new[] { sixty, Mode(4, 120f, 1440, 3200) }, false, 60, 1, 60f);
        Check("fractional_119_88", Mode(1, 59.94f), new[] { Mode(1, 59.94f), Mode(3, 119.88f) }, false, 120, 3, 119.88f);
        Check("fractional_59_94_fallback", Mode(1, 59.94f), new[] { Mode(1, 59.94f), ninety }, false, 60, 1, 59.94f);
        Check("null_modes", sixty, null, false, 60, 0, 0f);
        Check("empty_modes", sixty, new DisplayFrameRateController.Mode[0], false, 60, 0, 0f);
        Check("invalid_current_width", Mode(1, 60f, 0, 2400), new[] { high }, false, 60, 0, 0f);
        Check("invalid_current_height", Mode(1, 60f, 1080, -1), new[] { high }, false, 60, 0, 0f);
        Check("invalid_modes_ignored", sixty, new[] {
            Mode(0, 120f), Mode(-1, 120f), Mode(4, float.NaN), Mode(5, float.PositiveInfinity),
            Mode(6, float.NegativeInfinity), Mode(7, -120f), Mode(8, 0f),
            Mode(9, 120f, 0, 2400), Mode(10, 120f, 1080, -1), sixty
        }, false, 60, 1, 60f);
        Check("outside_tolerance_excluded", sixty, new[] { sixty, Mode(3, 119.49f), Mode(4, 120.51f) }, false, 60, 1, 60f);
        Check("tolerance_boundary_included", sixty, new[] { sixty, Mode(3, 119.5f) }, false, 120, 3, 119.5f);
        Check("power_saver_falls_back", high, new[] { sixty, high }, true, 60, 1, 60f);
        Check("power_saver_120_only_no_exact_60_mode", high, new[] { high }, true, 60, 0, 0f);
        Check("portrait_physical_dimensions", sixty, new[] { sixty, high, Mode(4, 120f, 2400, 1080) }, false, 120, 3, 120f);
        Check("landscape_physical_dimensions", Mode(1, 60f, 2400, 1080), new[] {
            Mode(1, 60f, 2400, 1080), Mode(3, 120f, 2400, 1080), high
        }, false, 120, 3, 120f);
        Check("swapped_dimensions_not_same_mode", sixty, new[] { sixty, Mode(3, 120f, 2400, 1080) }, false, 60, 1, 60f);
        Check("closest_refresh_wins", sixty, new[] { Mode(3, 119.75f), Mode(4, 120f) }, false, 120, 4, 120f);

        // Equal-distance ties prefer the active mode; otherwise stable lowest ID.
        // Check reversed arrays so device enumeration order cannot alter policy.
        Check("tie_prefers_current_forward", high, new[] { Mode(4, 120f), high }, false, 120, 3, 120f);
        Check("tie_prefers_current_reverse", high, new[] { high, Mode(4, 120f) }, false, 120, 3, 120f);
        Check("tie_lowest_id_forward", sixty, new[] { Mode(3, 120f), Mode(4, 120f) }, false, 120, 3, 120f);
        Check("tie_lowest_id_reverse", sixty, new[] { Mode(4, 120f), Mode(3, 120f) }, false, 120, 3, 120f);
        Check("equidistant_rates_forward", sixty, new[] { Mode(3, 119.75f), Mode(4, 120.25f) }, false, 120, 3, 119.75f);
        Check("equidistant_rates_reverse", sixty, new[] { Mode(4, 120.25f), Mode(3, 119.75f) }, false, 120, 3, 119.75f);
    }

    private static void Check(string name, DisplayFrameRateController.Mode current,
        DisplayFrameRateController.Mode[] modes, bool powerSaving, int fps, int id, float hz)
    {
        Test(name, () =>
        {
            DisplayFrameRateController.Selection actual = DisplayFrameRateController.SelectMode(current, modes, powerSaving);
            Require(actual.targetFps == fps && actual.modeId == id && Mathf.Abs(actual.requestedHz - hz) < 0.0001f,
                "Expected " + Format(fps, id, hz) + "; actual " + Format(actual.targetFps, actual.modeId, actual.requestedHz));
            return Format(actual.targetFps, actual.modeId, actual.requestedHz);
        });
    }

    private static string Format(int fps, int id, float hz)
    {
        return "fps=" + fps + ", mode=" + id + ", hz=" + hz.ToString("F3", CultureInfo.InvariantCulture);
    }

    private static void RunLifecycleTests()
    {
        Type type = typeof(DisplayFrameRateController);
        FieldInfo singleton = type.GetField("instance", StaticFlags);
        if (singleton == null) throw new MissingFieldException(type.FullName, "instance");
        object originalSingleton = singleton.GetValue(null);
        if (originalSingleton != null)
            throw new InvalidOperationException("Existing display controller detected; lifecycle checks refuse to touch a live singleton.");
        int oldTarget = Application.targetFrameRate;
        float oldFixedDelta = Time.fixedDeltaTime;
        var owner = new GameObject("DisplayRateDiagnostic_Owner") { hideFlags = HideFlags.HideAndDontSave };
        var other = new GameObject("DisplayRateDiagnostic_Other") { hideFlags = HideFlags.HideAndDontSave };
        owner.SetActive(false);
        other.SetActive(false);
        DisplayFrameRateController controller = null;
        try
        {
            controller = owner.AddComponent<DisplayFrameRateController>();
            Invoke(controller, "Awake");
            Test("lifecycle_awake_safe_60", () =>
            {
                Require(Application.targetFrameRate == 60, "Awake must begin with a safe 60 FPS cap.");
                Require(ReferenceEquals(singleton.GetValue(null), controller), "Awake did not register singleton.");
                return "Explicit inactive-component Awake establishes 60 FPS and singleton.";
            });
            Test("lifecycle_duplicate_owner_preserves_120", () =>
            {
                Apply(controller, 120, 3, 120f);
                DisplayFrameRateController.EnsureOn(owner);
                DisplayFrameRateController.EnsureOn(other);
                Require(owner.GetComponents<DisplayFrameRateController>().Length == 1, "Duplicate component on owner.");
                Require(other.GetComponents<DisplayFrameRateController>().Length == 0, "Second owner gained a duplicate component.");
                Require(Application.targetFrameRate == 120, "Ensuring another Manager must not reset the persistent cap.");
                return "Existing instance preserved across EnsureOn calls; target remains 120.";
            });
            Test("lifecycle_focus_and_pause_preserve_cap", () =>
            {
                Apply(controller, 120, 3, 120f);
                Invoke(controller, "OnApplicationFocus", false);
                Invoke(controller, "OnApplicationFocus", true);
                Invoke(controller, "OnApplicationPause", true);
                Invoke(controller, "OnApplicationPause", false);
                Require(Application.targetFrameRate == 120, "Lifecycle notifications unexpectedly reset FPS cap.");
                Require((bool)Get(controller, "forceRequest"), "Resume did not schedule a fresh display request.");
                Require((float)Get(controller, "nextProbe") == 0f, "Resume did not schedule an immediate query.");
                return "Focus/resume preserve current cap and schedule refresh re-evaluation.";
            });
            Test("lifecycle_main_thread_observation_consumed", () =>
            {
                Set(controller, "nextProbe", float.MaxValue); // Never query the Editor/native display.
                Set(controller, "pendingObservation", Observation(60, 1, 60f, true));
                Set(controller, "probePending", true);
                Invoke(controller, "Update");
                Require(Application.targetFrameRate == 60, "Pending fallback observation was not applied.");
                Require(Get(controller, "pendingObservation") == null, "Observation was not consumed.");
                Require(!(bool)Get(controller, "probePending"), "Completed query remained pending.");
                return "Pending power-saving fallback applied once on the main thread; no probe launched.";
            });
            Test("lifecycle_no_observation_preserves_cap", () =>
            {
                Apply(controller, 120, 3, 120f);
                Set(controller, "nextProbe", float.MaxValue);
                for (int i = 0; i < 5; i++) Invoke(controller, "Update");
                Require(Application.targetFrameRate == 120, "Quiet Update reset selected FPS cap.");
                return "Five quiet updates preserve 120 FPS without new native requests.";
            });
            Test("lifecycle_does_not_change_physics", () =>
            {
                Require(Time.fixedDeltaTime == oldFixedDelta, "Display policy modified fixed timestep.");
                return "Fixed timestep remains " + oldFixedDelta.ToString("F4", CultureInfo.InvariantCulture) + ".";
            });
            Test("lifecycle_destroy_clears_singleton", () =>
            {
                Invoke(controller, "OnDestroy");
                Require(singleton.GetValue(null) == null, "OnDestroy did not clear owned singleton.");
                return "Explicit inactive-component OnDestroy clears ownership.";
            });
        }
        finally
        {
            if (controller != null) Invoke(controller, "OnDestroy");
            UnityEngine.Object.DestroyImmediate(owner);
            UnityEngine.Object.DestroyImmediate(other);
            singleton.SetValue(null, originalSingleton);
            Application.targetFrameRate = oldTarget;
            Time.fixedDeltaTime = oldFixedDelta;
        }
    }

    private static object Observation(int fps, int mode, float hz, bool saving = false)
    {
        Type type = typeof(DisplayFrameRateController).GetNestedType("Observation", BindingFlags.NonPublic);
        if (type == null) throw new MissingMemberException("DisplayFrameRateController.Observation");
        object observation = Activator.CreateInstance(type, true);
        Set(observation, "selection", new DisplayFrameRateController.Selection { targetFps = fps, modeId = mode, requestedHz = hz });
        Set(observation, "activeHz", hz);
        Set(observation, "activeModeId", mode);
        Set(observation, "powerSaving", saving);
        return observation;
    }

    private static void Apply(DisplayFrameRateController controller, int fps, int mode, float hz)
    {
        Invoke(controller, "ApplyObservation", Observation(fps, mode, hz));
    }

    private static void Test(string name, Func<string> action)
    {
        var result = new Result { name = name };
        try { result.detail = action(); result.passed = true; }
        catch (Exception exception) { result.detail = exception.ToString(); }
        Results.Add(result);
        Debug.Log("[DisplayDiagnostic] " + name + " " + (result.passed ? "PASS" : "FAIL") + ": " + result.detail);
    }

    private static void Require(bool condition, string failure)
    {
        if (!condition) throw new InvalidOperationException(failure);
    }

    private static object Get(object target, string field)
    {
        FieldInfo info = target.GetType().GetField(field, InstanceFlags);
        if (info == null) throw new MissingFieldException(target.GetType().FullName, field);
        return info.GetValue(target);
    }

    private static void Set(object target, string field, object value)
    {
        FieldInfo info = target.GetType().GetField(field, InstanceFlags);
        if (info == null) throw new MissingFieldException(target.GetType().FullName, field);
        info.SetValue(target, value);
    }

    private static void Invoke(object target, string method, params object[] arguments)
    {
        MethodInfo info = target.GetType().GetMethod(method, InstanceFlags);
        if (info == null) throw new MissingMethodException(target.GetType().FullName, method);
        info.Invoke(target, arguments);
    }
}
