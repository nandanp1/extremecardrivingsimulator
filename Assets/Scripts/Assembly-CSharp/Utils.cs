using UnityEngine;

public class Utils
{
	public static string ColorRBGAToHex(Color color)
	{
		float num = color.r * 255f;
		float num2 = color.g * 255f;
		float num3 = color.b * 255f;
		string hex = GetHex(Mathf.FloorToInt(num / 16f));
		string hex2 = GetHex(Mathf.RoundToInt(num % 16f));
		string hex3 = GetHex(Mathf.FloorToInt(num2 / 16f));
		string hex4 = GetHex(Mathf.RoundToInt(num2 % 16f));
		string hex5 = GetHex(Mathf.FloorToInt(num3 / 16f));
		string hex6 = GetHex(Mathf.RoundToInt(num3 % 16f));
		return hex + hex2 + hex3 + hex4 + hex5 + hex6;
	}

	private static string GetHex(int n)
	{
		string text = "0123456789ABCDEF";
		return string.Empty + text[Mathf.Min(n, text.Length - 1)];
	}
}
