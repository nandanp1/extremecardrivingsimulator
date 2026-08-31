using System;
using System.Collections;
using AxesInMotion.UnityHelpers;
using UnityEngine;

public class FacebookScreenshotMode : MonoBehaviour
{
	private enum FacebookScreenshotState
	{
		Disable = 0,
		InitializingScreenshotMode = 1,
		EnteringScreenshotMode = 2,
		PreparingScreenshot = 3,
		SendingScreenshot = 4,
		LeavingScreenshotMode = 5,
		FinishingScreenshotMode = 6
	}

	private enum SwipeDirectionIndex
	{
		Up = 0,
		Right = 1,
		Down = 2,
		Left = 3
	}

	public delegate void FacebookScreenshotCallback();

	[Tooltip("Cámara de la escena que se desee utilizar en el modo screenshot")]
	public new Camera camera;

	[Tooltip("Animator del pivot de la escena. Solo se usa para (des/)habilitarlo.")]
	public Animator animator;

	[Tooltip("Slider de NGUI par hacer el roll de la cámara. Solo se usa para establecer el valor roll inicial de la cámara.")]
	public UISlider slRoll;

	[Tooltip("Luz de la escena que sincronizamos con el movimiento de la cámara.")]
	public new Light light;

	[Tooltip("Textura del panel preview para mostrar el screenshot antes de enviar.")]
	public UITexture texScreenshot;

	[Tooltip("Velocidad lineal al entrar/salir del modo screenshot")]
	public float speed = 5f;

	[Tooltip("Velocidad angular al entrar/salir del modo screenshot")]
	public float angularSpeed = 5f;

	[Tooltip("Diferencia lineal mínima entre la posición actual y la deseada. Se usa para determinar el fin de la translación.")]
	public float distanceThreshold = 0.1f;

	[Tooltip("Diferencia angular mínima entre la rotación actual y la deseada. Se usa para determinar el fin de la rotación.")]
	public float angularDistanceThreshold = 0.1f;

	[Tooltip("Diferencia lineal mínima entre el ángulo actual y el deseada. Se usa para determinar el fin del cambio de FOV.")]
	public float zoomThreshold = 0.1f;

	[Tooltip("Velocidad de rotación")]
	public float rotationSpeed = 100f;

	[Tooltip("Velocidad de zoom")]
	public float zoomSpeed = 10f;

	[Tooltip("Ángulo mínimo de rotación sobre el eje Y del coche.")]
	public float yawMin = -180f;

	[Tooltip("Ángulo máximo de rotación sobre el eje Y del coche.")]
	public float yawMax = 180f;

	[Tooltip("Ángulo mínimo de rotación sobre el eje X del coche.")]
	public float pitchMin = 5f;

	[Tooltip("Ángulo máximo de rotación sobre el eje X del coche.")]
	public float pitchMax = 45f;

	[Tooltip("Ángulo mínimo de rotación sobre el eje Z de la cámara.")]
	public float rollMin = -30f;

	[Tooltip("Ángulo máximo de rotación sobre el eje Z de la cámara.")]
	public float rollMax = 30f;

	[Tooltip("Ángulo mínimo de FOV para el zoom")]
	public float zoomMin = 20f;

	[Tooltip("Ángulo máximo de FOV para el zoom")]
	public float zoomMax = 60f;

	[Tooltip("(Des)Habilita limitar la rotación en eje x")]
	public bool clampPitch = true;

	[Tooltip("(Des)Habilita limitar la rotación en eje y")]
	public bool clampYaw;

	[Tooltip("Componente Antialiasing de cámara, se espera que este componente esté en la cámara principal.")]
	public AntialiasingAsPostEffect antialiasingAsPostEffect;

	[Tooltip("Componente FastBloom de cámara, se espera que este componente esté en la cámara principal.")]
	public FastBloom fastBloom;

	[Tooltip("Timeout de espera del envío de screenshot")]
	public float timeout = 50f;

	[HideInInspector]
	public Vector3 groundLimit = Vector3.zero;

	private float _roll;

	private float _zoom;

	private Transform _target;

	private Vector3 _lastPosition;

	private Quaternion _lastRotation;

	private float _defaultFOV;

	private Quaternion _defaultLightRotation;

	private Vector3 _toPosition;

	private Quaternion _toRotation;

	private Vector3 _auxPosition;

	private Quaternion _auxRotation;

	private float _deltaPitchAngle;

	private float _deltaYawAngle;

	private float _angularDistance;

	private float _distance;

	private bool _firstTwoFingerDistance = true;

	private float _lastTwoFingerDistance;

	private float _deltaTwoFingerDistance;

	private float _sign = 1f;

	private bool _twisting;

	private bool[] _swipeDirections = new bool[4];

	private Vector3 _relativeToCenter;

	private float _magnitude;

	private float _twoFingerDistance;

	private FacebookScreenshotState _state;

	private FacebookScreenshotCallback _onInitialized;

	private FacebookScreenshotCallback _onTakenScreenshot;

	private FacebookScreenshotCallback _onFinished;

	private FacebookScreenshotCallback _onSending;

	private FacebookScreenshotCallback _onSent;

	private FacebookScreenshotCallback _onFailed;

	private FacebookScreenshotCallback _onTimedout;

	private FacebookScreenshotCallback _onBeforePerformCameraRotation;

	private float _timeToDie;

	private Texture2D _screenshot;

	private float _xLength;

	private float _yLength;

	private string _message;

	private bool _useScaledTime;

	private Vector3 _lastCameraPosition;

	private Quaternion _lastCameraRotation;

	private bool _isLoggiging;

	private void OnEnable()
	{
		EasyTouch.On_Swipe += OnSwipe;
		EasyTouch.On_Twist += OnTwist;
		EasyTouch.On_TwistEnd += OnTwistEnd;
		EasyTouch.On_Cancel += OnCancel;
	}

	private void OnDisable()
	{
		EasyTouch.On_Swipe -= OnSwipe;
		EasyTouch.On_Twist -= OnTwist;
		EasyTouch.On_TwistEnd -= OnTwistEnd;
		EasyTouch.On_Cancel -= OnCancel;
	}

	private void OnDestroy()
	{
		EasyTouch.On_Swipe -= OnSwipe;
		EasyTouch.On_Twist -= OnTwist;
		EasyTouch.On_TwistEnd -= OnTwistEnd;
		EasyTouch.On_Cancel -= OnCancel;
	}

	private void Update()
	{
		while (true)
		{
			switch (_state)
			{
			case FacebookScreenshotState.Disable:
				return;
			case FacebookScreenshotState.InitializingScreenshotMode:
				InitializingScreenshotMode();
				break;
			case FacebookScreenshotState.EnteringScreenshotMode:
				if (EnteringScreenshotMode())
				{
					break;
				}
				return;
			case FacebookScreenshotState.PreparingScreenshot:
				PerformCameraRotation(GetDeltaTime());
				return;
			case FacebookScreenshotState.SendingScreenshot:
				if (_timeToDie == 0f && !_isLoggiging)
				{
					_timeToDie = GetTime() + timeout;
				}
				if (_timeToDie > 0f && GetTime() > _timeToDie)
				{
					TakeScreenshotTimedoutCallback();
				}
				return;
			case FacebookScreenshotState.LeavingScreenshotMode:
				if (LeavingScreenshotMode())
				{
					break;
				}
				return;
			case FacebookScreenshotState.FinishingScreenshotMode:
				FinishingScreenshotMode();
				return;
			default:
				throw new NotImplementedException();
			}
		}
	}

	private void InitializingScreenshotMode()
	{
		_state = FacebookScreenshotState.Disable;
		_defaultFOV = camera.fieldOfView;
		if (light != null)
		{
			_defaultLightRotation = light.transform.rotation;
		}
		_useScaledTime = false;
		if (animator != null)
		{
			animator.enabled = false;
		}
		_lastPosition = camera.transform.position;
		_lastRotation = camera.transform.rotation;
		_auxPosition = camera.transform.position;
		_auxRotation = camera.transform.rotation;
		camera.transform.LookAt(_target.position);
		_toPosition = camera.transform.position;
		_toRotation = camera.transform.rotation;
		SetRollFromCamera();
		_zoom = camera.fieldOfView;
		camera.transform.position = _auxPosition;
		camera.transform.rotation = _auxRotation;
		_state = FacebookScreenshotState.EnteringScreenshotMode;
		_isLoggiging = false;
		_timeToDie = 0f;
	}

	private bool EnteringScreenshotMode()
	{
		_distance = Vector3.Distance(_toPosition, camera.transform.position);
		_angularDistance = Quaternion.Angle(_toRotation, camera.transform.rotation);
		camera.transform.position = Vector3.MoveTowards(camera.transform.position, _toPosition, _distance * speed * GetDeltaTime());
		camera.transform.rotation = Quaternion.RotateTowards(camera.transform.rotation, _toRotation, _angularDistance * angularSpeed * GetDeltaTime());
		if (_distance < distanceThreshold && _angularDistance < angularDistanceThreshold)
		{
			camera.transform.position = _toPosition;
			camera.transform.rotation = _toRotation;
			EnableTouch();
			_onInitialized();
			return true;
		}
		return false;
	}

	private bool LeavingScreenshotMode()
	{
		_distance = Vector3.Distance(_lastPosition, camera.transform.position);
		_angularDistance = Quaternion.Angle(_lastRotation, camera.transform.rotation);
		camera.transform.position = Vector3.MoveTowards(camera.transform.position, _lastPosition, _distance * speed * GetDeltaTime());
		camera.transform.rotation = Quaternion.RotateTowards(camera.transform.rotation, _lastRotation, _angularDistance * angularSpeed * GetDeltaTime());
		if (camera.fieldOfView > _defaultFOV)
		{
			camera.fieldOfView += (0f - speed) * 5f * GetDeltaTime();
			if (camera.fieldOfView < _defaultFOV)
			{
				camera.fieldOfView = _defaultFOV;
			}
		}
		else if (camera.fieldOfView < _defaultFOV)
		{
			camera.fieldOfView += speed * 5f * GetDeltaTime();
			if (camera.fieldOfView > _defaultFOV)
			{
				camera.fieldOfView = _defaultFOV;
			}
		}
		if (_distance < distanceThreshold && _angularDistance < angularDistanceThreshold)
		{
			camera.transform.position = _lastPosition;
			camera.transform.rotation = _lastRotation;
			camera.fieldOfView = _defaultFOV;
			_state = FacebookScreenshotState.FinishingScreenshotMode;
			return true;
		}
		return false;
	}

	private void FinishingScreenshotMode()
	{
		if (animator != null)
		{
			animator.enabled = true;
		}
		camera.transform.position = _lastPosition;
		camera.transform.rotation = _lastRotation;
		if (light != null)
		{
			light.transform.rotation = _defaultLightRotation;
		}
		if (antialiasingAsPostEffect != null)
		{
			antialiasingAsPostEffect.enabled = false;
		}
		if (fastBloom != null)
		{
			fastBloom.enabled = false;
		}
		_state = FacebookScreenshotState.Disable;
		if (_onFinished != null)
		{
			_onFinished();
		}
		else
		{
			Debug.LogWarning("_onFinished is null");
		}
	}

	public void Initialize(FacebookScreenshotCallback onInitialized, FacebookScreenshotCallback onBeforePerformCameraRotation)
	{
		_state = FacebookScreenshotState.InitializingScreenshotMode;
		_onInitialized = onInitialized;
		_onBeforePerformCameraRotation = onBeforePerformCameraRotation;
	}

	public void CancelScreenshot(FacebookScreenshotCallback onCanceled)
	{
		DisableTouch();
		_state = FacebookScreenshotState.LeavingScreenshotMode;
		_onFinished = onCanceled;
	}

	public void TakeScreenshot(FacebookScreenshotCallback takenScreenshot)
	{
		DisableTouch();
		_onTakenScreenshot = takenScreenshot;
		if (antialiasingAsPostEffect != null)
		{
			antialiasingAsPostEffect.enabled = true;
		}
		if (fastBloom != null)
		{
			fastBloom.enabled = true;
		}
		StartCoroutine(TakeScreenshotCoroutine(OnTakenScreenshotCallback));
	}

	public IEnumerator TakeScreenshotCoroutine(FacebookScreenshotCallback onTokenScreenshotCallback)
	{
		yield return new WaitForEndOfFrame();
		_screenshot = ImageHelper.GetScreenshotAsTexture();
		texScreenshot.mainTexture = _screenshot;
		texScreenshot.fixedAspect = true;
		if (antialiasingAsPostEffect != null)
		{
			antialiasingAsPostEffect.enabled = false;
		}
		if (fastBloom != null)
		{
			fastBloom.enabled = false;
		}
		onTokenScreenshotCallback();
	}

	private void OnTakenScreenshotCallback()
	{
		if (_onTakenScreenshot != null)
		{
			_onTakenScreenshot();
		}
	}

	public void FeedScreenshot(string message, FacebookScreenshotCallback onFinished, FacebookScreenshotCallback onSending, FacebookScreenshotCallback onSent, FacebookScreenshotCallback onFailed, FacebookScreenshotCallback onTimedout)
	{
		_onSent = onSent;
		_onSending = onSending;
		_onFinished = onFinished;
		_onFailed = onFailed;
		_onTimedout = onTimedout;
		_message = message;
		if (MySingleton<Manager>.Instance.facebookManager.isLoggedIn)
		{
			OnBeforeFeedCallback();
			MySingleton<Manager>.Instance.facebookManager.FeedImage(message, _screenshot, TakeScreenshotSuccessCallback, TakeScreenshotFailedCallback);
		}
		else
		{
			_isLoggiging = true;
			MySingleton<Manager>.Instance.facebookManager.Login(OnLoginSuccess, OnLoginCancel, OnLoginFailed);
		}
	}

	public void OnLoginSuccess()
	{
		_isLoggiging = false;
		FeedScreenshot(_message, _onFinished, _onSending, _onSent, _onFailed, _onTimedout);
	}

	public void OnLoginCancel()
	{
		_isLoggiging = false;
		EnableTouch();
		if (_onInitialized != null)
		{
			_onInitialized();
		}
		else
		{
			Debug.LogWarning("_onInitialized is null");
		}
	}

	private void OnLoginFailed()
	{
		_isLoggiging = false;
		EnableTouch();
		if (_onFailed != null)
		{
			_onFailed();
		}
		else
		{
			Debug.LogWarning("_onFailed is null");
		}
	}

	private void TakeScreenshotSuccessCallback()
	{
		_timeToDie = 0f;
		_isLoggiging = false;
		if (_onSent != null)
		{
			_onSent();
		}
	}

	private void TakeScreenshotFailedCallback()
	{
		_timeToDie = 0f;
		_isLoggiging = false;
		if (_onFailed != null)
		{
			_onFailed();
		}
	}

	private void TakeScreenshotTimedoutCallback()
	{
		if (_timeToDie != 0f)
		{
			_timeToDie = 0f;
			_isLoggiging = false;
			if (_onTimedout != null)
			{
				_onTimedout();
			}
		}
	}

	public void EnableTouch()
	{
		EasyTouch.SetEnabled(true);
		_state = FacebookScreenshotState.PreparingScreenshot;
	}

	public void DisableTouch()
	{
		EasyTouch.SetEnabled(false);
	}

	public bool IsSending()
	{
		return _state == FacebookScreenshotState.SendingScreenshot;
	}

	public bool IsPreparing()
	{
		return _state == FacebookScreenshotState.PreparingScreenshot;
	}

	public void OnBeforeFeedCallback()
	{
		_state = FacebookScreenshotState.SendingScreenshot;
		if (_onSending != null)
		{
			_onSending();
		}
		else
		{
			Debug.LogWarning("_onSending is null");
		}
	}

	public void Roll_OnValueChange(UISlider slider)
	{
		_roll = Mathf.Lerp(rollMin, rollMax, slider.value) - camera.transform.localRotation.eulerAngles.z;
		camera.transform.localRotation *= Quaternion.AngleAxis(_roll, Vector3.forward);
	}

	private void OnSwipe(Gesture gesture)
	{
		if (_twisting && gesture.touchCount == 1)
		{
			OnTwistEnd(null);
		}
		if (!_twisting && gesture.touchCount <= 1)
		{
			PerformSwipeDirections(gesture.swipe);
			_xLength += gesture.swipeVector.x;
			_yLength += gesture.swipeVector.y;
			_xLength = Mathf.Clamp(_xLength, -100f, 100f);
			_yLength = Mathf.Clamp(_yLength, -100f, 100f);
		}
	}

	private void OnTwist(Gesture gesture)
	{
		_twisting = true;
		_twoFingerDistance = gesture.twoFingerDistance;
		if (_firstTwoFingerDistance)
		{
			_lastTwoFingerDistance = _twoFingerDistance;
			_deltaTwoFingerDistance = 0f;
			_firstTwoFingerDistance = false;
		}
		else
		{
			_deltaTwoFingerDistance = 0f - (_twoFingerDistance - _lastTwoFingerDistance);
			_lastTwoFingerDistance = _twoFingerDistance;
			_zoom += _deltaTwoFingerDistance * zoomSpeed * GetDeltaTime();
			_zoom = Mathf.Clamp(_zoom, zoomMin, zoomMax);
		}
	}

	private void OnTwistEnd(Gesture gesture)
	{
		_firstTwoFingerDistance = true;
		_twisting = false;
	}

	private void OnCancel(Gesture gesture)
	{
		OnTwistEnd(null);
	}

	private void PerformSwipeDirections(EasyTouch.SwipeDirection swipe)
	{
		_swipeDirections = new bool[4];
		_swipeDirections[0] = swipe == EasyTouch.SwipeDirection.Up || swipe == EasyTouch.SwipeDirection.UpLeft || swipe == EasyTouch.SwipeDirection.UpRight;
		_swipeDirections[1] = swipe == EasyTouch.SwipeDirection.Right || swipe == EasyTouch.SwipeDirection.DownRight || swipe == EasyTouch.SwipeDirection.UpRight;
		_swipeDirections[2] = swipe == EasyTouch.SwipeDirection.Down || swipe == EasyTouch.SwipeDirection.DownRight || swipe == EasyTouch.SwipeDirection.DownLeft;
		_swipeDirections[3] = swipe == EasyTouch.SwipeDirection.Left || swipe == EasyTouch.SwipeDirection.UpLeft || swipe == EasyTouch.SwipeDirection.DownLeft;
	}

	private void PerformCameraRotation(float deltaTime)
	{
		if (MySingleton<Manager>.Instance.isRollingScreenshotMode)
		{
			return;
		}
		if (_onBeforePerformCameraRotation != null)
		{
			_onBeforePerformCameraRotation();
		}
		bool flag = false;
		_relativeToCenter = _target.position - camera.transform.position;
		_magnitude = _relativeToCenter.magnitude;
		_lastCameraPosition = camera.transform.position;
		_lastCameraRotation = camera.transform.rotation;
		camera.transform.position = _target.position;
		float z = camera.transform.localRotation.eulerAngles.z;
		camera.transform.localRotation.SetEulerAngles(camera.transform.localRotation.eulerAngles.x, camera.transform.localRotation.eulerAngles.y, 0f);
		if (_swipeDirections[0] || _swipeDirections[2])
		{
			_deltaPitchAngle = camera.transform.rotation.eulerAngles.x + (0f - _yLength) * rotationSpeed * deltaTime;
			if (clampPitch)
			{
				_deltaPitchAngle = Mathf.Clamp(_deltaPitchAngle, pitchMin, pitchMax) - camera.transform.rotation.eulerAngles.x;
			}
			else
			{
				_deltaPitchAngle -= camera.transform.rotation.eulerAngles.x;
			}
			camera.transform.Rotate(Vector3.right, _deltaPitchAngle, Space.Self);
			flag = true;
		}
		if (_swipeDirections[1] || _swipeDirections[3])
		{
			_deltaYawAngle = camera.transform.rotation.eulerAngles.y + _xLength * rotationSpeed * deltaTime;
			if (clampYaw)
			{
				_deltaYawAngle = Mathf.Clamp(_deltaYawAngle, yawMin, yawMax) - camera.transform.rotation.eulerAngles.y;
			}
			else
			{
				_deltaYawAngle -= camera.transform.rotation.eulerAngles.y;
			}
			camera.transform.Rotate(Vector3.up, _deltaYawAngle, Space.World);
			if (light != null)
			{
				light.transform.Rotate(Vector3.up, _deltaYawAngle, Space.World);
			}
			flag = true;
		}
		camera.transform.position -= camera.transform.forward * _magnitude;
		camera.fieldOfView = _zoom;
		if (flag)
		{
			camera.transform.localRotation.SetEulerAngles(camera.transform.localRotation.eulerAngles.x, camera.transform.localRotation.eulerAngles.y, z);
			if (groundLimit.sqrMagnitude > 0f)
			{
				ScrennshotCameraLimit component = camera.GetComponent<ScrennshotCameraLimit>();
				if ((component.collidingUp && _swipeDirections[2]) || (component.collidingDown && _swipeDirections[0]) || (component.collidingLeft && _swipeDirections[1]) || (component.collidingRight && _swipeDirections[3]))
				{
					camera.transform.position = _lastCameraPosition;
					camera.transform.rotation = _lastCameraRotation;
				}
			}
		}
		_swipeDirections = new bool[4];
		_xLength = 0f;
		_yLength = 0f;
	}

	public void SetTarget(Transform target)
	{
		_target = target;
	}

	public Transform GetTarget()
	{
		return _target;
	}

	private void SetRollFromCamera()
	{
		_roll = camera.transform.localRotation.eulerAngles.z;
		if (_roll > 180f)
		{
			_roll -= 360f;
		}
		slRoll.value = Mathf.InverseLerp(rollMin, rollMax, _roll);
	}

	private float GetDeltaTime()
	{
		return (!_useScaledTime) ? Time.unscaledDeltaTime : Time.deltaTime;
	}

	private float GetTime()
	{
		return (!_useScaledTime) ? Time.unscaledTime : Time.time;
	}
}
