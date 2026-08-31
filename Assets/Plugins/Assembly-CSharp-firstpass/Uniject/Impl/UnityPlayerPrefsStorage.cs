using CodeStage.AntiCheat.ObscuredTypes;

namespace Uniject.Impl
{
	public class UnityPlayerPrefsStorage : IStorage
	{
		public int GetInt(string key, int defaultValue)
		{
			return ObscuredPrefs.GetInt(key, defaultValue);
		}

		public void SetInt(string key, int value)
		{
			ObscuredPrefs.SetInt(key, value);
		}

		public string GetString(string key, string defaultValue)
		{
			return ObscuredPrefs.GetString(key, defaultValue);
		}

		public void SetString(string key, string val)
		{
			ObscuredPrefs.SetString(key, val);
		}
	}
}
