using UnityEngine;

public class WorkshopDoorLogic : MonoBehaviour
{
	public const float DELAY_FOR_GO_TO_WORKSHOP = 0f;

	private const string boolOpen = "Open";

	public Transform spawnPoint;

	public Animator doorAnimator;

	private static int hashBoolOpen = Animator.StringToHash("Open");

	public void ProcessEvent(WorkshopDoorEventTrigger.TriggerAction triggerAction, WorkshopDoorEventTrigger.WorkshopDoorEventTriggerType workshopEvenTrigger)
	{
		switch (workshopEvenTrigger)
		{
		case WorkshopDoorEventTrigger.WorkshopDoorEventTriggerType.OpenDoor:
			switch (triggerAction)
			{
			case WorkshopDoorEventTrigger.TriggerAction.Enter:
				doorAnimator.SetBool(hashBoolOpen, true);
				break;
			case WorkshopDoorEventTrigger.TriggerAction.Exit:
				doorAnimator.SetBool(hashBoolOpen, false);
				break;
			}
			break;
		case WorkshopDoorEventTrigger.WorkshopDoorEventTriggerType.CarInside:
			switch (triggerAction)
			{
			case WorkshopDoorEventTrigger.TriggerAction.Enter:
				doorAnimator.SetBool(hashBoolOpen, false);
				Invoke("GoToWorkshop", 0f);
				break;
			case WorkshopDoorEventTrigger.TriggerAction.Exit:
				doorAnimator.SetBool(hashBoolOpen, true);
				break;
			}
			break;
		}
	}

	private void GoToWorkshop()
	{
		if (MySingleton<Manager>.Instance.gameMode == Manager.GameMode.Checkpoint)
		{
			MySingleton<Manager>.Instance.currentCheckPoint = MySingleton<Manager>.Instance.gameLogic.checkPointPrefab.GetComponent<CheckpointLogic>().actualIndex;
		}
		MySingleton<Manager>.Instance.gameLogic.GoToWorkshop(base.transform.gameObject.GetComponent<InfoWorkshop>());
	}

	public bool GetStatusBoolOpen()
	{
		return doorAnimator.GetBool(hashBoolOpen);
	}
}
