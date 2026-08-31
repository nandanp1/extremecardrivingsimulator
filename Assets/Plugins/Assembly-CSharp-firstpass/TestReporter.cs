using UnityEngine;

public class TestReporter : MonoBehaviour
{
	public int logTestCount = 1000;

	private int currentLogTestCount;

	private Reporter reporter;

	private GUIStyle style;

	private Rect rect1;

	private Rect rect2;

	private Rect rect3;

	private Rect rect4;

	private Rect rect5;

	private Rect rect6;

	private void Start()
	{
		Application.runInBackground = true;
		reporter = Object.FindFirstObjectByType(typeof(Reporter)) as Reporter;
		Debug.Log("test long text sdf asdfg asdfg sdfgsdfg sdfg sfgsdfgsdfg sdfg sdf gsdfg sfdg sf gsdfg sdfg asdfg sdfgsdfg sdfg sdf gsdfg sfdg sf gsdfg sdfg asdfg sdfgsdfg sdfg sdf gsdfg sfdg sf gsdfg sdfg asdfg sdfgsdfg sdfg sdf gsdfg sfdg sf gsdfg sdfg asdfg sdfgsdfg sdfg sdf gsdfg sfdg sf gsdfg sdfg asdfg ssssssssssssssssssssssasdf asdf asdf asdf adsf \n dfgsdfg sdfg sdf gsdfg sfdg sf gsdfg sdfg asdfasdf asdf asdf asdf adsf \n dfgsdfg sdfg sdf gsdfg sfdg sf gsdfg sdfg asdfasdf asdf asdf asdf adsf \n dfgsdfg sdfg sdf gsdfg sfdg sf gsdfg sdfg asdfasdf asdf asdf asdf adsf \n dfgsdfg sdfg sdf gsdfg sfdg sf gsdfg sdfg asdfasdf asdf asdf asdf adsf \n dfgsdfg sdfg sdf gsdfg sfdg sf gsdfg sdfg asdf");
		style = new GUIStyle();
		style.alignment = TextAnchor.MiddleCenter;
		style.normal.textColor = Color.white;
		style.wordWrap = true;
		for (int i = 0; i < 10; i++)
		{
			Debug.Log("Test Collapsed log");
			Debug.LogWarning("Test Collapsed Warning");
			Debug.LogError("Test Collapsed Error");
		}
		rect1 = new Rect(Screen.width / 2 - 120, Screen.height / 2 - 225, 240f, 50f);
		rect2 = new Rect(Screen.width / 2 - 120, Screen.height / 2 - 175, 240f, 100f);
		rect3 = new Rect(Screen.width / 2 - 120, Screen.height / 2 - 50, 240f, 50f);
		rect4 = new Rect(Screen.width / 2 - 120, Screen.height / 2, 240f, 50f);
		rect5 = new Rect(Screen.width / 2 - 120, Screen.height / 2 + 50, 240f, 50f);
		rect6 = new Rect(Screen.width / 2 - 120, Screen.height / 2 + 100, 240f, 50f);
	}

	private void Update()
	{
		int num = 0;
		while (currentLogTestCount < logTestCount && num < 10)
		{
			Debug.Log("Test Log " + currentLogTestCount);
			Debug.LogError("Test LogError " + currentLogTestCount);
			Debug.LogWarning("Test LogWarning " + currentLogTestCount);
			num++;
			currentLogTestCount++;
		}
	}

	private void OnGUI()
	{
		if ((bool)reporter && !reporter.show)
		{
			GUI.Label(rect1, "Draw circle on screen to show logs", style);
			GUI.Label(rect2, "To use Reporter just drag drop reporter prefab in first scene your game start at ", style);
			if (GUI.Button(rect3, "Load ReporterScene"))
			{
				LegacySceneManager.LoadLevel("ReporterScene");
			}
			if (GUI.Button(rect4, "Load test1"))
			{
				LegacySceneManager.LoadLevel("test1");
			}
			if (GUI.Button(rect5, "Load test2"))
			{
				LegacySceneManager.LoadLevel("test2");
			}
			GUI.Label(rect6, "fps : " + reporter.fps.ToString("0.0"), style);
		}
	}
}
