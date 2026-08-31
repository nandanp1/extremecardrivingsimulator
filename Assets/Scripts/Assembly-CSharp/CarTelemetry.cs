using UnityEngine;

public class CarTelemetry : MonoBehaviour
{
	public CarControl Target;

	public bool Enabled;

	public bool Gizmos;

	public bool Velocity;

	public bool Curves;

	public GUIStyle m_Style = new GUIStyle();

	private int SizeX = 160;

	private int SizeY = 64;

	private GUICanvas m_graphF;

	private GUICanvas m_graphS;

	private GUICanvas m_graphStabF;

	private GUICanvas m_graphStabR;

	private GUICanvas m_graphSpring;

	private GUICanvas m_graphBar0;

	private GUICanvas m_graphBar1;

	private GUICanvas m_graphBar2;

	private CarFrictionCurve SavedForwardCurve = new CarFrictionCurve();

	private CarFrictionCurve SavedSidewaysCurve = new CarFrictionCurve();

	private float savedForwardScale = 1f;

	private float SavedStabBarFront = 1f;

	private float SavedStabBarRear = 1f;

	private string m_sDescription = string.Empty;

	private float m_LastSpeedMS;

	private float m_LastSpeedLatMS;

	private Vector3 m_MaxAngularVelocity = new Vector3(0f, 0f, 0f);

	private void Start()
	{
		m_graphF = new GUICanvas(SizeX, SizeY, 10f, 10f);
		m_graphS = new GUICanvas(SizeX, SizeY, 10f, 25f);
		m_graphF.SetAlpha(0.5f);
		m_graphS.SetAlpha(0.5f);
		m_graphF.Clear(Color.black);
		m_graphF.Grid(1f, 2f, new Color(0f, 0.2f, 0f));
		for (int i = 10; (float)i < m_graphF.CanvasHeight(); i += 10)
		{
			m_graphF.LineH(i, new Color(0f, 0.5f, 0f));
		}
		m_graphF.Save();
		m_graphS.Clear(Color.black);
		m_graphS.Grid(1f, 2f, new Color(0f, 0.2f, 0f));
		for (int j = 10; (float)j < m_graphS.CanvasHeight(); j += 10)
		{
			m_graphS.LineH(j, new Color(0f, 0.5f, 0f));
		}
		m_graphS.Save();
		m_graphStabF = new GUICanvas(96, 64, 1f, 1f);
		m_graphStabR = new GUICanvas(96, 64, 1f, 1f);
		m_graphStabF.SetAlpha(0.5f);
		m_graphStabR.SetAlpha(0.5f);
		m_graphStabF.Clear(Color.black);
		m_graphStabF.Grid(0.25f, 0.25f, new Color(0f, 0.2f, 0f));
		m_graphStabF.Save();
		m_graphStabR.Clear(Color.black);
		m_graphStabR.Grid(0.25f, 0.25f, new Color(0f, 0.2f, 0f));
		m_graphStabR.Save();
		if ((bool)Target)
		{
			UpdateFrictionCurves();
			UpdateStabBarCurves();
		}
		m_graphSpring = new GUICanvas(32, 64, 1f, 1f);
		m_graphSpring.SetAlpha(0.5f);
		m_graphSpring.Clear(Color.black);
		m_graphSpring.Grid(0.5f, 0.25f, new Color(0f, 0.2f, 0f));
		m_graphSpring.Save();
		m_graphBar0 = new GUICanvas(16, 16, 1f, 1f);
		m_graphBar0.Clear(new Color(0f, 0.5f, 0f, 0.5f));
		m_graphBar1 = new GUICanvas(16, 16, 1f, 1f);
		m_graphBar1.Clear(new Color(0.5f, 0.5f, 0f, 0.5f));
		m_graphBar2 = new GUICanvas(16, 16, 1f, 1f);
		m_graphBar2.Clear(new Color(0.75f, 0f, 0f, 0.5f));
	}

	private bool IsSameCurve(CarFrictionCurve c1, CarFrictionCurve c2)
	{
		return c1.grip == c2.grip && c1.gripRange == c2.gripRange && c1.drift == c2.drift && c1.driftSlope == c2.driftSlope;
	}

	private void CloneCurve(CarFrictionCurve original, CarFrictionCurve cloned)
	{
		cloned.grip = original.grip;
		cloned.gripRange = original.gripRange;
		cloned.drift = original.drift;
		cloned.driftSlope = original.driftSlope;
	}

	private void UpdateFrictionCurves()
	{
		if (Target.WheelFL.getWheelCollider() != null)
		{
			m_graphF.Restore();
			m_graphF.LineV(Target.WheelFL.getForwardPeakSlip(), Color.gray);
			m_graphF.LineV(Target.WheelFL.getForwardMaxSlip(), Color.gray);
			CarWheelFriction.DrawScaledFrictionCurve(m_graphF, CarWheelFriction.MCforward, Target.ForwardWheelFriction, Target.WheelFL.getWheelCollider().forwardFriction.extremumSlip, Color.green);
			CloneCurve(Target.ForwardWheelFriction, SavedForwardCurve);
			m_graphS.Restore();
			CarWheelFriction.DrawFrictionCurve(m_graphS, CarWheelFriction.MCsideways, Target.SidewaysWheelFriction, Color.green);
			m_graphS.LineV(Target.WheelFL.getSidewaysPeakSlip(), Color.gray);
			m_graphS.LineV(Target.WheelFL.getSidewaysMaxSlip(), Color.gray);
			CloneCurve(Target.SidewaysWheelFriction, SavedSidewaysCurve);
		}
	}

	private void UpdateStabBarCurves()
	{
		m_graphStabF.Restore();
		m_graphStabF.BiasCurve(Vector2.zero, Vector2.one, Target.AntiRollFront.AntiRollBias, Color.green);
		SavedStabBarFront = Target.AntiRollFront.AntiRollBias;
		m_graphStabR.Restore();
		m_graphStabR.BiasCurve(Vector2.zero, Vector2.one, Target.AntiRollRear.AntiRollBias, Color.green);
		SavedStabBarRear = Target.AntiRollRear.AntiRollBias;
	}

	private void Update()
	{
		float num = 1f;
		if (Target.WheelFL.getWheelCollider() != null)
		{
			num = Target.WheelFL.getWheelCollider().forwardFriction.extremumSlip;
		}
		if ((bool)Target && (!IsSameCurve(Target.ForwardWheelFriction, SavedForwardCurve) || !IsSameCurve(Target.SidewaysWheelFriction, SavedSidewaysCurve) || num != savedForwardScale))
		{
			UpdateFrictionCurves();
			savedForwardScale = num;
		}
		if (Target.AntiRollFront.AntiRollBias != SavedStabBarFront || Target.AntiRollRear.AntiRollBias != SavedStabBarRear)
		{
			UpdateStabBarCurves();
		}
		if (Gizmos)
		{
			DoWheelGizmos(Target.WheelFL);
			DoWheelGizmos(Target.WheelFR);
			DoWheelGizmos(Target.WheelRL);
			DoWheelGizmos(Target.WheelRR);
			Vector3 position = Target.CenterOfMass.position;
			Vector3 vector = Target.transform.forward * 0.05f;
			Vector3 vector2 = Target.transform.up * 0.05f;
			Vector3 vector3 = Target.transform.right * 0.05f;
			Debug.DrawLine(position - vector, position + vector, Color.white, 0f, false);
			Debug.DrawLine(position - vector2, position + vector2, Color.white, 0f, false);
			Debug.DrawLine(position - vector3, position + vector3, Color.white, 0f, false);
		}
	}

	private Color GetSlipColor(float Slip)
	{
		Slip = Mathf.Abs(Slip);
		if (Slip <= 1f)
		{
			return Color.green;
		}
		if (Slip < 2f)
		{
			return Color.yellow;
		}
		return Color.red;
	}

	private string DoWheelTelemetry(CarWheel Wheel, float Compression)
	{
		WheelHit hit = default(WheelHit);
		WheelCollider wheelCollider = Wheel.getWheelCollider();
		if (wheelCollider.GetGroundHit(out hit))
		{
			float forwardSlipRatio = Wheel.getForwardSlipRatio();
			float sidewaysSlipRatio = Wheel.getSidewaysSlipRatio();
			return string.Format("{0} RPM:{1,4:0.} S:{2,5:0.00} F:{3,5:0.} FS:{4,5:0.00} SS:{5,5:0.00} FSR:{6,5:0.00} SSR:{7,5:0.00}\n", Wheel.gameObject.name, wheelCollider.rpm, Compression, hit.force, hit.forwardSlip, hit.sidewaysSlip, forwardSlipRatio, sidewaysSlipRatio, wheelCollider.sidewaysFriction.stiffness, wheelCollider.forwardFriction.stiffness);
		}
		return string.Format("{0} RPM:{1,4:0.} S:{2,5:0.00}\n", Wheel.gameObject.name, wheelCollider.rpm, Compression);
	}

	private void DoWheelGizmos(CarWheel Wheel)
	{
		WheelHit hit = default(WheelHit);
		WheelCollider wheelCollider = Wheel.getWheelCollider();
		if (wheelCollider.GetGroundHit(out hit))
		{
			float forwardSlipRatio = Wheel.getForwardSlipRatio();
			float sidewaysSlipRatio = Wheel.getSidewaysSlipRatio();
			float num = (0f - wheelCollider.transform.InverseTransformPoint(hit.point).y - wheelCollider.radius) / wheelCollider.suspensionDistance;
			RaycastHit hitInfo = default(RaycastHit);
			if (Physics.Raycast(Wheel.transform.position, -Wheel.transform.up, out hitInfo, (wheelCollider.suspensionDistance + wheelCollider.radius) * Wheel.transform.lossyScale.y))
			{
				hit.point = hitInfo.point;
			}
			Debug.DrawLine(hit.point, hit.point + Wheel.transform.up * (hit.force / 10000f), (!(num <= 0f)) ? Color.white : Color.magenta, 0f, false);
			Debug.DrawLine(hit.point, hit.point - Wheel.transform.forward * hit.forwardSlip, GetSlipColor(forwardSlipRatio), 0f, false);
			Debug.DrawLine(hit.point, hit.point - Wheel.transform.right * hit.sidewaysSlip, GetSlipColor(sidewaysSlipRatio), 0f, false);
			if (Velocity)
			{
				Debug.DrawLine(hit.point, hit.point + Target.GetComponent<Rigidbody>().GetPointVelocity(hit.point), Color.blue, 0f, false);
			}
		}
	}

	private void DoTelemetry()
	{
		m_sDescription = string.Empty;
		if (Enabled && (bool)Target)
		{
			float compression = 0f;
			float compression2 = 0f;
			float compression3 = 0f;
			float compression4 = 0f;
			float num = 0f;
			float num2 = 0f;
			if ((bool)Target.AntiRollFront)
			{
				compression = Target.AntiRollFront.getExtensionL();
				compression2 = Target.AntiRollFront.getExtensionR();
				num = Target.AntiRollFront.getAntiRollRatio();
			}
			if ((bool)Target.AntiRollRear)
			{
				compression3 = Target.AntiRollRear.getExtensionL();
				compression4 = Target.AntiRollRear.getExtensionR();
				num2 = Target.AntiRollRear.getAntiRollRatio();
			}
			m_sDescription += DoWheelTelemetry(Target.WheelFL, compression);
			m_sDescription += DoWheelTelemetry(Target.WheelFR, compression2);
			m_sDescription += DoWheelTelemetry(Target.WheelRL, compression3);
			m_sDescription += DoWheelTelemetry(Target.WheelRR, compression4);
			m_sDescription += string.Format("Friction F: [{0,5:0.000}, {1,5:0.000}] S: [{2,5:0.000}, {3,5:0.000}]\n", Target.WheelFL.getForwardPeakSlip(), Target.WheelFL.getForwardMaxSlip(), Target.WheelFL.getSidewaysPeakSlip(), Target.WheelFL.getSidewaysMaxSlip());
			m_sDescription += string.Format("Grip F:{0,4:0.00} R:{1,4:0.00} Stab F:{2,5:0.000} R:{3,5:0.000} Steer L:{4,5:0.0} R:{5,5:0.0}\n", Target.WheelFL.getDriftFactor(), Target.WheelRL.getDriftFactor(), num, num2, Target.getSteerL(), Target.getSteerR());
			float num3 = Vector3.Dot(Target.GetComponent<Rigidbody>().linearVelocity, Target.transform.forward);
			float num4 = Vector3.Dot(Target.GetComponent<Rigidbody>().linearVelocity, Target.transform.right);
			float num5 = Target.transform.localEulerAngles.z;
			if (num5 > 180f)
			{
				num5 -= 360f;
			}
			float num6 = Target.transform.localEulerAngles.x;
			if (num6 > 180f)
			{
				num6 -= 360f;
			}
			Vector3 angularVelocity = Target.GetComponent<Rigidbody>().angularVelocity;
			if (Mathf.Abs(angularVelocity.x) > m_MaxAngularVelocity.x)
			{
				m_MaxAngularVelocity.x = Mathf.Abs(angularVelocity.x);
			}
			if (Mathf.Abs(angularVelocity.y) > m_MaxAngularVelocity.y)
			{
				m_MaxAngularVelocity.y = Mathf.Abs(angularVelocity.y);
			}
			if (Mathf.Abs(angularVelocity.z) > m_MaxAngularVelocity.z)
			{
				m_MaxAngularVelocity.z = Mathf.Abs(angularVelocity.z);
			}
			m_sDescription += string.Format("\nSpeed:{0,6:0.00} m/s {1,5:0.0} km/h {2,5:0.0} mph\n  Abs:{3,6:0.00} m/s Lat:{4,5:0.00} m/s\n  Acc:{5,5:0.00} m/s2 Lat:{6,5:0.00} m/s2\nPitch:{7,6:0.00} Roll:{8,6:0.00}  Max:{9,6:0.00}\n", num3, (double)num3 * 3.6, (double)num3 * 2.237, Target.GetComponent<Rigidbody>().linearVelocity.magnitude, num4, (num3 - m_LastSpeedMS) / Time.deltaTime, (num4 - m_LastSpeedLatMS) / Time.deltaTime, num6, num5, Target.getMaxRollAngle());
			m_sDescription += string.Format(" AngV: {0,5:0.00},{2,5:0.00} Max:{3,5:0.00},{5,5:0.00}\n", angularVelocity.x, angularVelocity.y, angularVelocity.z, m_MaxAngularVelocity.x, m_MaxAngularVelocity.y, m_MaxAngularVelocity.z);
			m_LastSpeedMS = num3;
			m_LastSpeedLatMS = num4;
			m_sDescription += string.Format("\nGear: {0} Accel:{1,5:0.00} Brake:{2,5:0.00} Handbrake:{3,5:0.00} Steer:{4,5:0.00}\nMotorMax:{5,4:0.0} BrakeMax:{6,4:0.0}\n", Target.getGear(), Target.getMotor(), Target.getBrake(), Target.getHandBrake(), Input.GetAxis("Horizontal"), Target.motorMax, Target.brakeMax);
			CarSettings component = Target.GetComponent<CarSettings>();
			if ((bool)component)
			{
				m_sDescription += string.Format("StabMode: {0}{1}{2}\n{3}{4}{5}{6}{7}\n", (component.stabilizerMode != 0) ? string.Empty : "Auto (", component.getStabilizerModeStr(), (component.stabilizerMode != 0) ? string.Empty : ") ", (!component.abs) ? string.Empty : "ABS ", (!component.tc) ? string.Empty : "TC ", (!component.esp) ? string.Empty : "ESP ", component.getTractionAxleStr() + " ", (!((double)Time.timeScale < 1.0)) ? string.Empty : "Slow-Motion ");
			}
		}
	}

	private void FixedUpdate()
	{
		DoTelemetry();
	}

	private void DrawCurvePerformance(int x, int y, GUICanvas graph, float slip, float slipRatio)
	{
		slip = Mathf.Abs(slip);
		slipRatio = Mathf.Abs(slipRatio);
		if (slip > 0f)
		{
			if (slip > graph.CanvasWidth())
			{
				slip = graph.CanvasWidth();
			}
			GUICanvas gUICanvas = ((slipRatio < 1f) ? m_graphBar0 : ((!(slipRatio < 2f)) ? m_graphBar2 : m_graphBar1));
			gUICanvas.GUIStretchDraw(x, y, (int)(slip * graph.ScaleX()), (int)graph.PixelsHeight());
		}
	}

	private void DrawWheelCurves(int x, int y, GUICanvas forwardCurve, GUICanvas sidewaysCurve, CarWheel Wheel)
	{
		forwardCurve.GUIDraw(x, y);
		sidewaysCurve.GUIDraw(x, y + (int)(forwardCurve.PixelsHeight() * 1.1f));
		WheelHit hit = default(WheelHit);
		WheelCollider wheelCollider = Wheel.getWheelCollider();
		if (wheelCollider.GetGroundHit(out hit))
		{
			DrawCurvePerformance(x, y, forwardCurve, hit.forwardSlip, Wheel.getForwardSlipRatio());
			DrawCurvePerformance(x, y + (int)(forwardCurve.PixelsHeight() * 1.1f), sidewaysCurve, hit.sidewaysSlip, Wheel.getSidewaysSlipRatio());
		}
	}

	private void DrawStabBarCurve(int x, int y, GUICanvas graph, CarAntiRollBar Bar)
	{
		GUICanvas gUICanvas = m_graphBar0;
		float num = Mathf.Abs(Bar.getAntiRollRatio());
		float num2 = Mathf.Abs(Bar.getAntiRollTravel());
		if (num > 0.75f)
		{
			gUICanvas = m_graphBar2;
		}
		else if (num > 0.5f)
		{
			gUICanvas = m_graphBar1;
		}
		graph.GUIDraw(x, y);
		gUICanvas.GUIStretchDraw(x, y, (int)(num2 * graph.ScaleX()), (int)graph.PixelsHeight());
	}

	private void DrawSuspensionPerformance(int x, int y, GUICanvas graph, WheelCollider WheelCol, float travel)
	{
		WheelHit hit = default(WheelHit);
		bool groundHit = WheelCol.GetGroundHit(out hit);
		travel = 1f - travel;
		float num = hit.force / WheelCol.suspensionSpring.spring;
		if (groundHit)
		{
			GUICanvas gUICanvas = ((!(travel >= 1f)) ? m_graphBar0 : m_graphBar2);
			GUICanvas gUICanvas2 = ((!(num >= 1f)) ? m_graphBar0 : m_graphBar1);
			travel = Mathf.Clamp01(travel);
			num = Mathf.Clamp01(num);
			gUICanvas.GUIStretchDraw(x, y + (int)((1f - travel) * graph.PixelsHeight()), (int)(0.5f * graph.ScaleX()), (int)(travel * graph.PixelsHeight()));
			gUICanvas2.GUIStretchDraw(x + (int)(0.5f * graph.ScaleX()), y + (int)((1f - num) * graph.PixelsHeight()), (int)(0.5f * graph.ScaleX()), (int)(num * graph.PixelsHeight()));
		}
	}

	private void DrawSuspension(int x, int y, CarAntiRollBar Bar)
	{
		m_graphSpring.GUIDraw(x, y);
		m_graphSpring.GUIDraw(x + (int)m_graphSpring.PixelsWidth() + 4, y);
		DrawSuspensionPerformance(x, y, m_graphSpring, Bar.WheelL, Bar.getExtensionL());
		DrawSuspensionPerformance(x + (int)m_graphSpring.PixelsWidth() + 4, y, m_graphSpring, Bar.WheelR, Bar.getExtensionR());
	}

	private void OnGUI()
	{
		if (Enabled)
		{
			GUI.Box(new Rect(8f, 8f, 440f, 231f), "Telemetry (B to hide)");
			GUI.Label(new Rect(16f, 28f, 600f, 205f), m_sDescription, m_Style);
			if (Curves)
			{
				DrawWheelCurves(460, 4, m_graphF, m_graphS, Target.WheelFL);
				DrawWheelCurves(460 + (int)(m_graphF.PixelsWidth() * 1.1f), 4, m_graphF, m_graphS, Target.WheelFR);
				DrawSuspension(460 + (int)(m_graphF.PixelsWidth() * 2.2f) + 14, 4, Target.AntiRollFront);
				DrawStabBarCurve(460 + (int)(m_graphF.PixelsWidth() * 2.2f), 4 + (int)(m_graphF.PixelsHeight() * 1.1f), m_graphStabF, Target.AntiRollFront);
				DrawWheelCurves(460, 4 + (int)(m_graphF.PixelsHeight() * 2.2f), m_graphF, m_graphS, Target.WheelRL);
				DrawWheelCurves(460 + (int)(m_graphF.PixelsWidth() * 1.1f), 4 + (int)(m_graphF.PixelsHeight() * 2.2f), m_graphF, m_graphS, Target.WheelRR);
				DrawSuspension(460 + (int)(m_graphF.PixelsWidth() * 2.2f) + 14, 4 + (int)(m_graphF.PixelsHeight() * 2.2f), Target.AntiRollRear);
				DrawStabBarCurve(460 + (int)(m_graphF.PixelsWidth() * 2.2f), 4 + (int)(m_graphF.PixelsHeight() * 3.3f), m_graphStabR, Target.AntiRollRear);
			}
		}
	}
}
