public class RewardCredit : RewardItem
{
	public RewardCredit(int creditAmount)
	{
		rewardAmount = creditAmount;
	}

	public override void GiveReward()
	{
		Unibiller.CreditBalance("credits", rewardAmount);
		MySingleton<Manager>.Instance.guiCurrentLogic.OnRewardClaimed(this);
	}
}
