using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public static class GUIMenuChallangeMode
{
	private const string DYNAMIC_INSTANCE_NAME = "dynamicInstance";

	private const string DYNAMIC_INSTANCE_SPRITE_TICK_NAME = "spTick";

	private const string DYNAMIC_INSTANCE_LABEL_DESCRIPTION_NAME = "lbDescription";

	public static bool MakeMenuItems(UIWidget coChallangeChk, InfoCar infoCar)
	{
		if (infoCar.challangeExtension.circuitsStatus == null)
		{
			infoCar.challangeExtension.LoadPlayerPref();
		}
		for (int i = 0; i < coChallangeChk.transform.parent.childCount; i++)
		{
			if (coChallangeChk.transform.parent.GetChild(i).name == "dynamicInstance")
			{
				Object.Destroy(coChallangeChk.transform.parent.GetChild(i).gameObject);
			}
		}
		Resources.UnloadUnusedAssets();
		if (infoCar.isChallangeReward && infoCar.challegeRewardType == InfoCar.ChallengeRewardType.TRIAL)
		{
			foreach (ChallangeSceneIndex key2 in infoCar.challangeExtension.circuitsStatus.Keys)
			{
				GameObject gameObject = NGUITools.AddChild(coChallangeChk.transform.parent.gameObject, coChallangeChk.gameObject);
				gameObject.name = "dynamicInstance";
				gameObject.transform.Find("spTick").GetComponent<UISprite>().spriteName = ((!infoCar.challangeExtension.circuitsStatus[key2]) ? "checkbox_not_marked" : "checkbox_marked");
				gameObject.transform.Find("lbDescription").GetComponent<UILabel>().text = ChallangeConfiguration.GetChallangeCircuitName(infoCar.challangeExtension.challangeKey, key2);
				NGUITools.SetActive(gameObject.gameObject, true);
			}
			if (!infoCar.challangeExtension.completed)
			{
				return false;
			}
		}
		if (infoCar.isChallangeReward && infoCar.challegeRewardType == InfoCar.ChallengeRewardType.CRASHABLE)
		{
			for (int j = 1; j <= 4; j++)
			{
				GameObject gameObject2 = NGUITools.AddChild(coChallangeChk.transform.parent.gameObject, coChallangeChk.gameObject);
				gameObject2.name = "dynamicInstance";
				string key = "chgBest_Crashable__2Game_Crashable_0" + j;
				float num = ObscuredPrefs.GetInt(key);
				gameObject2.transform.Find("spTick").GetComponent<UISprite>().spriteName = ((!(num >= (float)CrashableChallangeManager.CRASHABLE_MINIMUM_SCORE)) ? "checkbox_not_marked" : "checkbox_marked");
				gameObject2.transform.Find("lbDescription").GetComponent<UILabel>().text = Localization.Get("challange_crashable_name") + " " + j;
				NGUITools.SetActive(gameObject2.gameObject, true);
			}
		}
		return true;
	}

	public static bool Completed(InfoCar infoCar)
	{
		switch (infoCar.challegeRewardType)
		{
		case InfoCar.ChallengeRewardType.TRIAL:
			if (infoCar.challangeExtension.circuitsStatus == null)
			{
				infoCar.challangeExtension.LoadPlayerPref();
			}
			return MySingleton<Manager>.Instance.stats.AllTrialsCompleted;
		case InfoCar.ChallengeRewardType.RADAR:
			return MySingleton<Manager>.Instance.stats.AllRadarsCompleted;
		case InfoCar.ChallengeRewardType.CRASHABLE:
			return MySingleton<Manager>.Instance.stats.AllCrasheablesCompleted;
		default:
			return false;
		}
	}
}
