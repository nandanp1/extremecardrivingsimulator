public class Task : ITask
{
	public string taskId = "None";

	public float targetValue = 1f;

	private float _progress;

	public RewardItem reward;

	public float progress
	{
		get
		{
			return _progress;
		}
	}

	public Task(string id, RewardItem reward)
		: this(id, 1f, reward)
	{
	}

	public Task(string id, float target, RewardItem reward)
	{
		taskId = id;
		targetValue = target;
		this.reward = reward;
	}

	public bool IsComplete()
	{
		return _progress >= targetValue;
	}

	public void AddProgress(float deltaValue)
	{
		SetProgress(_progress + deltaValue);
	}

	public void SetProgress(float newProgress)
	{
		if (!IsComplete())
		{
			_progress = newProgress;
			if (IsComplete() && (bool)MySingleton<Manager>.Instance.guiCurrentLogic)
			{
				MySingleton<Manager>.Instance.taskManager.OnTaskFinished(this);
			}
		}
	}

	public float GetProgress01()
	{
		return progress / targetValue;
	}

	public void Reset(float newCurrentValue = -1f)
	{
		if (newCurrentValue > 0f)
		{
			targetValue = newCurrentValue;
		}
		_progress = 0f;
		Save();
	}

	public void Save()
	{
		SettingsManager.SetTaskValue(this);
	}

	public void Load()
	{
		float rewardValue = SettingsManager.GetRewardValue(taskId);
		SetProgress(rewardValue);
	}
}
