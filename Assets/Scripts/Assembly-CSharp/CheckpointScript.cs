using UnityEngine;

public class CheckpointScript : MonoBehaviour
{
	public bool useMasterDefaults = true;

	public Vector3 size = Vector3.one;

	public Vector3 offset = new Vector3(0f, 0.5f, 0f);

	public bool enableOnDebugBuild = true;

	public bool isGoal;

	[HideInInspector]
	public bool done;

	[HideInInspector]
	public CheckpointMaster checkpointMaster;

	[HideInInspector]
	public bool isCurrent;

	[HideInInspector]
	public Vector3 halfSize = Vector3.one;

	private Vector3 _center;

	private void Awake()
	{
		checkpointMaster = GetComponentInParent<CheckpointMaster>();
	}

	private void Start()
	{
		if (useMasterDefaults)
		{
			size = checkpointMaster.defaultCheckpointSize;
			offset = checkpointMaster.defaultCheckpointOffset;
		}
		halfSize = size * 0.5f;
		_center = base.transform.position + new Vector3(offset.x * size.x, offset.y * size.y, offset.z * size.z);
	}

	public bool CheckIn(Vector3 position, float radius)
	{
		return Mathf.Abs(position.x - _center.x) - radius <= halfSize.x && Mathf.Abs(position.y - _center.y) - radius <= halfSize.y && Mathf.Abs(position.z - _center.z) - radius <= halfSize.z;
	}

	public bool CheckIn(Vector3 position, Vector3 radius)
	{
		return Mathf.Abs(position.x - _center.x) - radius.x <= halfSize.x && Mathf.Abs(position.y - _center.y) - radius.y <= halfSize.y && Mathf.Abs(position.z - _center.z) - radius.z <= halfSize.z;
	}

	public void Show()
	{
		base.gameObject.SetActive(true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(false);
	}

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying)
		{
			Awake();
			Start();
		}
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireCube(_center, size);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(_center, 0.5f);
		Gizmos.color = Color.cyan;
		Gizmos.DrawLine(_center, _center + halfSize);
		Gizmos.DrawLine(_center, _center - halfSize);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(base.transform.position, 0.25f);
	}
}
