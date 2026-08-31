using UnityEngine;

public class GUIOptions : MonoBehaviour
{
	public TSTrafficSpawner spawner;

	private float totalCars;

	private void Start()
	{
		if (spawner == null)
		{
			spawner = Object.FindFirstObjectByType<TSTrafficSpawner>();
		}
		totalCars = spawner.TrafficCars.Length;
	}

	private void OnGUI()
	{
		GUI.Label(new Rect(10f, Screen.height - 45, 350f, 25f), "Target Amount of cars: " + spawner.amount);
		GUI.Label(new Rect(10f, Screen.height - 25, 350f, 25f), "Actual Amount of cars on scene: " + (totalCars - (float)spawner.totalFarCars));
		spawner.amount = Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(10f, Screen.height - 55, 250f, 25f), spawner.amount, 0f, totalCars));
		GUI.Label(new Rect(10f, Screen.height - 110, 350f, 25f), "Respawn Time " + spawner.respawnUpSideDownTime);
		spawner.respawnUpSideDownTime = GUI.HorizontalSlider(new Rect(120f, Screen.height - 110, 100f, 25f), spawner.respawnUpSideDownTime, 0f, 20f);
		spawner.respawnIfUpSideDown = GUI.Toggle(new Rect(10f, Screen.height - 90, 250f, 25f), spawner.respawnIfUpSideDown, "Auto Respawn upside down cars?");
		spawner.gameObject.SetActive(GUI.Toggle(new Rect(10f, Screen.height - 170, 250f, 25f), spawner.gameObject.activeSelf, "Enable spawner?"));
	}
}
