using UnityEngine;

public class UIRewardHour : UIRewardRow
{
	public UILabel lbNoConnection;

	public GameObject parentConnection;

	public bool firstCheck = true;

	protected override void OnRefresh()
	{
		bool flag = MySingleton<Manager>.Instance.timeManager.HasTimeFromInternet();
		if (task.IsComplete() && firstCheck)
		{
			firstCheck = false;
		}
		NGUITools.SetActive(parentConnection.gameObject, flag);
		NGUITools.SetActive(lbNoConnection.gameObject, !flag);
	}
}
