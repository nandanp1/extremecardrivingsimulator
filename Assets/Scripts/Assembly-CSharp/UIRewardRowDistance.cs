using UnityEngine;

public class UIRewardRowDistance : UIRewardRow
{
	[SerializeField]
	private UILabel taskTitle;

	protected override void OnRefresh()
	{
		string format = Localization.Get("drive_distance");
		string arg = Converter.MetersToCurrentUnits(task.targetValue, true);
		taskTitle.text = string.Format(format, arg);
	}
}
