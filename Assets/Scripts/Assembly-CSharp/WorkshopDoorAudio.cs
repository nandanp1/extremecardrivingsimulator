using UnityEngine;

public class WorkshopDoorAudio : MonoBehaviour
{
	public AudioClip audioClip;

	private bool inited;

	private WorkshopDoorLogic workshopLogic;

	private void PlayOpen()
	{
		Init();
		if (workshopLogic.GetStatusBoolOpen())
		{
			PlayAudio();
		}
	}

	private void PlayClose()
	{
		Init();
		if (!workshopLogic.GetStatusBoolOpen())
		{
			PlayAudio();
		}
	}

	private void Init()
	{
		if (!inited)
		{
			base.gameObject.AddComponent<AudioSource>();
			GetComponent<AudioSource>().playOnAwake = false;
			GetComponent<AudioSource>().clip = audioClip;
			if (!workshopLogic)
			{
				workshopLogic = base.transform.parent.GetComponent<WorkshopDoorLogic>();
			}
			inited = true;
		}
	}

	private void PlayAudio()
	{
		GetComponent<AudioSource>().Play();
	}
}
