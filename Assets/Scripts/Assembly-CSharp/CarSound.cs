using UnityEngine;

public class CarSound : MonoBehaviour
{
	public AudioSource horn;

	public AudioSource EngineAudio;

	public float engineIdleRPM = 600f;

	public float engineGearDownRPM = 3000f;

	public float engineGearUpRPM = 5000f;

	public float engineMaxRPM = 6000f;

	public int gearCount = 5;

	public float transmissionRatio = 13.6f;

	public float engineGearUpSpeed = 5f;

	public float engineGearDownSpeed = 20f;

	public float engineAudioIdlePitch = 0.6f;

	public float engineAudioMaxPitch = 3.5f;

	public float engineAudioIdleVolume = 0.4f;

	public float engineAudioMaxVolume = 0.55f;

	public AudioSource EngineExtraAudio;

	public float engineExtraMinRPM = 4000f;

	public float engineExtraMaxRPM = 5600f;

	public float engineExtraMinPitch = 0.8f;

	public float engineExtraMaxPitch = 1.5f;

	public float engineExtraMinVolume = 0.1f;

	public float engineExtraMaxVolume = 1f;

	public AudioSource TransmissionAudio;

	public float transmissionMinRPM = 4000f;

	public float transmissionMaxRPM = 12000f;

	public float transmissionMinPitch = 0.5f;

	public float transmissionMaxPitch = 1.6f;

	public float transmissionMinVolume = 0.1f;

	public float transmissionMaxVolume = 0.8f;

	public AudioSource VelocityAudio;

	public float velocityMin = 3f;

	public float velocityMax = 30f;

	public float velocityMinPitch = 0.5f;

	public float velocityMaxPitch = 1f;

	public float velocityMinVolume;

	public float velocityMaxVolume = 0.4f;

	public AudioSource SkidAudio;

	public float skidMin = 0.2f;

	public float skidMax = 1f;

	public float skidMaxVolume = 1f;

	public float skidMinPitch = 1f;

	public float skidMaxPitch = 0.9f;

	public AudioSource OffroadAudio;

	public float offroadSilent = 0.02f;

	public float offroadMin = 1f;

	public float offroadMax = 10f;

	public float offroadMinPitch = 0.3f;

	public float offroadMaxPitch = 1.5f;

	public float offroadMinVolume = 0.3f;

	public float offroadMaxVolume = 0.8f;

	public AudioClip WheelBumpAudio;

	public float bumpMinForce = 4000f;

	public float bumpMaxForce = 18000f;

	public float bumpMinVolume = 0.2f;

	public float bumpMaxVolume = 0.6f;

	public AudioSource BodyDragAudio;

	public AudioSource BodyDragOffroadAudio;

	public float dragSilent = 0.01f;

	public float dragMin = 2f;

	public float dragMax = 20f;

	public float dragMinPitch = 0.9f;

	public float dragMaxPitch = 1f;

	public float dragMinVolume = 0.9f;

	public float dragMaxVolume = 1f;

	public AudioClip BodyImpactAudio;

	public AudioClip BodyImpactOffroadAudio;

	public float impactMin = 0.1f;

	public float impactMax = 10f;

	public float impactMinPitch = 0.3f;

	public float impactMaxPitch = 0.6f;

	public float impactRandomPitch = 0.1f;

	public float impactMinVolume = 0.8f;

	public float impactMaxVolume = 1f;

	public float impactRandomVolume = 0.1f;

	public AudioClip BodyScratchAudio;

	public float scratchMin = 2f;

	public float scratchRandom = 0.02f;

	public float scratchInterval = 0.2f;

	public float scratchMinPitch = 0.7f;

	public float scratchMaxPitch = 1.1f;

	public float scratchMinVolume = 0.9f;

	public float scratchMaxVolume = 1f;

	private float m_lastScratchTime;

	public int currentGear;

	public float transmissionRPM;

	public float engineRPM;

	public float skidValue;

	public float offroadValue;

	private CarVisuals m_CarVisuals;

	private CarSettings m_CarSettings;

	private Rigidbody m_Rigidbody;

	private float m_frontWheelRadius;

	private float m_rearWheelRadius;

	private float m_averageWheelRadius;

	private float m_drivenWheelRadius = 0.35f;

	private int m_cachedTractionAxle = int.MinValue;

	private int m_lastGear;

	private float m_engineDamp;

	[Header("Impact audio values")]
	public AudioSource impactAudioSource;

	public float minVolume = 0.6f;

	public float maxVolume = 0.8f;

	public float minPitch = 0.7f;

	public float maxPitch = 1.4f;

	private void OnEnable()
	{
		m_CarVisuals = GetComponent<CarVisuals>();
		m_CarSettings = GetComponent<CarSettings>();
		m_Rigidbody = GetComponent<Rigidbody>();
		CacheWheelRadii();
		if (gearCount < 2)
		{
			gearCount = 2;
		}
		m_engineDamp = engineGearUpSpeed;
	}

	private void CacheWheelRadii()
	{
		float frontRadiusSum = 0f;
		int frontWheelCount = 0;
		float rearRadiusSum = 0f;
		int rearWheelCount = 0;
		CarControl component = GetComponent<CarControl>();
		if (component != null)
		{
			AddWheelRadius(GetEffectiveWheelRadius(component.WheelFL), ref frontRadiusSum, ref frontWheelCount);
			AddWheelRadius(GetEffectiveWheelRadius(component.WheelFR), ref frontRadiusSum, ref frontWheelCount);
			AddWheelRadius(GetEffectiveWheelRadius(component.WheelRL), ref rearRadiusSum, ref rearWheelCount);
			AddWheelRadius(GetEffectiveWheelRadius(component.WheelRR), ref rearRadiusSum, ref rearWheelCount);
		}
		float totalRadius = frontRadiusSum + rearRadiusSum;
		int totalWheelCount = frontWheelCount + rearWheelCount;
		if (totalWheelCount == 0)
		{
			WheelCollider[] componentsInChildren = GetComponentsInChildren<WheelCollider>(true);
			foreach (WheelCollider wheel in componentsInChildren)
			{
				AddWheelRadius(GetEffectiveWheelRadius(wheel), ref totalRadius, ref totalWheelCount);
			}
		}
		m_averageWheelRadius = ((totalWheelCount > 0) ? (totalRadius / (float)totalWheelCount) : 0.35f);
		m_frontWheelRadius = ((frontWheelCount > 0) ? (frontRadiusSum / (float)frontWheelCount) : m_averageWheelRadius);
		m_rearWheelRadius = ((rearWheelCount > 0) ? (rearRadiusSum / (float)rearWheelCount) : m_averageWheelRadius);
		m_cachedTractionAxle = int.MinValue;
		RefreshDrivenWheelRadius();
	}

	private static float GetEffectiveWheelRadius(CarWheel wheel)
	{
		return (wheel != null) ? GetEffectiveWheelRadius(wheel.GetComponent<WheelCollider>()) : 0f;
	}

	private static float GetEffectiveWheelRadius(WheelCollider wheel)
	{
		return (wheel != null) ? Mathf.Abs(wheel.radius * wheel.transform.lossyScale.y) : 0f;
	}

	private static void AddWheelRadius(float radius, ref float radiusSum, ref int wheelCount)
	{
		if (radius > 0.001f)
		{
			radiusSum += radius;
			wheelCount++;
		}
	}

	private void RefreshDrivenWheelRadius()
	{
		int num = ((m_CarSettings != null) ? m_CarSettings.tractionAxle : 2);
		if (num == m_cachedTractionAxle)
		{
			return;
		}
		m_cachedTractionAxle = num;
		switch (num)
		{
		case 0:
			m_drivenWheelRadius = m_frontWheelRadius;
			break;
		case 1:
			m_drivenWheelRadius = m_rearWheelRadius;
			break;
		default:
			m_drivenWheelRadius = m_averageWheelRadius;
			break;
		}
		m_drivenWheelRadius = Mathf.Max(0.001f, m_drivenWheelRadius);
	}

	private void Update()
	{
		RefreshDrivenWheelRadius();
		float roadAngularVelocity = 0f;
		if (m_Rigidbody != null)
		{
			float longitudinalSpeed = Vector3.Dot(m_Rigidbody.linearVelocity, base.transform.forward);
			roadAngularVelocity = longitudinalSpeed / m_drivenWheelRadius;
		}
		// Gear and transmission RPM follow signed road speed.  Per-wheel visual
		// spin can legitimately diverge during a burnout or a hard slide and must
		// not make the road-speed gear display jump.
		transmissionRPM = roadAngularVelocity * 57.29578f / 6f;
		transmissionRPM *= transmissionRatio;
		float value;
		if (Mathf.Abs(roadAngularVelocity) < 1f)
		{
			currentGear = 0;
			value = engineIdleRPM + Mathf.Abs(transmissionRPM);
		}
		else if (transmissionRPM >= 0f)
		{
			float num2 = engineGearUpRPM - engineIdleRPM;
			if (transmissionRPM < num2)
			{
				currentGear = 1;
				value = transmissionRPM + engineIdleRPM;
			}
			else
			{
				float num3 = engineGearUpRPM - engineGearDownRPM;
				currentGear = 2 + (int)((transmissionRPM - num2) / num3);
				if (currentGear > gearCount)
				{
					currentGear = gearCount;
					value = transmissionRPM - num2 - (float)(gearCount - 2) * num3 + engineGearDownRPM;
				}
				else
				{
					value = Mathf.Repeat(transmissionRPM - num2, num3) + engineGearDownRPM;
				}
			}
		}
		else
		{
			currentGear = -1;
			value = Mathf.Abs(transmissionRPM) + engineIdleRPM;
		}
		value = Mathf.Clamp(value, 10f, engineMaxRPM);
		if (currentGear != m_lastGear)
		{
			m_engineDamp = ((currentGear <= m_lastGear) ? engineGearDownSpeed : engineGearUpSpeed);
			m_lastGear = currentGear;
		}
		engineRPM = Mathf.Lerp(engineRPM, value, m_engineDamp * Time.deltaTime);
		if ((bool)EngineAudio)
		{
			ProcessContinuousAudio(EngineAudio, engineRPM, engineIdleRPM, engineMaxRPM, engineAudioIdlePitch, engineAudioMaxPitch, engineAudioIdleVolume, engineAudioMaxVolume);
		}
		if ((bool)EngineExtraAudio)
		{
			ProcessContinuousAudio(EngineExtraAudio, engineRPM, engineExtraMinRPM, engineExtraMaxRPM, engineExtraMinPitch, engineExtraMaxPitch, engineExtraMinVolume, engineExtraMaxVolume);
		}
		if ((bool)TransmissionAudio)
		{
			ProcessContinuousAudio(TransmissionAudio, Mathf.Abs(transmissionRPM), transmissionMinRPM, transmissionMaxRPM, transmissionMinPitch, transmissionMaxPitch, transmissionMinVolume, transmissionMaxVolume);
		}
		if ((bool)VelocityAudio)
		{
			ProcessContinuousAudio(VelocityAudio, m_Rigidbody.linearVelocity.magnitude, velocityMin, velocityMax, velocityMinPitch, velocityMaxPitch, velocityMinVolume, velocityMaxVolume);
		}
		// Tire, skid and off-road audio below intentionally continues to use the
		// per-wheel visual/slip telemetry; only gear selection was decoupled.
		float num4 = 0f;
		float num5 = 0f;
		int num6 = 0;
		if (m_CarVisuals.skidValueFL >= 0f)
		{
			num4 += m_CarVisuals.skidValueFL;
		}
		else
		{
			num5 -= m_CarVisuals.skidValueFL;
			num6++;
		}
		if (m_CarVisuals.skidValueFR >= 0f)
		{
			num4 += m_CarVisuals.skidValueFR;
		}
		else
		{
			num5 -= m_CarVisuals.skidValueFR;
			num6++;
		}
		if (m_CarVisuals.skidValueRL >= 0f)
		{
			num4 += m_CarVisuals.skidValueRL;
		}
		else
		{
			num5 -= m_CarVisuals.skidValueRL;
			num6++;
		}
		if (m_CarVisuals.skidValueRR >= 0f)
		{
			num4 += m_CarVisuals.skidValueRR;
		}
		else
		{
			num5 -= m_CarVisuals.skidValueRR;
			num6++;
		}
		if (num6 > 1)
		{
			num5 /= (float)num6;
		}
		skidValue = Mathf.Lerp(skidValue, num4, 40f * Time.deltaTime);
		if ((bool)SkidAudio)
		{
			ProcessContinuousAudio(SkidAudio, skidValue, skidMin, skidMax, skidMinPitch, skidMaxPitch, 0f, skidMaxVolume);
		}
		offroadValue = Mathf.Lerp(offroadValue, num5, 20f * Time.deltaTime);
		if ((bool)OffroadAudio)
		{
			ProcessSpeedBasedAudio(OffroadAudio, offroadValue, offroadSilent, offroadMin, offroadMax, 0f, offroadMinPitch, offroadMaxPitch, offroadMinVolume, offroadMaxVolume);
		}
		if ((bool)WheelBumpAudio)
		{
			ProcessWheelBumpAudio(m_CarVisuals.suspensionStressFL, m_CarVisuals.PivotFL);
			ProcessWheelBumpAudio(m_CarVisuals.suspensionStressFR, m_CarVisuals.PivotFR);
			ProcessWheelBumpAudio(m_CarVisuals.suspensionStressRL, m_CarVisuals.PivotRL);
			ProcessWheelBumpAudio(m_CarVisuals.suspensionStressRR, m_CarVisuals.PivotRR);
		}
		float magnitude = m_CarVisuals.localDragVelocity.magnitude;
		if ((bool)BodyDragAudio)
		{
			ProcessSpeedBasedAudio(BodyDragAudio, (m_CarVisuals.localDragSoftSurface && (bool)BodyDragOffroadAudio) ? 0f : magnitude, dragSilent, dragMin, dragMax, dragMinPitch, dragMinPitch, dragMaxPitch, dragMinVolume, dragMaxVolume);
		}
		if ((bool)BodyDragOffroadAudio)
		{
			ProcessSpeedBasedAudio(BodyDragOffroadAudio, (!m_CarVisuals.localDragSoftSurface) ? 0f : magnitude, dragSilent, dragMin, dragMax, dragMinPitch, dragMinPitch, dragMaxPitch, dragMinVolume, dragMaxVolume);
		}
		float magnitude2 = m_CarVisuals.localImpactVelocity.magnitude;
		if (((bool)BodyImpactAudio || (bool)BodyImpactOffroadAudio) && magnitude2 > impactMin)
		{
			float num7 = Mathf.InverseLerp(impactMin, impactMax, magnitude2);
			ProcessOneTimeAudio(impactAudioSource, magnitude2, impactMin, impactMax, minPitch, maxPitch, minVolume, maxVolume);
		}
		if ((bool)BodyScratchAudio && magnitude > scratchMin && !m_CarVisuals.localDragSoftSurface && Random.value < scratchRandom && Time.time - m_lastScratchTime > scratchInterval)
		{
			PlayOneTime(BodyScratchAudio, base.transform.TransformPoint(m_CarVisuals.localDragPosition), Random.Range(scratchMinVolume, scratchMaxVolume), Random.Range(scratchMinPitch, scratchMaxPitch));
			m_lastScratchTime = Time.time;
		}
	}

	private void ProcessContinuousAudio(AudioSource Audio, float audioValue, float audioMin, float audioMax, float minPitch, float maxPitch, float minVolume, float maxVolume)
	{
		float t = Mathf.InverseLerp(audioMin, audioMax, audioValue);
		Audio.pitch = Mathf.Lerp(minPitch, maxPitch, t);
		Audio.volume = Mathf.Lerp(minVolume, maxVolume, t);
		if (!Audio.isPlaying)
		{
			Audio.Play();
		}
		Audio.loop = true;
	}

	private void ProcessOneTimeAudio(AudioSource Audio, float audioValue, float audioMin, float audioMax, float minPitch, float maxPitch, float minVolume, float maxVolume)
	{
		float t = Mathf.InverseLerp(audioMin, audioMax, audioValue);
		Audio.pitch = Mathf.Lerp(minPitch, maxPitch, t);
		Audio.volume = Mathf.Lerp(minVolume, maxVolume, t);
		if (!Audio.isPlaying)
		{
			Audio.Play();
		}
	}

	private void ProcessSpeedBasedAudio(AudioSource Audio, float audioValue, float audioSilent, float audioMin, float audioMax, float silentPitch, float minPitch, float maxPitch, float minVolume, float maxVolume)
	{
		if (audioValue < audioSilent)
		{
			if (Audio.isPlaying)
			{
				Audio.Stop();
			}
			return;
		}
		if (audioValue < audioMin)
		{
			float t = Mathf.InverseLerp(audioSilent, audioMin, audioValue);
			Audio.pitch = Mathf.Lerp(silentPitch, minPitch, t);
			Audio.volume = Mathf.Lerp(0f, minVolume, t);
		}
		else
		{
			float t = Mathf.InverseLerp(audioMin, audioMax, audioValue);
			Audio.pitch = Mathf.Lerp(minPitch, maxPitch, t);
			Audio.volume = Mathf.Lerp(minVolume, maxVolume, t);
		}
		if (!Audio.isPlaying)
		{
			Audio.Play();
		}
		Audio.loop = true;
	}

	private void ProcessWheelBumpAudio(float suspensionStress, Transform suspensionPoint)
	{
		float num = Mathf.InverseLerp(bumpMinForce, bumpMaxForce, suspensionStress);
		if (num > 0f)
		{
			PlayOneTime(WheelBumpAudio, suspensionPoint.position, Mathf.Lerp(bumpMinVolume, bumpMaxVolume, num));
		}
	}

	private void PlayOneTime(AudioClip clip, Vector3 position, float volume)
	{
		PlayOneTime(clip, position, volume, 1f);
	}

	private void PlayOneTime(AudioClip clip, Vector3 position, float volume, float pitch)
	{
		if (!(clip == null))
		{
			GameObject gameObject = new GameObject("One shot audio");
			gameObject.transform.parent = base.transform;
			gameObject.transform.position = position;
			AudioSource audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.clip = clip;
			audioSource.volume = volume;
			audioSource.pitch = pitch;
			audioSource.Play();
			Object.Destroy(gameObject, clip.length);
		}
	}
}
