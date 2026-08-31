using System;
using System.Collections;
using UnityEngine;

public class TSSimpleCar : MonoBehaviour
{
	public WheelCollider[] FrontWheels = new WheelCollider[0];

	public WheelCollider[] BackWheels = new WheelCollider[0];

	public WheelCollider[] aditionalBrakeWheels = new WheelCollider[0];

	public bool playerControlled;

	public bool engineAudioEnabled = true;

	public float[] GearRatio = new float[5] { 3.5f, 2.5f, 2f, 1.5f, 1f };

	public int CurrentGear;

	public float EngineTorque = 600f;

	public float MaxEngineRPM = 3000f;

	public float MinEngineRPM = 1000f;

	public float maxAcceleration = 5f;

	public float brakeTorque = 400f;

	public Renderer[] brakeLigths;

	public float brakeIntensityRate = 0.2f;

	public bool enableDisableBrakeLights;

	public Shader brakeLightsShader;

	public string propertyName = "_Intensity";

	public bool superSimplePhysics;

	public float turnSpeed = 3f;

	public float carGrip = 70f;

	public float maxSpeed = 50f;

	public float maxSpeedToTurn = 0.2f;

	public ParticleSystem[] crashedSmokes;

	public GameObject turnRightLight;

	public GameObject turnLeftLight;

	public Transform CoM;

	private float EngineRPM;

	public float brake;

	private float RPM;

	public Rigidbody body;

	public Rigidbody[] bodies;

	private float steering;

	private Transform myTransform;

	private TSTrafficAI trafficAI;

	private float lastSpeed;

	public float speed;

	private float myAccel;

	private bool blinking;

	private Transform[] frontWheels;

	private Transform[] rearWheels;

	private float mySpeed;

	public Vector3 velo = Vector3.zero;

	private Vector3 tempVec;

	private Vector3 flatVelo;

	private Vector3 flatDir;

	private float slideSpeed;

	private Vector3 engineForce = Vector3.zero;

	private float actualGrip;

	private Vector3 impulse = Vector3.zero;

	private Vector3 dir;

	private AudioSource audioSource;

	private TSCommonProperties commonProperties;

	private Vector3 rotationAmount;

	private float rotation;

	public bool magic;

	public float throttle;

	public Vector3 localspeed;

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
		body = GetComponent<Rigidbody>();
		bodies = GetComponentsInChildren<Rigidbody>();
		trafficAI = GetComponent<TSTrafficAI>();
		audioSource = GetComponent<AudioSource>();
		if (audioSource == null)
		{
			engineAudioEnabled = false;
		}
		commonProperties = GetComponent<TSCommonProperties>();
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
		if (superSimplePhysics)
		{
			frontWheels = new Transform[FrontWheels.Length];
			rearWheels = new Transform[BackWheels.Length];
			for (int i = 0; i < FrontWheels.Length; i++)
			{
				frontWheels[i] = FrontWheels[i].transform;
				FrontWheels[i].enabled = false;
			}
			for (int j = 0; j < BackWheels.Length; j++)
			{
				rearWheels[j] = BackWheels[j].transform;
				BackWheels[j].enabled = false;
			}
			for (int k = 0; k < bodies.Length; k++)
			{
				if (CoM != null && k == 0)
				{
					bodies[k].centerOfMass = new Vector3(CoM.localPosition.x, -2f, CoM.localPosition.z);
				}
				else
				{
					bodies[k].centerOfMass = new Vector3(bodies[k].centerOfMass.x, -2f, bodies[k].centerOfMass.z);
				}
			}
		}
		else
		{
			if (!(CoM != null))
			{
				return;
			}
			for (int l = 0; l < bodies.Length; l++)
			{
				if (l == 0)
				{
					bodies[l].centerOfMass = new Vector3(CoM.localPosition.x * base.transform.localScale.x, CoM.localPosition.y * base.transform.localScale.y, CoM.localPosition.z * base.transform.localScale.z);
				}
			}
			body.centerOfMass = new Vector3(CoM.localPosition.x * base.transform.localScale.x, CoM.localPosition.y * base.transform.localScale.y, CoM.localPosition.z * base.transform.localScale.z);
		}
	}

	private void OnEnable()
	{
		if (trafficAI != null)
		{
			TSTrafficAI tSTrafficAI = trafficAI;
			tSTrafficAI.OnUpdateAI = (TSTrafficAI.OnUpdateAIDelegate)Delegate.Combine(tSTrafficAI.OnUpdateAI, new TSTrafficAI.OnUpdateAIDelegate(OnAIUpdate));
			TSTrafficAI tSTrafficAI2 = trafficAI;
			tSTrafficAI2.UpdateCarSpeed = (TSTrafficAI.GetCarSpeedDelegate)Delegate.Combine(tSTrafficAI2.UpdateCarSpeed, new TSTrafficAI.GetCarSpeedDelegate(UpdateCarSpeed));
			TSTrafficAI tSTrafficAI3 = trafficAI;
			tSTrafficAI3.OnTurnLeft = (TSTrafficAI.OnTurnLeftDelegate)Delegate.Combine(tSTrafficAI3.OnTurnLeft, new TSTrafficAI.OnTurnLeftDelegate(OnTurnLeft));
			TSTrafficAI tSTrafficAI4 = trafficAI;
			tSTrafficAI4.OnTurnRight = (TSTrafficAI.OnTurnRigthDelegate)Delegate.Combine(tSTrafficAI4.OnTurnRight, new TSTrafficAI.OnTurnRigthDelegate(OnTurnRight));
		}
		if (!body)
		{
			if (!GetComponent<Rigidbody>())
			{
				GetComponent<TSCarLocomotionSwitcher>().SwitchToNoPhysics();
				return;
			}
			body = GetComponent<Rigidbody>();
		}
		if (!(CoM != null))
		{
			return;
		}
		for (int i = 0; i < bodies.Length; i++)
		{
			if (i == 0)
			{
				bodies[i].centerOfMass = new Vector3(CoM.localPosition.x * base.transform.localScale.x, CoM.localPosition.y * base.transform.localScale.y, CoM.localPosition.z * base.transform.localScale.z);
			}
		}
		body.centerOfMass = new Vector3(CoM.localPosition.x * base.transform.localScale.x, CoM.localPosition.y * base.transform.localScale.y, CoM.localPosition.z * base.transform.localScale.z);
	}

	private void OnDisable()
	{
		if ((bool)trafficAI)
		{
			TSTrafficAI tSTrafficAI = trafficAI;
			tSTrafficAI.OnUpdateAI = (TSTrafficAI.OnUpdateAIDelegate)Delegate.Remove(tSTrafficAI.OnUpdateAI, new TSTrafficAI.OnUpdateAIDelegate(OnAIUpdate));
			TSTrafficAI tSTrafficAI2 = trafficAI;
			tSTrafficAI2.UpdateCarSpeed = (TSTrafficAI.GetCarSpeedDelegate)Delegate.Remove(tSTrafficAI2.UpdateCarSpeed, new TSTrafficAI.GetCarSpeedDelegate(UpdateCarSpeed));
			TSTrafficAI tSTrafficAI3 = trafficAI;
			tSTrafficAI3.OnTurnLeft = (TSTrafficAI.OnTurnLeftDelegate)Delegate.Remove(tSTrafficAI3.OnTurnLeft, new TSTrafficAI.OnTurnLeftDelegate(OnTurnLeft));
			TSTrafficAI tSTrafficAI4 = trafficAI;
			tSTrafficAI4.OnTurnRight = (TSTrafficAI.OnTurnRigthDelegate)Delegate.Remove(tSTrafficAI4.OnTurnRight, new TSTrafficAI.OnTurnRigthDelegate(OnTurnRight));
		}
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

	private void Update()
	{
		if (!crashed)
		{
			if (superSimplePhysics && !upSideDown)
			{
				for (int i = 0; i < bodies.Length; i++)
				{
					float num = Mathf.Clamp((bodies[i].linearVelocity.magnitude - 10f) / 35f, -1f, 2f);
					bodies[i].centerOfMass = new Vector3(bodies[i].centerOfMass.x, 0f - num, bodies[i].centerOfMass.z);
					body.centerOfMass = new Vector3(bodies[i].centerOfMass.x, 0f - num, bodies[i].centerOfMass.z);
				}
				if (engineAudioEnabled)
				{
					audioSource.pitch = Mathf.Abs(mySpeed / maxSpeed) + 0.75f;
					if (audioSource.pitch > 2f)
					{
						audioSource.pitch = 2f;
					}
				}
				BrakeLight(brake);
			}
			else if (!superSimplePhysics)
			{
				if (playerControlled)
				{
					steering = Input.GetAxis("Horizontal");
					float num2 = EngineTorque * Input.GetAxis("Vertical");
					float num3 = Mathf.Max(1f - mySpeed / maxSpeed, 0.05f);
					float steerAngle = 55f * Mathf.Clamp(steering, -1f * num3, 1f * num3);
					float num4 = 0f;
					for (int j = 0; j < FrontWheels.Length; j++)
					{
						FrontWheels[j].steerAngle = steerAngle;
						FrontWheels[j].brakeTorque = num4;
					}
					for (int k = 0; k < BackWheels.Length; k++)
					{
						if (mySpeed < maxSpeed)
						{
							BackWheels[k].motorTorque = num2 / ((float)BackWheels.Length / 2f);
						}
						else
						{
							BackWheels[k].motorTorque = (0f - EngineTorque) / 5f;
						}
						BackWheels[k].brakeTorque = num4;
					}
					for (int l = 0; l < aditionalBrakeWheels.Length; l++)
					{
						aditionalBrakeWheels[l].brakeTorque = num4;
					}
					if (num4 != 0f)
					{
						brake++;
					}
					else
					{
						brake -= 0.05f;
					}
					brake = Mathf.Clamp01(brake);
				}
				for (int m = 0; m < BackWheels.Length; m++)
				{
					RPM += BackWheels[m].rpm;
				}
				RPM /= BackWheels.Length;
				EngineRPM = RPM * GearRatio[CurrentGear];
				ShiftGears();
				if (engineAudioEnabled)
				{
					audioSource.pitch = Mathf.Abs(EngineRPM / MaxEngineRPM) + 0.75f;
					if (audioSource.pitch > 2f)
					{
						audioSource.pitch = 2f;
					}
				}
				if (FrontWheels != null && FrontWheels.Length > 0)
				{
					BrakeLight(FrontWheels[0].brakeTorque);
				}
			}
		}
		if (Mathf.Abs(base.transform.localRotation.x) >= 20f)
		{
			Debug.LogError("inclinacion excesiva - error");
		}
	}

	public void BrakeLight(float brakeInput)
	{
		if (!enableDisableBrakeLights || brakeLigths == null)
		{
			return;
		}
		for (int i = 0; i < brakeLigths.Length; i++)
		{
			if (brake != 0f && !brakeLigths[i].enabled)
			{
				brakeLigths[i].enabled = true;
			}
			else if (brake == 0f && brakeLigths[i].enabled)
			{
				brakeLigths[i].enabled = false;
			}
		}
	}

	private void RotateVisualWheels(float steering)
	{
		float num = mySpeed * 1.6f * Time.deltaTime;
		rotation += num;
		rotationAmount = Vector3.right * num * 57.29578f;
		for (int i = 0; i < frontWheels.Length; i++)
		{
			frontWheels[i].localEulerAngles = new Vector3(rotation * 57.29578f, steering, 0f);
		}
		for (int j = 0; j < rearWheels.Length; j++)
		{
			rearWheels[j].Rotate(rotationAmount);
		}
	}

	private void SuperSimplePhysicsUpdate(float throttle, float steering)
	{
		velo = body.linearVelocity;
		tempVec = new Vector3(velo.x, 0f, velo.z);
		flatVelo = tempVec;
		dir = myTransform.TransformDirection(Vector3.forward);
		tempVec = new Vector3(dir.x, 0f, dir.z);
		flatDir = Vector3.Normalize(tempVec);
		slideSpeed = Vector3.Dot(myTransform.right, flatVelo);
		mySpeed = flatVelo.magnitude;
		engineForce = flatDir * (EngineTorque * 10f * throttle) * body.mass;
		actualGrip = Mathf.Lerp(100f, carGrip, mySpeed * 0.02f);
		impulse = myTransform.right * ((0f - slideSpeed) * body.mass * Mathf.Abs(Physics.gravity.y) * actualGrip);
	}

	private void FixedUpdate()
	{
		if (!crashed && superSimplePhysics && !upSideDown)
		{
			float num = 0.02f / Time.fixedDeltaTime;
			if (mySpeed < maxSpeed)
			{
				if (frontWheels != null && frontWheels.Length > 0)
				{
					body.AddForceAtPosition(engineForce * Time.deltaTime * num, new Vector3(myTransform.position.x, FrontWheels[0].transform.position.y, myTransform.position.z));
				}
				else
				{
					body.AddForce(engineForce * Time.deltaTime * num);
				}
			}
			if (mySpeed > maxSpeedToTurn)
			{
				Quaternion quaternion = Quaternion.Euler(new Vector3(0f, turnSpeed * steering, 0f));
				body.MoveRotation(body.rotation * quaternion);
			}
			if (brake > 0f && body.linearVelocity.sqrMagnitude > 0f)
			{
				if (frontWheels != null && frontWheels.Length > 0)
				{
					body.AddForceAtPosition(-body.linearVelocity.normalized * brakeTorque * brake * Time.deltaTime * num, new Vector3(myTransform.position.x, FrontWheels[0].transform.position.y, myTransform.position.z));
				}
				else
				{
					body.AddForce(Mathf.Sign(localspeed.z) * -flatDir * brakeTorque * 10f * body.mass * brake * Time.deltaTime * num);
				}
			}
			body.AddForce(impulse * Time.deltaTime * num);
		}
		else
		{
			mySpeed = body.linearVelocity.magnitude;
		}
		myAccel = (speed - lastSpeed) / Time.deltaTime;
		if (myAccel > maxAcceleration)
		{
			body.linearDamping += 0.1f;
		}
		else if (body.linearDamping > 0.1f)
		{
			body.linearDamping -= 0.1f;
		}
		if (body.linearDamping < speed / 125f)
		{
			body.linearDamping = speed / 125f;
		}
		lastSpeed = speed;
	}

	public void OnAIUpdate(float steering, float brake, float throttle, bool isUpSideDown)
	{
		if (isUpSideDown)
		{
			commonProperties.crashed = true;
		}
		upSideDown = isUpSideDown;
		this.steering = steering;
		this.throttle = throttle;
		float steerAngle = 55f * Mathf.Clamp(steering, -1f, 1f);
		this.brake = brake;
		float num = brakeTorque * brake;
		float num2 = EngineTorque * throttle;
		if (num2 < 0f)
		{
			num2 = 0f;
		}
		if (crashed)
		{
			steerAngle = 0f;
			num = brakeTorque * 1f;
			num2 = 0f;
			throttle = 0f;
			steering = 0f;
			ActivatecrashedSmoke(true);
		}
		else
		{
			ActivatecrashedSmoke(false);
		}
		if (superSimplePhysics)
		{
			RotateVisualWheels(steerAngle);
			SuperSimplePhysicsUpdate(throttle, steering);
		}
		else
		{
			for (int i = 0; i < FrontWheels.Length; i++)
			{
				FrontWheels[i].steerAngle = steerAngle;
				FrontWheels[i].brakeTorque = num;
			}
			for (int j = 0; j < BackWheels.Length; j++)
			{
				if (mySpeed < maxSpeed)
				{
					BackWheels[j].motorTorque = num2 / ((float)BackWheels.Length / 2f);
				}
				BackWheels[j].brakeTorque = num;
			}
			for (int k = 0; k < aditionalBrakeWheels.Length; k++)
			{
				aditionalBrakeWheels[k].brakeTorque = num;
			}
		}
		if (num != 0f)
		{
			this.brake++;
		}
		else
		{
			this.brake -= 0.05f;
		}
		this.brake = Mathf.Clamp01(this.brake);
	}

	public void UpdateCarSpeed(out float carSpeed)
	{
		localspeed = myTransform.InverseTransformDirection(body.linearVelocity);
		speed = (carSpeed = localspeed.magnitude);
	}

	private void ShiftGears()
	{
		int currentGear = CurrentGear;
		if (EngineRPM >= MaxEngineRPM)
		{
			for (int i = 0; i < GearRatio.Length; i++)
			{
				if (RPM * GearRatio[i] < MaxEngineRPM)
				{
					currentGear = i;
					break;
				}
			}
			CurrentGear = currentGear;
		}
		if (!(EngineRPM <= MinEngineRPM))
		{
			return;
		}
		currentGear = CurrentGear;
		for (int num = GearRatio.Length - 1; num >= 0; num--)
		{
			if (RPM * GearRatio[num] > MinEngineRPM)
			{
				currentGear = num;
				break;
			}
		}
		CurrentGear = currentGear;
	}

	private int[] getBrakeLights()
	{
		int num = 0;
		int[] array = new int[brakeLigths.Length];
		num = 0;
		if (brakeLigths.Length != 0)
		{
			Renderer[] array2 = brakeLigths;
			foreach (Renderer renderer in array2)
			{
				int num2 = 0;
				if (renderer != null)
				{
					Material[] sharedMaterials = renderer.sharedMaterials;
					foreach (Material material in sharedMaterials)
					{
						if (material != null && material.shader == brakeLightsShader)
						{
							array[num] = num2;
						}
						num2++;
					}
				}
				num++;
			}
		}
		return array;
	}

	public void ActivatecrashedSmoke(bool active)
	{
		for (int i = 0; i < crashedSmokes.Length; i++)
		{
			crashedSmokes[i].enableEmission = active;
			crashedSmokes[i].Play(active);
		}
	}
}
