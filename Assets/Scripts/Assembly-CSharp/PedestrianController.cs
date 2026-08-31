using UnityEngine;

public class PedestrianController : MonoBehaviour
{
	protected Animator animator;

	private float speed;

	public float direction;

	private Locomotion locomotion;

	private Rigidbody body;

	private void Start()
	{
		body = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
		locomotion = new Locomotion(animator);
		TSTrafficAI component = GetComponent<TSTrafficAI>();
		component.OnUpdateAI = OnAIUpdate;
		component.UpdateCarSpeed = UpdateSpeed;
	}

	private void UpdateSpeed(out float carSpeed)
	{
		carSpeed = body.linearVelocity.z;
	}

	private void OnAIUpdate(float steering, float brake, float throttle, bool isUpSideDown)
	{
		speed = Mathf.Clamp01(throttle - brake);
		direction = steering;
	}

	private void Update()
	{
		if ((bool)animator)
		{
			if (body.constraints != RigidbodyConstraints.FreezeRotation)
			{
				body.constraints = RigidbodyConstraints.FreezeRotation;
			}
			locomotion.Do(speed, 45f * direction);
		}
	}
}
