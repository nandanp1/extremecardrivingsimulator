using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarVisuals : MonoBehaviour
{
	public const string MENU_SCENE_NAME = "1Menu";

	public MeshRenderer[] paintableLODMesh;

	public GameObject exteriorRoot;

	public GameObject interiorRoot;

	public Collider colliderBase;

	public GameObject radarMarker;

	public GameObject nameMarker;

	public GameObject iconMarker;

	public Transform PivotFL;

	public Transform PivotFR;

	public Transform PivotRL;

	public Transform PivotRR;

	public Transform MeshFL;

	public Transform MeshFR;

	public Transform MeshRL;

	public Transform MeshRR;

	public Transform[] SteeringWheel;

	public Transform Body;

	public GameObject LightProjectors;

	public GameObject PositionLights;

	public GameObject Flares;

	public Color colorLights;

	private Material flareLightMat;

	private Material projectorDNLightMat;

	public Collider[] ignoredColliders;

	public float forwardSkidmarksBegin = 1.5f;

	public float forwardSkidmarksRange = 1f;

	public float sidewaysSkidmarksBegin = 1.5f;

	public float sidewaysSkidmarksRange = 1f;

	public float skidmarksWidth = 0.275f;

	public float skidmarksOffset;

	public bool alwaysDrawSkidmarks;

	public float forwardSmokeBegin = 5f;

	public float forwardSmokeRange = 3f;

	public float sidewaysSmokeBegin = 4f;

	public float sidewaysSmokeRange = 3f;

	public float smokeStartTime = 2f;

	public float smokePeakTime = 8f;

	public float smokeMaxTime = 10f;

	public float wheelGroundedBias = 0.02f;

	public int steeringWheelMax = 520;

	public float impactThreeshold = 0.6f;

	public float impactInterval = 0.2f;

	public float impactIntervalRandom = 0.4f;

	public float impactMinSpeed = 2f;

	public bool disableRaycast;

	public bool disableWheelVisuals;

	[HideInInspector]
	public float spinRateFL;

	[HideInInspector]
	public float spinRateFR;

	[HideInInspector]
	public float spinRateRL;

	[HideInInspector]
	public float spinRateRR;

	[HideInInspector]
	public float skidValueFL;

	[HideInInspector]
	public float skidValueFR;

	[HideInInspector]
	public float skidValueRL;

	[HideInInspector]
	public float skidValueRR;

	[HideInInspector]
	public float suspensionStressFL;

	[HideInInspector]
	public float suspensionStressFR;

	[HideInInspector]
	public float suspensionStressRL;

	[HideInInspector]
	public float suspensionStressRR;

	[HideInInspector]
	public Vector3 localImpactPosition = Vector3.zero;

	[HideInInspector]
	public Vector3 localImpactVelocity = Vector3.zero;

	[HideInInspector]
	public bool localImpactSoftSurface;

	[HideInInspector]
	public Vector3 localDragPosition = Vector3.zero;

	[HideInInspector]
	public Vector3 localDragVelocity = Vector3.zero;

	[HideInInspector]
	public bool localDragSoftSurface;

	[HideInInspector]
	public Vector3 localDragPositionDiscrete = Vector3.zero;

	[HideInInspector]
	public Vector3 localDragVelocityDiscrete = Vector3.zero;

	private CarControl m_Car;

	private Rigidbody m_Rigidbody;

	private Skidmarks m_skidmarks;

	private ParticleSystem m_skidSmoke;

	private float m_nextSkidEffectsLookupTime;

	private const float SkidEffectsLookupRetryInterval = 1f;

	// Recovered from the SkidSmoke EllipsoidParticleEmitter in Scenes/Common/2Game.unity.
	private const float SkidSmokeMinSize = 0.8f;

	private const float SkidSmokeMaxSize = 1f;

	private const float SkidSmokeMinLifetime = 7f;

	private const float SkidSmokeMaxLifetime = 8f;

	private const float SkidSmokeMinEmission = 40f;

	private const float SkidSmokeMaxEmission = 50f;

	private const float SkidSmokeEmitterVelocityScale = 0.1f;

	private const float SkidSmokeAngularVelocity = 0f;

	private const float SkidSmokeRandomAngularVelocity = 20f;

	private static readonly Vector3 SkidSmokeWorldVelocity = new Vector3(0f, 0.2f, 0f);

	private static readonly Vector3 SkidSmokeLocalVelocity = new Vector3(0f, 0.1f, 0f);

	private static readonly Vector3 SkidSmokeRandomVelocity = new Vector3(0.3f, 0.3f, 0.3f);

	private static readonly Vector3 SkidSmokeEllipsoid = new Vector3(0.3f, 0.2f, 0.2f);

	private WheelVisualData[] m_wheelData;

	private CarWheel m_WheelFL;

	private CarWheel m_WheelFR;

	private CarWheel m_WheelRL;

	private CarWheel m_WheelRR;

	public GameObject shadow;

	private int oldLayer;

	private int m_sumImpactCount;

	private int m_sumImpactCountSoft;

	private Vector3 m_sumImpactPosition = Vector3.zero;

	private Vector3 m_sumImpactVelocity = Vector3.zero;

	private float m_lastImpactTime;

	private void ReplaceLayerHierarchically(Transform parentTransform, string previousLayerName, string newLayerName)
	{
		for (int i = 0; i < parentTransform.transform.childCount; i++)
		{
			Transform child = parentTransform.GetChild(i);
			if (LayerMask.LayerToName(child.gameObject.layer).Equals(previousLayerName))
			{
				child.gameObject.layer = LayerMask.NameToLayer(newLayerName);
			}
			ReplaceLayerHierarchically(child, previousLayerName, newLayerName);
		}
	}

	public void SetCarColor(Color newColor)
	{
		for (int i = 0; i < paintableLODMesh.Length; i++)
		{
			if ((bool)paintableLODMesh[i])
			{
				paintableLODMesh[i].material.color = newColor;
			}
		}
	}

	private void Awake()
	{
		if (Flares != null)
		{
			FrontFlares component = Flares.GetComponent<FrontFlares>();
			if (component != null && component.flares.Length > 0)
			{
				flareLightMat = Flares.GetComponent<FrontFlares>().flares[0].material;
			}
			if (flareLightMat != null && projectorDNLightMat != null)
			{
				flareLightMat.color = colorLights;
				projectorDNLightMat.color = colorLights;
			}
		}
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		SceneManager.sceneLoaded += OnSceneLoaded;
		m_Rigidbody = GetComponent<Rigidbody>();
		TryResolveSkidEffects(true);
		m_Car = GetComponent<CarControl>();
		m_WheelFL = m_Car.WheelFL;
		m_WheelFR = m_Car.WheelFR;
		m_WheelRL = m_Car.WheelRL;
		m_WheelRR = m_Car.WheelRR;
		m_wheelData = new WheelVisualData[4];
		for (int i = 0; i < 4; i++)
		{
			m_wheelData[i] = new WheelVisualData();
		}
		m_wheelData[0].colliderOffset = base.transform.InverseTransformDirection(PivotFL.position - m_WheelFL.transform.position).x;
		m_wheelData[1].colliderOffset = base.transform.InverseTransformDirection(PivotFR.position - m_WheelFR.transform.position).x;
		m_wheelData[2].colliderOffset = base.transform.InverseTransformDirection(PivotRL.position - m_WheelRL.transform.position).x;
		m_wheelData[3].colliderOffset = base.transform.InverseTransformDirection(PivotRR.position - m_WheelRR.transform.position).x;
		m_wheelData[0].skidmarkOffset = m_wheelData[0].colliderOffset - skidmarksOffset;
		m_wheelData[1].skidmarkOffset = m_wheelData[1].colliderOffset + skidmarksOffset;
		m_wheelData[2].skidmarkOffset = m_wheelData[2].colliderOffset - skidmarksOffset;
		m_wheelData[3].skidmarkOffset = m_wheelData[3].colliderOffset + skidmarksOffset;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		TryResolveSkidEffects(true);
	}

	private bool TryResolveSkidEffects(bool forceLookup)
	{
		if ((bool)m_skidmarks && (bool)m_skidSmoke)
		{
			return true;
		}
		if (!forceLookup && Time.unscaledTime < m_nextSkidEffectsLookupTime)
		{
			return false;
		}

		m_nextSkidEffectsLookupTime = Time.unscaledTime + SkidEffectsLookupRetryInterval;
		m_skidmarks = UnityEngine.Object.FindFirstObjectByType(typeof(Skidmarks)) as Skidmarks;
		m_skidSmoke = (bool)m_skidmarks ? m_skidmarks.GetComponentInChildren<ParticleSystem>() : null;
		return (bool)m_skidmarks && (bool)m_skidSmoke;
	}

	private void Update()
	{
		TryResolveSkidEffects(false);
		DoWheelVisuals(m_WheelFL, MeshFL, PivotFL, m_wheelData[0]);
		DoWheelVisuals(m_WheelFR, MeshFR, PivotFR, m_wheelData[1]);
		DoWheelVisuals(m_WheelRL, MeshRL, PivotRL, m_wheelData[2]);
		DoWheelVisuals(m_WheelRR, MeshRR, PivotRR, m_wheelData[3]);
		spinRateFL = m_wheelData[0].angularVelocity;
		spinRateFR = m_wheelData[1].angularVelocity;
		spinRateRL = m_wheelData[2].angularVelocity;
		spinRateRR = m_wheelData[3].angularVelocity;
		skidValueFL = m_wheelData[0].skidValue;
		skidValueFR = m_wheelData[1].skidValue;
		skidValueRL = m_wheelData[2].skidValue;
		skidValueRR = m_wheelData[3].skidValue;
		suspensionStressFL = m_wheelData[0].suspensionStress;
		suspensionStressFR = m_wheelData[1].suspensionStress;
		suspensionStressRL = m_wheelData[2].suspensionStress;
		suspensionStressRR = m_wheelData[3].suspensionStress;
		ProcessImpacts();
		ProcessDrags(Vector3.zero, Vector3.zero, false);
		float steerL = m_Car.getSteerL();
		float steerR = m_Car.getSteerR();
		Collider[] array = ignoredColliders;
		foreach (Collider collider in array)
		{
			oldLayer = collider.gameObject.layer;
			collider.gameObject.layer = 2;
		}
		DoWheelPosition(m_WheelFL, PivotFL, steerL, m_wheelData[0]);
		DoWheelPosition(m_WheelFR, PivotFR, steerR, m_wheelData[1]);
		DoWheelPosition(m_WheelRL, PivotRL, 0f, m_wheelData[2]);
		DoWheelPosition(m_WheelRR, PivotRR, 0f, m_wheelData[3]);
		Collider[] array2 = ignoredColliders;
		foreach (Collider collider2 in array2)
		{
			collider2.gameObject.layer = oldLayer;
		}
		Transform[] steeringWheel = SteeringWheel;
		foreach (Transform transform in steeringWheel)
		{
			float num = ((!((double)m_Car.steerInput >= 0.0)) ? steerL : steerR);
			Vector3 localEulerAngles = transform.localEulerAngles;
			localEulerAngles.z = (float)(-steeringWheelMax) * num / m_Car.steerMax;
			transform.localEulerAngles = localEulerAngles;
		}
	}

	private static bool IsHardSurface(Collider col)
	{
		return !col.sharedMaterial || col.attachedRigidbody != null;
	}

	private static bool IsStaticSurface(Collider col)
	{
		return !col.attachedRigidbody;
	}

	private void DoWheelVisuals(CarWheel Wheel, Transform Graphic, Transform Pivot, WheelVisualData wheelData)
	{
		WheelHit hit = default(WheelHit);
		WheelCollider wheelCollider = Wheel.getWheelCollider();
		if (LegacySceneManager.LoadedLevelName != "1Menu" && !disableWheelVisuals && wheelCollider != null && wheelCollider.GetGroundHit(out hit))
		{
			wheelData.suspensionStress = hit.force - wheelData.lastSuspensionForce;
			wheelData.lastSuspensionForce = hit.force;
			wheelData.wheelVelocity = m_Rigidbody.GetPointVelocity(hit.point);
			if ((bool)hit.collider.attachedRigidbody)
			{
				wheelData.wheelVelocity -= hit.collider.attachedRigidbody.GetPointVelocity(hit.point);
			}
			wheelData.groundSpeed = Pivot.transform.InverseTransformDirection(wheelData.wheelVelocity);
			wheelData.groundSpeed.y = 0f;
			float forwardPeakSlip = Wheel.getForwardPeakSlip();
			float forwardMaxSlip = Wheel.getForwardMaxSlip();
			float motorInput = Wheel.motorInput;
			float brakeInput = Wheel.brakeInput;
			float num = Mathf.Abs(motorInput) - Mathf.Max(brakeInput);
			float num3;
			if (num >= 0f)
			{
				float num2 = num - forwardPeakSlip;
				if (num2 > 0f)
				{
					num3 = Mathf.Abs(wheelData.groundSpeed.z) + num2;
					if (motorInput < 0f)
					{
						num3 = 0f - num3;
					}
				}
				else
				{
					num3 = wheelData.groundSpeed.z;
				}
			}
			else
			{
				float num2 = Mathf.InverseLerp(forwardMaxSlip, forwardPeakSlip, 0f - num);
				num3 = wheelData.groundSpeed.z * num2;
			}
			if (m_Car.serviceMode)
			{
				num3 = RpmToMs(wheelCollider.rpm, wheelCollider.radius * Wheel.transform.lossyScale.y);
			}
			wheelData.angularVelocity = num3 / (wheelCollider.radius * Wheel.transform.lossyScale.y);
			if (wheelData.lastSkidmark != -1 && wheelData.skidmarkTime < Time.fixedDeltaTime)
			{
				wheelData.skidmarkTime += Time.deltaTime;
			}
			else
			{
				bool flag = IsHardSurface(hit.collider);
				bool flag2 = IsStaticSurface(hit.collider);
				float num4 = wheelData.skidmarkTime;
				if (num4 == 0f)
				{
					num4 = Time.deltaTime;
				}
				wheelData.skidmarkTime = 0f;
				float a = Mathf.InverseLerp(forwardSkidmarksBegin, forwardSkidmarksBegin + forwardSkidmarksRange, Mathf.Abs(Wheel.getForwardSlipRatio()));
				float b = Mathf.InverseLerp(sidewaysSkidmarksBegin, sidewaysSkidmarksBegin + sidewaysSkidmarksRange, Mathf.Abs(Wheel.getSidewaysSlipRatio()));
				wheelData.skidValue = Mathf.Max(a, b);
				float num5 = Mathf.Min(forwardSkidmarksBegin, 2f);
				if (num < 0f && Mathf.Abs(Wheel.getForwardSlipRatio()) >= num5)
				{
					a = Mathf.InverseLerp(forwardSkidmarksBegin, forwardSkidmarksBegin + forwardSkidmarksRange, Mathf.Abs(wheelData.groundSpeed.z + num5));
					wheelData.skidValue = Mathf.Max(a, b);
				}
				if (flag)
				{
					float num6 = Mathf.Clamp01(hit.force / wheelCollider.suspensionSpring.spring);
					wheelData.skidValue *= num6;
					float num7 = wheelData.skidValue;
					if (alwaysDrawSkidmarks && wheelData.groundSpeed.magnitude > 0.01f)
					{
						num7 = num6;
					}
					if (num7 > 0f)
					{
						if (flag2 && (bool)m_skidmarks)
						{
							wheelData.lastSkidmark = m_skidmarks.AddSkidMark(hit.point + wheelData.wheelVelocity * num4 + base.transform.right * wheelData.skidmarkOffset, hit.normal, num7, skidmarksWidth, wheelData.lastSkidmark);
						}
						else
						{
							wheelData.lastSkidmark = -1;
						}
					}
					else
					{
						wheelData.lastSkidmark = -1;
					}
				}
				else
				{
					wheelData.skidValue = 0f - Mathf.Max(Mathf.Abs(wheelData.angularVelocity) * wheelCollider.radius * Wheel.transform.lossyScale.y, wheelData.groundSpeed.magnitude);
					wheelData.lastSkidmark = -1;
				}
				if (flag)
				{
					a = Mathf.InverseLerp(forwardSmokeBegin, forwardSmokeBegin + forwardSmokeRange, Mathf.Abs(Wheel.getForwardSlipRatio()));
					b = Mathf.InverseLerp(sidewaysSmokeBegin, sidewaysSmokeBegin + sidewaysSmokeRange, Mathf.Abs(Wheel.getSidewaysSlipRatio())) * Wheel.getDriftFactor();
				}
				else
				{
					a = 0f;
					b = 0f;
				}
				float num8 = Mathf.Max(a, b);
				float num9 = wheelData.skidSmokeIntensity;
				if (b > 0f && num9 < b * smokePeakTime)
				{
					num9 = b * smokePeakTime;
				}
				float num10 = Mathf.Min(forwardSmokeBegin, 2f);
				if (flag && num < 0f && Mathf.Abs(Wheel.getForwardSlipRatio()) >= num10)
				{
					a = Mathf.InverseLerp(forwardSmokeBegin, forwardSmokeBegin + forwardSmokeRange, Mathf.Abs(wheelData.groundSpeed.z + num10));
					num8 = Mathf.Max(a, b);
					if (num9 < smokeStartTime)
					{
						num9 = smokeStartTime;
					}
				}
				num9 = ((!(num8 > 0f)) ? (num9 - num4) : (num9 + num4));
				if (num9 >= smokeMaxTime)
				{
					num9 = smokeMaxTime;
				}
				else if (num9 < 0f)
				{
					num9 = 0f;
				}
				num8 *= Mathf.InverseLerp(smokeStartTime, smokePeakTime, num9);
				Vector3 vector = hit.point + base.transform.up * wheelCollider.radius * Wheel.transform.lossyScale.y * 0.5f + base.transform.right * wheelData.skidmarkOffset;
				if (num8 > 0f && (bool)m_skidSmoke)
				{
					float num11 = UnityEngine.Random.Range(SkidSmokeMinEmission, SkidSmokeMaxEmission);
					float num12 = wheelData.skidSmokeTime * num11;
					float num13 = Time.time * num11;
					int num14 = Mathf.CeilToInt(num13) - Mathf.CeilToInt(num12);
					int num15 = Mathf.CeilToInt(num12);
					Vector3 vector2 = wheelCollider.transform.TransformDirection(SkidSmokeLocalVelocity) + SkidSmokeWorldVelocity;
					vector2 += Pivot.forward * (wheelData.groundSpeed.z - num3) * 0.125f;
					vector2 += wheelData.wheelVelocity * SkidSmokeEmitterVelocityScale;
					for (int i = 0; i < num14; i++)
					{
						float t = Mathf.InverseLerp(num12, num13, num15 + i);
						Vector3 vector3 = new Vector3(UnityEngine.Random.Range(0f - SkidSmokeEllipsoid.x, SkidSmokeEllipsoid.x), UnityEngine.Random.Range(0f - SkidSmokeEllipsoid.y, SkidSmokeEllipsoid.y), UnityEngine.Random.Range(0f - SkidSmokeEllipsoid.z, SkidSmokeEllipsoid.z));
						Vector3 vector4 = new Vector3(UnityEngine.Random.Range(0f - SkidSmokeRandomVelocity.x, SkidSmokeRandomVelocity.x), UnityEngine.Random.Range(0f - SkidSmokeRandomVelocity.y, SkidSmokeRandomVelocity.y), UnityEngine.Random.Range(0f - SkidSmokeRandomVelocity.z, SkidSmokeRandomVelocity.z));
						float num16 = UnityEngine.Random.Range(SkidSmokeMinSize, SkidSmokeMaxSize);
						float num17 = UnityEngine.Random.Range(SkidSmokeMinLifetime, SkidSmokeMaxLifetime);
						float rotation = UnityEngine.Random.value * 360f;
						float angularVelocity = SkidSmokeAngularVelocity + UnityEngine.Random.Range(0f - SkidSmokeRandomAngularVelocity, SkidSmokeRandomAngularVelocity);
						ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
						{
							position = Vector3.Lerp(wheelData.skidSmokePos, vector, t) + vector3,
							velocity = vector2 + vector4,
							startSize = num16,
							startLifetime = num17 * num8,
							startColor = Color.white,
							rotation = rotation,
							angularVelocity = angularVelocity
						};
						m_skidSmoke.Emit(emitParams, 1);
					}
				}
				wheelData.skidSmokeTime = Time.time;
				wheelData.skidSmokePos = vector;
				wheelData.skidSmokeIntensity = num9;
			}
		}
		else
		{
			if (!wheelCollider)
			{
				return;
			}
			wheelData.angularVelocity = wheelCollider.rpm * 6f * ((float)Math.PI / 180f);
			wheelData.suspensionStress = 0f - wheelData.lastSuspensionForce;
			wheelData.lastSuspensionForce = 0f;
			wheelData.skidValue = 0f;
			wheelData.lastSkidmark = -1;
			wheelData.skidSmokeTime = Time.time;
			wheelData.skidSmokePos = Wheel.transform.position - Wheel.transform.up * ((wheelCollider.suspensionDistance + wheelCollider.radius * 0.5f) * Wheel.transform.lossyScale.y) + base.transform.right * wheelData.skidmarkOffset;
			wheelData.skidSmokeIntensity -= Time.deltaTime;
		}
		Graphic.Rotate(wheelData.angularVelocity * 57.29578f * Time.deltaTime, 0f, 0f);
	}

	private void DoWheelPosition(CarWheel Wheel, Transform WheelMesh, float steerAngle, WheelVisualData wheelData)
	{
		Vector3 position = Vector3.zero;
		bool flag = false;
		WheelCollider wheelCollider = Wheel.getWheelCollider();
		if (!wheelCollider)
		{
			return;
		}
		if (!disableRaycast)
		{
			RaycastHit hitInfo = default(RaycastHit);
			if (Physics.Raycast(Wheel.transform.position, -Wheel.transform.up, out hitInfo, (wheelCollider.suspensionDistance + wheelCollider.radius) * Wheel.transform.lossyScale.y, MySingleton<Manager>.Instance.carWheelLayerMask))
			{
				position = hitInfo.point + Wheel.transform.up * (wheelCollider.radius * Wheel.transform.lossyScale.y - wheelGroundedBias) + base.transform.right * wheelData.colliderOffset;
				flag = true;
			}
		}
		else
		{
			WheelHit hit = default(WheelHit);
			if (wheelCollider.GetGroundHit(out hit))
			{
				position = hit.point + Wheel.transform.up * (wheelCollider.radius * Wheel.transform.lossyScale.y - wheelGroundedBias) + base.transform.right * wheelData.colliderOffset;
				flag = true;
			}
		}
		if (flag)
		{
			WheelMesh.position = position;
		}
		else
		{
			WheelMesh.position = Wheel.transform.position - Wheel.transform.up * (wheelCollider.suspensionDistance * Wheel.transform.lossyScale.y + wheelGroundedBias) + base.transform.right * wheelData.colliderOffset;
		}
		Vector3 localEulerAngles = WheelMesh.localEulerAngles;
		localEulerAngles.y = Wheel.transform.localEulerAngles.y + steerAngle;
		localEulerAngles.z = Wheel.transform.localEulerAngles.z;
		WheelMesh.localEulerAngles = localEulerAngles;
	}

	private float RpmToMs(float Rpm, float Radius)
	{
		return (float)Math.PI * Radius * Rpm / 30f;
	}

	private float MsToRpm(float Ms, float Radius)
	{
		return 30f * Ms / ((float)Math.PI * Radius);
	}

	private void ProcessImpacts()
	{
		if (Time.time - m_lastImpactTime >= impactInterval && m_sumImpactCount > 0)
		{
			localImpactPosition = m_sumImpactPosition / m_sumImpactCount;
			localImpactVelocity = m_sumImpactVelocity;
			localImpactSoftSurface = m_sumImpactCountSoft > m_sumImpactCount / 2;
			localDragPositionDiscrete = localDragPosition;
			localDragVelocityDiscrete = localDragVelocity;
			m_sumImpactCount = 0;
			m_sumImpactCountSoft = 0;
			m_sumImpactPosition = Vector3.zero;
			m_sumImpactVelocity = Vector3.zero;
			m_lastImpactTime = Time.time + impactInterval * UnityEngine.Random.Range(0f - impactIntervalRandom, impactIntervalRandom);
		}
		else
		{
			localImpactPosition = Vector3.zero;
			localImpactVelocity = Vector3.zero;
			localDragVelocityDiscrete = Vector3.zero;
		}
	}

	private void ProcessDrags(Vector3 dragPosition, Vector3 dragVelocity, bool dragSoftSurface)
	{
		if (dragVelocity.sqrMagnitude > 0.001f)
		{
			localDragPosition = Vector3.Lerp(localDragPosition, dragPosition, 10f * Time.deltaTime);
			localDragVelocity = Vector3.Lerp(localDragVelocity, dragVelocity, 20f * Time.deltaTime);
			localDragSoftSurface = dragSoftSurface;
		}
		else
		{
			localDragVelocity = Vector3.Lerp(localDragVelocity, Vector3.zero, 10f * Time.deltaTime);
		}
	}

	private void ProcessContacts(Collision col, bool forceImpact)
	{
		int num = 0;
		int num2 = 0;
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		int num3 = 0;
		int num4 = 0;
		Vector3 zero3 = Vector3.zero;
		Vector3 zero4 = Vector3.zero;
		float num5 = impactMinSpeed * impactMinSpeed;
		ContactPoint[] contacts = col.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			ContactPoint contactPoint = contacts[i];
			Collider collider = contactPoint.thisCollider;
			Collider collider2 = contactPoint.otherCollider;
			if (collider == null || collider.attachedRigidbody != m_Rigidbody)
			{
				collider = contactPoint.otherCollider;
				collider2 = contactPoint.thisCollider;
			}
			if (collider.GetType() == typeof(WheelCollider) || collider2.GetType() == typeof(WheelCollider))
			{
				continue;
			}
			Vector3 pointVelocity = m_Rigidbody.GetPointVelocity(contactPoint.point);
			if ((bool)collider2 && (bool)collider2.attachedRigidbody)
			{
				pointVelocity -= collider2.attachedRigidbody.GetPointVelocity(contactPoint.point);
			}
			float num6 = Vector3.Dot(pointVelocity, contactPoint.normal);
			if (num6 < 0f - impactThreeshold || (forceImpact && col.relativeVelocity.sqrMagnitude > num5))
			{
				num++;
				zero += contactPoint.point;
				zero2 += col.relativeVelocity;
				if ((bool)collider2 && !IsHardSurface(collider2))
				{
					num2++;
				}
			}
			else if (num6 < impactThreeshold)
			{
				num3++;
				zero3 += contactPoint.point;
				zero4 += pointVelocity;
				if ((bool)collider2 && !IsHardSurface(collider2))
				{
					num4++;
				}
			}
		}
		if (num > 0)
		{
			zero /= (float)num;
			zero2 /= (float)num;
			m_sumImpactCount++;
			m_sumImpactPosition += base.transform.InverseTransformPoint(zero);
			m_sumImpactVelocity += base.transform.InverseTransformDirection(zero2);
			if (num2 > num / 2)
			{
				m_sumImpactCountSoft++;
			}
		}
		if (num3 > 0)
		{
			zero3 /= (float)num3;
			zero4 /= (float)num3;
			ProcessDrags(base.transform.InverseTransformPoint(zero3), base.transform.InverseTransformDirection(zero4), num4 > num3 / 2);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		ProcessContacts(collision, true);
	}

	private void OnCollisionStay(Collision collision)
	{
		ProcessContacts(collision, false);
	}

	private static float Lin2Log(float value)
	{
		return Mathf.Log(Mathf.Abs(value) + 1f) * Mathf.Sign(value);
	}

	private static Vector3 Lin2Log(Vector3 value)
	{
		return Vector3.ClampMagnitude(value, Lin2Log(value.magnitude));
	}
}
