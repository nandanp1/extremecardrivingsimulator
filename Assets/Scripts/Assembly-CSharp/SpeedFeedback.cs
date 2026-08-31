using UnityEngine;

[RequireComponent(typeof(VehicleCamera))]
public class SpeedFeedback : MonoBehaviour
{
	private CarLogic _carLogic;

	private VehicleCamera _vehicleCamera;

	public float minFOV = 60f;

	public float maxFOV = 100f;

	[Range(0.01f, 0.99f)]
	public float smoothness = 0.9f;

	[Range(0.01f, 10f)]
	public float maxDollyDistance = 1f;

	[Range(-5f, 5f)]
	public float maxDollyAngle = 1f;

	[Range(0f, 2f)]
	public float verticalShaking = 0.2f;

	[Range(0f, 2f)]
	public float fovShaking = 0.2f;

	[Range(0.01f, 0.99f)]
	public float fovShakingUmbral = 0.8f;

	public float baseSpeed = 2.5f;

	[Range(2f, 10000f)]
	public float speedInterpolation = 2f;

	private float actualSpeed;

	private float oldSpeed = -1f;

	private float actualFOV;

	private float originalDistance;

	private float originalAngle;

	private float originalHeight;

	private void Start()
	{
		_carLogic = MySingleton<Manager>.Instance.gameLogic.currentCar.GetComponent<CarLogic>();
		_vehicleCamera = GetComponent<VehicleCamera>();
		originalDistance = _vehicleCamera.distance;
		originalAngle = _vehicleCamera.xAngle;
		originalHeight = _vehicleCamera.height;
	}

	private void Update()
	{
		if (_carLogic == null)
		{
			_carLogic = MySingleton<Manager>.Instance.gameLogic.currentCar.GetComponent<CarLogic>();
			if (_carLogic == null)
			{
				return;
			}
		}
		actualSpeed += (Mathf.Abs(_carLogic.speedInMetersPerSec) - actualSpeed) / speedInterpolation;
		actualFOV = minFOV * smoothness + minFOV * (1f - smoothness) * (actualSpeed / baseSpeed);
		actualFOV = Mathf.Clamp(actualFOV, minFOV, maxFOV);
		if (actualFOV >= maxFOV * fovShakingUmbral)
		{
			actualFOV += Random.Range(0f - fovShaking, fovShaking);
		}
		_vehicleCamera.distance = originalDistance + (0f - maxDollyDistance) * ((actualFOV - minFOV) / (maxFOV - minFOV));
		_vehicleCamera.height = originalHeight - verticalShaking + _vehicleCamera.transform.eulerAngles.x / _vehicleCamera.xAngle * verticalShaking;
		_vehicleCamera.height = Mathf.Clamp(_vehicleCamera.height, 0f, originalHeight + verticalShaking);
		_vehicleCamera.xAngle = originalAngle + maxDollyAngle * ((actualFOV - minFOV) / (maxFOV - minFOV));
		_vehicleCamera.GetComponent<Camera>().fieldOfView = actualFOV;
	}
}
