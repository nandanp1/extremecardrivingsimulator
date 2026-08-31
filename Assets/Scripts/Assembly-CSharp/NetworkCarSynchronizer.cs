using System.Collections;
using AxesInMotion.Network;
using UnityEngine;

public class NetworkCarSynchronizer : NetworkBehaviour
{
	public enum SynchronizationType
	{
		None = 0,
		InterpolationWithLerp = 1
	}

	public struct Snapshot
	{
		public Vector3 syncPosition;

		public Quaternion syncRotation;

		public double timeStamp;

		public int tick;

		public Snapshot(Vector3 syncPosition, Quaternion syncRotation, double timeStamp, int tick)
		{
			this.syncPosition = syncPosition;
			this.syncRotation = syncRotation;
			this.timeStamp = timeStamp;
			this.tick = tick;
		}
	}

	private Rigidbody sharedRigidbody;

	private Transform sharedTransform;

	public SynchronizationType synchronizationType = SynchronizationType.InterpolationWithLerp;

	private bool thereIsUpdate;

	public float syncDelay;

	private float syncTime;

	public float interpolationBackTime = 0.1f;

	public Snapshot[] buffer = new Snapshot[25];

	private int bufferLength = 25;

	public int statesCount;

	private int ticks;

	private float maxTimeWithNaNErrors = 1.5f;

	private float lastNaNErrorTimestamp;

	private bool hasNaNError;

	private float spareLerp;

	private bool lastSnapshotReached = true;

	private bool statusReset;

	private Snapshot lastState;

	private Snapshot targetState;

	private float t;

	public int currentIndex = -1;

	public float averageFactor = 1f;

	private double lastPing;

	private double averagePing;

	private int pingSamples;

	private double averageJitter;

	private int jitterSamples;

	protected override void Awake()
	{
		base.Awake();
		if (MySingleton<Manager>.Instance.gameMode != Manager.GameMode.Multiplayer)
		{
			Object.DestroyImmediate(this);
			return;
		}
		if (base.observed == null)
		{
			base.observed = this;
		}
		if (sharedRigidbody == null)
		{
			sharedRigidbody = GetComponent<Rigidbody>();
		}
		if (sharedTransform == null)
		{
			sharedTransform = base.transform;
		}
		if (!base.isMine && (bool)sharedRigidbody)
		{
			sharedRigidbody.useGravity = false;
			sharedRigidbody.isKinematic = true;
		}
	}

	private void Start()
	{
		if (base.isMine)
		{
			StartCoroutine("SaveMyTransform");
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			SendProperties(ref stream);
		}
		else
		{
			ReceiveProperties(ref stream, info);
		}
	}

	private void SendProperties(ref PhotonStream stream)
	{
		stream.SendNext(sharedTransform.position);
		stream.SendNext(sharedTransform.rotation);
		stream.SendNext(ticks);
		ticks++;
	}

	private void ReceiveProperties(ref PhotonStream stream, PhotonMessageInfo info)
	{
		Snapshot snapshot = new Snapshot((Vector3)stream.ReceiveNext(), (Quaternion)stream.ReceiveNext(), info.timestamp, (int)stream.ReceiveNext());
		if (statesCount > 1 && snapshot.timeStamp < buffer[0].timeStamp)
		{
			return;
		}
		if (synchronizationType == SynchronizationType.None)
		{
			buffer[0] = snapshot;
			return;
		}
		if (snapshot.timeStamp == buffer[0].timeStamp)
		{
			buffer[0] = snapshot;
			return;
		}
		for (int num = bufferLength - 1; num >= 1; num--)
		{
			buffer[num] = buffer[num - 1];
		}
		buffer[0] = snapshot;
		statesCount = Mathf.Min(statesCount + 1, bufferLength);
	}

	private void CheckNanValues()
	{
		Vector3 position = sharedTransform.position;
		if (float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z))
		{
			if (hasNaNError)
			{
				float num = Time.time - lastNaNErrorTimestamp;
				if (num >= maxTimeWithNaNErrors)
				{
					Debug.LogWarning("La posicion interpolada tiene anomalias. Desactivando interpolacion");
					synchronizationType = SynchronizationType.None;
				}
			}
			else
			{
				hasNaNError = true;
				lastNaNErrorTimestamp = Time.time;
			}
		}
		else
		{
			hasNaNError = false;
		}
	}

	private void Update()
	{
		if (base.isMine)
		{
			return;
		}
		if (synchronizationType == SynchronizationType.None)
		{
			sharedTransform.position = buffer[0].syncPosition;
			sharedTransform.rotation = buffer[0].syncRotation;
		}
		else if (synchronizationType == SynchronizationType.InterpolationWithLerp)
		{
			if (lastSnapshotReached)
			{
				if (!statusReset)
				{
					lastState = new Snapshot(sharedTransform.position, sharedTransform.rotation, targetState.timeStamp, targetState.tick);
					statusReset = true;
				}
				bool flag = false;
				spareLerp = Mathf.Max(syncDelay - interpolationBackTime, 0f);
				if (GetNextSnapshot(ref targetState, lastState.timeStamp))
				{
					float num = 0.02f;
					interpolationBackTime = num - spareLerp;
					syncTime = 0f;
					t = 0f;
					lastSnapshotReached = false;
					statusReset = false;
				}
			}
			if (!lastSnapshotReached)
			{
				syncTime += Time.deltaTime;
				t = syncTime / interpolationBackTime;
				sharedTransform.position = Vector3LerpUnclamped(lastState.syncPosition, targetState.syncPosition, t);
				sharedTransform.rotation = Quaternion.Slerp(lastState.syncRotation, targetState.syncRotation, t);
				lastSnapshotReached = t >= 1f;
			}
		}
		if (synchronizationType == SynchronizationType.InterpolationWithLerp)
		{
			CheckNanValues();
		}
	}

	private Vector3 Vector3LerpUnclamped(Vector3 from, Vector3 to, float t)
	{
		return new Vector3(from.x + (to.x - from.x) * t, from.y + (to.y - from.y) * t, from.z + (to.z - from.z) * t);
	}

	private bool GetNextSnapshot(ref Snapshot nextSnapShot, double timeStamp)
	{
		bool flag = false;
		if (statesCount == 0)
		{
			return false;
		}
		for (int num = bufferLength - 1; num >= 0; num--)
		{
			if (buffer[num].timeStamp > timeStamp)
			{
				nextSnapShot = buffer[num];
				flag = true;
				currentIndex = num;
				break;
			}
		}
		if (!flag)
		{
			nextSnapShot = buffer[0];
		}
		statesCount = Mathf.Max(statesCount - 1, 0);
		return flag;
	}

	private void CalculateAveragePing(double newPing)
	{
		averagePing = ((double)pingSamples * averagePing + newPing) / (double)(pingSamples + 1);
		pingSamples++;
		double num = lastPing - newPing;
		if (num < 0.0)
		{
			num *= -1.0;
		}
		averageJitter = ((double)jitterSamples * averageJitter + num) / (double)(jitterSamples + 1);
		jitterSamples++;
		lastPing = newPing;
	}

	private IEnumerator SaveMyTransform()
	{
		while (MySingleton<Manager>.Instance.networkManager.connected)
		{
			MySingleton<Manager>.Instance.connection.reconnectionData = new Connection.ReconnectData(MySingleton<Manager>.Instance.networkRooms.currentRoom.name, MySingleton<Manager>.Instance.networkGame.miniGameSelected, base.transform.position, base.transform.rotation);
			yield return new WaitForSeconds(2f);
		}
	}

	private void OnEnable()
	{
		if (MySingleton<Manager>.Instance.gameMode != Manager.GameMode.Multiplayer)
		{
			base.observed = null;
			Object.Destroy(this);
		}
	}

	private void OnDestroy()
	{
		StopCoroutine("SaveMyTransform");
	}
}
