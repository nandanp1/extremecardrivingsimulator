using UnityEngine;

public class Instantiater : MonoBehaviour
{
	public enum Game
	{
		None = 0,
		ExtremeCarDrivingSimulator = 1,
		ExtremeSUVDrivingSimulator = 2
	}

	public Game currentGame;

	public GameObject menuDecoration;

	private void Awake()
	{
		if (currentGame != Game.None)
		{
		}
	}
}
