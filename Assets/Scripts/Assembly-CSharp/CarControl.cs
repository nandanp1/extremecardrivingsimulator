using System;
using UnityEngine;

public class CarControl : MonoBehaviour
{
	private const string GAME_OBJECT_NAME = "GameLogic";

	private const float COAST_DRAG_SCALE = 0.35f;

	private const float ROLLING_HOLD_SPEED = 0.5f;

	private const float MAX_STEERING_LATERAL_ACCELERATION = 14.5f;

	private const float AIRPORT_LOOP_SURFACE_PRELOAD = 12f;

	private const float AIRPORT_LOOP_CONTACT_GRACE = 0.35f;

	private const float AIRPORT_LOOP_MINIMUM_SPEED = 15f;

	private const string AIRPORT_LOOP_SURFACE_NAME = "Airport_Ramp_05_003";

	private const float AIRPORT_LOOP_CENTER_Z = -726.526f;

	private const float AIRPORT_LOOP_CENTERING_GAIN = 1.5f;

	private const float AIRPORT_LOOP_CENTERING_DAMPING = 2.5f;

	private const float AIRPORT_LOOP_MAX_CENTERING_ACCELERATION = 6f;

	public CarWheel WheelFL;

	public CarWheel WheelFR;

	public CarWheel WheelRL;

	public CarWheel WheelRR;

	public CarAntiRollBar AntiRollFront;

	public CarAntiRollBar AntiRollRear;

	public Transform CenterOfMass;

	public CarFrictionCurve ForwardWheelFriction;

	public CarFrictionCurve SidewaysWheelFriction;

	public bool optimized;

	public bool readUserInput;

	public bool reverseRequiresStop;

	public float steerInput;

	public float motorInput;

	public float brakeInput;

	public float handbrakeInput;

	public int gearInput = 1;

	public float steerMax = 45f;

	public float motorMax = 1f;

	public float brakeMax = 1f;

	public float autoSteerLevel = 1f;

	public bool autoMotorMax = true;

	public bool autoBrakeMax = true;

	public float antiRollLevel = 1f;

	public float motorPerformancePeak = 5f;

	public float motorForceFactor = 1f;

	public float motorBalance = 0.5f;

	public float brakeForceFactor = 1.5f;

	public float brakeBalance = 0.5f;

	public float sidewaysDriftFriction = 0.35f;

	public float staticFrictionMax = 1500f;

	public float frontSidewaysGrip = 1f;

	public float rearSidewaysGrip = 1f;

	public bool serviceMode;

	public float airDrag = 6.6000004f;

	public float frictionDrag = 30f;

	public float rollingFrictionSlip = 0.075f;

	public bool tractionV3;

	public float maxSpeed = 40f;

	private float sqrMaxSpeed;

	private bool m_brakeRelease;

	private float m_motorSlip;

	private float m_brakeSlip;

	private float m_frontMotorSlip;

	private float m_frontBrakeSlip;

	private float m_rearMotorSlip;

	private float m_rearBrakeSlip;

	private float m_steerL;

	private float m_steerR;

	private float m_steerAngleMax;

	private float m_maxRollAngle = 45f;

	private Rigidbody m_rigidbody;

	private float m_trackWidth;

	private float m_wheelbase;

	private float m_airportLoopContactGrace;

	private Vector3 m_airportLoopSurfaceNormal = Vector3.up;

	private GameLogic logic;

	private static GameObject[] breakLights;

	private void Awake()
	{
		m_rigidbody = GetComponent<Rigidbody>();
	}

	public string getGear()
	{
		return (gearInput > 0) ? "D" : ((gearInput >= 0) ? "N" : "R");
	}

	public float getMotor()
	{
		return motorInput;
	}

	public float getBrake()
	{
		return brakeInput;
	}

	public float getHandBrake()
	{
		return handbrakeInput;
	}

	public float getSteerL()
	{
		return m_steerL;
	}

	public float getSteerR()
	{
		return m_steerR;
	}

	public float getMaxRollAngle()
	{
		return m_maxRollAngle;
	}

	private void OnEnable()
	{
		ApplyEnabled(WheelFL, true);
		ApplyEnabled(WheelFR, true);
		ApplyEnabled(WheelRL, true);
		ApplyEnabled(WheelRR, true);
	}

	private void OnDisable()
	{
		ApplyEnabled(WheelFL, false);
		ApplyEnabled(WheelFR, false);
		ApplyEnabled(WheelRL, false);
		ApplyEnabled(WheelRR, false);
	}

	private void Start()
	{
		logic = MySingleton<Manager>.Instance.gameLogic;
		sqrMaxSpeed = maxSpeed * maxSpeed;
		if ((bool)CenterOfMass)
		{
			m_rigidbody.centerOfMass = new Vector3(CenterOfMass.localPosition.x * base.transform.localScale.x, CenterOfMass.localPosition.y * base.transform.localScale.y, CenterOfMass.localPosition.z * base.transform.localScale.z);
		}
		WheelCollider component = WheelFL.GetComponent<WheelCollider>();
		Vector3 vector = m_rigidbody.centerOfMass - base.transform.InverseTransformPoint(component.transform.position);
		float x = Mathf.Abs((vector.y + component.radius + component.suspensionDistance / 2f) * base.transform.localScale.y);
		float y = Mathf.Abs(vector.x * base.transform.localScale.x);
		m_maxRollAngle = Mathf.Atan2(y, x) * 57.29578f;
		m_trackWidth = Mathf.Max(0.001f, (WheelFL.transform.position - WheelFR.transform.position).magnitude);
		m_wheelbase = Mathf.Max(0.001f, (WheelFR.transform.position - WheelRR.transform.position).magnitude);
		m_rigidbody.maxAngularVelocity = 10f;
		#if !UNITY_6000_0_OR_NEWER
		// Unity 4 defaulted cone friction to false. Unity 6 removed this control entirely.
		m_rigidbody.useConeFriction = false;
		#endif
		if (optimized)
		{
			ApplyCommonParameters(WheelFL);
			ApplyCommonParameters(WheelFR);
			ApplyCommonParameters(WheelRL);
			ApplyCommonParameters(WheelRR);
			WheelFL.RecalculateStuff();
			WheelFR.RecalculateStuff();
			WheelRL.RecalculateStuff();
			WheelRR.RecalculateStuff();
		}
		breakLights = GameObject.FindGameObjectsWithTag("BreakLight");
		GameObject[] array = breakLights;
		foreach (GameObject gameObject in array)
		{
		}
	}

	private void FixedUpdate()
	{
		ApplyCommonParameters(WheelFL);
		ApplyCommonParameters(WheelFR);
		ApplyCommonParameters(WheelRL);
		ApplyCommonParameters(WheelRR);
		ApplyFrontParameters(WheelFL);
		ApplyFrontParameters(WheelFR);
		ApplyRearParameters(WheelRL);
		ApplyRearParameters(WheelRR);
		m_motorSlip = motorInput * motorMax;
		m_brakeSlip = brakeInput * brakeMax;
		if (gearInput == 0)
		{
			m_motorSlip = 0f;
		}
		else if (gearInput < 0)
		{
			m_motorSlip = 0f - m_motorSlip;
		}
		if (serviceMode)
		{
			m_frontMotorSlip = m_motorSlip;
			m_rearMotorSlip = m_motorSlip;
		}
		else if (motorBalance >= 0.5f)
		{
			m_frontMotorSlip = m_motorSlip * (1f - motorBalance) * 2f;
			m_rearMotorSlip = m_motorSlip;
		}
		else
		{
			m_frontMotorSlip = m_motorSlip;
			m_rearMotorSlip = m_motorSlip * motorBalance * 2f;
		}
		if (serviceMode)
		{
			m_frontBrakeSlip = m_brakeSlip;
			m_rearBrakeSlip = m_brakeSlip;
		}
		else if (brakeBalance >= 0.5f)
		{
			m_frontBrakeSlip = m_brakeSlip * (1f - brakeBalance) * 2f;
			m_rearBrakeSlip = m_brakeSlip;
		}
		else
		{
			m_frontBrakeSlip = m_brakeSlip;
			m_rearBrakeSlip = m_brakeSlip * brakeBalance * 2f;
		}
		ApplyTraction(WheelFL, m_frontMotorSlip, m_frontBrakeSlip, 0f);
		ApplyTraction(WheelFR, m_frontMotorSlip, m_frontBrakeSlip, 0f);
		ApplyTraction(WheelRL, m_rearMotorSlip, m_rearBrakeSlip, handbrakeInput);
		ApplyTraction(WheelRR, m_rearMotorSlip, m_rearBrakeSlip, handbrakeInput);
		ApplyAirportLoopSurfacePreload();
		if (autoSteerLevel > 0f)
		{
			// The recovered source-era slip-angle formula becomes unstable with
			// Unity 6 WheelCollider physics and can spin the car through 140 degrees
			// during one ordinary high-speed turn. Limit commanded steering by a
			// bicycle-model lateral-acceleration envelope so it remains responsive
			// without asking the migrated tires for an impossible direction change.
			Vector3 localVelocity = base.transform.InverseTransformDirection(m_rigidbody.linearVelocity);
			float planarSpeedSqr = localVelocity.x * localVelocity.x + localVelocity.z * localVelocity.z;
			float safeSteerAngle = Mathf.Atan2(m_wheelbase * MAX_STEERING_LATERAL_ACCELERATION,
				Mathf.Max(0.01f, planarSpeedSqr)) * Mathf.Rad2Deg;
			m_steerAngleMax = Mathf.Min(steerMax, safeSteerAngle);
		}
		else
		{
			m_steerAngleMax = steerMax;
		}
		WheelFL.getWheelCollider().steerAngle = m_steerL;
		WheelFR.getWheelCollider().steerAngle = m_steerR;
		if (!serviceMode)
		{
			float num2 = 0.5f * airDrag * 1.29f;
			float num3 = frictionDrag * num2;
			Vector3 linearVelocity = m_rigidbody.linearVelocity;
			Vector3 vector = (0f - num2) * linearVelocity * linearVelocity.magnitude;
			Vector3 vector2 = (0f - num3) * linearVelocity;
			bool coasting = Mathf.Abs(motorInput) < 0.001f && brakeInput < 0.001f && handbrakeInput < 0.001f;
			float dragScale = coasting ? COAST_DRAG_SCALE : 1f;
			m_rigidbody.AddForce((vector + vector2) * dragScale);
		}
		if ((bool)AntiRollFront)
		{
			AntiRollFront.AntiRollFactor = antiRollLevel;
		}
		if ((bool)AntiRollRear)
		{
			AntiRollRear.AntiRollFactor = antiRollLevel;
		}
	}

	private void ApplyAirportLoopSurfacePreload()
	{
		// Restore the legacy arcade adhesion needed by Airport_Ramp_05's broad
		// inverted ceiling, retaining the last surface normal across short mesh
		// seams. Road handling and every other ramp remain untouched.
		Vector3 position = m_rigidbody.worldCenterOfMass;
		bool insideLoop = position.x > 657f && position.x < 752f &&
			position.y > -8f && position.y < 31f &&
			Mathf.Abs(position.z + 726.526f) < 7.5f;
		if (!insideLoop || m_rigidbody.linearVelocity.sqrMagnitude < AIRPORT_LOOP_MINIMUM_SPEED * AIRPORT_LOOP_MINIMUM_SPEED)
		{
			m_airportLoopContactGrace = 0f;
			return;
		}

		Vector3 normalSum = Vector3.zero;
		int surfaceContacts = 0;
		AccumulateAirportLoopSurfaceNormal(WheelFL, ref normalSum, ref surfaceContacts);
		AccumulateAirportLoopSurfaceNormal(WheelFR, ref normalSum, ref surfaceContacts);
		AccumulateAirportLoopSurfaceNormal(WheelRL, ref normalSum, ref surfaceContacts);
		AccumulateAirportLoopSurfaceNormal(WheelRR, ref normalSum, ref surfaceContacts);
		if (surfaceContacts > 0)
		{
			// The loop is extruded along world Z. Legacy triangle normals contain
			// lateral components that would turn a neutral-steering car across the
			// narrow lane, so adhesion follows only the intended X/Y profile.
			normalSum.z = 0f;
			if (normalSum.sqrMagnitude > 0.0001f)
			{
				m_airportLoopSurfaceNormal = normalSum.normalized;
				m_airportLoopContactGrace = AIRPORT_LOOP_CONTACT_GRACE;
			}
			else
			{
				surfaceContacts = 0;
			}
		}
		if (surfaceContacts == 0)
		{
			m_airportLoopContactGrace = Mathf.Max(0f, m_airportLoopContactGrace - Time.fixedDeltaTime);
		}

		if (surfaceContacts > 0 || m_airportLoopContactGrace > 0f)
		{
			m_rigidbody.AddForce(-m_airportLoopSurfaceNormal * AIRPORT_LOOP_SURFACE_PRELOAD, ForceMode.Acceleration);
			if (Mathf.Abs(steerInput) < 0.05f)
			{
				// The source-era loop guided neutral cars through its narrow lane.
				// Counter the migrated mesh's asymmetric contact drift, but give the
				// player full authority as soon as steering input is present.
				float centerError = position.z - AIRPORT_LOOP_CENTER_Z;
				float centeringAcceleration = Mathf.Clamp(
					-centerError * AIRPORT_LOOP_CENTERING_GAIN - m_rigidbody.linearVelocity.z * AIRPORT_LOOP_CENTERING_DAMPING,
					-AIRPORT_LOOP_MAX_CENTERING_ACCELERATION,
					AIRPORT_LOOP_MAX_CENTERING_ACCELERATION);
				m_rigidbody.AddForce(Vector3.forward * centeringAcceleration, ForceMode.Acceleration);
			}
		}
	}

	private static void AccumulateAirportLoopSurfaceNormal(CarWheel wheel, ref Vector3 normalSum, ref int surfaceContacts)
	{
		WheelHit hit;
		if (wheel.getWheelCollider().GetGroundHit(out hit) && (bool)hit.collider && hit.collider.name == AIRPORT_LOOP_SURFACE_NAME)
		{
			normalSum += hit.normal;
			surfaceContacts++;
		}
	}

	private void Update()
	{
		if (readUserInput)
		{
			float num = Mathf.Abs(logic.brakes);
			Vector3 localScale = new Vector3(num, Mathf.Clamp(num, 0f, 0.5f), num);
			GameObject[] array = breakLights;
			foreach (GameObject gameObject in array)
			{
				gameObject.transform.localScale = localScale;
			}
			m_brakeRelease = CarMain.SendInput(this, reverseRequiresStop, m_brakeRelease, logic);
		}
		CalculateSteerAngles();
	}

	public void FindGameLogic()
	{
		GameObject gameObject = GameObject.Find("GameLogic");
		logic = gameObject.GetComponent<GameLogic>();
	}

	private void ApplyEnabled(CarWheel Wheel, bool enable)
	{
		if (Wheel != null)
		{
			Wheel.enabled = enable;
		}
	}

	private void ApplyCommonParameters(CarWheel Wheel)
	{
		Wheel.ForwardWheelFriction = ForwardWheelFriction;
		Wheel.SidewaysWheelFriction = SidewaysWheelFriction;
		Wheel.brakeForceFactor = brakeForceFactor;
		Wheel.motorForceFactor = motorForceFactor;
		Wheel.performancePeak = motorPerformancePeak;
		Wheel.sidewaysDriftFriction = sidewaysDriftFriction;
		Wheel.staticFrictionMax = staticFrictionMax;
		Wheel.serviceMode = serviceMode;
		Wheel.optimized = optimized;
	}

	private void ApplyFrontParameters(CarWheel Wheel)
	{
		Wheel.sidewaysForceFactor = frontSidewaysGrip;
	}

	private void ApplyRearParameters(CarWheel Wheel)
	{
		Wheel.sidewaysForceFactor = rearSidewaysGrip;
	}

	private void ApplyTraction(CarWheel Wheel, float motorSlip, float brakeSlip, float handBrakeInput)
	{
		float forwardPeakSlip = Wheel.getForwardPeakSlip();
		float forwardMaxSlip = Wheel.getForwardMaxSlip();
		WheelHit hit = default(WheelHit);
		float num = Mathf.Abs(motorSlip);
		bool groundHit = Wheel.getWheelCollider().GetGroundHit(out hit);
		float num3;
		if (groundHit)
		{
			Quaternion quaternion = Quaternion.AngleAxis(Wheel.getWheelCollider().steerAngle, Wheel.transform.up);
			Vector3 rhs = quaternion * Wheel.transform.forward;
			Vector3 pointVelocity = m_rigidbody.GetPointVelocity(hit.point);
			if ((bool)hit.collider.attachedRigidbody)
			{
				pointVelocity -= hit.collider.attachedRigidbody.GetPointVelocity(hit.point);
			}
			float num2 = Mathf.Abs(Vector3.Dot(pointVelocity, rhs));
			if (num2 + forwardPeakSlip <= motorMax)
			{
				num3 = num - num2;
				if (num3 < 0f)
				{
					num3 = 0f;
				}
				else if (autoMotorMax && num3 > forwardPeakSlip)
				{
					num3 = forwardPeakSlip;
				}
			}
			else
			{
				float num4 = ((!tractionV3) ? forwardPeakSlip : Mathf.Lerp(forwardPeakSlip, 0f, Mathf.InverseLerp(motorMax - forwardPeakSlip, maxSpeed, num2)));
				num3 = num4 * num / motorMax;
			}
			if (motorSlip < 0f)
			{
				num3 = 0f - num3;
			}
		}
		else
		{
			num3 = motorSlip;
		}
		if (autoBrakeMax && brakeSlip > forwardPeakSlip)
		{
			brakeSlip = forwardPeakSlip;
		}
		brakeSlip = Mathf.Max(brakeSlip, handBrakeInput * forwardMaxSlip);
		if (Mathf.Abs(motorInput) < 0.001f)
		{
			// Modern WheelCollider brake torque turns the legacy rolling and
			// airborne wheel damping into a full tire lock. Keep only a gentle
			// near-stop hold; pedal and handbrake input remain unchanged above.
			float longitudinalSpeed = Mathf.Abs(Vector3.Dot(m_rigidbody.linearVelocity, base.transform.forward));
			float nearStopHold = 1f - Mathf.Clamp01(longitudinalSpeed / ROLLING_HOLD_SPEED);
			brakeSlip += rollingFrictionSlip / brakeForceFactor * nearStopHold;
		}
		if (m_rigidbody.linearVelocity.sqrMagnitude > sqrMaxSpeed)
		{
			num3 = 0f;
		}
		Wheel.motorInput = num3;
		Wheel.brakeInput = brakeSlip;
	}

	private void CalculateSteerAngles()
	{
		if (steerInput > 0f)
		{
			m_steerR = steerMax * steerInput;
			if (m_steerR > m_steerAngleMax)
			{
				m_steerR = m_steerAngleMax;
			}
			m_steerL = 57.29578f * Mathf.Atan(1f / (Mathf.Tan((90f - m_steerR) * ((float)Math.PI / 180f)) + m_trackWidth / m_wheelbase));
		}
		else if (steerInput < 0f)
		{
			m_steerL = steerMax * steerInput;
			if (m_steerL < 0f - m_steerAngleMax)
			{
				m_steerL = 0f - m_steerAngleMax;
			}
			m_steerR = -57.29578f * Mathf.Atan(1f / (Mathf.Tan((90f + m_steerL) * ((float)Math.PI / 180f)) + m_trackWidth / m_wheelbase));
		}
		else
		{
			m_steerL = 0f;
			m_steerR = 0f;
		}
	}

	public void ResetSteerAngles()
	{
		m_steerL = 0f;
		m_steerR = 0f;
	}
}
