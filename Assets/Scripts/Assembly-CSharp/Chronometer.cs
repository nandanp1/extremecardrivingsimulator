using UnityEngine;

public class Chronometer
{
	private static float _last;

	private static float _elapsed;

	private static float _remaining;

	private static bool _paused;

	private static bool _running;

	public static void Start()
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

	public static void Resume()
	{
		if (!_running && _paused)
		{
			_paused = false;
			_running = true;
			_last = Time.time;
		}
	}

	public static void Pause()
	{
		if (!_paused)
		{
			UpdateElapsed();
			_running = false;
			_paused = true;
		}
	}

	public static void Stop()
	{
		UpdateElapsed();
		_paused = false;
		_running = false;
	}

	public static void Reset()
	{
		_running = false;
		_paused = false;
		_elapsed = 0f;
		_remaining = 0f;
		_last = 0f;
	}

	private static void UpdateElapsed()
	{
		if (_running)
		{
			_elapsed += Time.time - _last;
			_last = Time.time;
		}
	}

	public static float GetElapsed()
	{
		UpdateElapsed();
		return _elapsed;
	}

	private static void UpdateRemaining()
	{
		if (_running)
		{
			_remaining -= Time.time - _last;
			_last = Time.time;
		}
	}

	public static float GetRemaining(float totalRemainingTime)
	{
		if (_remaining == 0f)
		{
			_remaining = totalRemainingTime;
		}
		return GetRemaining();
	}

	public static float GetRemaining()
	{
		UpdateRemaining();
		return _remaining;
	}

	public static bool IsRunning()
	{
		return _running;
	}

	public static bool IsPaused()
	{
		return _paused;
	}
}
