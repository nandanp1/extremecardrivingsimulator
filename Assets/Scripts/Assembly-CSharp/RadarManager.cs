using UnityEngine;

public class RadarManager : MonoBehaviour
{
	public ChallangeRadarLogic[] radars;

	public Transform nextRadar;

	public Transform lastCheckpoint;

	public float lastSpeedCheckedMPS;

	private void OnEnable()
	{
		MySingleton<Manager>.Instance.guiChallangeModeLogic.radarManager = this;
		nextRadar = radars[0].target;
	}
}
