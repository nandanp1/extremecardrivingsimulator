using UnityEngine;

public class StatsLogicOverall : MonoBehaviour
{
	public const float UPDATEOVERALLSTATS_TIME_REPEATING = 1f;

	public UILabel lbDistanceDriven;

	private void Start()
	{
		UpdateOverallStats();
	}

	private void OnEnable()
	{
		UpdateOverallStats();
	}

	private void UpdateOverallStats()
	{
		MySingleton<Manager>.Instance.stats.SaveDataIntoPlayerPrefs();
		StatsManager stats = MySingleton<Manager>.Instance.stats;
		lbDistanceDriven.text = Converter.MetersToCurrentUnits(stats.metersDrove, true);
	}
}
