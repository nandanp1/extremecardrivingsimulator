using UnityEngine;

public class CollectableEntryBehaviour : MonoBehaviour
{
	public UISprite carIcon;

	public UILabel progress;

	public UIProgressBar progressBar;

	public void Init(CollectablesEntryInfo info)
	{
		carIcon.spriteName = info.collectableIconName;
		int collectablesGot = info.collectablesGot;
		int collectablesTotal = info.collectablesTotal;
		progress.text = string.Empty + collectablesGot + " / " + collectablesTotal;
		float value = (float)collectablesGot / (float)collectablesTotal;
		progressBar.value = value;
	}
}
