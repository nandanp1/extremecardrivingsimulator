using UnityEngine;

public class SwitchLightmapIndex : MonoBehaviour
{
	[SerializeField]
	private int indexLightmap;

	private void OnEnable()
	{
		GetComponent<Renderer>().lightmapIndex = indexLightmap;
	}
}
