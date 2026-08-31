public class ChallangePlayerPref
{
	public float bestTime;

	public float totalSpeed;

	public int totalCrashables;

	public ChallangePlayerPref(ChallangePlayerPrefIdentifier identifier)
		: this(identifier.GetKey(), identifier.GetSceneIndex())
	{
	}

	public ChallangePlayerPref(ChallangeKey key, ChallangeSceneIndex sceneIndex)
	{
		Load(key, sceneIndex);
	}

	public void Load(ChallangeKey key, ChallangeSceneIndex sceneIndex)
	{
		if (key == ChallangeKey.Trial)
		{
			bestTime = SettingsManager.GetChallangeCircuitBestTime(key.ToString(), sceneIndex);
		}
		if (key == ChallangeKey.Radar)
		{
			totalSpeed = SettingsManager.GetChallangeRadarsBestSpeed(key.ToString(), sceneIndex);
		}
		if (key == ChallangeKey.Crashable)
		{
			totalCrashables = SettingsManager.GetChallangeBestCrashables(key.ToString(), sceneIndex);
		}
	}

	public void SaveBestTime(ChallangeKey key, ChallangeSceneIndex sceneIndex)
	{
		SettingsManager.SetChallangeCircuitBestTime(key.ToString(), sceneIndex, bestTime);
	}

	public void SaveBestSpeed(ChallangeKey key, ChallangeSceneIndex sceneIndex)
	{
		SettingsManager.SetChallangeRadarsBestSpeed(key.ToString(), sceneIndex, totalSpeed);
	}

	public void SaveBestCrashables(ChallangeKey key, ChallangeSceneIndex sceneIndex)
	{
		SettingsManager.SetChallangeBestCrashables(key.ToString(), sceneIndex, totalCrashables);
	}
}
