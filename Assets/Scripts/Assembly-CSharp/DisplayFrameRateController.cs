using System;
using UnityEngine;

// Persistent presentation policy. Physics remains at the recovered 50 Hz step;
// interpolated rigidbodies can be rendered at either 60 or 120 FPS.
public sealed class DisplayFrameRateController : MonoBehaviour
{
	public struct Mode
	{
		public int id, width, height;
		public float hz;
		public Mode(int id, int width, int height, float hz)
		{
			this.id = id; this.width = width; this.height = height; this.hz = hz;
		}
	}

	public struct Selection
	{
		public int targetFps, modeId;
		public float requestedHz;
	}

	private sealed class Observation
	{
		public Selection selection;
		public float activeHz;
		public int activeModeId;
		public bool powerSaving;
		public string error;
	}

	private static DisplayFrameRateController instance;
	private readonly object observationLock = new object();
	private Observation pendingObservation;
	private bool probePending, forceRequest = true, focused = true;
	private float nextProbe;
	private int lastRequestedMode = -1, lastRequestedTarget = -1, lastDisplayId = -1;
	private int loggedMode = -1, loggedTarget = -1;
	private bool loggedPowerSaving;
	private string lastError;
	private float activeHz, requestedHz;
	private double sampleStart;
	private int sampleFrames;

	public static void EnsureOn(GameObject owner)
	{
		if (instance == null) owner.AddComponent<DisplayFrameRateController>();
	}

	// Pure policy shared by runtime and Editor regression tests. Never compare
	// Android physical modes to Screen.width: this app renders at reduced DPI.
	public static Selection SelectMode(Mode current, Mode[] supported, bool powerSaving)
	{
		Mode best = default(Mode);
		int target = 60;
		if (!powerSaving && TryFindMode(current, supported, 120f, out best)) target = 120;
		else TryFindMode(current, supported, 60f, out best);
		return new Selection { targetFps = target, modeId = best.id, requestedHz = best.id > 0 ? best.hz : 0f };
	}

	private static bool TryFindMode(Mode current, Mode[] supported, float desiredHz, out Mode best)
	{
		best = default(Mode);
		float bestDistance = float.MaxValue;
		if (supported == null || current.width <= 0 || current.height <= 0) return false;
		foreach (Mode mode in supported)
		{
			float distance = Mathf.Abs(mode.hz - desiredHz);
			if (mode.id <= 0 || mode.width != current.width || mode.height != current.height ||
				float.IsNaN(mode.hz) || float.IsInfinity(mode.hz) || distance > 0.5f) continue;
			bool tied = Mathf.Approximately(distance, bestDistance);
			if ((!tied && distance < bestDistance) || (tied &&
				(mode.id == current.id || (best.id != current.id && mode.id < best.id))))
			{
				best = mode;
				bestDistance = distance;
			}
		}
		return best.id > 0;
	}

	private void Awake()
	{
		if (instance != null && instance != this) { Destroy(this); return; }
		instance = this;
		Application.targetFrameRate = 60; // Safe until the real display is queried.
		ResetSample();
	}

	private void OnDestroy()
	{
		if (instance == this) instance = null;
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		focused = hasFocus;
		if (hasFocus) { forceRequest = true; nextProbe = 0f; ResetSample(); }
	}

	private void OnApplicationPause(bool paused)
	{
		focused = !paused;
		if (!paused) { forceRequest = true; nextProbe = 0f; ResetSample(); }
	}

	private void ResetSample()
	{
		sampleStart = Time.realtimeSinceStartupAsDouble;
		sampleFrames = 0;
	}

	private void Update()
	{
		if (!focused) return;
		Observation observation = null;
		lock (observationLock)
		{
			if (pendingObservation != null)
			{
				observation = pendingObservation;
				pendingObservation = null;
				probePending = false;
			}
		}
		if (observation != null) ApplyObservation(observation);
		if (!probePending && Time.unscaledTime >= nextProbe)
		{
			nextProbe = Time.unscaledTime + 2f;
			bool request = forceRequest;
			forceRequest = false;
			ProbeDisplay(request);
		}
		sampleFrames++;
		double elapsed = Time.realtimeSinceStartupAsDouble - sampleStart;
		if (elapsed >= 10.0)
		{
			// Update frequency is not a promise of compositor-presented FPS.
			Debug.Log("[DisplayRate] targetFps=" + Application.targetFrameRate +
				", requestedHz=" + requestedHz.ToString("F2") + ", activeHz=" + activeHz.ToString("F2") +
				", measuredUnityFps=" + (sampleFrames / elapsed).ToString("F1") + ", sampleSeconds=" + elapsed.ToString("F1"));
			ResetSample();
		}
	}

	private void ApplyObservation(Observation observation)
	{
		Application.targetFrameRate = observation.selection.targetFps;
		activeHz = observation.activeHz;
		requestedHz = observation.selection.requestedHz;
		if (observation.error != null)
		{
			if (lastError != observation.error) Debug.LogWarning("[DisplayRate] Display query unavailable; targeting 60 FPS: " + observation.error);
			lastError = observation.error;
			return;
		}
		lastError = null;
		if (loggedMode != observation.activeModeId || loggedTarget != observation.selection.targetFps || loggedPowerSaving != observation.powerSaving)
		{
			Debug.Log("[DisplayRate] selectedTargetFps=" + observation.selection.targetFps +
				", requestedModeId=" + observation.selection.modeId + ", requestedHz=" + requestedHz.ToString("F2") +
				", activeModeId=" + observation.activeModeId + ", activeHz=" + activeHz.ToString("F2") +
				", powerSaving=" + observation.powerSaving + ". Display preferences remain subject to Android policy.");
			loggedMode = observation.activeModeId;
			loggedTarget = observation.selection.targetFps;
			loggedPowerSaving = observation.powerSaving;
			ResetSample();
		}
	}

	private void PublishObservation(Observation observation)
	{
		lock (observationLock) pendingObservation = observation;
	}

	private void ProbeDisplay(bool request)
	{
		probePending = true;
#if UNITY_ANDROID && !UNITY_EDITOR
		try
		{
			using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
			{
				activity.Call("runOnUiThread", new AndroidJavaRunnable(() => QueryAndroidDisplay(request)));
			}
		}
		catch (Exception exception)
		{
			PublishObservation(new Observation { selection = new Selection { targetFps = 60 }, error = exception.Message });
		}
#else
		float hz = (float)Screen.currentResolution.refreshRateRatio.value;
		PublishObservation(new Observation { selection = new Selection { targetFps = Mathf.Abs(hz - 120f) < 0.5f ? 120 : 60 }, activeHz = hz });
#endif
	}

#if UNITY_ANDROID && !UNITY_EDITOR
	private static Mode ReadMode(AndroidJavaObject mode)
	{
		return new Mode(mode.Call<int>("getModeId"), mode.Call<int>("getPhysicalWidth"),
			mode.Call<int>("getPhysicalHeight"), mode.Call<float>("getRefreshRate"));
	}

	private void QueryAndroidDisplay(bool force)
	{
		// This callback runs asynchronously on Android's UI thread. Catch errors
		// here, and pass data back; never call Unity APIs from this thread.
		try
		{
			using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
			using (var window = activity.Call<AndroidJavaObject>("getWindow"))
			using (var decor = window.Call<AndroidJavaObject>("getDecorView"))
			using (var display = decor.Call<AndroidJavaObject>("getDisplay"))
			using (var currentObject = display.Call<AndroidJavaObject>("getMode"))
			using (var power = activity.Call<AndroidJavaObject>("getSystemService", "power"))
			{
				Mode current = ReadMode(currentObject);
				AndroidJavaObject[] objects = display.Call<AndroidJavaObject[]>("getSupportedModes");
				Mode[] modes = new Mode[objects.Length];
				try { for (int i = 0; i < objects.Length; i++) modes[i] = ReadMode(objects[i]); }
				finally { foreach (AndroidJavaObject mode in objects) mode.Dispose(); }
				bool saving = power.Call<bool>("isPowerSaveMode");
				Selection selection = SelectMode(current, modes, saving);
				int displayId = display.Call<int>("getDisplayId");
				if (force || selection.modeId != lastRequestedMode || selection.targetFps != lastRequestedTarget || displayId != lastDisplayId)
				{
					using (var attributes = window.Call<AndroidJavaObject>("getAttributes"))
					{
						// Explicit same-resolution mode request (API 23+, app min 25).
						// Android ignores preferredRefreshRate with a nonzero mode ID.
						attributes.Set("preferredDisplayModeId", selection.modeId);
						using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
						{
							// API 34+ accepts this as a Surface frame-rate hint. Older
							// systems require a supported rate; do not invent a 60 Hz mode.
							float rateHint = selection.modeId == 0 && version.GetStatic<int>("SDK_INT") >= 34
								? selection.targetFps : 0f;
							attributes.Set("preferredRefreshRate", rateHint);
						}
						window.Call("setAttributes", attributes);
					}
					lastRequestedMode = selection.modeId;
					lastRequestedTarget = selection.targetFps;
					lastDisplayId = displayId;
				}
				// Do not reassert an unchanged preference every poll: battery,
				// thermal, user refresh settings and game-mode caps remain in charge.
				PublishObservation(new Observation { selection = selection, activeHz = display.Call<float>("getRefreshRate"),
					activeModeId = current.id, powerSaving = saving });
			}
		}
		catch (Exception exception)
		{
			PublishObservation(new Observation { selection = new Selection { targetFps = 60 }, error = exception.Message });
		}
	}
#endif
}
