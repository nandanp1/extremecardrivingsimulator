using System.Collections;
using AxesInMotion.Network;
using UnityEngine;

public class NetworkCarIconMarker : NetworkBehaviour
{
	private GameObject iconMarker;

	private UIFollowTarget followTarget;

	public float offset = 0.5f;

	private void OnEnable()
	{
		if (MySingleton<Manager>.Instance.gameMode != Manager.GameMode.Multiplayer)
		{
			if ((bool)iconMarker)
			{
				Object.Destroy(iconMarker);
			}
			Object.Destroy(this);
		}
	}

	public GameObject Initialize(Color color)
	{
		GameObject ui3DRoot = MySingleton<Manager>.Instance.guiGameLogic.ui3DRoot;
		GameObject prefab = Resources.Load("PlayerIconMarker") as GameObject;
		iconMarker = NGUITools.AddChild(ui3DRoot, prefab);
		NetworkPlayer networkPlayer = new NetworkPlayer(networkView.owner);
		StartCoroutine("WaitAndInitialize", color);
		return iconMarker;
	}

	private IEnumerator WaitAndInitialize(Color color)
	{
		yield return null;
		GameObject parent3DUIRoot = MySingleton<Manager>.Instance.guiGameLogic.ui3DRoot;
		NetworkPlayer player = new NetworkPlayer(networkView.owner);
		iconMarker.name = "PlayerIconMarker " + player.name;
		followTarget = iconMarker.GetComponent<UIFollowTarget>();
		GameObject markerSpawn = new GameObject("Icon Marker Reference");
		markerSpawn.transform.position = base.transform.position + base.transform.up * (2f + offset);
		markerSpawn.transform.rotation = base.transform.rotation;
		markerSpawn.transform.SetParent(base.transform);
		followTarget.SetTarget(markerSpawn.transform);
		followTarget.uiCamera = parent3DUIRoot.GetComponentInChildren<Camera>();
		color.a = 1f;
		((UISprite)followTarget.widget).color = color;
		followTarget.disableIfInvisible = true;
	}

	private void OnDestroy()
	{
		Object.Destroy(iconMarker);
	}
}
