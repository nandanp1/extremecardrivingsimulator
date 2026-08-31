using UnityEngine;

public class WorkshopDoorEventTrigger : MonoBehaviour
{
	public enum WorkshopDoorEventTriggerType
	{
		None = 0,
		OpenDoor = 1,
		CarInside = 2
	}

	public enum TriggerAction
	{
		None = 0,
		Enter = 1,
		Exit = 2
	}

	public WorkshopDoorEventTriggerType eventTriggerType;

	private WorkshopDoorLogic workshopLogic;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Car"))
		{
			if (other.GetComponentInParent<InfoCar>().paintable)
			{
				ProcessEvent(TriggerAction.Enter);
			}
			else
			{
				MySingleton<Manager>.Instance.guiGameLogic.ShowCantPaintPanel();
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Car"))
		{
			ProcessEvent(TriggerAction.Exit);
		}
	}

	private void Init()
	{
		if (!workshopLogic)
		{
			workshopLogic = base.transform.parent.GetComponent<WorkshopDoorLogic>();
		}
	}

	private void ProcessEvent(TriggerAction triggerAction)
	{
		Init();
		workshopLogic.ProcessEvent(triggerAction, eventTriggerType);
	}
}
