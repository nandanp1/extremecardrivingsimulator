using UnityEngine;

public class CrashableChallangeManager : MonoBehaviour
{
	public static int CRASHABLE_MINIMUM_SCORE = 30;

	public static int crashableCounter;

	public int crashableCounterLocal;

	public int minimumCrashesLocal = CRASHABLE_MINIMUM_SCORE;

	public static float CRASHABLE_REMAINING_TIME = 60f;

	private float crashable_time_left = CRASHABLE_REMAINING_TIME;

	private float total_chrono_time;

	public GameObject crashableParentPrefab;

	public GameObject crashableParentInstantiated;

	public ChronometerEvent chronoEvents;

	public ChronometerClass localChrono;

	private bool timeover;

	private bool running;

	private bool initialized;

	public void Init()
	{
		minimumCrashesLocal = Mathf.Abs(CRASHABLE_MINIMUM_SCORE);
		total_chrono_time = Mathf.Abs(CRASHABLE_REMAINING_TIME);
		timeover = false;
		running = false;
		initialized = true;
	}

	public void Init(float time, int minscore)
	{
		minimumCrashesLocal = Mathf.Abs(minscore);
		total_chrono_time = Mathf.Abs(time);
		timeover = false;
		running = false;
		initialized = true;
	}

	private void Awake()
	{
		MySingleton<Manager>.Instance.guiChallangeModeLogic.crashableChallengeManager = this;
		crashableCounter = 0;
		crashableCounterLocal = 0;
		minimumCrashesLocal = CRASHABLE_MINIMUM_SCORE;
		total_chrono_time = CRASHABLE_REMAINING_TIME;
		localChrono = new ChronometerClass();
		timeover = false;
		running = false;
		chronoEvents = new ChronometerEvent();
		UpdateGUI();
	}

	public void SaveScore()
	{
		crashableCounter++;
		crashableCounterLocal++;
		UpdateGUI();
	}

	private void UpdateGUI()
	{
		MySingleton<Manager>.Instance.guiChallangeModeLogic.lbCrashablesCounter.text = crashableCounterLocal + " / 30";
	}

	private void TimeCounter()
	{
	}

	public void ResetValues()
	{
		localChrono.Reset();
		timeover = false;
		running = false;
		initialized = false;
		crashableCounter = 0;
		crashableCounterLocal = 0;
		UpdateGUI();
		if ((bool)crashableParentInstantiated)
		{
			Object.Destroy(crashableParentInstantiated);
			crashableParentInstantiated = Object.Instantiate(crashableParentPrefab) as GameObject;
			crashableParentInstantiated.transform.parent = base.gameObject.transform;
		}
	}

	private void Update()
	{
		if (!running && initialized && MySingleton<Manager>.Instance.gameLogic.GetSpeed() > 0 && !localChrono.IsPaused())
		{
			localChrono.Start();
			running = true;
		}
	}

	private void LateUpdate()
	{
		crashable_time_left = localChrono.GetRemaining(CRASHABLE_REMAINING_TIME);
		if (!timeover)
		{
			if (crashable_time_left <= 0f)
			{
				timeover = true;
				MySingleton<Manager>.Instance.guiChallangeModeLogic.lbElapsedTime.text = Converter.GetRaceTimeFromSeconds(0f);
				chronoEvents.TimeOver();
			}
			else if (MySingleton<Manager>.Instance.guiChallangeModeLogic.lbElapsedTime != null)
			{
				MySingleton<Manager>.Instance.guiChallangeModeLogic.lbElapsedTime.text = Converter.GetRaceTimeFromSeconds(crashable_time_left);
			}
		}
	}

	public void PauseChronometer()
	{
		if (localChrono != null)
		{
			localChrono.Pause();
		}
	}

	public void ResumeChronometer()
	{
		if (localChrono != null)
		{
			localChrono.Resume();
		}
	}
}
