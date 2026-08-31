using System.Collections.Generic;
using UnityEngine;

public class Crashable : MonoBehaviour
{
	public static int activeCrashablesLimit = 15;

	public static Queue<Crashable> activeCrashables = new Queue<Crashable>();

	public AudioClip crashSound;

	public Mesh mainMesh;

	private Collider trigger;

	private Collider rigidbodyCollider;

	private Rigidbody rBdy;

	private bool crashableReady;

	private bool crashed;

	private float resetDistance = 40f;

	public float mass = 5f;

	public bool respawnable = true;

	private Vector3 resetPosition;

	private Quaternion resetRotation;

	public List<EventDelegate> onCrash;

	public List<EventDelegate> onReset;

	public bool isPlane;

	private float objDistance;

	private void Awake()
	{
		if (mainMesh != null)
		{
			mainMesh = GetComponent<MeshFilter>().sharedMesh;
		}
		if (!isPlane)
		{
			trigger = base.gameObject.GetComponent<Collider>();
		}
		else
		{
			trigger = base.gameObject.AddMissingComponent<BoxCollider>();
		}
		EventDelegate.Add(onCrash, new EventDelegate(BasicBehaviourOnCrashed));
	}

	private void Start()
	{
		trigger.enabled = true;
		trigger.isTrigger = true;
		base.gameObject.layer = LayerMask.NameToLayer("Crashable Off");
		SetAndGetResetPositionAndRotation(false);
	}

	private void OnTriggerEnter(Collider carCollider)
	{
		LoadCrashable();
	}

	private void OnCollisionEnter(Collision col)
	{
		if (!crashed)
		{
			crashed = true;
			Crash();
			CheckCrashablesPopulation();
		}
		CheckResetDistance();
	}

	private void CheckResetDistance()
	{
		objDistance = Vector3.Distance(base.transform.position, resetPosition);
		if (objDistance >= resetDistance)
		{
			ResetCrashable();
		}
	}

	private void CheckCrashablesPopulation()
	{
		if (activeCrashables.Count <= activeCrashablesLimit)
		{
			return;
		}
		int num = activeCrashables.Count - activeCrashablesLimit;
		for (int i = 0; i < num; i++)
		{
			Crashable crashable = activeCrashables.Dequeue();
			if (crashable == null)
			{
				i--;
			}
			else
			{
				crashable.ResetCrashable();
			}
		}
	}

	private void Crash()
	{
		activeCrashables.Enqueue(this);
		EventDelegate.Execute(onCrash);
	}

	private void LoadCrashable()
	{
		if (!crashableReady)
		{
			base.gameObject.layer = LayerMask.NameToLayer("CrashableActive");
			trigger.enabled = false;
			CreateRigidCollider();
			CreateRigidBody();
			crashableReady = true;
		}
	}

	private void UnloadCrashable()
	{
		if (crashableReady)
		{
			Object.Destroy(base.gameObject.GetComponent<Rigidbody>());
			trigger.enabled = true;
			rigidbodyCollider.enabled = false;
			crashableReady = false;
		}
	}

	[ContextMenu("Reset Crashable")]
	private void ResetCrashable()
	{
		if (respawnable)
		{
			UnloadCrashable();
			base.gameObject.layer = LayerMask.NameToLayer("Crashable Off");
			SetAndGetResetPositionAndRotation(true);
			crashed = false;
		}
		else
		{
			base.gameObject.SetActive(false);
		}
		EventDelegate.Execute(onReset);
	}

	private void SetAndGetResetPositionAndRotation(bool resetPosition)
	{
		if (!resetPosition)
		{
			this.resetPosition = base.transform.position;
			resetRotation = base.transform.rotation;
		}
		else
		{
			base.transform.position = this.resetPosition;
			base.transform.rotation = resetRotation;
		}
	}

	private void CreateRigidCollider()
	{
		if (rigidbodyCollider == null)
		{
			if (!isPlane)
			{
				rigidbodyCollider = base.gameObject.AddComponent<MeshCollider>();
				MeshCollider meshCollider = (MeshCollider)rigidbodyCollider;
				meshCollider.sharedMesh = mainMesh;
				meshCollider.convex = true;
			}
			else
			{
				rigidbodyCollider = base.gameObject.AddComponent<BoxCollider>();
				((BoxCollider)rigidbodyCollider).size += 0.05f * Vector3.one;
			}
		}
		rigidbodyCollider.enabled = true;
	}

	private void CreateRigidBody()
	{
		rBdy = base.gameObject.AddMissingComponent<Rigidbody>();
		rBdy.mass = mass;
	}

	private void BasicBehaviourOnCrashed()
	{
		if ((bool)crashSound)
		{
			AudioSource.PlayClipAtPoint(crashSound, base.transform.position);
		}
	}
}
