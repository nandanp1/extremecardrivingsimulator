using System.Collections;
using AxesInMotion.Network;
using UnityEngine;

public class NetworkCarNameMarker : NetworkBehaviour
{
	private GameObject nameMarker;

	private UIFollowTarget followTarget;

	public float offset;

	private void OnEnable()
	{
		if (MySingleton<Manager>.Instance.gameMode != Manager.GameMode.Multiplayer)
		{
			if ((bool)nameMarker)
			{
				Object.Destroy(nameMarker);
			}
			Object.Destroy(this);
		}
	}

	public GameObject Initialize(Color color)
	{
		GameObject ui3DRoot = MySingleton<Manager>.Instance.guiGameLogic.ui3DRoot;
		GameObject prefab = Resources.Load("PlayerNameMarker") as GameObject;
		nameMarker = NGUITools.AddChild(ui3DRoot, prefab);
		StartCoroutine("WaitAndInitialize", color);
		return nameMarker;
	}

	private IEnumerator WaitAndInitialize(Color color)
	{
		yield return null;
		GameObject parent3DUIRoot = MySingleton<Manager>.Instance.guiGameLogic.ui3DRoot;
		NetworkPlayer player = new NetworkPlayer(networkView.owner);
		nameMarker.name = "PlayerNameMarker " + player.name;
		followTarget = nameMarker.GetComponent<UIFollowTarget>();
		GameObject markerSpawn = new GameObject("Name Marker Reference");
		markerSpawn.transform.position = base.transform.position + base.transform.up * (2f + offset);
		markerSpawn.transform.rotation = base.transform.rotation;
		markerSpawn.transform.SetParent(base.transform);
		followTarget.SetTarget(markerSpawn.transform);
		followTarget.uiCamera = parent3DUIRoot.GetComponentInChildren<Camera>();
		((UILabel)followTarget.widget).text = "[c][" + Utils.ColorRBGAToHex(color) + "]" + player.name + "[-][/c]";
		followTarget.disableIfInvisible = true;
	}

	private void OnDestroy()
	{
		Object.Destroy(nameMarker);
	}
}
