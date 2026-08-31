using System.Collections.Generic;
using UnityEngine;

public class RealTimeLight : MonoBehaviour
{
	public delegate void LightsChangeDelegate(bool isOn);

	public delegate void DuskTimeDelegate(bool isOn);

	public delegate void DawnTimeDelegate(bool isOn);

	private const float dayDuration = 30f;

	public Color ambientLightColorDay = new Color(0.361f, 0.367f, 0.41f);

	public Transform sunLightTransform;

	public Color sunLightColor = new Color(1f, 0.99f, 0.87f);

	public float sunLightIntensity = 1f;

	public Color ambientLightColorNight = new Color(0.16f, 0.18f, 0.22f);

	public Transform moonLightTransform;

	public Color moonLightColor = new Color(0.64f, 0.63f, 0.75f);

	public float moonlightIntensity = 0.5f;

	public Color currentColor;

	[HideInInspector]
	public static bool timeAdvancing;

	private Light myLight;

	private float myTime;

	public bool lightsOn;

	private float lerpDuration = 0.002f;

	private bool DuskTimeEventSent;

	private bool DawnTimeEventSent;

	public List<Material> allDNMaterials;

	public Material[] fixedMaterials;

	private int timeMultiplier = 1;

	[HideInInspector]
	public float time;

	public bool isDay = true;

	public bool enableShadows = true;

	[Range(0.1f, 0.99f)]
	public float dayTimePercentage = 0.5f;

	public float dayDurationInSeconds = 60f;

	[Range(0f, 100f)]
	public float dayNightTimeTransitionPercentage = 15f;

	[Range(0f, 100f)]
	public float DawnShadowStart = 15f;

	[Range(0f, 100f)]
	public float DuskShadowStop = 15f;

	public bool isFreezed;

	private float dayNightTransitionDuration;

	private float dayNightTransitionDurationClock;

	private float DawnShadowTime;

	private float DuskShadowTime;

	private bool DayNightCicleEnabled = true;

	private float timeUntilNextSwap;

	public bool carShadowActive
	{
		get
		{
			return enableShadows;
		}
	}

	public event LightsChangeDelegate OnLightChange;

	public event DuskTimeDelegate OnDuskTime;

	public event DawnTimeDelegate OnDawnTime;

	private void Start()
	{
		Challange.OnChallengeStarted += ChallengeStarted;
		dayNightTransitionDurationClock = 0f;
		dayNightTransitionDuration = dayDurationInSeconds / 2f * dayNightTimeTransitionPercentage / 100f;
		DawnShadowTime = dayNightTransitionDuration * DawnShadowStart / 100f;
		DuskShadowTime = dayNightTransitionDuration * DuskShadowStop / 100f;
		lerpDuration = 1f / dayNightTransitionDuration;
		myLight = GetComponent<Light>();
		AddFixedMaterials();
		SetDayNight(0f);
		if (SettingsManager.GetStarterPackBoughtOrEnded())
		{
			timeUntilNextSwap = Time.time + dayDurationInSeconds * dayTimePercentage * Random.Range(0.4f, 1f);
		}
		else
		{
			timeUntilNextSwap = Time.time + dayDurationInSeconds * dayTimePercentage;
		}
		SwapLights();
	}

	private void OnDestroy()
	{
		Challange.OnChallengeStarted -= ChallengeStarted;
	}

	private void SwapLights()
	{
		if (DayNightCicleEnabled)
		{
			if (!isDay)
			{
				lightsOn = false;
				currentColor = moonLightColor;
			}
			else
			{
				lightsOn = true;
				currentColor = sunLightColor;
			}
			AddFixedMaterials();
			TimeAdvance();
		}
	}

	private void LateUpdate()
	{
		if (isFreezed)
		{
			ChangeLight();
			return;
		}
		if (timeAdvancing && DayNightCicleEnabled)
		{
			ChangeLight();
			TimeForward(Time.deltaTime);
		}
		if (Time.time >= timeUntilNextSwap)
		{
			if (isDay)
			{
				timeUntilNextSwap = Time.time + dayDurationInSeconds * (1f - dayTimePercentage);
			}
			else
			{
				timeUntilNextSwap = Time.time + dayDurationInSeconds * dayTimePercentage;
			}
			SwapLights();
		}
	}

	private void ChangeLight()
	{
		myTime = time;
		currentColor = Color.Lerp(sunLightColor, moonLightColor, myTime);
		myLight.color = currentColor;
		myLight.intensity = Mathf.Lerp(sunLightIntensity, moonlightIntensity, myTime);
		myLight.transform.rotation = Quaternion.Slerp(sunLightTransform.rotation, moonLightTransform.rotation, myTime);
		RenderSettings.ambientLight = Color.Lerp(ambientLightColorDay, ambientLightColorNight, myTime);
	}

	[HideInInspector]
	public void TimeAdvance()
	{
		timeAdvancing = true;
		if (isDay)
		{
			if (OnLightChange != null)
			{
				OnLightChange(true);
			}
			timeMultiplier = 1;
		}
		else
		{
			if (OnLightChange != null)
			{
				OnLightChange(false);
			}
			timeMultiplier = -1;
		}
		timeAdvancing = true;
	}

	public void TimeForward(float deltaTime)
	{
		if (!timeAdvancing)
		{
			return;
		}
		dayNightTransitionDurationClock += deltaTime;
		if (dayNightTransitionDurationClock >= dayNightTransitionDuration)
		{
			dayNightTransitionDurationClock = 0f;
			DuskTimeEventSent = false;
			DawnTimeEventSent = false;
		}
		else
		{
			if (dayNightTransitionDurationClock >= DawnShadowTime && !isDay)
			{
				enableShadows = true;
				if (OnDawnTime != null && !DawnTimeEventSent)
				{
					OnDawnTime(true);
					DawnTimeEventSent = true;
				}
			}
			if (dayNightTransitionDurationClock >= DuskShadowTime && isDay)
			{
				enableShadows = false;
				if (OnDuskTime != null && !DuskTimeEventSent)
				{
					OnDuskTime(true);
					DuskTimeEventSent = true;
				}
			}
		}
		if (isDay)
		{
			time = Mathf.Clamp(time + lerpDuration * deltaTime * (float)timeMultiplier, 0f, 1f);
		}
		else
		{
			time = Mathf.Clamp(time + lerpDuration * deltaTime * (float)timeMultiplier, 0f, 1f);
		}
		SetDayNight(time);
		if (time >= 1f)
		{
			time = 1f;
			timeAdvancing = false;
			timeAdvancing = false;
			isDay = false;
			dayNightTransitionDurationClock = 0f;
			DuskTimeEventSent = false;
			DawnTimeEventSent = false;
			MySingleton<Manager>.Instance.gameLogic.ForceSwitchCarLights();
		}
		else if (time <= 0f)
		{
			time = 0f;
			timeAdvancing = false;
			timeAdvancing = false;
			isDay = true;
			dayNightTransitionDurationClock = 0f;
			DuskTimeEventSent = false;
			DawnTimeEventSent = false;
		}
	}

	public void AddFixedMaterials()
	{
		if (fixedMaterials == null)
		{
			return;
		}
		Material[] array = fixedMaterials;
		foreach (Material item in array)
		{
			if (!allDNMaterials.Contains(item))
			{
				allDNMaterials.Add(item);
			}
		}
	}

	public void AddDNMaterials(List<Material> sceneMaterials, List<Material> commonMaterials)
	{
		foreach (Material sceneMaterial in sceneMaterials)
		{
			if (!allDNMaterials.Contains(sceneMaterial))
			{
				allDNMaterials.Add(sceneMaterial);
			}
		}
		foreach (Material commonMaterial in commonMaterials)
		{
			if (!allDNMaterials.Contains(commonMaterial))
			{
				allDNMaterials.Add(commonMaterial);
			}
		}
	}

	public void RemoveDNMaterials(List<Material> sceneMaterials)
	{
		foreach (Material sceneMaterial in sceneMaterials)
		{
			if (allDNMaterials.Contains(sceneMaterial))
			{
				allDNMaterials.Remove(sceneMaterial);
			}
		}
	}

	public void SetDayNight(float time)
	{
		for (int i = 0; i < allDNMaterials.Count; i++)
		{
			if ((bool)allDNMaterials[i])
			{
				allDNMaterials[i].SetFloat("_DayNight", time);
			}
		}
	}

	public void FreezeDayNightCicle(bool active)
	{
		if (active)
		{
			DayNightCicleEnabled = false;
			time = 0f;
			SetDayNight(time);
			ChangeLight();
			timeAdvancing = false;
			isDay = true;
			lightsOn = false;
			if (OnLightChange != null)
			{
				OnLightChange(false);
			}
		}
		else
		{
			DayNightCicleEnabled = true;
			time = 0f;
			SetDayNight(time);
			isDay = true;
		}
	}

	private void ChallengeStarted(bool active)
	{
		if (active)
		{
			FreezeDayNightCicle(true);
		}
		else
		{
			FreezeDayNightCicle(false);
		}
	}
}
