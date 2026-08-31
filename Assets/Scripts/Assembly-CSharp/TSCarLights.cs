using UnityEngine;

public class TSCarLights : MonoBehaviour
{
	public new static RealTimeLight light;

	public GameObject FrontLights;

	public GameObject BackLights;

	private void Start()
	{
		if (!light)
		{
			light = Object.FindFirstObjectByType<RealTimeLight>();
		}
		light.OnLightChange += SwitchLight;
		if (FrontLights != null)
		{
			FrontLights.SetActive(light.lightsOn);
		}
		if (BackLights != null)
		{
			BackLights.SetActive(light.lightsOn);
		}
	}

	public void ForceUpdate()
	{
	}

	private void SwitchLight(bool onOff)
	{
		if (FrontLights != null)
		{
			FrontLights.SetActive(onOff);
		}
		if (BackLights != null)
		{
			BackLights.SetActive(onOff);
		}
	}

	private void OnDestroy()
	{
		if (MySingleton<Manager>.Instance != null)
		{
			light.OnLightChange -= SwitchLight;
		}
	}
}
