using CodeStage.AntiCheat.Detectors;
using UnityEngine;

public class DetectorsUsageExample : MonoBehaviour
{
	[HideInInspector]
	public bool injectionDetected;

	[HideInInspector]
	public bool speedHackDetected;

	[HideInInspector]
	public bool obscuredTypeCheatDetected;

	private void Start()
	{
		SpeedHackDetector.StartDetection(OnSpeedHackDetected);
		InjectionDetector.StartDetection(OnInjectionDetected);
		ObscuredCheatingDetector.StartDetection(OnObscuredTypeCheatingDetected);
		ObscuredCheatingDetector.Instance.autoDispose = true;
		ObscuredCheatingDetector.Instance.keepAlive = true;
	}

	private void OnSpeedHackDetected()
	{
		speedHackDetected = true;
		Debug.LogWarning("Speed hack detected!");
	}

	private void OnInjectionDetected()
	{
		injectionDetected = true;
		Debug.LogWarning("Injection detected!");
	}

	private void OnObscuredTypeCheatingDetected()
	{
		obscuredTypeCheatDetected = true;
		Debug.LogWarning("Obscured type cheating detected!");
	}
}
