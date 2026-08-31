using UnityEngine;

public class CarAntiRollBar : MonoBehaviour
{
	public WheelCollider WheelL;

	public WheelCollider WheelR;

	public float AntiRoll = 5000f;

	public float AntiRollFactor = 1f;

	public float AntiRollBias = 0.5f;

	public int StrictMode;

	private float m_extensionL;

	private float m_extensionR;

	private float m_antiRollForce;

	private float m_antiRollRatio;

	private float m_lastExponent;

	private float m_lastBias = -1f;

	public float getExtensionL()
	{
		return m_extensionL;
	}

	public float getExtensionR()
	{
		return m_extensionR;
	}

	public float getAntiRollForce()
	{
		return m_antiRollForce;
	}

	public float getAntiRollRatio()
	{
		return m_antiRollRatio;
	}

	public float getAntiRollTravel()
	{
		return m_extensionL - m_extensionR;
	}

	private void FixedUpdate()
	{
		WheelHit hit = default(WheelHit);
		WheelHit hit2 = default(WheelHit);
		bool groundHit = WheelL.GetGroundHit(out hit);
		if (groundHit)
		{
			m_extensionL = (0f - WheelL.transform.InverseTransformPoint(hit.point).y - WheelL.radius) / WheelL.suspensionDistance;
		}
		else
		{
			m_extensionL = 1f;
		}
		bool groundHit2 = WheelR.GetGroundHit(out hit2);
		if (groundHit2)
		{
			m_extensionR = (0f - WheelR.transform.InverseTransformPoint(hit2.point).y - WheelR.radius) / WheelR.suspensionDistance;
		}
		else
		{
			m_extensionR = 1f;
		}
		m_antiRollRatio = Bias(m_extensionL - m_extensionR, AntiRollBias);
		m_antiRollForce = m_antiRollRatio * AntiRoll * AntiRollFactor;
		if (groundHit || StrictMode == 2)
		{
			GetComponent<Rigidbody>().AddForceAtPosition(WheelL.transform.up * (0f - m_antiRollForce), WheelL.transform.position);
		}
		else if (StrictMode == 1)
		{
			GetComponent<Rigidbody>().AddForce(WheelL.transform.up * (0f - m_antiRollForce));
		}
		if (groundHit2 || StrictMode == 2)
		{
			GetComponent<Rigidbody>().AddForceAtPosition(WheelR.transform.up * m_antiRollForce, WheelR.transform.position);
		}
		else if (StrictMode == 1)
		{
			GetComponent<Rigidbody>().AddForce(WheelL.transform.up * m_antiRollForce);
		}
	}

	private float BiasRaw(float x, float fBias)
	{
		if (x <= 0f)
		{
			return 0f;
		}
		if (x >= 1f)
		{
			return 1f;
		}
		if (fBias != m_lastBias)
		{
			if (fBias <= 0f)
			{
				return (!(x >= 1f)) ? 0f : 1f;
			}
			if (fBias >= 1f)
			{
				return (!(x > 0f)) ? 0f : 1f;
			}
			if (fBias == 0.5f)
			{
				return x;
			}
			m_lastExponent = Mathf.Log(fBias) * -1.4427f;
			m_lastBias = fBias;
		}
		return Mathf.Pow(x, m_lastExponent);
	}

	private float Bias(float x, float fBias)
	{
		float num = ((!(fBias <= 0.5f)) ? (1f - BiasRaw(1f - Mathf.Abs(x), 1f - fBias)) : BiasRaw(Mathf.Abs(x), fBias));
		return (!(x < 0f)) ? num : (0f - num);
	}
}
