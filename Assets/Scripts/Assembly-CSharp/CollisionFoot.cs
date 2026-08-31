using UnityEngine;

public class CollisionFoot : MonoBehaviour
{
	[HideInInspector]
	public GameObject effect;

	private NormalCharacterMotor playerScript;

	private bool activeStep;

	private void Start()
	{
		playerScript = base.transform.root.GetComponent<NormalCharacterMotor>();
	}

	private void Update()
	{
		Vector3 direction = base.transform.TransformDirection(Vector3.forward);
		RaycastHit hitInfo;
		if (Physics.Raycast(base.transform.position, direction, out hitInfo, 0.05f))
		{
			for (int i = 0; i < playerScript.stepSounds.Length; i++)
			{
				if (hitInfo.collider.CompareTag(playerScript.stepSounds[i].tag) && !activeStep)
				{
					playerScript.step1c = playerScript.stepSounds[i].step1;
					playerScript.step2c = playerScript.stepSounds[i].step2;
					base.transform.root.SendMessage("OnFootLift", SendMessageOptions.DontRequireReceiver);
					activeStep = true;
				}
			}
		}
		else
		{
			activeStep = false;
		}
	}
}
