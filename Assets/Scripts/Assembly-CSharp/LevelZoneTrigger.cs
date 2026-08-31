using System.Collections.Generic;
using UnityEngine;

public class LevelZoneTrigger : MonoBehaviour
{
	public enum TriggerMode
	{
		None = 0,
		TriggerForLoad = 1,
		TriggerForUnload = 2,
		TriggerLimitLoadingZone = 3
	}

	private const int MAX_SIZE_LIGHTMAPSTEMP = 3;

	private Color color = Color.cyan;

	public bool wiredgizmo;

	public Texture2D[] lastLightmaps;

	public TriggerMode triggerMode;

	public bool isTriggerForLoad
	{
		get
		{
			return triggerMode == TriggerMode.TriggerForLoad;
		}
	}

	public bool isTriggerForUnload
	{
		get
		{
			return triggerMode == TriggerMode.TriggerForUnload;
		}
	}

	public bool isTriggerForLimitLoadingZone
	{
		get
		{
			return triggerMode == TriggerMode.TriggerLimitLoadingZone;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		base.transform.parent.GetComponent<LevelZone>().ProcessTriggerEvent(this, other, true);
	}

	private void OnTriggerExit(Collider other)
	{
		LevelZone component = base.transform.parent.GetComponent<LevelZone>();
		if (isTriggerForUnload && component.isBigZone)
		{
			List<LightmapData> list = new List<LightmapData>();
			for (int i = 0; i < 3 && i < LightmapSettings.lightmaps.Length; i++)
			{
				list.Add(LightmapSettings.lightmaps[i]);
			}
			LightmapSettings.lightmaps = list.ToArray();
		}
		base.transform.parent.GetComponent<LevelZone>().ProcessTriggerEvent(this, other, false);
	}

	private void OnDrawGizmos()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		if (triggerMode == TriggerMode.TriggerForUnload)
		{
			color = Color.red;
		}
		Gizmos.color = color;
		if (wiredgizmo)
		{
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}
		else
		{
			Gizmos.DrawCube(Vector3.zero, Vector3.one);
		}
	}
}
