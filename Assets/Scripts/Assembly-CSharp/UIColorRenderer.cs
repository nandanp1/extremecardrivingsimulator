using UnityEngine;

public class UIColorRenderer : MonoBehaviour
{
	public void OnColorChange()
	{
		GetComponent<Renderer>().sharedMaterial.color = UIColorPicker.current.value;
	}
}
