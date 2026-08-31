using UnityEngine;

public class CarWheel : MonoBehaviour
{
	public float motorInput;

	public float brakeInput;

	public float motorForceFactor = 1f;

	public float brakeForceFactor = 1f;

	public float sidewaysForceFactor = 1f;

	public float sidewaysDriftFriction = 0.35f;

	public float performancePeak = 2.8f;

	public float staticFrictionMax = 1500f;

	public CarFrictionCurve ForwardWheelFriction = new CarFrictionCurve();

	public CarFrictionCurve SidewaysWheelFriction = new CarFrictionCurve();

	public bool optimized;

	public bool serviceMode;

	private WheelCollider m_wheel;

	private Rigidbody m_rigidbody;

	private Vector2 m_forwardFrictionPeak = new Vector2(1f, 1f);

	private Vector2 m_sidewaysFrictionPeak = new Vector2(1f, 1f);

	private float m_forwardSlipRatio;

	private float m_sidewaysSlipRatio;

	private float m_driftFactor = 1f;

	private WheelHit m_wheelHit;

	private bool m_grounded;

	private float m_DeltaTimeFactor = 1f;

	private int m_vehicleWheelCount = 1;

	private float m_averageNormalLoad;

	public float getForwardPeakSlip()
	{
		// EVP4's PhysX 2 WheelCollider reported a curve extremum that was not the
		// measured tire-force peak.  The Unity 6 migration stores the measured
		// peak explicitly, so no additional legacy multiplier belongs here.
		return m_wheel.forwardFriction.extremumSlip;
	}

	public float getForwardMaxSlip()
	{
		return m_wheel.forwardFriction.asymptoteSlip;
	}

	public float getSidewaysPeakSlip()
	{
		// This recovered EVP4 value is a lateral speed in metres per second.
		// CarControl uses it to limit steer angle as speed rises.  Unity 6's
		// sidewaysFriction slip axis is an angle, so the two values must not be
		// multiplied together.
		return m_sidewaysFrictionPeak.x;
	}

	public float getSidewaysMaxSlip()
	{
		// Keep the public EVP4 telemetry API in lateral-speed units as well.
		return 1f + SidewaysWheelFriction.gripRange;
	}

	public Vector2 getSidewaysPeak()
	{
		return m_sidewaysFrictionPeak;
	}

	public Vector2 getSidewaysMax()
	{
		float maxSlip = 1f + SidewaysWheelFriction.gripRange;
		return new Vector2(maxSlip, CarWheelFriction.GetValue(CarWheelFriction.MCsideways, SidewaysWheelFriction.drift, maxSlip));
	}

	public float getForwardSlipRatio()
	{
		return m_forwardSlipRatio;
	}

	public float getSidewaysSlipRatio()
	{
		return m_sidewaysSlipRatio;
	}

	public float getDriftFactor()
	{
		return m_driftFactor;
	}

	public WheelCollider getWheelCollider()
	{
		return m_wheel;
	}

	private void Start()
	{
		base.gameObject.layer = LayerMask.NameToLayer("Wheel");
		m_wheel = GetComponent<WheelCollider>();
		m_rigidbody = m_wheel.attachedRigidbody;
		CacheVehicleNormalLoad();
		if (optimized)
		{
			RecalculateStuff();
		}
		if (Debug.isDebugBuild)
		{
			Debug.Log("Converted legacy wheel physics for " + base.name + ": sprungMass=" + m_wheel.sprungMass.ToString("F3") + "kg, vehicleWheels=" + m_vehicleWheelCount + ", averageNormalLoad=" + m_averageNormalLoad.ToString("F1") + "N");
		}
	}

	public void RecalculateStuff()
	{
		m_forwardFrictionPeak = CarWheelFriction.GetPeakValue(CarWheelFriction.MCforward, ForwardWheelFriction.grip, ForwardWheelFriction.gripRange, ForwardWheelFriction.drift);
		m_sidewaysFrictionPeak = CarWheelFriction.GetPeakValue(CarWheelFriction.MCsideways, SidewaysWheelFriction.grip, SidewaysWheelFriction.gripRange, SidewaysWheelFriction.drift);
	}

	private void FixedUpdate()
	{
		m_DeltaTimeFactor = Time.fixedDeltaTime * Time.fixedDeltaTime * 2500f;
		m_grounded = m_wheel.GetGroundHit(out m_wheelHit);
		if (!optimized)
		{
			RecalculateStuff();
		}
		float num = Mathf.Abs(motorInput) - brakeInput;
		float slipFactor = ((!serviceMode) ? (performancePeak / m_forwardFrictionPeak.y) : 1f);
		float stiffness = ((!(num >= 0f)) ? brakeForceFactor : motorForceFactor);
		WheelFrictionCurve modernForwardFriction = GetModernForwardFrictionCurve(ForwardWheelFriction, stiffness, slipFactor);
		m_wheel.forwardFriction = modernForwardFriction;
		m_wheel.motorTorque = SlipToModernTorque(motorInput, modernForwardFriction);
		m_wheel.brakeTorque = Mathf.Abs(SlipToModernTorque(brakeInput, modernForwardFriction));
		m_driftFactor = Mathf.Lerp(1f, sidewaysDriftFriction, Mathf.InverseLerp(m_wheel.forwardFriction.extremumSlip, m_wheel.forwardFriction.asymptoteSlip, Mathf.Abs((!m_grounded) ? num : m_wheelHit.forwardSlip)));
		stiffness = ((!serviceMode) ? (m_driftFactor * sidewaysForceFactor) : 1f);
		m_wheel.sidewaysFriction = GetModernSidewaysFrictionCurve(stiffness);
		if (m_grounded)
		{
			WheelFrictionCurve sidewaysFriction = m_wheel.sidewaysFriction;
			WheelFrictionCurve forwardFriction = m_wheel.forwardFriction;
			m_forwardSlipRatio = GetWheelSlipRatio(m_wheelHit.forwardSlip, forwardFriction.extremumSlip, forwardFriction.asymptoteSlip);
			m_sidewaysSlipRatio = GetWheelSlipRatio(m_wheelHit.sidewaysSlip, sidewaysFriction.extremumSlip, sidewaysFriction.asymptoteSlip);
			// The recovered forward curves all use a flat tail.  Unity 6 already
			// keeps asymptoteValue beyond asymptoteSlip, so feeding the modern
			// coefficient back through EVP4's PhysX 2 lookup would corrupt units.
			if ((bool)m_wheelHit.collider.sharedMaterial)
			{
				forwardFriction.stiffness *= (m_wheelHit.collider.sharedMaterial.dynamicFriction + 1f) * 0.5f;
				sidewaysFriction.stiffness *= (m_wheelHit.collider.sharedMaterial.dynamicFriction + 1f) * 0.5f;
				Vector3 pointVelocity = m_rigidbody.GetPointVelocity(m_wheelHit.point);
				m_rigidbody.AddForceAtPosition(pointVelocity * pointVelocity.magnitude * (0f - m_wheelHit.force) * m_wheelHit.collider.sharedMaterial.dynamicFriction * 0.001f, m_wheelHit.point);
			}
			m_wheel.sidewaysFriction = sidewaysFriction;
			m_wheel.forwardFriction = forwardFriction;
		}
		else
		{
			m_forwardSlipRatio = 0f;
			m_sidewaysSlipRatio = 0f;
		}
	}

	private WheelFrictionCurve GetModernSidewaysFrictionCurve(float Stiffness)
	{
		// PhysX 3+ reports lateral slip as an angle in radians.  EVP4's recovered
		// lateral-speed points therefore cannot be placed directly on this axis.
		// Keep modern slip-angle points, but restore the measured EVP4 lateral
		// acceleration so the force peak and tail match the recovered car data.
		float gravity = Mathf.Max(0.001f, Physics.gravity.magnitude);
		float peakAcceleration = Mathf.Max(0f, m_sidewaysFrictionPeak.y);
		float maxSlip = 1f + SidewaysWheelFriction.gripRange;
		float asymptoteAcceleration = Mathf.Max(0f, CarWheelFriction.GetValue(CarWheelFriction.MCsideways, SidewaysWheelFriction.drift, maxSlip));
		return new WheelFrictionCurve
		{
			extremumSlip = 0.2f,
			extremumValue = peakAcceleration * m_DeltaTimeFactor / gravity,
			asymptoteSlip = 0.35f,
			asymptoteValue = asymptoteAcceleration * m_DeltaTimeFactor / gravity,
			stiffness = Stiffness
		};
	}

	private WheelFrictionCurve GetModernForwardFrictionCurve(CarFrictionCurve Friction, float Stiffness, float SlipFactor)
	{
		// EVP4 measured the real PhysX 2 response for each serialized gradient.
		// Reconstruct that measured curve as explicit Unity 6 acceleration/load
		// coefficients.  At the normal fixed timestep the peak acceleration is
		// exactly CarControl.motorPerformancePeak (8 m/s^2 for Coche1).
		float gravity = Mathf.Max(0.001f, Physics.gravity.magnitude);
		float peakSlip = Mathf.Max(0.001f, m_forwardFrictionPeak.x * SlipFactor);
		float peakAcceleration = Mathf.Max(0f, m_forwardFrictionPeak.y * SlipFactor);
		float asymptoteSlip = Mathf.Max(peakSlip + 0.001f, (1f + Friction.gripRange) * SlipFactor);
		float asymptoteAcceleration = Mathf.Max(0f, CarWheelFriction.GetValue(CarWheelFriction.MCforward, Friction.drift, 1f + Friction.gripRange) * SlipFactor);
		return new WheelFrictionCurve
		{
			extremumSlip = peakSlip,
			extremumValue = peakAcceleration * m_DeltaTimeFactor / gravity,
			asymptoteSlip = asymptoteSlip,
			asymptoteValue = asymptoteAcceleration * m_DeltaTimeFactor / gravity,
			stiffness = Stiffness
		};
	}

	private void CacheVehicleNormalLoad()
	{
		m_vehicleWheelCount = 1;
		m_averageNormalLoad = m_wheel.sprungMass * Physics.gravity.magnitude;
		if (m_rigidbody != null)
		{
			m_vehicleWheelCount = Mathf.Max(1, m_rigidbody.GetComponentsInChildren<WheelCollider>(true).Length);
			m_averageNormalLoad = m_rigidbody.mass * Physics.gravity.magnitude / m_vehicleWheelCount;
		}
		m_averageNormalLoad = Mathf.Max(1f, m_averageNormalLoad);
	}

	private float SlipToModernTorque(float Slip, WheelFrictionCurve ForwardFriction)
	{
		if (Mathf.Abs(Slip) < 0.0001f)
		{
			return 0f;
		}
		float radius = Mathf.Max(0.01f, m_wheel.radius * Mathf.Abs(m_wheel.transform.lossyScale.y));
		// Equal left/right drive torque avoids a yaw moment from the asymmetric
		// recovered sprung-mass values.  Axle balance is already encoded in Slip.
		float peakForce = m_averageNormalLoad * ForwardFriction.extremumValue * ForwardFriction.stiffness;
		float demand = Mathf.Abs(Slip) / Mathf.Max(0.001f, ForwardFriction.extremumSlip);
		// Limit tiny recovered wheel inertias from producing numerical spikes.
		// A grounded 2x peak still allows visible wheelspin with TC disabled.
		demand = Mathf.Min(demand, m_grounded ? 2f : 1f);
		return Mathf.Sign(Slip) * peakForce * radius * demand;
	}

	private float GetWheelSlipRatio(float Slip, float PeakSlip, float MaxSlip)
	{
		float num = ((!(Slip >= 0f)) ? (0f - Slip) : Slip);
		float num2 = ((num < PeakSlip) ? (num / PeakSlip) : ((!(num < MaxSlip)) ? (2f + num - MaxSlip) : (1f + Mathf.InverseLerp(PeakSlip, MaxSlip, num))));
		return (!(Slip >= 0f)) ? (0f - num2) : num2;
	}
}
