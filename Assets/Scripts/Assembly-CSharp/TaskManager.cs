using UnityEngine;

public class TaskManager : MonoBehaviour
{
	public enum TaskId
	{
		None = 0,
		keyTaskDistance1 = 1,
		WatchVideoForPaintjob = 10,
		WatchVideoForCredits = 11,
		keyTaskHourReward = 12,
		keyTaskDailyReward = 13
	}

	public const float TIME_FOR_SAVE_REWARDS = 2f;

	public const int DISTANCE_25KM_REWARD = 100;

	public const int DISTANCE_FOR_REMOVE_ADS_FIRST_OFFER = 50000;

	public const int DISTANCE_FOR_REMOVE_ADS_SECOND_OFFER = 70000;

	public const float DISTANCE_25KM_TARGET = 25000f;

	public const int HOUR_REWARD_CREDITS = 50;

	public const float SECONDS_HOUR_REWARD_TARGET = 3600f;

	public const int DAILY_REWARD_CREDITS_DAY_1 = 50;

	public const int DAILY_REWARD_CREDITS_DAY_2 = 100;

	public const int DAILY_REWARD_CREDITS_DAY_3 = 150;

	public const int DAILY_REWARD_CREDITS_DAY_4 = 300;

	public const int DAILY_REWARD_CREDITS_DAY_5_MIN = 500;

	public const int DAILY_REWARD_CREDITS_DAY_5_MAX = 700;

	public const float HOURS_DAILY_REWARD_TARGET = 24f;

	public const int NUM_DAYS_FOR_DAILY_REWARDS = 5;

	public InfoCar dailyRewardCar;

	public Task taskDistance1;

	public Task taskWatchVideoForPaint;

	public Task taskWatchVideoForCredits;

	public Task taskHour;

	public Task taskDaily;

	public TaskId taskId;

	private void Start()
	{
		LoadAllTasks();
	}

	private void LoadAllTasks()
	{
		taskDistance1 = new Task(TaskId.keyTaskDistance1.ToString(), 25000f, new RewardCredit(100));
		taskDistance1.Load();
		taskWatchVideoForPaint = new Task(TaskId.WatchVideoForPaintjob.ToString(), new RewardPaintjob());
		taskWatchVideoForCredits = new Task(TaskId.WatchVideoForCredits.ToString(), new RewardCredit(75));
		taskHour = new Task(TaskId.keyTaskHourReward.ToString(), 3600f, new RewardCredit(50));
		taskHour.Load();
		taskDaily = InitTaskDaily();
		taskDaily.Load();
	}

	public Task InitTaskDaily()
	{
		int num = SettingsManager.GetDayForDailyReward() % 5;
		int creditAmount = 0;
		switch (num)
		{
		case 0:
			creditAmount = 50;
			break;
		case 1:
			creditAmount = 100;
			break;
		case 2:
			creditAmount = 150;
			break;
		case 3:
			creditAmount = 300;
			break;
		case 4:
			if (!SettingsManager.GetBool("keyFirstTimeLastDayDayliReward", true))
			{
				creditAmount = 500;
				break;
			}
			return new Task(TaskId.keyTaskDailyReward.ToString(), new RewardCar(dailyRewardCar));
		}
		return new Task(TaskId.keyTaskDailyReward.ToString(), new RewardCredit(creditAmount));
	}

	public void SaveAllTasks()
	{
		taskDistance1.Save();
		taskHour.Save();
		taskDaily.Save();
	}

	private void OnEnable()
	{
		LoadAllTasks();
	}

	private void OnDisable()
	{
		CancelInvoke("SaveAllTasks");
		SaveAllTasks();
	}

	public void StartingSaveDataRewardsLoop()
	{
		InvokeRepeating("SaveAllTasks", 0f, 2f);
	}

	public void CancelSaveDataRewardsLoop()
	{
		CancelInvoke("SaveAllTasks");
	}

	public Task GetTask(TaskId taskId)
	{
		switch (taskId)
		{
		case TaskId.keyTaskDistance1:
			return taskDistance1;
		case TaskId.WatchVideoForCredits:
			return taskWatchVideoForCredits;
		case TaskId.WatchVideoForPaintjob:
			return taskWatchVideoForPaint;
		case TaskId.keyTaskHourReward:
			return taskHour;
		case TaskId.keyTaskDailyReward:
			return taskDaily;
		default:
			Debug.LogError("Attempted to get an inexistent task.");
			return null;
		}
	}

	public void OnTaskFinished(Task finishedTask)
	{
		MySingleton<Manager>.Instance.guiCurrentLogic.OnTaskFinished(finishedTask);
	}

	public bool HasCompletedTasks()
	{
		if (taskDistance1.IsComplete() || taskHour.IsComplete() || taskDaily.IsComplete())
		{
			return true;
		}
		return false;
	}
}
