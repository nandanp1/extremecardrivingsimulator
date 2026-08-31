using UnityEngine;

public class ChronometerClass
{
	private float _last;

	private float _elapsed;

	private float _remaining;

	private float _remainingUnescaled;

	private bool _paused;

	private bool _running;

	public void Start()
	{
		if (!_running)
		{
			if (!_paused)
			{
				_elapsed = 0f;
			}
			else
			{
				_paused = false;
			}
			_running = true;
			_last = Time.time;
		}
	}

	public void Resume()
	{
		if (!_running && _paused)
		{
			_paused = false;
			_running = true;
			_last = Time.time;
		}
	}

	public void Pause()
	{
		if (!_paused)
		{
			UpdateElapsed();
			_running = false;
			_paused = true;
		}
	}

	public void Stop()
	{
		UpdateElapsed();
		_paused = false;
		_running = false;
	}

	public void Reset()
	{
		_running = false;
		_paused = false;
		_elapsed = 0f;
		_remaining = 0f;
		_remainingUnescaled = 0f;
		_last = 0f;
	}

	private void UpdateElapsed()
	{
		if (_running)
		{
			_elapsed += Time.time - _last;
			_last = Time.time;
		}
	}

	public float GetElapsed()
	{
		UpdateElapsed();
		return _elapsed;
	}

	private void UpdateRemaining()
	{
		if (_running)
		{
			_remaining -= Time.time - _last;
			_last = Time.time;
		}
	}

	private void UpdateRemainingUnscaled()
	{
		if (_running)
		{
			_remainingUnescaled -= Time.realtimeSinceStartup - _last;
			_last = Time.realtimeSinceStartup;
		}
	}

	public float GetRemaining(float totalRemainingTime)
	{
		if (_remaining == 0f)
		{
			_remaining = totalRemainingTime;
		}
		return GetRemaining();
	}

	public float GetRemaining()
	{
		UpdateRemaining();
		return _remaining;
	}

	public float GetRemainingUnescaled(float totalRemainingTime)
	{
		if (_remainingUnescaled == 0f)
		{
			_remainingUnescaled = totalRemainingTime;
		}
		return GetRemainingUnescaled();
	}

	public float GetRemainingUnescaled()
	{
		UpdateRemainingUnscaled();
		return _remainingUnescaled;
	}

	public bool IsRunning()
	{
		return _running;
	}

	public bool IsPaused()
	{
		return _paused;
	}
}
