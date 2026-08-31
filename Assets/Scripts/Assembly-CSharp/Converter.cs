using UnityEngine;

public static class Converter
{
	public const string EMPTY_SPEED = "---";

	public const string EMPTY_TIME = "-:--:---";

	public const string RACE_TIME_SEPARATOR = ":";

	public const float METERS_PER_SECOND_TO_KMH = 3.6f;

	public const float METERS_PER_SECOND_TO_MPH = 2.237f;

	public const float METERS_TO_KM = 0.001f;

	public const float METERS_TO_MI = 0.0006213712f;

	public const string UNITS_KMH = "KM/H";

	public const string UNITS_MPH = "MPH";

	public const string UNITS_KM = "KM";

	public const string UNITS_MI = "MI";

	public static string GetRaceTimeFromSeconds(float? floatSecondsNullable, bool withMilis = true)
	{
		if (!floatSecondsNullable.HasValue)
		{
			return "-:--:---";
		}
		float value = floatSecondsNullable.Value;
		string empty = string.Empty;
		int num = (int)value / 60;
		empty = empty + num + ":";
		value -= (float)(60 * num);
		int num2 = (int)value;
		value -= (float)num2;
		string text = num2.ToString("00");
		empty += text;
		if (withMilis)
		{
			empty += ":";
			string text2 = ((int)(value * 1000f)).ToString("000");
			empty += text2;
		}
		return empty;
	}

	public static string SecondsToHhMmSs(float seconds)
	{
		int num = (int)seconds / 3600;
		seconds %= 3600f;
		if (num >= 10)
		{
			return num + ":" + SecondsToMmSs(seconds);
		}
		return "0" + num + ":" + SecondsToMmSs(seconds);
	}

	public static string SecondsToMmSs(float seconds)
	{
		int num = (int)seconds / 60;
		seconds %= 60f;
		return num.ToString("00") + ":" + ((int)seconds).ToString("00");
	}

	public static float MSToKMH(float? metersPerSecond)
	{
		if (!metersPerSecond.HasValue)
		{
			return -1f;
		}
		return Mathf.Abs(metersPerSecond.Value * 3.6f);
	}

	public static float MSToMPH(float? metersPerSecond)
	{
		if (!metersPerSecond.HasValue)
		{
			return -1f;
		}
		return Mathf.Abs(metersPerSecond.Value * 2.237f);
	}

	public static float MetersToKM(float? meters)
	{
		if (!meters.HasValue)
		{
			return -1f;
		}
		return Mathf.Abs(meters.Value * 0.001f);
	}

	public static float MetersToMI(float? meters)
	{
		if (!meters.HasValue)
		{
			return -1f;
		}
		return Mathf.Abs(meters.Value * 0.0006213712f);
	}

	public static string MSToStringKMH(float? metersPerSecond, bool includeUnits)
	{
		string empty = string.Empty;
		float num = MSToKMH(metersPerSecond);
		empty = ((!(num >= 0f)) ? "---" : Mathf.RoundToInt(num).ToString());
		if (includeUnits)
		{
			empty += " KM/H";
		}
		return empty;
	}

	public static string MSToStringMPH(float? metersPerSecond, bool includeUnits)
	{
		string empty = string.Empty;
		float num = MSToMPH(metersPerSecond);
		empty = ((!(num >= 0f)) ? "---" : Mathf.RoundToInt(num).ToString());
		if (includeUnits)
		{
			empty += " MPH";
		}
		return empty;
	}

	public static string MetersToStringKM(float? meters, bool includeUnits, bool includeDecimals = false)
	{
		string empty = string.Empty;
		float num = MetersToKM(meters);
		empty = (includeDecimals ? ((!(num >= 0f)) ? "---" : num.ToString("0.00")) : ((!(num >= 0f)) ? "---" : Mathf.RoundToInt(num).ToString()));
		if (includeUnits)
		{
			empty += " KM";
		}
		return empty;
	}

	public static string MetersToStringMI(float? meters, bool includeUnits, bool includeDecimals = false)
	{
		string empty = string.Empty;
		float num = MetersToMI(meters);
		empty = (includeDecimals ? ((!(num >= 0f)) ? "---" : num.ToString("0.00")) : ((!(num >= 0f)) ? "---" : Mathf.RoundToInt(num).ToString()));
		if (includeUnits)
		{
			empty += " MI";
		}
		return empty;
	}

	public static string MSToCurrentUnits(float? metersPerSecond, bool includeUnits)
	{
		SettingsManager.Units units = MySingleton<Manager>.Instance.units;
		if (units != SettingsManager.Units.KMH && units == SettingsManager.Units.MPH)
		{
			return MSToStringMPH(metersPerSecond, includeUnits);
		}
		return MSToStringKMH(metersPerSecond, includeUnits);
	}

	public static string MetersToCurrentUnits(float? meters, bool includeUnits, bool includeDecimals = false)
	{
		SettingsManager.Units units = MySingleton<Manager>.Instance.units;
		if (units != SettingsManager.Units.KMH && units == SettingsManager.Units.MPH)
		{
			return MetersToStringMI(meters, includeUnits, includeDecimals);
		}
		return MetersToStringKM(meters, includeUnits, includeDecimals);
	}

	public static float CurrentUnitsToMetersPerSecond(float units)
	{
		SettingsManager.Units units2 = MySingleton<Manager>.Instance.units;
		if (units2 != SettingsManager.Units.KMH && units2 == SettingsManager.Units.MPH)
		{
			return units * 0.44704f;
		}
		return units * (5f / 18f);
	}

	public static float CurrentUnitsToKMH(float units)
	{
		SettingsManager.Units units2 = MySingleton<Manager>.Instance.units;
		if (units2 != SettingsManager.Units.KMH && units2 == SettingsManager.Units.MPH)
		{
			return units * 1.609344f;
		}
		return units;
	}

	public static string GetDistanceUnit()
	{
		switch (MySingleton<Manager>.Instance.units)
		{
		case SettingsManager.Units.MPH:
			return "MI";
		case SettingsManager.Units.KMH:
			return "KM";
		default:
			return string.Empty;
		}
	}
}
