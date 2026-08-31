using System;
using UnityEngine;

[Serializable]
public class CollectablesInfo : MonoBehaviour
{
	public CollectableZone zone;

	public Collectables.CollectableType collectableType;

	public string collectableIconName;

	public int collectableID;

	public int collectionID;

	public int rewardID;

	public int totalCollectables;

	public bool collected;
}
