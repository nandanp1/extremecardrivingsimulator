using System;
using UnityEngine;

public class CharacterInCar : MonoBehaviour
{
	public enum InCarMode
	{
		noCar = 0,
		openDoor = 1,
		inCar = 2,
		sit = 3,
		outCar = 4,
		closeDoor = 5
	}

	[Serializable]
	public class CarSound
	{
		public AudioClip OpenDoor;

		public AudioClip CloseDoor;

		public AudioClip music;
	}

	public LayerMask layerMask;

	public Transform myCamera;

	public Animator player;

	public InCarMode inCarMode;

	public CarSound carSounds;

	private Transform CurrentCar;

	private CarComponent CC;

	private bool RideCar;

	private Vector3 Pos;

	private Quaternion Rot;

	private bool inCar;

	private CharacterController controller;

	private static int openLeftCarState = Animator.StringToHash("Base.OpenLeftCar");

	private static int inLeftCarState = Animator.StringToHash("Base.InLeftCar");

	private static int idleInCarState = Animator.StringToHash("Base.IdleInCar");

	private static int outLeftCarState = Animator.StringToHash("Base.OutLeftCar");

	private static int closeLeftCarState = Animator.StringToHash("Base.CloseLeftCar");

	private float LeftCarDoorAngle;

	private void Start()
	{
		controller = base.transform.GetComponent<CharacterController>();
	}

	private void Update()
	{
		if (!RideCar)
		{
			Animator animator = player;
			Vector3 vector = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
			animator.SetFloat("PlayerSpeed", vector.magnitude / 4f);
			Collider[] array = Physics.OverlapSphere(base.transform.position, 0.5f, layerMask);
			Collider[] array2 = array;
			foreach (Collider collider in array2)
			{
				if (collider.transform.root.tag == "car" && Vector3.Distance(base.transform.position, collider.transform.position) < 1f && Input.GetKeyDown(KeyCode.F))
				{
					player.SetBool("InCar", true);
					CurrentCar = collider.transform.root;
					myCamera.GetComponent<VehicleCamera>().target = CurrentCar;
					CC = CurrentCar.GetComponent<CarComponent>();
					base.transform.GetComponent<CharacterController>().enabled = false;
					CurrentCar.GetComponent<VehicleGUI>().enabled = false;
					base.transform.GetComponent<NormalCharacterMotor>().enabled = false;
					base.transform.parent = CurrentCar;
					inCar = true;
					RideCar = true;
				}
			}
		}
		else
		{
			player.SetFloat("PlayerSpeed", 0f);
			InCar(CurrentCar);
		}
		if (!CurrentCar || !CurrentCar.GetComponent<CarComponent>())
		{
			return;
		}
		if (CC.CarComponents.LeftDoor.localEulerAngles.y > 1f && CC.CarComponents.LeftDoor.localEulerAngles.y < 10f)
		{
			if (LeftCarDoorAngle == 0f)
			{
				CC.CarComponents.LeftDHandle.GetComponent<AudioSource>().clip = carSounds.CloseDoor;
				CC.CarComponents.LeftDHandle.GetComponent<AudioSource>().Play();
			}
			else if (LeftCarDoorAngle == 60f)
			{
				CC.CarComponents.LeftDHandle.GetComponent<AudioSource>().clip = carSounds.OpenDoor;
				CC.CarComponents.LeftDHandle.GetComponent<AudioSource>().Play();
			}
		}
		CC.CarComponents.LeftDoor.localRotation = Quaternion.RotateTowards(CC.CarComponents.LeftDoor.localRotation, Quaternion.Euler(0f, LeftCarDoorAngle, 0f), Time.deltaTime * 250f);
	}

	private void InCar(Transform Car)
	{
		if (!RideCar)
		{
			return;
		}
		AnimatorStateInfo currentAnimatorStateInfo = player.GetCurrentAnimatorStateInfo(0);
		if (inCar)
		{
			if (currentAnimatorStateInfo.fullPathHash == openLeftCarState)
			{
				inCarMode = InCarMode.openDoor;
			}
			else if (currentAnimatorStateInfo.fullPathHash == inLeftCarState)
			{
				inCarMode = InCarMode.inCar;
			}
			if (currentAnimatorStateInfo.fullPathHash == idleInCarState)
			{
				inCarMode = InCarMode.sit;
				myCamera.GetComponent<MainCamera>().enabled = false;
				myCamera.GetComponent<VehicleCamera>().enabled = true;
				CC.CarComponents.LeftDHandle.GetComponent<AudioSource>().clip = carSounds.music;
				CC.CarComponents.LeftDHandle.GetComponent<AudioSource>().loop = true;
				if (!CC.CarComponents.LeftDHandle.GetComponent<AudioSource>().isPlaying)
				{
					CC.CarComponents.LeftDHandle.GetComponent<AudioSource>().Play();
				}
				CurrentCar.GetComponent<VehicleGUI>().enabled = true;
				CurrentCar.GetComponent<CarController>().enabled = true;
				CurrentCar.GetComponent<CarController>().carMode = CarMode.Player;
				CurrentCar.GetComponent<CarController>().torque = 50f;
				if (Input.GetKeyDown(KeyCode.F))
				{
					player.SetBool("InCar", false);
					player.SetBool("OutCar", false);
					inCar = false;
				}
			}
		}
		else if (currentAnimatorStateInfo.fullPathHash == outLeftCarState)
		{
			inCarMode = InCarMode.outCar;
			myCamera.GetComponent<MainCamera>().enabled = true;
			myCamera.GetComponent<MainCamera>().y = 15f;
			myCamera.GetComponent<MainCamera>().x = Car.eulerAngles.y + 90f;
			CC.CarComponents.LeftDHandle.GetComponent<AudioSource>().loop = false;
			myCamera.GetComponent<VehicleCamera>().enabled = false;
			CurrentCar.GetComponent<CarController>().carMode = CarMode.Empty;
			CurrentCar.GetComponent<VehicleGUI>().enabled = false;
			CurrentCar.GetComponent<CarController>().torque = 0f;
		}
		else if (currentAnimatorStateInfo.fullPathHash == closeLeftCarState)
		{
			inCarMode = InCarMode.closeDoor;
			player.SetBool("OutCar", true);
			base.transform.GetComponent<CharacterController>().enabled = true;
			base.transform.GetComponent<NormalCharacterMotor>().enabled = true;
			CurrentCar.GetComponent<VehicleGUI>().enabled = false;
			base.transform.eulerAngles = new Vector3(0f, base.transform.eulerAngles.y, 0f);
			base.transform.parent = null;
			RideCar = false;
		}
		switch (inCarMode)
		{
		case InCarMode.noCar:
			Pos = CC.CarComponents.LeftDHandle.localPosition;
			Rot = CC.CarComponents.LeftDHandle.localRotation;
			break;
		case InCarMode.openDoor:
			Pos = CC.CarComponents.LeftDHandle.localPosition;
			Rot = CC.CarComponents.LeftDHandle.localRotation;
			if (currentAnimatorStateInfo.normalizedTime > 0.4f)
			{
				LeftCarDoorAngle = 60f;
			}
			break;
		case InCarMode.inCar:
			Pos = CC.CarComponents.LeftChair.localPosition;
			Rot = CC.CarComponents.LeftChair.localRotation;
			if (currentAnimatorStateInfo.normalizedTime > 0.95f)
			{
				LeftCarDoorAngle = 0f;
			}
			break;
		case InCarMode.sit:
			Pos = CC.CarComponents.LeftChair.localPosition;
			Rot = CC.CarComponents.LeftChair.localRotation;
			break;
		case InCarMode.outCar:
			if (currentAnimatorStateInfo.normalizedTime > 0.4f)
			{
				Pos = CC.CarComponents.LeftDHandle.localPosition;
				Rot = CC.CarComponents.LeftDHandle.localRotation;
			}
			LeftCarDoorAngle = 60f;
			break;
		case InCarMode.closeDoor:
			Pos = CC.CarComponents.LeftDHandle.localPosition;
			Rot = CC.CarComponents.LeftDHandle.localRotation;
			LeftCarDoorAngle = 0f;
			break;
		}
		if (base.transform.parent != null)
		{
			base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, Pos, 10f * Time.deltaTime);
			base.transform.localRotation = Quaternion.Lerp(base.transform.localRotation, Rot, 15f * Time.deltaTime);
		}
	}
}
