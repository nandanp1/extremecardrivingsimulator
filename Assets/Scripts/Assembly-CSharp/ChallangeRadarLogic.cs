using UnityEngine;

public class ChallangeRadarLogic : MonoBehaviour
{
	public RadarManager radarManager;

	public Camera activeCamera;

	public int index;

	public float lastSpeedMPS;

	public GameObject aura;

	public Transform target;

	private bool flag = true;

	private GUIChallangeMode guiChallange;

	private void Start()
	{
		guiChallange = MySingleton<Manager>.Instance.guiChallangeModeLogic;
		radarManager = guiChallange.radarManager;
	}

	public void SaveScores(int index)
	{
		RadarFlash();
		lastSpeedMPS = Converter.CurrentUnitsToMetersPerSecond(MySingleton<Manager>.Instance.gameLogic.GetSpeed());
		radarManager.lastSpeedCheckedMPS = lastSpeedMPS;
		guiChallange.radarScoresMPS[index] = lastSpeedMPS;
		guiChallange.radarTotalSpeedMPS += lastSpeedMPS;
		guiChallange.lbTotalSpeed.text = Converter.MSToCurrentUnits(guiChallange.radarTotalSpeedMPS, true);
		guiChallange.lbLastSpeed.text = Converter.MSToCurrentUnits(lastSpeedMPS, true);
		guiChallange.lbRadar.text = string.Format("RADAR {0}", (index + 1).ToString());
	}

	private void RadarFlash()
	{
		TweenAlpha component = guiChallange.spFlash.GetComponent<TweenAlpha>();
		component.ResetToBeginning();
		component.PlayForward();
		guiChallange.audioFlash.ignoreListenerVolume = true;
		guiChallange.audioFlash.Play();
	}

	private void PointNextRadar(int index)
	{
		if (index < radarManager.radars.Length - 1)
		{
			radarManager.nextRadar = radarManager.radars[index + 1].target;
			radarManager.radars[index + 1].aura.SetActive(true);
			radarManager.radars[index + 1].GetComponent<Collider>().enabled = true;
		}
		else
		{
			radarManager.nextRadar = radarManager.lastCheckpoint;
			radarManager.lastCheckpoint.gameObject.SetActive(true);
		}
	}

	private void OnTriggerEnter(Collider col)
	{
		if (flag && col.tag == "Car")
		{
			flag = false;
			SaveScores(index);
			PointNextRadar(index);
			aura.SetActive(false);
			base.gameObject.GetComponent<Collider>().enabled = false;
			flag = true;
		}
	}
}
