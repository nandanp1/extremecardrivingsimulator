using UnityEngine;

public class TSSimpleCar_Wheel : MonoBehaviour
{
	public WheelCollider CorrespondingCollider;

	public GameObject SlipPrefab;

	private float RotationValue;

	private Vector3 pos1 = Vector3.zero;

	private Vector3 up = Vector3.zero;

	private float suspensionTravel;

	private float radius;

	private Transform wheelTransform;

	private float _compression;

	private TSSimpleCar simpleCarScript;

	private Transform myTransform;

	private float _antiRollBarForce;

	private Rigidbody myBody;

	private string wheelPosition;

	private Transform myParentTransform;

	public string WheelPosition
	{
		get
		{
			return wheelPosition;
		}
		set
		{
			wheelPosition = value;
		}
	}

	public float AntiRollBarForce
	{
		get
		{
			return _antiRollBarForce;
		}
		set
		{
			_antiRollBarForce = value;
		}
	}

	public float compression
	{
		get
		{
			return _compression;
		}
	}

	private void Awake()
	{
		myBody = base.transform.parent.parent.GetComponent<Rigidbody>();
		myTransform = base.transform;
		myParentTransform = myTransform.parent;
		simpleCarScript = base.transform.root.GetComponent<TSSimpleCar>();
		wheelTransform = CorrespondingCollider.transform;
		suspensionTravel = CorrespondingCollider.suspensionDistance;
		radius = CorrespondingCollider.radius;
		if (simpleCarScript.superSimplePhysics)
		{
			base.enabled = false;
			return;
		}
		Renderer componentInChildren = GetComponentInChildren<Renderer>();
		Collider component = componentInChildren.gameObject.GetComponent<Collider>();
		if (component != null)
		{
			component.enabled = false;
		}
	}

	private void OnEnable()
	{
		myBody = base.transform.parent.parent.GetComponent<Rigidbody>();
	}

	private void Update()
	{
		if (simpleCarScript.superSimplePhysics)
		{
			CorrespondingCollider.enabled = false;
			base.enabled = false;
			return;
		}
		pos1 = wheelTransform.position;
		up = wheelTransform.up;
		WheelHit hit;
		if (CorrespondingCollider.GetGroundHit(out hit))
		{
			_compression = 1f - (Vector3.Dot(pos1 - hit.point, up) - radius) / suspensionTravel;
		}
		else
		{
			_compression = suspensionTravel;
		}
		myTransform.localPosition = Vector3.up * (_compression - 1f) * suspensionTravel;
		myTransform.rotation = CorrespondingCollider.transform.rotation * Quaternion.Euler(RotationValue, CorrespondingCollider.steerAngle, 0f);
		RotationValue += CorrespondingCollider.rpm * 6f * Time.deltaTime;
		if (Mathf.Abs(hit.sidewaysSlip) > 2f && (bool)SlipPrefab)
		{
			Object.Instantiate(SlipPrefab, hit.point, Quaternion.identity);
		}
	}

	private void FixedUpdate()
	{
		if (simpleCarScript.superSimplePhysics)
		{
			CorrespondingCollider.enabled = false;
			base.enabled = false;
		}
		else
		{
			myBody.AddForceAtPosition(_antiRollBarForce * myParentTransform.up, myParentTransform.position);
		}
	}
}
