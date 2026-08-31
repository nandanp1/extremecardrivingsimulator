using UnityEngine;

public class CarWheelFriction : ScriptableObject
{
	public static float[] MCforward = new float[50]
	{
		0.561f, 1.114f, 1.659f, 2.195f, 2.724f, 3.246f, 3.759f, 4.266f, 4.765f, 5.257f,
		5.741f, 6.219f, 6.69f, 7.154f, 7.611f, 8.062f, 8.506f, 8.944f, 9.376f, 9.801f,
		10.223f, 10.636f, 11.044f, 11.443f, 11.839f, 12.229f, 12.614f, 12.99f, 13.363f, 13.731f,
		14.095f, 14.452f, 14.805f, 15.153f, 15.496f, 15.834f, 16.167f, 16.495f, 16.819f, 17.138f,
		17.453f, 17.772f, 18.078f, 18.38f, 18.667f, 18.97f, 19.26f, 19.545f, 19.826f, 20.103f
	};

	public static float[] MCsideways = new float[50]
	{
		0.555f, 1.095f, 1.622f, 2.135f, 2.636f, 3.124f, 3.6f, 4.06f, 4.513f, 4.951f,
		5.382f, 5.793f, 6.199f, 6.602f, 7.002f, 7.348f, 7.723f, 8.096f, 8.419f, 8.782f,
		9.101f, 9.403f, 9.779f, 10.026f, 10.322f, 10.695f, 10.921f, 11.157f, 11.465f, 11.807f,
		12.005f, 12.202f, 12.44f, 12.741f, 13.06f, 13.234f, 13.394f, 13.57f, 13.78f, 14.033f,
		14.333f, 14.567f, 14.707f, 14.835f, 14.969f, 15.119f, 15.291f, 15.489f, 15.714f, 15.966f
	};

	public static float MCt0 = 0.9f;

	public static float MCt1 = 1.3f;

	public static Vector2 GetPeakValue(float[] Coefs, float grip, float gripRange, float drift)
	{
		return GetSpLineMax(1f, GetValue(Coefs, grip, 1f), 1f + gripRange, GetValue(Coefs, drift, 1f + gripRange));
	}

	public static float GetSlope(float[] Coefs, float Slip, float Value)
	{
		float num = Value / Slip;
		if (num <= Coefs[0])
		{
			return Mathf.InverseLerp(0f, Coefs[0], num) * 50f;
		}
		if (num >= Coefs[Coefs.Length - 1])
		{
			return Coefs.Length * 50;
		}
		int num2 = -1;
		int num3 = Coefs.Length;
		while (num3 - num2 > 1)
		{
			int num4 = num2 + num3 >> 1;
			if (Coefs[num4] < num)
			{
				num2 = num4;
			}
			else
			{
				num3 = num4;
			}
		}
		float num5 = Coefs[num2];
		float num6 = Coefs[num3];
		float num7 = (num2 + 1) * 50;
		float num8 = (num3 + 1) * 50;
		return num7 + (num8 - num7) / (num6 - num5) * (num - num5);
	}

	public static float GetValue(float[] Coefs, float Slope, float Slip)
	{
		float num;
		if (Slope <= 50f)
		{
			num = Coefs[0] * Slope / 50f;
		}
		else if (Slope >= (float)Coefs.Length * 50f)
		{
			num = Coefs[Coefs.Length - 1];
		}
		else
		{
			int num2 = (int)(Slope / 50f);
			float num3 = num2 * 50;
			float num4 = (num2 + 1) * 50;
			float num5 = Coefs[num2 - 1];
			float num6 = Coefs[num2];
			num = num5 + (num6 - num5) / (num4 - num3) * (Slope - num3);
		}
		return num * Slip;
	}

	private static Vector2 GetSpLineMax(float Slip0, float Value0, float Slip1, float Value1)
	{
		Vector2 result = new Vector2(0f, float.NegativeInfinity);
		Vector2 vector = new Vector2(Slip0, Value0);
		Vector2 vector2 = new Vector2(Slip1, Value1);
		float num = Value0 / Slip0;
		float num2 = (1f + (Slip1 - Slip0) / (Slip1 - Slip0 + 1f)) * MCt0;
		Vector2 vector3 = new Vector2(Slip1 - Slip0, num * Slip1 - Value0) * num2;
		Vector2 vector4 = new Vector2(Slip1 - Slip0, 0f) * MCt1;
		int num3 = (int)((Slip1 - Slip0) * 10f);
		if (num3 < 10)
		{
			num3 = 10;
		}
		for (int i = 0; i <= num3; i++)
		{
			float num4 = i;
			num4 /= (float)num3;
			float num5 = num4 * num4;
			float num6 = num5 * num4;
			float num7 = 2f * num6 - 3f * num5 + 1f;
			float num8 = -2f * num6 + 3f * num5;
			float num9 = num6 - 2f * num5 + num4;
			float num10 = num6 - num5;
			Vector2 vector5 = num7 * vector + num8 * vector2 + num9 * vector3 + num10 * vector4;
			if (vector5.y >= result.y)
			{
				result = vector5;
				continue;
			}
			break;
		}
		return result;
	}

	public static void DrawFrictionCurve(GUICanvas Canvas, float[] Coefs, CarFrictionCurve curve, Color col)
	{
		float num = curve.grip;
		float drift = curve.drift;
		if (num < drift)
		{
			num = drift;
		}
		float num2 = 1f;
		float value = GetValue(Coefs, num, num2);
		float num3 = 1f + curve.gripRange;
		float value2 = GetValue(Coefs, drift, num3);
		Canvas.FrictionCurve(num2, value, num3, value2, curve.driftSlope, col);
	}

	public static void DrawScaledFrictionCurve(GUICanvas Canvas, float[] Coefs, CarFrictionCurve curve, float scale, Color col)
	{
		float num = curve.grip;
		float drift = curve.drift;
		if (num < drift)
		{
			num = drift;
		}
		float num2 = 1f * scale;
		float value = GetValue(Coefs, num, num2);
		float num3 = (1f + curve.gripRange) * scale;
		float value2 = GetValue(Coefs, drift, num3);
		Canvas.FrictionCurve(num2, value, num3, value2, curve.driftSlope, col);
	}
}
