using UnityEngine;

public class RealtimeReflections : MonoBehaviour
{
	public int cubemapSize = 128;

	public float nearClip = 0.01f;

	public float farClip = 500f;

	public bool oneFacePerFrame;

	public Material[] materials;

	public LayerMask layerMask = -1;

	private Camera cam;

	private RenderTexture renderTexture;

	private void OnEnable()
	{
		layerMask.value = -1;
	}

	private void Start()
	{
		UpdateCubemap(63);
	}

	private void LateUpdate()
	{
		if (oneFacePerFrame)
		{
			int num = Time.frameCount % 6;
			int faceMask = 1 << num;
			UpdateCubemap(faceMask);
		}
		else
		{
			UpdateCubemap(63);
		}
	}

	private void UpdateCubemap(int faceMask)
	{
		if (!cam)
		{
			GameObject gameObject = new GameObject("CubemapCamera", typeof(Camera));
			gameObject.hideFlags = HideFlags.HideAndDontSave;
			gameObject.transform.position = base.transform.position;
			gameObject.transform.rotation = Quaternion.identity;
			cam = gameObject.GetComponent<Camera>();
			cam.cullingMask = layerMask;
			cam.nearClipPlane = nearClip;
			cam.farClipPlane = farClip;
			cam.enabled = false;
		}
		if (!renderTexture)
		{
			renderTexture = new RenderTexture(cubemapSize, cubemapSize, 16);
			renderTexture.isPowerOfTwo = true;
			renderTexture.isCubemap = true;
			renderTexture.hideFlags = HideFlags.HideAndDontSave;
			Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				Material[] sharedMaterials = renderer.sharedMaterials;
				foreach (Material material in sharedMaterials)
				{
					if (material.HasProperty("_Cube"))
					{
						material.SetTexture("_Cube", renderTexture);
					}
				}
			}
		}
		cam.transform.position = base.transform.position;
		cam.RenderToCubemap(renderTexture, faceMask);
	}

	private void OnDisable()
	{
		Object.DestroyImmediate(cam);
		Object.DestroyImmediate(renderTexture);
	}
}
