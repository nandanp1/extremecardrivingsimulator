using UnityEngine;

public class ExampleListner : MonoBehaviour
{
	public GUIStyle style;

	private string label = "Click's: ";

	private int count;

	private void Start()
	{
		EventButtonExample.instance.addEventListener("click", onButtonClick);
		EventButtonExample.instance.addEventListener("click", onButtonClickData);
	}

	private void onButtonClick()
	{
		count++;
	}

	private void onButtonClickData(CEvent e)
	{
		Debug.Log("================================");
		Debug.Log("onButtonClickData");
		Debug.Log("dispatcher: " + e.dispatcher.ToString());
		Debug.Log("event data: " + e.data.ToString());
		Debug.Log("event name: " + e.name.ToString());
		Debug.Log("================================");
	}

	private void OnGUI()
	{
		GUI.Label(new Rect(0f, 0f, 200f, 200f), label + count, style);
	}
}
