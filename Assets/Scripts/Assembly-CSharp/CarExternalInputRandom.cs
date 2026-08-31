using UnityEngine;

public class CarExternalInputRandom : CarExternalInput
{
	public float steerInterval = 2f;

	public float steerIntervalMargin = 1f;

	public float steerSpeed = 4f;

	public float steerStraightRandom = 0.4f;

	public float throttleInterval = 5f;

	public float throttleIntervalMargin = 2f;

	public float throttleSpeed = 3f;

	public float throttleForwardRandom = 0.8f;

	public float throttleReverseRandom = 0.6f;

	private float m_targetSteer;

	private float m_nextSteerTime;

	private float m_targetThrottle;

	private float m_targetBrake;

	private int m_targetGear;

	private float m_nextThrottleTime;

	private void Awake()
	{
	}

	private void Update()
	{
		if (Time.time > m_nextSteerTime)
		{
			if (Random.value < steerStraightRandom)
			{
				m_targetSteer = 0f;
			}
			else
			{
				m_targetSteer = Random.Range(-1f, 1f);
			}
			m_nextSteerTime = Time.time + steerInterval + Random.Range(0f - steerIntervalMargin, steerIntervalMargin);
		}
		if (Time.time > m_nextThrottleTime)
		{
			float num = throttleForwardRandom;
			float magnitude = GetComponent<Rigidbody>().linearVelocity.magnitude;
			if (magnitude < 0.1f && m_targetBrake < 0.001f && m_targetGear != -1)
			{
				num *= 0.4f;
			}
			if (Random.value < num)
			{
				m_targetGear = 1;
				m_targetThrottle = Random.value;
				m_targetBrake = 0f;
			}
			else if (magnitude < 0.5f)
			{
				m_targetGear = -1;
				m_targetBrake = 0f;
				m_targetThrottle = Random.value;
			}
			else
			{
				m_targetThrottle = 0f;
				m_targetBrake = Random.value;
			}
			m_nextThrottleTime = Time.time + throttleInterval + Random.Range(0f - throttleIntervalMargin, throttleIntervalMargin);
		}
		m_CarControl.steerInput = Mathf.MoveTowards(m_CarControl.steerInput, m_targetSteer, steerSpeed * Time.deltaTime);
		m_CarControl.motorInput = Mathf.MoveTowards(m_CarControl.motorInput, m_targetThrottle, throttleSpeed * Time.deltaTime);
		m_CarControl.brakeInput = m_targetBrake;
		m_CarControl.gearInput = m_targetGear;
		m_CarControl.handbrakeInput = 0f;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (base.enabled && collision.contacts.Length > 0)
		{
			float num = Vector3.Dot(base.transform.forward, collision.contacts[0].normal);
			if (num > 0.8f || num < -0.8f)
			{
				m_nextThrottleTime -= throttleInterval * 0.5f;
			}
			if (num > -0.4f && num < 0.4f)
			{
				m_nextSteerTime -= steerInterval * 0.5f;
			}
		}
	}
}
