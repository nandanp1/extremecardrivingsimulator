using UnityEngine;

public class CarsColors : MonoBehaviour
{
	public enum ColorGroups
	{
		colorGroup0 = 0,
		colorGroup1 = 1,
		colorGroup2 = 2
	}

	public Color[] colorGroup0;

	public Color[] colorGroup1;

	public Color[] colorGroup2;

	private PaintCar paintCar;

	public ColorGroups colorGroups;

	public Color GetColorGroup(GameObject car)
	{
		paintCar = car.GetComponent<PaintCar>();
		if (paintCar.paintGroup == PaintCar.PaintGroup.paintGroup0)
		{
			return colorGroup0[Random.Range(0, colorGroup0.Length)];
		}
		return Color.magenta;
	}
}
