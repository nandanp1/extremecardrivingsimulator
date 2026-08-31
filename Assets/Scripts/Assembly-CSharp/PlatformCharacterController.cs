using UnityEngine;

public class PlatformCharacterController : MonoBehaviour
{
	private CharacterMotor motor;

	public float walkMultiplier = 0.5f;

	public bool defaultIsWalk;

	private void Start()
	{
		motor = GetComponent(typeof(CharacterMotor)) as CharacterMotor;
		if (motor == null)
		{
			Debug.Log("Motor is null!!");
		}
	}

	private void Update()
	{
		Vector3 vector = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f);
		if (vector.magnitude > 1f)
		{
			vector = vector.normalized;
		}
		vector = vector.normalized * Mathf.Pow(vector.magnitude, 2f);
		vector = Camera.main.transform.rotation * vector;
		Quaternion quaternion = Quaternion.FromToRotation(Camera.main.transform.forward * -1f, base.transform.up);
		vector = quaternion * vector;
		vector = Quaternion.Inverse(base.transform.rotation) * vector;
		if (walkMultiplier != 1f && (Input.GetKey("left shift") || Input.GetKey("right shift")) != defaultIsWalk)
		{
			vector *= walkMultiplier;
		}
		motor.desiredMovementDirection = vector;
	}
}
