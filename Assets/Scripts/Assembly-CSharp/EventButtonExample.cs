using UnityEngine;

public class EventButtonExample : EventDispatcher
{
	public static EventButtonExample instance;

	public float w = 150f;

	public float h = 50f;

	private void Awake()
	{
		instance = this;
	}

	private void OnGUI()
	{
		Rect position = new Rect(((float)Screen.width - w) / 2f, ((float)Screen.height - h) / 2f, w, h);
		if (GUI.Button(position, "click me"))
		{
			dispatch("click", "hello");
		}
	}
}
