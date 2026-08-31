using System;
using System.Collections.Generic;

[Serializable]
public class ChallangeInfoCarExtension
{
	public ChallangeKey challangeKey;

	[NonSerialized]
	public Dictionary<ChallangeSceneIndex, bool> circuitsStatus;

	[NonSerialized]
	public bool completed;

	private ChallangePlayerPref _playerPref;

	public void LoadPlayerPref()
	{
		completed = true;
		circuitsStatus = new Dictionary<ChallangeSceneIndex, bool>();
		ChallangeSceneIndex[] challangeCircuitsSceneIndex = ChallangeConfiguration.GetChallangeCircuitsSceneIndex(challangeKey);
		for (int i = 0; i < challangeCircuitsSceneIndex.Length; i++)
		{
			_playerPref = MySingleton<Manager>.Instance.challangeManager.GetPlayerPref(challangeKey, challangeCircuitsSceneIndex[i]);
			switch (challangeKey)
			{
			case ChallangeKey.Crashable:
			{
				int minimumCrashesLocal = MySingleton<Manager>.Instance.guiChallangeModeLogic.crashableChallengeManager.minimumCrashesLocal;
				circuitsStatus.Add(challangeCircuitsSceneIndex[i], _playerPref.totalCrashables > minimumCrashesLocal);
				break;
			}
			case ChallangeKey.Trial:
				circuitsStatus.Add(challangeCircuitsSceneIndex[i], _playerPref.bestTime > 0f);
				break;
			}
			if (!(_playerPref.bestTime > 0f))
			{
				completed = false;
			}
		}
	}
}
