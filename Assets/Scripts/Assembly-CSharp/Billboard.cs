using UnityEngine;

public class Billboard : MonoBehaviour
{
	public enum PlayMode
	{
		Linear = 0,
		PingPong = 1,
		RandomSprite = 2,
		NoAnimation = 3
	}

	public enum BillboardingMode
	{
		Billboard = 0,
		BillboardInAngle = 1,
		Horizontal = 2,
		Vertical = 3,
		None = 4
	}

	public Material material;

	public BillboardingMode billboardingMode;

	public PlayMode playMode;

	public bool loop;

	public Vector3 size = Vector3.one;

	public int rows;

	public int columns;

	public int spriteCount;

	public int framesPerSecond;

	public bool randomRotation;

	public bool saveStateOnDisable;

	public float billboardAngle;

	private int index;

	private float randomZAngle;

	private Mesh mesh;

	private MeshRenderer meshRender;

	private Camera mainCamera;

	private bool isPing = true;

	public float targetScale;

	private float actualScale;

	private float smoothVel;

	private void Awake()
	{
		CreateParticle();
		GetComponent<Renderer>().enabled = false;
	}

	private void OnEnable()
	{
		mainCamera = Camera.main;
		if (!saveStateOnDisable)
		{
			index = 0;
			isPing = true;
		}
		InvokeRepeating("UpdateParticle", 0f, 1f / (float)framesPerSecond);
	}

	private void OnDisable()
	{
		CancelInvoke("UpdateParticle");
	}

	private void Start()
	{
		base.transform.localScale = Vector3.zero;
		if (randomRotation)
		{
			randomZAngle = Random.Range(-180f, 180f);
		}
		else
		{
			randomZAngle = 0f;
		}
		if (playMode == PlayMode.NoAnimation)
		{
			GetComponent<Renderer>().enabled = true;
		}
	}

	private void UpdateParticle()
	{
		if (!(MySingleton<Manager>.Instance.gameLogic != null))
		{
			return;
		}
		Camera_BillboardingMode();
		if (playMode == PlayMode.NoAnimation)
		{
			return;
		}
		if (playMode == PlayMode.Linear)
		{
			if (++index == spriteCount)
			{
				if (!loop)
				{
					base.gameObject.SetActive(false);
					return;
				}
				index = 0;
			}
		}
		else if (playMode == PlayMode.PingPong)
		{
			if (isPing)
			{
				if (++index == spriteCount)
				{
					index--;
					isPing = false;
				}
			}
			else if (--index == 0)
			{
				if (!loop)
				{
					base.gameObject.SetActive(false);
					return;
				}
				index++;
				isPing = true;
			}
		}
		else if (playMode == PlayMode.RandomSprite)
		{
			index = Random.Range(0, spriteCount);
		}
		Vector2 scale = new Vector2(1f / (float)columns, 1f / (float)rows);
		float num = Mathf.Floor(index % columns);
		float num2 = Mathf.Floor(index / columns);
		Vector2 offset = new Vector2(num * scale.x, 1f - scale.y - num2 * scale.y);
		GetComponent<Renderer>().material.SetTextureOffset("_MainTex", offset);
		GetComponent<Renderer>().material.SetTextureScale("_MainTex", scale);
		GetComponent<Renderer>().enabled = true;
	}

	public void UpdateSize()
	{
		base.transform.localScale = size * actualScale;
		actualScale = Mathf.SmoothDamp(actualScale, targetScale, ref smoothVel, Time.deltaTime);
	}

	private void CreateParticle()
	{
		mesh = base.gameObject.AddComponent<MeshFilter>().mesh;
		meshRender = base.gameObject.AddComponent<MeshRenderer>();
		mesh.vertices = new Vector3[4]
		{
			new Vector3(-0.5f, -0.5f, 0f),
			new Vector3(-0.5f, 0.5f, 0f),
			new Vector3(0.5f, 0.5f, 0f),
			new Vector3(0.5f, -0.5f, 0f)
		};
		mesh.triangles = new int[6] { 0, 1, 2, 2, 3, 0 };
		mesh.uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f),
			new Vector2(1f, 0f)
		};
		meshRender.castShadows = false;
		meshRender.receiveShadows = false;
		mesh.RecalculateNormals();
		GetComponent<Renderer>().material = material;
	}

	private void Camera_BillboardingMode()
	{
		if (mainCamera == null)
		{
			return;
		}
		Vector3 vector = mainCamera.transform.position - base.transform.position;
		switch (billboardingMode)
		{
		case BillboardingMode.Billboard:
			base.transform.LookAt(mainCamera.transform.position);
			break;
		case BillboardingMode.BillboardInAngle:
		{
			base.transform.LookAt(mainCamera.transform.position);
			float y = base.transform.localEulerAngles.y;
			y = ((!(y >= 0f)) ? (y + 360f) : y);
			if (MySingleton<Manager>.Instance.gameLogic.lightsOn)
			{
				if (360f - billboardAngle <= y || y <= billboardAngle)
				{
					targetScale = 1f;
				}
				else
				{
					targetScale = 0f;
				}
			}
			UpdateSize();
			break;
		}
		case BillboardingMode.Horizontal:
			vector.x = (vector.z = 0f);
			base.transform.LookAt(mainCamera.transform.position - vector);
			break;
		case BillboardingMode.Vertical:
			vector.y = (vector.z = 0f);
			base.transform.LookAt(mainCamera.transform.position - vector);
			break;
		}
		base.transform.eulerAngles = new Vector3(base.transform.eulerAngles.x, base.transform.eulerAngles.y, randomZAngle);
	}
}
