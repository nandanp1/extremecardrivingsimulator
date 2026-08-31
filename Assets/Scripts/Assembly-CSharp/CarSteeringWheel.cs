using UnityEngine;

public class CarSteeringWheel : MonoBehaviour
{
	public new Camera camera;

	public float wheelAngleMax = 500f;

	public float speedRotateBack = 1000f;

	public float wheelAngleScaledToOne;

	private UIWidget uiWidget;

	private Vector3 wheelCenter;

	private float wheelAngle;

	private float wheelOldAngle;

	private bool isReset;

	private bool isPressed;

	private void Start()
	{
		uiWidget = GetComponent<UIWidget>();
	}

	private void Update()
	{
		if (!isPressed && !isReset)
		{
			resetWheel();
		}
	}

	private void OnPress(bool isPressed)
	{
		wheelCenter = camera.WorldToScreenPoint(uiWidget.worldCenter);
		log("Widget Center: " + wheelCenter.ToString());
		this.isPressed = isPressed;
		wheelOldAngle = Vector2.Angle(Vector2.up, new Vector3(UICamera.lastTouchPosition.x - wheelCenter.x, UICamera.lastTouchPosition.y - wheelCenter.y, 0f));
		isReset = false;
	}

	private void OnDrag(Vector2 delta)
	{
		if (isPressed)
		{
			doSteeringCalculations();
			doSteeringWidgetRotation();
		}
	}

	private void doSteeringCalculations()
	{
		float num = Vector2.Angle(Vector2.up, new Vector3(UICamera.lastTouchPosition.x - wheelCenter.x, UICamera.lastTouchPosition.y - wheelCenter.y, 0f));
		if (Vector2.Distance(new Vector2(UICamera.lastTouchPosition.x, UICamera.lastTouchPosition.y), new Vector2(wheelCenter.x, wheelCenter.y)) > 10f)
		{
			if (UICamera.lastTouchPosition.x > wheelCenter.x)
			{
				wheelAngle -= num - wheelOldAngle;
			}
			else
			{
				wheelAngle += num - wheelOldAngle;
			}
		}
		if (wheelAngle > wheelAngleMax)
		{
			wheelAngle = wheelAngleMax;
		}
		else if (wheelAngle < 0f - wheelAngleMax)
		{
			wheelAngle = 0f - wheelAngleMax;
		}
		wheelOldAngle = num;
		log("Wheel Angle: " + wheelAngle);
	}

	private void doSteeringWidgetRotation()
	{
		base.transform.rotation = Quaternion.Euler(0f, 0f, wheelAngle);
		if (wheelAngle != 0f)
		{
			wheelAngleScaledToOne = (0f - wheelAngle) / wheelAngleMax;
		}
		else
		{
			wheelAngleScaledToOne = 0f;
		}
		log("Wheel Angle Scale To One: " + wheelAngleScaledToOne);
	}

	private void resetWheel()
	{
		if (!Mathf.Approximately(0f, wheelAngle))
		{
			log("Resetting wheel angle: " + wheelAngle);
			doSteeringWidgetRotation();
			float num = speedRotateBack * Time.deltaTime;
			if (Mathf.Abs(num) > Mathf.Abs(wheelAngle))
			{
				wheelAngle = 0f;
			}
			else if (wheelAngle > 0f)
			{
				wheelAngle -= num;
			}
			else
			{
				wheelAngle += num;
			}
		}
		else
		{
			wheelAngle = 0f;
			wheelAngleScaledToOne = 0f;
			doSteeringWidgetRotation();
			isReset = true;
		}
	}

	public void reset()
	{
		wheelAngleScaledToOne = 0f;
		wheelAngle = 0f;
		wheelOldAngle = 0f;
		isReset = false;
		isPressed = false;
	}

	private void log(string info)
	{
	}
}
