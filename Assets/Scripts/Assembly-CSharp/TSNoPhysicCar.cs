using System;
using System.Collections;
using UnityEngine;

public class TSNoPhysicCar : MonoBehaviour
{
	public Renderer[] brakeLigths;

	public float brakeIntensityRate = 0.2f;

	public bool enableDisableBrakeLights;

	public bool engineAudioEnabled = true;

	public Shader brakeLightsShader;

	public string propertyName = "_Intensity";

	public float turnSpeed = 3f;

	public float maxSpeed = 50f;

	public float maxSpeedToTurn = 0.2f;

	public GameObject turnRightLight;

	public GameObject turnLeftLight;

	private float steering;

	private float steerAngle;

	public float throttle;

	public float brake;

	public Vector3 lastPosition;

	private bool blinking;

	public Transform myTransform;

	public TSTrafficAI trafficAI;

	public float speed;

	public Vector3 velo = Vector3.zero;

	private AudioSource audioSource;

	public TSNavigation navigation;

	public Transform rearWheel;

	public Transform[] frontTires;

	public Transform[] rearTires;

	private TSCommonProperties commonProperties;

	public float v0;

	public float v;

	public float a;

	public float t;

	public float engineForce;

	private TSPoints nextWaypoint;

	private Vector3 fixedForward;

	public float angle;

	private Vector3 rotationAmount;

	private float rotation;

	public bool upSideDown
	{
		get
		{
			return commonProperties.upSideDown;
		}
		set
		{
			commonProperties.upSideDown = value;
		}
	}

	public bool crashed
	{
		get
		{
			return commonProperties.crashed;
		}
		set
		{
			commonProperties.crashed = value;
		}
	}

	private void Awake()
	{
		myTransform = base.transform;
		commonProperties = GetComponent<TSCommonProperties>();
		trafficAI = GetComponent<TSTrafficAI>();
		audioSource = GetComponent<AudioSource>();
		navigation = GetComponent<TSNavigation>();
		if (audioSource == null)
		{
			engineAudioEnabled = false;
		}
		if ((bool)GetComponent<Rigidbody>() && base.enabled)
		{
			UnityEngine.Object.DestroyImmediate(GetComponent<Rigidbody>());
		}
	}

	private void OnEnable()
	{
		myTransform = base.transform;
		if ((bool)trafficAI)
		{
			trafficAI.bodies = new Rigidbody[0];
			trafficAI.physicalLocomotion = false;
			TSTrafficAI tSTrafficAI = trafficAI;
			tSTrafficAI.OnUpdateAI = (TSTrafficAI.OnUpdateAIDelegate)Delegate.Combine(tSTrafficAI.OnUpdateAI, new TSTrafficAI.OnUpdateAIDelegate(OnAIUpdate));
			TSTrafficAI tSTrafficAI2 = trafficAI;
			tSTrafficAI2.UpdateCarSpeed = (TSTrafficAI.GetCarSpeedDelegate)Delegate.Combine(tSTrafficAI2.UpdateCarSpeed, new TSTrafficAI.GetCarSpeedDelegate(UpdateCarSpeed));
			TSTrafficAI tSTrafficAI3 = trafficAI;
			tSTrafficAI3.OnTurnLeft = (TSTrafficAI.OnTurnLeftDelegate)Delegate.Combine(tSTrafficAI3.OnTurnLeft, new TSTrafficAI.OnTurnLeftDelegate(OnTurnLeft));
			TSTrafficAI tSTrafficAI4 = trafficAI;
			tSTrafficAI4.OnTurnRight = (TSTrafficAI.OnTurnRigthDelegate)Delegate.Combine(tSTrafficAI4.OnTurnRight, new TSTrafficAI.OnTurnRigthDelegate(OnTurnRight));
		}
		if (!crashed && !upSideDown)
		{
			myTransform.eulerAngles = new Vector3(myTransform.eulerAngles.x, myTransform.eulerAngles.y, 0f);
		}
		nextWaypoint = null;
		angle = 0f;
	}

	private void OnDisable()
	{
		if ((bool)trafficAI)
		{
			trafficAI.physicalLocomotion = true;
			TSTrafficAI tSTrafficAI = trafficAI;
			tSTrafficAI.OnUpdateAI = (TSTrafficAI.OnUpdateAIDelegate)Delegate.Remove(tSTrafficAI.OnUpdateAI, new TSTrafficAI.OnUpdateAIDelegate(OnAIUpdate));
			TSTrafficAI tSTrafficAI2 = trafficAI;
			tSTrafficAI2.UpdateCarSpeed = (TSTrafficAI.GetCarSpeedDelegate)Delegate.Remove(tSTrafficAI2.UpdateCarSpeed, new TSTrafficAI.GetCarSpeedDelegate(UpdateCarSpeed));
			TSTrafficAI tSTrafficAI3 = trafficAI;
			tSTrafficAI3.OnTurnLeft = (TSTrafficAI.OnTurnLeftDelegate)Delegate.Remove(tSTrafficAI3.OnTurnLeft, new TSTrafficAI.OnTurnLeftDelegate(OnTurnLeft));
			TSTrafficAI tSTrafficAI4 = trafficAI;
			tSTrafficAI4.OnTurnRight = (TSTrafficAI.OnTurnRigthDelegate)Delegate.Remove(tSTrafficAI4.OnTurnRight, new TSTrafficAI.OnTurnRigthDelegate(OnTurnRight));
		}
		nextWaypoint = null;
	}

	private void Start()
	{
		if (turnLeftLight != null)
		{
			turnLeftLight.SetActive(false);
		}
		if (turnRightLight != null)
		{
			turnRightLight.SetActive(false);
		}
		if (brakeLightsShader == null)
		{
			brakeLightsShader = Shader.Find("Car/LightsEmmissive");
		}
	}

	private void Update()
	{
		if (!crashed && !upSideDown)
		{
			angle = 0f;
			CheckPitch();
			speed = GetSpeed();
			if ((double)angle > 1.5 || (double)angle < -1.5)
			{
				myTransform.RotateAround(rearWheel.position, myTransform.right, speed * angle * Time.deltaTime);
			}
			lastPosition = myTransform.position;
			Vector3 vector = lastPosition + myTransform.forward * speed * Time.deltaTime;
			velo = (vector - lastPosition) / Time.deltaTime;
			myTransform.transform.position = vector;
			if (speed > maxSpeedToTurn)
			{
				Quaternion quaternion = Quaternion.Euler(new Vector3(0f, turnSpeed * steering, 0f));
				myTransform.rotation *= quaternion;
			}
		}
		if (Mathf.Abs(base.transform.localRotation.x) >= 20f)
		{
			Debug.LogError("inclinacion excesiva - error");
		}
	}

	private float GetSpeed()
	{
		a = 0f;
		v0 = speed;
		if (throttle > 0f)
		{
			a = commonProperties.engineTorque * throttle;
		}
		if (brake > 0f && speed > 0f)
		{
			a -= commonProperties.brakeTorque * brake;
		}
		a = Mathf.Min(a, commonProperties.maxAcceleration);
		engineForce = a * commonProperties.mass;
		t = Time.deltaTime;
		v = v0 + a * t;
		v = Mathf.Min(v, commonProperties.maxSpeed);
		return v;
	}

	private void CheckPitch()
	{
		nextWaypoint = navigation.waypoints[navigation.currentWaypoint];
		if (nextWaypoint != null)
		{
			fixedForward = nextWaypoint.point + Vector3.up * 0.3f - rearWheel.position;
			fixedForward = Vector3.ProjectOnPlane(fixedForward, rearWheel.right);
			angle = SignedAngleBetween(rearWheel.forward, fixedForward, rearWheel.right);
			angle = Mathf.Clamp(angle, -30f, 30f);
		}
	}

	[ContextMenu("Unreserve")]
	public void UnreserveAll()
	{
		navigation.UnreserveAll(true);
	}

	private float SignedAngleBetween(Vector3 a, Vector3 b, Vector3 n)
	{
		float num = Vector3.Angle(a, b);
		float num2 = Mathf.Sign(Vector3.Dot(n, Vector3.Cross(a, b)));
		return num2 * num;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		if (nextWaypoint != null)
		{
			Gizmos.DrawSphere(nextWaypoint.point, 0.5f);
		}
		Gizmos.color = Color.red;
		Gizmos.DrawRay(rearWheel.position, fixedForward * 4f);
		Gizmos.color = Color.green;
		Gizmos.DrawRay(rearWheel.position, myTransform.forward * 4f);
	}

	private void OnTurnRight(bool isTurning)
	{
		if (turnRightLight != null)
		{
			if (isTurning)
			{
				StartCoroutine(LightBlinking(turnRightLight, 0.2f));
				return;
			}
			blinking = false;
			turnRightLight.SetActive(false);
		}
	}

	private void OnTurnLeft(bool isTurning)
	{
		if (turnLeftLight != null)
		{
			if (isTurning)
			{
				StartCoroutine(LightBlinking(turnLeftLight, 0.2f));
				return;
			}
			blinking = false;
			turnLeftLight.SetActive(false);
		}
	}

	private IEnumerator LightBlinking(GameObject blinkingLight, float blinkingTiming)
	{
		blinking = true;
		while (blinking)
		{
			if (blinkingLight.activeSelf)
			{
				blinkingLight.SetActive(false);
			}
			else
			{
				blinkingLight.SetActive(true);
			}
			yield return new WaitForSeconds(blinkingTiming);
		}
		blinkingLight.SetActive(false);
	}

	public void UpdateCarSpeed(out float carSpeed)
	{
		carSpeed = speed;
	}

	public void OnAIUpdate(float steering, float brake, float throttle, bool isUpSideDown)
	{
		upSideDown = isUpSideDown;
		crashed = upSideDown;
		this.steering = steering;
		steerAngle = 55f * Mathf.Clamp(steering, -1f, 1f);
		this.throttle = throttle;
		this.brake = brake;
		if (crashed)
		{
			steerAngle = 0f;
			throttle = 0f;
			steering = 0f;
			base.gameObject.tag = "Car";
			UnreserveAll();
		}
		RotateVisualWheels(steerAngle);
	}

	private void RotateVisualWheels(float steering)
	{
		float num = speed * 1.6f * Time.deltaTime;
		rotation += num;
		rotationAmount = Vector3.right * num * 57.29578f;
		for (int i = 0; i < frontTires.Length; i++)
		{
			frontTires[i].localEulerAngles = new Vector3(rotation * 57.29578f, steering, 0f);
		}
		for (int j = 0; j < rearTires.Length; j++)
		{
			rearTires[j].Rotate(rotationAmount);
		}
	}
}
