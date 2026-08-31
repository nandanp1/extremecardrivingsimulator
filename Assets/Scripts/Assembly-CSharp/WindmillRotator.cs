using UnityEngine;

public class WindmillRotator : MonoBehaviour
{
	public float speed;

	public Vector3 axes = Vector3.up;

	private Transform cachedTransform;

	private bool visible;

	private void Start()
	{
		cachedTransform = base.transform;
	}

	private void OnBecameVisible()
	{
		visible = true;
	}

	private void OnBecameInVisible()
	{
		visible = false;
	}

	private void Update()
	{
		if (visible)
		{
			cachedTransform.Rotate(axes * Time.deltaTime * speed);
		}
	}
}
