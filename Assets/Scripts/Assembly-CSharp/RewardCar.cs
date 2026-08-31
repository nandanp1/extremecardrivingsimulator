using UnityEngine;

public class RewardCar : RewardItem
{
	private InfoCar rewardCar;

	public RewardCar(InfoCar car)
	{
		rewardCar = car;
	}

	public override void GiveReward()
	{
		if (Debug.isDebugBuild)
		{
			Debug.Log("REgalar COCHE");
		}
	}
}
