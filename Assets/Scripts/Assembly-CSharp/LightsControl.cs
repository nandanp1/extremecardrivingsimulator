using UnityEngine;

public class LightsControl : MonoBehaviour
{
	public new static RealTimeLight light;

	public GameObject LightCone;

	private void Start()
	{
		if (!light)
		{
			light = Object.FindFirstObjectByType<RealTimeLight>();
		}
		if ((bool)light)
		{
			light.OnLightChange += SwitchLight;
			base.gameObject.SetActive(light.lightsOn);
			if (LightCone != null)
			{
				LightCone.SetActive(light.lightsOn);
			}
		}
		else
		{
			base.gameObject.SetActive(false);
		}
	}

	public void ForceUpdate()
	{
		base.gameObject.SetActive(!light.isDay);
	}

	private void SwitchLight(bool onOff)
	{
		base.gameObject.SetActive(onOff);
		if (LightCone != null)
		{
			LightCone.SetActive(onOff);
		}
	}

	private void OnDestroy()
	{
		if (MySingleton<Manager>.Instance != null && light != null)
		{
			light.OnLightChange -= SwitchLight;
		}
	}
}
