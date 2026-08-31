using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Challange : MonoBehaviour
{
	public delegate void ChallengeHasStarted(bool inProgress);

	[Tooltip("Key utilizada para identificar en PlayerPref y escenas de menú")]
	public ChallangeKey key;

	[Tooltip("Tipo de desafío")]
	public ChallangeType type;

	[Tooltip("")]
	public ChallangeScoringSystem scoringSystem;

	[Tooltip("")]
	public InfoCar infoCar;

	[Tooltip("Circuitos del desafío")]
	public ChallangeCircuit[] circuits;

	[HideInInspector]
	public List<ChallangeCircuit> enabledCircuits;

	private int _currentCircuitIndex = -1;

	private List<ChallangeCircuit> _circuitsResult;

	private ChallangePlayerPref _playerPref;

	private ChallangeReward _challangeReward;

	public static event ChallengeHasStarted OnChallengeStarted;

	private void Awake()
	{
		_circuitsResult = new List<ChallangeCircuit>();
		_challangeReward = default(ChallangeReward);
	}

	private void Start()
	{
		enabledCircuits = new List<ChallangeCircuit>();
		SetEnabledCircuits();
	}

	public void PlayCircuit(ChallangeCircuit circuit)
	{
		for (int i = 0; i < circuits.Length; i++)
		{
			if (circuit == circuits[i])
			{
				SetCurrentCircuit(i);
				EnterCircuitAsync(i);
				LoadPlayerPref();
				if (OnChallengeStarted != null && MySingleton<Manager>.Instance.isNewGUI)
				{
					OnChallengeStarted(true);
				}
				break;
			}
		}
	}

	public void LeaveCircuit()
	{
		ExitCircuit(_currentCircuitIndex);
		UnsetCurrentCircuit();
		switch (MySingleton<Manager>.Instance.challangeModeManager.GetChallangeInActivationZone().key)
		{
		case ChallangeKey.Radar:
			MySingleton<Manager>.Instance.guiChallangeModeLogic.ResetRadarValues();
			break;
		case ChallangeKey.Crashable:
			MySingleton<Manager>.Instance.guiChallangeModeLogic.ResetCrashablesValues();
			break;
		}
		if (OnChallengeStarted != null && MySingleton<Manager>.Instance.isNewGUI)
		{
			OnChallengeStarted(false);
		}
	}

	private void SetCurrentCircuit(int index)
	{
		if (-1 >= index || index >= circuits.Length)
		{
			Debug.LogError(string.Format("el index pasado a EnterCircuit no es válido. Se le pasó el valor {0}", index));
		}
		else
		{
			_currentCircuitIndex = index;
		}
	}

	private void UnsetCurrentCircuit()
	{
		_currentCircuitIndex = -1;
	}

	private void EnterCircuitAsync(int index)
	{
		if (-1 >= index || index >= circuits.Length)
		{
			Debug.LogError(string.Format("el index pasado a EnterCircuit no es válido. Se le pasó el valor {0}", index));
		}
		else
		{
			circuits[index].LoadCircuitAsync();
		}
	}

	private void ExitCircuit(int index)
	{
		if (-1 >= index || index >= circuits.Length)
		{
			Debug.LogError(string.Format("el index pasado a EnterCircuit no es válido. Se le pasó el valor {0}", index));
		}
		else
		{
			circuits[index].UnloadCircuit();
		}
	}

	public List<ChallangeCircuit> GetCircuitsInActivationZone()
	{
		_circuitsResult.Clear();
		for (int i = 0; i < circuits.Length; i++)
		{
			if (circuits[i].gameObject.activeSelf && circuits[i].isInActivationZone)
			{
				_circuitsResult.Add(circuits[i]);
			}
		}
		return _circuitsResult;
	}

	public bool IsCompleted()
	{
		for (int i = 0; i < circuits.Length; i++)
		{
			if (circuits[i].gameObject.activeSelf && !circuits[i].completed)
			{
				return false;
			}
		}
		return true;
	}

	private void SetEnabledCircuits()
	{
		enabledCircuits.Clear();
		for (int i = 0; i < circuits.Length; i++)
		{
			if (circuits[i].gameObject.activeSelf)
			{
				enabledCircuits.Add(circuits[i]);
			}
		}
	}

	public ChallangeCircuit GetFirstCircuit(ChallangeSceneIndex sceneIndex)
	{
		for (int i = 0; i < circuits.Length; i++)
		{
			if (circuits[i].sceneIndex == sceneIndex)
			{
				return circuits[i];
			}
		}
		return null;
	}

	public bool IsInCircuit()
	{
		return _currentCircuitIndex > -1;
	}

	public ChallangeCircuit GetCurrentCircuit()
	{
		return (!IsInCircuit()) ? null : circuits[_currentCircuitIndex];
	}

	public string GetCircuitName(ChallangeCircuit circuit)
	{
		return ChallangeConfiguration.GetChallangeCircuitName(key, circuit.sceneIndex);
	}

	public string GetResume()
	{
		return Localization.Get(ChallangeConfiguration.GetChallangeLocalizationKey_Resume(key));
	}

	public void LoadCircuitPlayerPref(ChallangeCircuit circuit)
	{
		_playerPref = MySingleton<Manager>.Instance.challangeManager.GetPlayerPref(key, circuit.sceneIndex);
		circuit.bestSpeed = _playerPref.totalSpeed;
		circuit.bestTime = _playerPref.bestTime;
		circuit.bestCrashable = _playerPref.totalCrashables;
		if (type == ChallangeType.Crashable)
		{
			circuit.completed = _playerPref.totalCrashables >= CrashableChallangeManager.CRASHABLE_MINIMUM_SCORE;
		}
		else
		{
			circuit.completed = _playerPref.bestTime > 0f;
		}
		circuit.earnProportion = !circuit.completed;
	}

	private void LoadPlayerPref()
	{
		switch (scoringSystem)
		{
		case ChallangeScoringSystem.AvgProportionOrMoney:
		{
			float proportion = 100f / (float)enabledCircuits.Count;
			for (int j = 0; j < enabledCircuits.Count; j++)
			{
				LoadCircuitPlayerPref(circuits[j]);
				circuits[j].proportion = proportion;
				circuits[j].money = ChallangeConfiguration.GetChallangeCircuit_Money(key, circuits[j].sceneIndex);
			}
			break;
		}
		case ChallangeScoringSystem.ProportionOrMoney:
		{
			for (int k = 0; k < enabledCircuits.Count; k++)
			{
				LoadCircuitPlayerPref(circuits[k]);
				circuits[k].proportion = ChallangeConfiguration.GetChallangeCircuit_Proportion(key, circuits[k].sceneIndex);
				circuits[k].money = ChallangeConfiguration.GetChallangeCircuit_Money(key, circuits[k].sceneIndex);
			}
			break;
		}
		case ChallangeScoringSystem.Proportion:
			throw new NotImplementedException("Falta programar lógica para ChallangeScoringSystem.Proportion");
		case ChallangeScoringSystem.Money:
		{
			for (int i = 0; i < enabledCircuits.Count; i++)
			{
				LoadCircuitPlayerPref(circuits[i]);
				circuits[i].money = ChallangeConfiguration.GetChallangeCircuit_Money(key, circuits[i].sceneIndex);
			}
			break;
		}
		}
		SetReward();
	}

	public void SaveCurrentCircuitPlayerPrefs(float elapsedTime)
	{
		_playerPref = MySingleton<Manager>.Instance.challangeManager.GetPlayerPref(key, GetCurrentCircuit().sceneIndex);
		if (_playerPref.bestTime == 0f || _playerPref.bestTime > elapsedTime)
		{
			_playerPref.bestTime = elapsedTime;
			_playerPref.SaveBestTime(key, GetCurrentCircuit().sceneIndex);
		}
		if (_challangeReward.rewardType == ChallangeRewardType.Money)
		{
			BillingManager.CreditBalance((decimal)_challangeReward.money);
		}
		LoadPlayerPref();
	}

	public void SaveRadarsScorePlayerPrefs(float totalSpeed)
	{
		_playerPref = MySingleton<Manager>.Instance.challangeManager.GetPlayerPref(key, GetCurrentCircuit().sceneIndex);
		if (_playerPref.totalSpeed == 0f || _playerPref.totalSpeed < totalSpeed)
		{
			_playerPref.totalSpeed = totalSpeed;
			_playerPref.SaveBestSpeed(key, GetCurrentCircuit().sceneIndex);
			Debug.Log("Guardado en playerpref: " + totalSpeed + " metros por segundo");
		}
		if (_challangeReward.rewardType == ChallangeRewardType.Money)
		{
			BillingManager.CreditBalance((decimal)_challangeReward.money);
		}
		LoadPlayerPref();
	}

	public void SaveCrashableScorePlayerPrefs(int totalCrashables)
	{
		_playerPref = MySingleton<Manager>.Instance.challangeManager.GetPlayerPref(key, GetCurrentCircuit().sceneIndex);
		if (_playerPref.totalCrashables == 0 || _playerPref.totalCrashables < totalCrashables)
		{
			_playerPref.totalCrashables = totalCrashables;
			_playerPref.SaveBestCrashables(key, GetCurrentCircuit().sceneIndex);
			Debug.Log("Guardado en playerpref: " + totalCrashables + " crashables");
		}
		if (_challangeReward.rewardType == ChallangeRewardType.Money)
		{
			BillingManager.CreditBalance((decimal)_challangeReward.money);
		}
		LoadPlayerPref();
	}

	private void SetReward()
	{
		switch (scoringSystem)
		{
		case ChallangeScoringSystem.AvgProportionOrMoney:
		{
			bool flag = true;
			float num = 0f;
			for (int i = 0; i < enabledCircuits.Count; i++)
			{
				if (i != _currentCircuitIndex)
				{
					if (circuits[i].completed)
					{
						num += circuits[i].proportion;
					}
					else
					{
						flag = false;
					}
				}
			}
			_challangeReward.proportion = circuits[_currentCircuitIndex].proportion;
			_challangeReward.money = circuits[_currentCircuitIndex].money;
			if (flag && !circuits[_currentCircuitIndex].completed)
			{
				_challangeReward.completed = true;
				_challangeReward.progress = 100f;
				_challangeReward.rewardType = ChallangeRewardType.Car;
			}
			else
			{
				_challangeReward.completed = false;
				_challangeReward.rewardType = (circuits[_currentCircuitIndex].completed ? ChallangeRewardType.Money : ChallangeRewardType.Percentage);
				_challangeReward.progress = num + circuits[_currentCircuitIndex].proportion;
			}
			break;
		}
		case ChallangeScoringSystem.ProportionOrMoney:
			throw new NotImplementedException("Falta programar lógica para ChallangeScoringSystem.AverangeAboutCircuit");
		case ChallangeScoringSystem.Proportion:
			throw new NotImplementedException("Falta programar lógica para ChallangeScoringSystem.OnlyCircuitValue");
		case ChallangeScoringSystem.Money:
			_challangeReward.money = circuits[_currentCircuitIndex].money;
			_challangeReward.rewardType = ChallangeRewardType.Money;
			break;
		}
	}

	public ChallangeReward GetReward()
	{
		return _challangeReward;
	}

	public void ShareCurrentCircuitScoreLeaderboardTime(float result)
	{
		if (GetCurrentCircuit() != null)
		{
			StartCoroutine(ShareScoreLeaderboardBackgroundTime(result));
		}
	}

	public void ShareCurrentCircuitScoreLeaderboardSpeed(float result)
	{
		if (GetCurrentCircuit() != null)
		{
			StartCoroutine(ShareScoreLeaderboardBackgroundSpeed(result));
		}
	}

	public void ShareCurrentCircuitScoreLeaderboardCrashables(int result)
	{
		if (GetCurrentCircuit() != null)
		{
			StartCoroutine(ShareScoreLeaderboardBackgroundCrashables(result));
		}
	}

	private IEnumerator ShareScoreLeaderboardBackgroundTime(float result)
	{
		yield return null;
		GetCurrentCircuit().ShareScoreTrialLeaderboard(key, result);
	}

	private IEnumerator ShareScoreLeaderboardBackgroundSpeed(float result)
	{
		yield return null;
		GetCurrentCircuit().ShareScoreRadarLeaderboard(key, result);
	}

	private IEnumerator ShareScoreLeaderboardBackgroundCrashables(int result)
	{
		yield return null;
		GetCurrentCircuit().ShareScoreCrashablesLeaderboard(key, result);
	}

	public void ShowCurrentCircuitLeaderboard()
	{
		ShowCircuitLeaderboard(GetCurrentCircuit());
	}

	public void ShowCircuitLeaderboard(ChallangeCircuit circuit)
	{
		if (circuit != null)
		{
			circuit.ShowLeaderboard(key);
		}
	}

	public int GetCurrentCircuitIndex()
	{
		return _currentCircuitIndex;
	}
}
