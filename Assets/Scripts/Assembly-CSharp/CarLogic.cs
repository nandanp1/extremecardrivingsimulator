using UnityEngine;

public class CarLogic : MonoBehaviour
{
	public enum Stunt
	{
		None = 0,
		Drifting = 1,
		Jumping = 2,
		Burning = 3
	}

	public const float MAX_VEL_FOR_REPOSITION = 3f;

	public const float MAX_ANG_VEL_FOR_REPOSITION = 3f;

	private const string NAME_GAME_OBJECT_NITRO_PARTICLES = "NitroParticlesParent";

	private const string NAME_GAME_OBJECT_BRAKE_LIGHT_PARENT = "BrakelightsParent";

	private const string NAME_GAME_OBJECT_BRAKE_LIGHT_EMISSIVE_MAT = "BrakelightsMat";

	private const string EMISSIVE_PROPERTY_NAME_SHADER = "_EmissiveAmount";

	private const float MIN_VELOCITY_REVERSE_FOR_BRAKE_LIGHT = 2f;

	private const float TIME_UPDATE_JUMP_AND_DRIFT = 0.15f;

	private const float VEL_MAX_STOPED_CAR = 0.5f;

	private const float MAX_ANGLE_STRAIGHT = 0.99f;

	private const float MAX_VEL_BURNOUT = 8f;

	private const float MAX_ANGLE_DRIFTING = 0.978f;

	private int emissivePropertyId = Shader.PropertyToID("_EmissiveAmount");

	public GameLogic gameLogic;

	private WheelCollider wheelFL;

	private WheelCollider wheelFR;

	private WheelCollider wheelRL;

	private WheelCollider wheelRR;

	[HideInInspector]
	public CarControl carControl;

	private Transform myTransform;

	private bool isJumping;

	public int airState;

	private float metersDrive;

	private Vector3? lastPositionDrive;

	private bool isDriving;

	private int lastWheelsSlidingValue;

	public int wheelsSlidingValue;

	private Rigidbody myRigidbody;

	private Vector2 forwardCar = Vector3.zero;

	private Vector2 forwardVelCar = Vector3.zero;

	private Transform[] brakesLights;

	public Material materialEmissive;

	public bool isAiring
	{
		get
		{
			return airState < 1;
		}
	}

	public bool isSliding
	{
		get
		{
			return wheelsSlidingValue > 0;
		}
	}

	public float speedInMetersPerSec
	{
		get
		{
			return Vector3.Dot(GetComponent<Rigidbody>().linearVelocity, base.transform.forward);
		}
	}

	private void Awake()
	{
		carControl = GetComponent<CarControl>();
		if ((bool)carControl.WheelFL)
		{
			wheelFL = carControl.WheelFL.GetComponent<WheelCollider>();
		}
		if ((bool)carControl.WheelFR)
		{
			wheelFR = carControl.WheelFR.GetComponent<WheelCollider>();
		}
		if ((bool)carControl.WheelRL)
		{
			wheelRL = carControl.WheelRL.GetComponent<WheelCollider>();
		}
		if ((bool)carControl.WheelRR)
		{
			wheelRR = carControl.WheelRR.GetComponent<WheelCollider>();
		}
		myTransform = base.transform;
	}

	public void LockXZMovements(bool state)
	{
		if (state)
		{
			GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
			GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			GetComponent<Rigidbody>().constraints = (RigidbodyConstraints)42;
		}
		else
		{
			GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
		}
	}

	private void OnEnable()
	{
		gameLogic = Object.FindFirstObjectByType<GameLogic>();
		InvokeRepeating("UpdateJumpAndDrift", 0f, 0.15f);
	}

	private void OnDisable()
	{
		CancelInvoke("UpdateJumpAndDrift");
	}

	private void UpdateJumpAndDrift()
	{
		if (!isAiring || wheelFL.isGrounded || wheelFR.isGrounded || wheelRL.isGrounded || wheelRR.isGrounded)
		{
			if (!isDriving)
			{
				isDriving = true;
			}
			Vector3? vector = lastPositionDrive;
			if (!vector.HasValue)
			{
				lastPositionDrive = myTransform.position;
			}
			else
			{
				Vector3? vector2 = lastPositionDrive;
				metersDrive = Vector3.Distance(vector2.Value, myTransform.position);
				lastPositionDrive = myTransform.position;
			}
			gameLogic.Driving(metersDrive);
		}
		else if (isDriving)
		{
			isDriving = false;
			lastPositionDrive = null;
		}
	}
}
