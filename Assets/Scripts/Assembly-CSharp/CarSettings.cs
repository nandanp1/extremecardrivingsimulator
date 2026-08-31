using UnityEngine;

public class CarSettings : MonoBehaviour
{
	public string description;

	public bool hasABS = true;

	public bool hasTC = true;

	public bool hasESP = true;

	public int hasTractionModes = 1;

	public int mainTractionAxle = 1;

	public bool hasVariableStabilizer;

	public int mainStabilizerMode;

	public bool abs = true;

	public bool tc = true;

	public bool esp = true;

	public int tractionAxle;

	public int stabilizerMode;

	public float stabilizerFactor = 1f;

	public float espLevel = 1f;

	public bool externalInput;

	public bool bypassTC;

	public bool bypassABS;

	public bool serviceMode;

	private CarControl m_Car;

	private CarExternalInput m_ExternalInput;

	private float STAB_OFFROAD = 0.25f;

	private float STAB_COMFORT = 0.8f;

	private float STAB_SPORT = 1f;

	private void Start()
	{
		m_Car = GetComponent<CarControl>();
		m_ExternalInput = GetComponent<CarExternalInput>();
		if ((bool)m_ExternalInput)
		{
			m_ExternalInput.enabled = externalInput;
		}
	}

	private void Update()
	{
		if (serviceMode)
		{
			return;
		}
		tc = tc && hasTC;
		abs = abs && hasABS;
		esp = esp && hasESP;
		if (hasTractionModes > 2 || hasTractionModes < 0)
		{
			hasTractionModes = 0;
		}
		switch (hasTractionModes)
		{
		case 1:
			if (tractionAxle > 2)
			{
				tractionAxle = mainTractionAxle;
			}
			else if (tractionAxle != mainTractionAxle)
			{
				tractionAxle = 2;
			}
			break;
		case 2:
			if (tractionAxle > 2)
			{
				tractionAxle = 0;
			}
			else if (tractionAxle < 0)
			{
				tractionAxle = 2;
			}
			break;
		default:
			if (mainTractionAxle > 2)
			{
				mainTractionAxle = 2;
			}
			else if (mainTractionAxle < 0)
			{
				mainTractionAxle = 0;
			}
			tractionAxle = mainTractionAxle;
			break;
		}
		m_Car.autoMotorMax = tc && !bypassTC;
		m_Car.autoBrakeMax = abs && !bypassABS;
		m_Car.autoSteerLevel = ((!esp) ? 0f : espLevel);
		switch (tractionAxle)
		{
		case 0:
			m_Car.motorBalance = 0f;
			m_Car.motorForceFactor = 2f;
			break;
		case 1:
			m_Car.motorBalance = 1f;
			m_Car.motorForceFactor = 2f;
			break;
		case 2:
			m_Car.motorBalance = 0.5f;
			m_Car.motorForceFactor = 1f;
			break;
		}
		if (!hasVariableStabilizer)
		{
			stabilizerMode = mainStabilizerMode;
		}
		float num = 0f;
		if (stabilizerMode < 0)
		{
			stabilizerMode = 4;
		}
		else if (stabilizerMode > 4)
		{
			stabilizerMode = 0;
		}
		switch (stabilizerMode)
		{
		case 0:
		{
			float magnitude = m_Car.GetComponent<Rigidbody>().linearVelocity.magnitude;
			num = ((!(magnitude < 3.25f)) ? ((!(magnitude < 19.5f)) ? STAB_SPORT : STAB_COMFORT) : STAB_OFFROAD);
			break;
		}
		case 1:
			num = STAB_OFFROAD;
			break;
		case 2:
			num = STAB_COMFORT;
			break;
		case 3:
			num = STAB_SPORT;
			break;
		}
		m_Car.antiRollLevel = num * stabilizerFactor;
		if ((bool)m_ExternalInput)
		{
			m_ExternalInput.enabled = externalInput;
			if (externalInput)
			{
				m_Car.readUserInput = false;
			}
		}
	}

	public bool HasExternalInput()
	{
		return m_ExternalInput != null;
	}

	public string getStabilizerModeStr()
	{
		string result = "none";
		if (stabilizerFactor == 0f)
		{
			return "none";
		}
		switch (stabilizerMode)
		{
		case 0:
		{
			float num = m_Car.antiRollLevel / stabilizerFactor;
			result = ((!(num < STAB_OFFROAD)) ? ((!(num < STAB_COMFORT)) ? ((!(num < STAB_SPORT)) ? "Sport" : "Comfort") : "Offroad") : "none");
			break;
		}
		case 1:
			result = "Offroad";
			break;
		case 2:
			result = "Comfort";
			break;
		case 3:
			result = "Sport";
			break;
		}
		return result;
	}

	public string getStabilizerModeShortStr()
	{
		string result = "none";
		if (stabilizerFactor == 0f)
		{
			return "none";
		}
		switch (stabilizerMode)
		{
		case 0:
			result = "AUTO";
			break;
		case 1:
			result = "offroad";
			break;
		case 2:
			result = "comfort";
			break;
		case 3:
			result = "sport";
			break;
		}
		return result;
	}

	public string getTractionAxleStr()
	{
		string result = "?";
		switch (tractionAxle)
		{
		case 0:
			result = "front";
			break;
		case 1:
			result = "rear";
			break;
		case 2:
			result = "4x4";
			break;
		}
		return result;
	}
}
