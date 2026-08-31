using UnityEngine;

public class PaintCar : MonoBehaviour
{
	public enum PaintGroup
	{
		paintGroup0 = 0,
		paintGroup1 = 1,
		paintGroup2 = 2
	}

	public Material[] carMaterial;

	public Renderer[] carRenderer;

	public PaintGroup paintGroup;

	public void IniPaintMaterials(Color color)
	{
		for (int i = 0; i < carRenderer.Length; i++)
		{
			carRenderer[i].material = carMaterial[i];
			carRenderer[i].material.SetColor("_Color", color);
		}
	}
}
