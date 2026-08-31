using UnityEngine;

public class CarLightRay : MonoBehaviour
{
	public enum CarLightState
	{
		light = 0,
		shadow = light
	}

	private const float LIGHT_INTERPOLATION_OFFSET = 0.05f;

	public float onShadowValue = 0.3f;

	public float onLampValue = 1.4f;

	public float referenceLightValue = 1f;

	public Transform rayOrigin;

	public float rayDistance;

	public LayerMask layerMask;

	public float lampStartOffset = -5f;

	public float lampRayDistance = 5f;

	public LayerMask lampsLayerMask;

	private new Light light;

	private CarVisuals carVisuals;

	private float objetiveLightValue = 1f;

	private bool darkenCar;

	private bool lightCar;

	private float interpolationParam;

	private float speedInterpolation = 3f;

	private float lightValue = 1f;

	private float speedInterpolationMin = 0.1f;

	private float speedInterpolationMax = 0.33f;

	private float MinSpeedMS = 0.277778f;

	private float MaxSpeedMS = 33.3333f;

	private bool underLampLight;

	private CarLightState LightState;

	private void Awake()
	{
		// Unity 6 no longer permits engine API calls from MonoBehaviour field
		// initializers. Keep any value restored from the recovered scene/prefab,
		// and only reproduce the legacy default when nothing was serialized.
		if (lampsLayerMask.value == 0)
		{
			lampsLayerMask = LayerMask.NameToLayer("LampLight");
		}
	}

	private void Start()
	{
		objetiveLightValue = referenceLightValue;
		light = Object.FindFirstObjectByType<Light>();
		carVisuals = GetComponentInParent<CarVisuals>();
	}

	private void Update()
	{
		lightValue = carVisuals.Body.GetComponent<Renderer>().material.GetFloat("_LightIntensity");
		LaunchRay();
		InterpolateCarLight(Time.deltaTime);
	}

	private void InterpolateCarLight(float deltaTime)
	{
		float num = Vector3.Dot(GetComponentInParent<CarControl>().GetComponent<Rigidbody>().linearVelocity, GetComponentInParent<CarControl>().transform.forward);
		float num2 = speedInterpolationMax;
		if (num < MaxSpeedMS && num > 0.5f)
		{
			num2 = Mathf.Abs(num / MaxSpeedMS) * speedInterpolationMax;
		}
		else if (num < 0.5f)
		{
			num2 = speedInterpolationMin;
		}
		lightValue = Mathf.SmoothDamp(lightValue, objetiveLightValue, ref interpolationParam, Time.deltaTime / num2);
		carVisuals.Body.GetComponent<Renderer>().material.SetFloat("_LightIntensity", lightValue);
		if (lightValue <= onShadowValue)
		{
			LightState = CarLightState.light;
		}
		else if (lightValue >= referenceLightValue)
		{
			LightState = CarLightState.light;
		}
	}

	private void LaunchRay()
	{
		Ray ray = new Ray(rayOrigin.transform.position, -light.transform.forward);
		Debug.DrawRay(ray.origin, -light.transform.forward * rayDistance, Color.blue);
		Debug.DrawLine(rayOrigin.transform.position + Vector3.up * lampStartOffset, rayOrigin.transform.position + Vector3.up * lampRayDistance, Color.green);
		RaycastHit hitInfo;
		if (Physics.Linecast(rayOrigin.transform.position + Vector3.up * lampStartOffset, rayOrigin.transform.position + Vector3.up * lampRayDistance, lampsLayerMask))
		{
			if (objetiveLightValue <= onLampValue)
			{
				objetiveLightValue = onLampValue;
			}
		}
		else if (Physics.Raycast(ray, out hitInfo, rayDistance, layerMask))
		{
			if (objetiveLightValue >= referenceLightValue)
			{
				objetiveLightValue = onShadowValue;
			}
		}
		else if (objetiveLightValue <= onShadowValue || objetiveLightValue >= onLampValue)
		{
			objetiveLightValue = referenceLightValue;
		}
	}
}
