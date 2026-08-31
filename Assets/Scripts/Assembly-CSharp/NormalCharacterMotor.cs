using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class NormalCharacterMotor : CharacterMotor
{
	[Serializable]
	public class step_sound
	{
		public AudioClip step1;

		public AudioClip step2;

		public string tag;
	}

	public LayerMask layerMask;

	public float maxRotationSpeed = 270f;

	public Transform hiter;

	[HideInInspector]
	public CharacterController controller;

	private bool firstframe = true;

	private float speed = 1f;

	private Vector3 movement;

	private Animator MyAnimator;

	private AudioSource stepAudio;

	public Camera myCamera;

	public float health = 100f;

	public RawImage HealthGUI;

	[HideInInspector]
	public AudioClip step1c;

	[HideInInspector]
	public AudioClip step2c;

	public step_sound[] stepSounds;

	private float step_speed;

	private float step;

	private void Start()
	{
		MyAnimator = base.transform.GetComponent<Animator>();
		controller = GetComponent<CharacterController>();
		stepAudio = GetComponent<AudioSource>();
	}

	private void Update()
	{
		RectTransform healthRectTransform = HealthGUI.rectTransform;
		healthRectTransform.anchorMin = Vector2.zero;
		healthRectTransform.anchorMax = Vector2.one;
		healthRectTransform.offsetMin = Vector2.zero;
		healthRectTransform.offsetMax = Vector2.zero;
		UpdateFacingDirection();
		UpdateVelocity();
		step_speed = Mathf.Clamp(Mathf.Abs(movement.z) + Mathf.Abs(movement.x), 0f, 4f);
		stepAudio.volume = Mathf.Clamp01(step_speed / 2f);
	}

	private void Health()
	{
		health = Mathf.Clamp(health, 0f, 100f);
		health = Mathf.MoveTowards(health, 100f, Time.deltaTime * 10f);
		if (health == 0f)
		{
			HealthGUI.enabled = false;
		}
		else
		{
			HealthGUI.enabled = true;
		}
		HealthGUI.color = new Color(255f, 0f, 0f, Mathf.Abs(health - 100f) / 100f);
	}

	private void OnFootLift()
	{
		if (!stepAudio.isPlaying)
		{
			step++;
			step = Mathf.Repeat(step, 2f);
			if (step == 0f)
			{
				stepAudio.clip = step1c;
				stepAudio.Play();
			}
			if (step == 1f)
			{
				stepAudio.clip = step2c;
				stepAudio.Play();
			}
		}
	}

	private void UpdateFacingDirection()
	{
		float magnitude = base.desiredFacingDirection.magnitude;
		Vector3 v = base.transform.rotation * base.desiredMovementDirection * (1f - magnitude) + base.desiredFacingDirection * magnitude;
		v = Util.ProjectOntoPlane(v, base.transform.up);
		v = alignCorrection * v;
		if (v.sqrMagnitude > 0.01f)
		{
			Vector3 v2 = Util.ConstantSlerp(base.transform.forward, v, maxRotationSpeed * Time.deltaTime);
			v2 = Util.ProjectOntoPlane(v2, base.transform.up);
			Quaternion rotation = default(Quaternion);
			rotation.SetLookRotation(v2, base.transform.up);
			base.transform.rotation = rotation;
		}
	}

	private void UpdateVelocity()
	{
		Health();
		Vector3 vector = controller.velocity;
		if (firstframe)
		{
			vector = Vector3.zero;
			firstframe = false;
		}
		if (base.grounded)
		{
			vector = Util.ProjectOntoPlane(vector, base.transform.up);
		}
		base.jumping = false;
		if (base.grounded)
		{
			movement = vector;
			Vector3 vector2 = base.desiredVelocity - vector;
			if (vector2.magnitude > maxVelocityChange)
			{
				vector2 = vector2.normalized * maxVelocityChange;
			}
			movement += vector2;
			if (canJump && Input.GetButton("Jump"))
			{
				MyAnimator.SetBool("Jump", true);
				movement += base.transform.up * Mathf.Sqrt(2f * jumpHeight * gravity);
				base.jumping = true;
			}
			else
			{
				MyAnimator.SetBool("Jump", false);
				Animator myAnimator = MyAnimator;
				Vector3 vector3 = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
				myAnimator.SetFloat("PlayerSpeed", vector3.magnitude / 4f);
			}
		}
		movement += base.transform.up * (0f - gravity) * Time.deltaTime;
		if (base.jumping)
		{
			movement -= base.transform.up * (0f - gravity) * Time.deltaTime / 2f;
		}
		CollisionFlags collisionFlags = controller.Move(movement * Time.deltaTime);
		base.grounded = (collisionFlags & CollisionFlags.Below) != 0;
		Collider[] array = Physics.OverlapSphere(base.transform.position, 0.5f, layerMask);
		speed = Mathf.MoveTowards(speed, 1f, 0.02f);
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			RaycastHit hitInfo;
			if ((!(collider.transform.tag == "carcollider") && !(collider.transform.tag == "Player")) || !collider.transform.root.GetComponent<CarController>() || !Physics.Linecast(base.transform.position, collider.transform.position, out hitInfo, 1))
			{
				continue;
			}
			if (Vector3.Distance(base.transform.position, collider.transform.root.GetComponent<CarController>().hitFront.position) < 2f)
			{
				if (health == 100f && collider.transform.root.GetComponent<Rigidbody>().linearVelocity.magnitude > 1f)
				{
					hiter.GetComponent<AudioSource>().Play();
					health -= hitInfo.rigidbody.angularVelocity.magnitude * 300f;
				}
				base.transform.position = Vector3.Lerp(base.transform.position, base.transform.position + hitInfo.normal, collider.transform.root.GetComponent<Rigidbody>().linearVelocity.magnitude / 50f);
			}
			if (collider.transform.root.GetComponent<Rigidbody>().linearVelocity.magnitude > 0.05f)
			{
				base.transform.position = base.transform.position + hitInfo.normal / 50f;
				if (Vector3.Distance(base.transform.position, hitInfo.point) < 1f)
				{
					speed = 0f;
				}
			}
			Debug.DrawLine(base.transform.position, hitInfo.point, Color.red);
		}
	}
}
