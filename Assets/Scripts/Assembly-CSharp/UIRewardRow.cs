using UnityEngine;

public class UIRewardRow : MonoBehaviour
{
	public enum RewardRowFormatter
	{
		Default = 0,
		Time = 1,
		Distance = 2,
		Hour = 3
	}

	public TaskManager.TaskId taskId;

	public RewardRowFormatter rowFormatter;

	[SerializeField]
	private GameObject completeParent;

	[SerializeField]
	private GameObject incompleteParent;

	[SerializeField]
	private UILabel creditsReward;

	[SerializeField]
	private UILabel lbProgress;

	[SerializeField]
	private UISprite spProgress;

	public ProgressFormatter formatter;

	[HideInInspector]
	public Task task;

	private void Start()
	{
		task = MySingleton<Manager>.Instance.taskManager.GetTask(taskId);
		formatter = InitFormatter();
		Refresh();
	}

	public void Refresh()
	{
		OnRefresh();
		creditsReward.text = task.reward.rewardAmount.ToString();
		UIAutoResizeLabel component = creditsReward.GetComponent<UIAutoResizeLabel>();
		if ((bool)component)
		{
			component.RecenterInParent();
		}
		bool flag = task.IsComplete();
		if (flag)
		{
			ShowCompleteTask();
		}
		else
		{
			ShowIncompleteTask();
		}
		NGUITools.SetActive(completeParent.gameObject, flag);
		NGUITools.SetActive(incompleteParent.gameObject, !flag);
	}

	protected virtual void OnRefresh()
	{
	}

	private void ShowCompleteTask()
	{
	}

	private void ShowIncompleteTask()
	{
		spProgress.fillAmount = task.GetProgress01();
		lbProgress.text = formatter.Format(task.progress, task.targetValue);
	}

	public void OnClaim()
	{
		if (task.IsComplete())
		{
			task.reward.GiveReward();
			task.Reset(-1f);
			if (task.taskId == "keyTaskDistance1")
			{
				ManagerAnalytics.instance.FireRetentionClaim("km", "none");
			}
			if (task.taskId == "keyTaskHourReward")
			{
				ManagerAnalytics.instance.FireRetentionClaim("hourly", "none");
			}
		}
		else if (Debug.isDebugBuild)
		{
			Debug.LogError("Tried to claim a reward for an incomplete task.");
		}
		Refresh();
	}

	private ProgressFormatter InitFormatter()
	{
		switch (rowFormatter)
		{
		case RewardRowFormatter.Time:
			return new TimeFormatter();
		case RewardRowFormatter.Distance:
			return new DistanceFormatter();
		case RewardRowFormatter.Hour:
			return new HourFormatter();
		default:
			return new ProgressFormatter();
		}
	}
}
