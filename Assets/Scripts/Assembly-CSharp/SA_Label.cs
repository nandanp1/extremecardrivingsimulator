using UnityEngine;

public class SA_Label : MonoBehaviour
{
	public string text
	{
		get
		{
			TextMesh componentInChildren = base.gameObject.GetComponentInChildren<TextMesh>();
			return componentInChildren.text;
		}
		set
		{
			TextMesh[] componentsInChildren = base.gameObject.GetComponentsInChildren<TextMesh>();
			TextMesh[] array = componentsInChildren;
			foreach (TextMesh textMesh in array)
			{
				textMesh.text = value;
			}
		}
	}
}
