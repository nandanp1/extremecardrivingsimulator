using System;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointMaster : MonoBehaviour
{
	public Vector3 defaultCheckpointSize = Vector3.one;

	public Vector3 defaultCheckpointOffset = new Vector3(0f, 0.5f, 0f);

	public CheckpointOrderBy orderBy = CheckpointOrderBy.ByNumber;

	public CheckpointExecutionMode executionMode = CheckpointExecutionMode.Secuential;

	[HideInInspector]
	public List<CheckpointScript> checkpoints;

	private int _currentCheckpointIndex;

	private int _lastCheckpointIndex;

	private int _intAux;

	private AudioSource _audioSource;

	public CheckpointMasterCallbackEvent OnPassThroughCheckpointEvent;

	public CheckpointMasterCallbackEvent OnPassThroughAllCheckpointEvent;

	private void Awake()
	{
		checkpoints = new List<CheckpointScript>(GetComponentsInChildren<CheckpointScript>());
		_audioSource = GetComponent<AudioSource>();
	}

	private void Start()
	{
		OrderBy();
		InitializeCheckpoints();
	}

	private void OrderBy()
	{
		switch (orderBy)
		{
		case CheckpointOrderBy.None:
			break;
		case CheckpointOrderBy.ByNumber:
			try
			{
				checkpoints.Sort((CheckpointScript p1, CheckpointScript p2) => int.Parse(p1.name).CompareTo(int.Parse(p2.name)));
				break;
			}
			catch (Exception)
			{
				Debug.LogError("Error al ordenar los checkpoints, revisar los nombres: TODOS DEBEN SER NUMÉRICOS");
				break;
			}
		case CheckpointOrderBy.ByName:
			checkpoints.Sort((CheckpointScript p1, CheckpointScript p2) => p1.name.CompareTo(p2.name));
			break;
		}
	}

	private void InitializeCheckpoints()
	{
		_lastCheckpointIndex = -1;
		switch (executionMode)
		{
		case CheckpointExecutionMode.None:
			_currentCheckpointIndex = -1;
			break;
		case CheckpointExecutionMode.Secuential:
		{
			_currentCheckpointIndex = 0;
			for (int j = 0; j < checkpoints.Count; j++)
			{
				checkpoints[j].done = false;
				checkpoints[j].Hide();
			}
			break;
		}
		case CheckpointExecutionMode.Random:
		{
			_currentCheckpointIndex = UnityEngine.Random.Range(0, checkpoints.Count);
			for (int i = 0; i < checkpoints.Count; i++)
			{
				checkpoints[i].done = false;
				checkpoints[i].Hide();
			}
			break;
		}
		}
		checkpoints[_currentCheckpointIndex].Show();
	}

	public void Check(Vector3 position, Vector3 radius)
	{
		if (!IsCompleted())
		{
			switch (executionMode)
			{
			case CheckpointExecutionMode.None:
				throw new NotImplementedException("no se ha programado DoCheck para CheckpointExecutionMode.None");
			case CheckpointExecutionMode.Secuential:
				DoCheckSecuentialMode(position, radius);
				break;
			case CheckpointExecutionMode.Random:
				throw new NotImplementedException("no se ha programado DoCheck para CheckpointExecutionMode.Random");
			}
		}
	}

	private void DoCheckSecuentialMode(Vector3 position, Vector3 radius)
	{
		if (IsInCurrentCheckpoint(position, radius) || IsInGoalCheckpoint(position, radius))
		{
			PlayDoneAudio();
			if (OnPassThroughCheckpointEvent != null)
			{
				OnPassThroughCheckpointEvent(GetCurrent());
			}
			SetCurrentCheckpointDone();
			if (!GoToNextCheckpoint() && OnPassThroughAllCheckpointEvent != null)
			{
				OnPassThroughAllCheckpointEvent(GetLast());
			}
		}
	}

	private bool GoToNextCheckpoint()
	{
		if (HasRemainingCheckpoints())
		{
			switch (executionMode)
			{
			case CheckpointExecutionMode.None:
				throw new NotImplementedException("no se ha programado GoToNextCheckpoint para CheckpointExecutionMode.None");
			case CheckpointExecutionMode.Secuential:
				HideCurrent();
				_lastCheckpointIndex = _currentCheckpointIndex;
				_currentCheckpointIndex++;
				SetCheckpointAsCurrent(_currentCheckpointIndex);
				ShowCurrent();
				break;
			case CheckpointExecutionMode.Random:
				throw new NotImplementedException("no se ha programado GoToNextCheckpoint para CheckpointExecutionMode.Random");
			}
			return true;
		}
		HideCurrent();
		_lastCheckpointIndex = _currentCheckpointIndex;
		return false;
	}

	private bool IsInCurrentCheckpoint(Vector3 position, float radius)
	{
		return IsInCheckpoint(_currentCheckpointIndex, position, radius);
	}

	private bool IsInCheckpoint(int index, Vector3 position, float radius)
	{
		return checkpoints[index].CheckIn(position, radius);
	}

	private bool IsInCheckpoint(int index, Vector3 position, Vector3 radius)
	{
		return checkpoints[index].CheckIn(position, radius);
	}

	private bool IsInCurrentCheckpoint(Vector3 position, Vector3 radius)
	{
		return GetCurrent().CheckIn(position, radius);
	}

	private bool IsInGoalCheckpoint(Vector3 position, Vector3 radius)
	{
		for (int i = 0; i < checkpoints.Count; i++)
		{
			if (checkpoints[i].isGoal && checkpoints[i].CheckIn(position, radius))
			{
				return true;
			}
		}
		return false;
	}

	private int IsInGoalCheckpoint(Vector3 position, float radius)
	{
		for (int i = 0; i < checkpoints.Count; i++)
		{
			if (checkpoints[i].isGoal && checkpoints[i].CheckIn(position, radius))
			{
				return i;
			}
		}
		return -1;
	}

	private bool HasRemainingCheckpoints()
	{
		for (int i = 0; i < checkpoints.Count; i++)
		{
			if (!checkpoints[i].done)
			{
				return true;
			}
		}
		return false;
	}

	private void SetCurrentCheckpointDone()
	{
		SetCheckpointDone(_currentCheckpointIndex);
	}

	private void SetAllCheckpointDone()
	{
		for (int i = 0; i < checkpoints.Count; i++)
		{
			checkpoints[i].done = true;
		}
	}

	private void SetCheckpointDone(int index)
	{
		checkpoints[index].done = true;
	}

	private void SetCheckpointAsCurrent(int index)
	{
		for (int i = 0; i < checkpoints.Count; i++)
		{
			if (checkpoints[i].isCurrent)
			{
				checkpoints[i].isCurrent = false;
			}
		}
		checkpoints[index].isCurrent = true;
	}

	private void ShowCurrent()
	{
		checkpoints[_currentCheckpointIndex].Show();
	}

	private void HideCurrent()
	{
		checkpoints[_currentCheckpointIndex].Hide();
	}

	public void Restart()
	{
		InitializeCheckpoints();
	}

	public int GetCount()
	{
		return checkpoints.Count;
	}

	public int GetDoneCount()
	{
		_intAux = 0;
		for (int i = 0; i < checkpoints.Count; i++)
		{
			if (checkpoints[i].done)
			{
				_intAux++;
			}
		}
		return _intAux;
	}

	public int GetRemaining()
	{
		return GetCount() - GetDoneCount();
	}

	public CheckpointScript GetCurrent()
	{
		return checkpoints[_currentCheckpointIndex];
	}

	public CheckpointScript GetLast()
	{
		return (_currentCheckpointIndex <= 0) ? null : checkpoints[_lastCheckpointIndex];
	}

	public bool IsCompleted()
	{
		switch (executionMode)
		{
		case CheckpointExecutionMode.None:
			throw new NotImplementedException("no se ha programado GoToNextCheckpoint para CheckpointExecutionMode.None");
		case CheckpointExecutionMode.Secuential:
			return _lastCheckpointIndex == _currentCheckpointIndex;
		case CheckpointExecutionMode.Random:
			throw new NotImplementedException("no se ha programado GoToNextCheckpoint para CheckpointExecutionMode.Random");
		default:
			throw new NotImplementedException("no se ha programado GoToNextCheckpoint para CheckpointExecutionMode.Random");
		}
	}

	public void ShowAll()
	{
		for (int i = 0; i < checkpoints.Count; i++)
		{
			checkpoints[i].Show();
		}
	}

	public void ShowLast()
	{
		checkpoints[checkpoints.Count - 1].Show();
	}

	public void HideAllButCurrent()
	{
		for (int i = 0; i < checkpoints.Count; i++)
		{
			checkpoints[i].Hide();
		}
		checkpoints[_currentCheckpointIndex].Show();
	}

	private void PlayDoneAudio()
	{
		if (_audioSource != null)
		{
			_audioSource.Play();
		}
	}
}
