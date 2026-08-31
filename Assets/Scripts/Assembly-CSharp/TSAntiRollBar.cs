using UnityEngine;

public class TSAntiRollBar : MonoBehaviour
{
	public TSSimpleCar_Wheel wheel1;

	public TSSimpleCar_Wheel wheel2;

	public float coefficient = 6000f;

	private void FixedUpdate()
	{
		float num = (wheel1.compression - wheel2.compression) * coefficient;
		wheel1.AntiRollBarForce = num;
		wheel2.AntiRollBarForce = 0f - num;
	}
}
