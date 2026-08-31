using UnityEngine;

public class CarMain : MonoBehaviour
{
	public CarControl[] Cars;

	public int startupCar;

	public CameraControl MainCamera;

	public Light MainLight;

	public bool reverseRequiresStop = true;

	public Rigidbody Payload;

	public float slowTime = 0.25f;

	public bool showHelp;

	public bool guiEnabled = true;

	public GUIStyle BigTextStyle;

	public GUIStyle TextStyle;

	public GUIStyle BoxStyle;

	public GUISkin guiSkin;

	private int m_currentCar;

	private CarControl m_Car;

	private CarTelemetry m_CarTelemetry;

	private CarSettings m_CarSettings;

	private CarCameras m_CarCameras;

	private CarSound m_CarSound;

	private bool m_bBrakeRelease;

	private static float GEARSPEEDMIN = 0.2f;

	private GameLogic logic;

	private static GameObject[] breakLights;

	public CarControl getSelectedCar()
	{
		return Cars[m_currentCar];
	}

	private void Awake()
	{
		breakLights = GameObject.FindGameObjectsWithTag("BreakLight");
	}

	private void Start()
	{
		logic = Object.FindFirstObjectByType<GameLogic>();
		m_CarTelemetry = GetComponent<CarTelemetry>();
		m_currentCar = startupCar;
		if (m_currentCar >= Cars.Length)
		{
			m_currentCar = Cars.Length - 1;
		}
		for (int i = 0; i < Cars.Length; i++)
		{
			DisableCar(Cars[i]);
		}
		SelectCar(Cars[m_currentCar]);
	}

	private void Update()
	{
		if (!m_CarSettings.externalInput)
		{
			m_bBrakeRelease = SendInput(m_Car, reverseRequiresStop, m_bBrakeRelease, logic);
		}
		bool flag = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
		if (Input.GetKeyDown(KeyCode.P))
		{
			Debug.Break();
		}
		if (Input.GetKeyDown(KeyCode.T))
		{
			if (Time.timeScale < 1f)
			{
				Time.timeScale = 1f;
			}
			else
			{
				Time.timeScale = slowTime;
			}
		}
		if (Input.GetKeyDown(KeyCode.E) && (bool)Payload)
		{
			Payload.GetComponent<Rigidbody>().AddForce(new Vector3(0f, 6f, 0f), ForceMode.VelocityChange);
		}
		if (Input.GetKeyDown(KeyCode.R) && !flag)
		{
			LegacySceneManager.LoadLevel(0);
		}
		if (Input.GetKeyDown(KeyCode.PageUp))
		{
			SwitchCar(-1);
		}
		if (Input.GetKeyDown(KeyCode.PageDown))
		{
			SwitchCar(1);
		}
		if (Input.GetKeyDown(KeyCode.Return))
		{
			m_Car.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
			m_Car.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			m_Car.transform.localEulerAngles = new Vector3(0f, m_Car.transform.localEulerAngles.y, 0f);
			m_Car.transform.position += new Vector3(0f, 1.6f, 0f);
		}
		if (Input.GetKeyDown(KeyCode.R) && flag)
		{
			CarDamage component = m_Car.GetComponent<CarDamage>();
			if ((bool)component)
			{
				component.DoBounce();
			}
		}
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			m_CarSettings.abs = !m_CarSettings.abs;
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			m_CarSettings.tc = !m_CarSettings.tc;
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			m_CarSettings.esp = !m_CarSettings.esp;
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			m_CarSettings.tractionAxle++;
		}
		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			m_CarSettings.stabilizerMode++;
		}
		if (Input.GetKeyDown(KeyCode.I))
		{
			m_CarSettings.externalInput = !m_CarSettings.externalInput;
		}
		if (Input.GetKeyDown(KeyCode.C))
		{
			MainCamera.Next();
		}
		if (Input.GetKeyDown(KeyCode.F1))
		{
			MainCamera.SwitchTo(0);
		}
		if (Input.GetKeyDown(KeyCode.F2))
		{
			MainCamera.SwitchTo(1);
		}
		if (Input.GetKeyDown(KeyCode.F3))
		{
			MainCamera.SwitchTo(2);
		}
		if (Input.GetKeyDown(KeyCode.F4))
		{
			MainCamera.SwitchTo(3);
		}
		if (Input.GetKeyDown(KeyCode.F5))
		{
			MainCamera.SwitchTo(4);
		}
		if (Input.GetKeyDown(KeyCode.F6))
		{
			MainCamera.SwitchTo(5);
		}
		if (Input.GetKeyDown(KeyCode.M))
		{
			MainCamera.ToggleMap();
		}
		if (Input.GetKeyDown(KeyCode.V))
		{
			m_CarCameras.Next();
		}
		if (Input.GetKeyDown(KeyCode.N))
		{
			MainCamera.showMirrors = !MainCamera.showMirrors;
		}
		if ((bool)m_CarTelemetry && Input.GetKeyDown(KeyCode.B))
		{
			if (!flag)
			{
				m_CarTelemetry.Enabled = !m_CarTelemetry.Enabled;
			}
			else
			{
				m_CarTelemetry.Curves = !m_CarTelemetry.Curves;
			}
		}
		if (Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.Escape))
		{
			showHelp = !showHelp;
		}
		if (Input.GetKeyDown(KeyCode.F12))
		{
			if (MainLight.GetComponent<Light>().shadows != LightShadows.None)
			{
				MainLight.GetComponent<Light>().shadows = LightShadows.None;
				MainCamera.enableImageEffects = false;
			}
			else
			{
				MainLight.GetComponent<Light>().shadows = LightShadows.Soft;
				MainCamera.enableImageEffects = true;
			}
		}
	}

	private void SwitchCar(int iDir)
	{
		if (Cars.Length >= 2)
		{
			DisableCar(Cars[m_currentCar]);
			m_currentCar += (int)Mathf.Sign(iDir);
			if (m_currentCar < 0)
			{
				m_currentCar = Cars.Length - 1;
			}
			else if (m_currentCar >= Cars.Length)
			{
				m_currentCar = 0;
			}
			SelectCar(Cars[m_currentCar]);
		}
	}

	private void DisableCar(CarControl Car)
	{
		CarCameras component = Car.GetComponent<CarCameras>();
		component.showFixedCams = false;
		Car.motorInput = 0f;
		Car.brakeInput = 1f;
		CarSettings component2 = Car.GetComponent<CarSettings>();
		if ((bool)component2)
		{
			component2.bypassABS = true;
		}
		Car.readUserInput = false;
	}

	private void SelectCar(CarControl Car)
	{
		if ((bool)m_Car)
		{
			MainCamera.Target2 = m_CarCameras.CameraLookAtPoint;
		}
		m_Car = Car;
		m_CarSettings = m_Car.GetComponent<CarSettings>();
		m_CarCameras = m_Car.GetComponent<CarCameras>();
		m_CarSound = m_Car.GetComponent<CarSound>();
		MainCamera.Target = m_Car.transform;
		m_CarCameras.showFixedCams = true;
		m_CarSettings.bypassABS = false;
		if ((bool)m_CarTelemetry)
		{
			m_CarTelemetry.Target = Car;
		}
	}

	public static bool SendInput(CarControl Car, bool reverseRequiresStop, bool brakeRelease, GameLogic gameLogic)
	{
		float steerInput = 0f;
		if (MySingleton<Manager>.Instance.controls == Manager.Controls.Wheel)
		{
			steerInput = gameLogic.steer.wheelAngleScaledToOne;
		}
		else if (MySingleton<Manager>.Instance.controls == Manager.Controls.Accelerometer)
		{
			steerInput = Mathf.Clamp(Input.acceleration.x * 1.3f, -1f, 1f);
		}
		else if (MySingleton<Manager>.Instance.controls == Manager.Controls.Buttons)
		{
			steerInput = gameLogic.dirLeft + gameLogic.dirRight;
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		num = gameLogic.gas;
		num2 = gameLogic.brakes * -1f;
		num3 = gameLogic.handbreak;
		float num4 = Vector3.Dot(Car.GetComponent<Rigidbody>().linearVelocity, Car.transform.forward);
		float num5 = Vector3.Dot(Car.GetComponent<Rigidbody>().linearVelocity, Car.transform.right);
		float magnitude = Car.GetComponent<Rigidbody>().angularVelocity.magnitude;
		float num6 = num4 * num4;
		num5 *= num5;
		float motorInput = 0f;
		float num7 = 0f;
		if (reverseRequiresStop)
		{
			if (num6 < GEARSPEEDMIN && num == 0f && num2 == 0f)
			{
				brakeRelease = true;
				Car.gearInput = 0;
			}
			if (brakeRelease)
			{
				if (num6 < GEARSPEEDMIN)
				{
					if (num2 > 0f)
					{
						Car.gearInput = -1;
					}
					if (num > 0f)
					{
						Car.gearInput = 1;
					}
				}
				else if (num5 < GEARSPEEDMIN && magnitude < GEARSPEEDMIN)
				{
					if (num4 > 0f && Car.gearInput <= 0 && (num > 0f || num2 > 0f))
					{
						Car.gearInput = 1;
					}
					if (num4 < 0f && Car.gearInput >= 0 && (num > 0f || num2 > 0f))
					{
						Car.gearInput = -1;
					}
				}
			}
			if (Car.gearInput < 0)
			{
				motorInput = num2;
				num7 = num;
			}
			else
			{
				motorInput = num;
				num7 = num2;
			}
			if (num7 > 0f)
			{
				brakeRelease = false;
			}
		}
		else
		{
			if (num4 > GEARSPEEDMIN)
			{
				Car.gearInput = 1;
				motorInput = num;
				num7 = num2;
			}
			else if (num4 <= GEARSPEEDMIN && num2 > GEARSPEEDMIN)
			{
				Car.gearInput = -1;
				motorInput = num2;
				num7 = 0f;
			}
			else if (num > GEARSPEEDMIN && num2 <= 0f)
			{
				Car.gearInput = 1;
				motorInput = num;
				num7 = 0f;
			}
			else if (num > GEARSPEEDMIN)
			{
				Car.gearInput = 1;
			}
			else if (num2 > GEARSPEEDMIN)
			{
				Car.gearInput = -1;
			}
			else
			{
				Car.gearInput = 0;
			}
			brakeRelease = false;
		}
		Car.steerInput = steerInput;
		Car.motorInput = motorInput;
		Car.brakeInput = num7;
		Car.handbrakeInput = num3;
		return brakeRelease;
	}

	private Rect ScrRect(float x, float y, float wd, float ht)
	{
		return new Rect(x * (float)Screen.width, y * (float)Screen.height, wd * (float)Screen.width, ht * (float)Screen.height);
	}

	private void GUIBoxedText(Rect pos, string text, GUIStyle textStyle, Color textColor)
	{
		GUI.Box(pos, string.Empty);
		Color textColor2 = textStyle.normal.textColor;
		textStyle.normal.textColor = textColor;
		GUI.Label(pos, text, textStyle);
		textStyle.normal.textColor = textColor2;
	}

	private void OnGUI()
	{
		if (!guiEnabled)
		{
			return;
		}
		GUI.skin = guiSkin;
		CarControl selectedCar = getSelectedCar();
		Rect rect = ScrRect(0.25f, 0.89f, 0.35f, 0.2f);
		if (m_CarSettings.HasExternalInput())
		{
			m_CarSettings.externalInput = GUI.Toggle(new Rect(rect.x + 160f, rect.y + 2f, 50f, 18f), m_CarSettings.externalInput, "AI", guiSkin.button);
		}
		if (!GUI.Toggle(new Rect(rect.x + 220f, rect.y + 2f, 50f, 18f), true, "car >", guiSkin.button) || GUI.Button(new Rect(rect.x + 280f, rect.y, 400f, 20f), m_CarSettings.description, "helptext"))
		{
			SwitchCar(1);
		}
		float magnitude = new Vector2(Vector3.Dot(selectedCar.GetComponent<Rigidbody>().linearVelocity, selectedCar.transform.forward), Vector3.Dot(selectedCar.GetComponent<Rigidbody>().linearVelocity, selectedCar.transform.right)).magnitude;
		GUI.Label(new Rect(rect.x, rect.y, 70f, 40f), string.Format("{0:0.}", magnitude * 3.6f), BigTextStyle);
		int num;
		string arg;
		if ((bool)m_CarSound)
		{
			num = m_CarSound.currentGear;
			arg = ((num > 0) ? num.ToString() : ((num >= 0) ? "N" : "R"));
		}
		else
		{
			num = selectedCar.gearInput;
			arg = selectedCar.getGear();
		}
		GUI.Label(new Rect(rect.x, rect.y + 42f, 70f, 40f), string.Format("{0:0.} mph", (double)magnitude * 2.237), TextStyle);
		GUIBoxedText(new Rect(rect.x + 80f, rect.y + 6f, 55f, 50f), string.Format("{0}", arg), BoxStyle, (num < 0) ? Color.white : ((num <= 0) ? new Color(0f, 0.5f, 0f) : new Color(0.6f, 0.6f, 0.6f)));
		if (m_CarSettings.hasABS)
		{
			m_CarSettings.abs = GUI.Toggle(new Rect(rect.x + 160f, rect.y + 26f, 50f, 30f), m_CarSettings.abs, "ABS", guiSkin.button);
		}
		if (m_CarSettings.hasTC)
		{
			m_CarSettings.tc = GUI.Toggle(new Rect(rect.x + 220f, rect.y + 26f, 50f, 30f), m_CarSettings.tc, "TC", guiSkin.button);
		}
		if (m_CarSettings.hasESP)
		{
			m_CarSettings.esp = GUI.Toggle(new Rect(rect.x + 280f, rect.y + 26f, 50f, 30f), m_CarSettings.esp, "ESP", guiSkin.button);
		}
		bool flag = m_CarSettings.hasTractionModes > 0;
		if (flag && GUI.Toggle(new Rect(rect.x + 340f, rect.y + 26f, 50f, 30f), flag, m_CarSettings.getTractionAxleStr(), guiSkin.button) != flag)
		{
			m_CarSettings.tractionAxle++;
		}
		flag = m_CarSettings.hasVariableStabilizer;
		if (flag && GUI.Toggle(new Rect(rect.x + 400f, rect.y + 26f, 60f, 30f), flag, m_CarSettings.getStabilizerModeShortStr(), guiSkin.button) != flag)
		{
			m_CarSettings.stabilizerMode++;
		}
		showHelp = GUI.Toggle(new Rect(rect.x + 470f, rect.y + 26f, 60f, 30f), showHelp, "H Help", "helptext");
		rect.y -= 350f;
		rect.x += 20f;
		rect.width = 520f;
		rect.height = 270f;
		if (showHelp)
		{
			if (GUI.Button(rect, "Offroader v4 / 2011.11.02 - by Edy", guiSkin.box))
			{
				showHelp = false;
			}
			rect.x += 10f;
			rect.y += 35f;
			string text = "Arrows / WSAD - Accelerate, brake, turn\nSpace - handbrake\nReturn - unflip car\nshift+R - repair damage\n\n12345 - ABS, TC, ESP, 4x4, Stabilization\nI - Enable AI (pseudo-random)\n\nPgUp / PgDown - select vehicle\nE - Eject payload\nR - Restart demo\nT - Slow motion\n\n";
			string text2 = "C - camera (also F1-F6)\nV - secondary camera\nM - minimap\nN - mirrors (bus driver only)\nB - telemetry\nshift+B - tire telemetry\n\nmouse - move view in cameras F1,F3,F5\nmouseWheel - zoom in F4,F5, dist in F3\nNumPad - move camera position in F4,F5\n\nF12 - toggle low quality mode\n";
			string text3 = "Switching gears between (D) and reverse (R) requires the car to completely stop, release accelerate and brake keys, then press accelerate (forward) or brake (reverse).";
			GUI.Label(rect, text);
			Rect position = rect;
			position.x += 265f;
			GUI.Label(position, text2);
			position = rect;
			position.y += 190f;
			position.width -= 10f;
			GUI.Label(position, text3);
		}
	}
}
