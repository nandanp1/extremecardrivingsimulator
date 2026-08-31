using UnityEngine;

public class LightingConfiguration : MonoBehaviour
{
	public Color ambientLightColorDay = new Color(0.361f, 0.367f, 0.41f);

	public Color ambientLightColorNight = new Color(0.16f, 0.18f, 0.22f);

	public Light sunLight;

	public Light moonLight;

	public Transform[] cityZones;

	public float lightmapScale;

	private GameObject[] lights;
}
