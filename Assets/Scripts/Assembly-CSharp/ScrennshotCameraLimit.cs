using UnityEngine;

public class ScrennshotCameraLimit : MonoBehaviour
{
	private LayerMask layerMask;

	private float rayDistance;

	private Ray selectedRay;

	public bool collidingUp { get; set; }

	public bool collidingDown { get; set; }

	public bool collidingRight { get; set; }

	public bool collidingLeft { get; set; }

	public void Start()
	{
		collidingUp = false;
		collidingDown = false;
		collidingLeft = false;
		collidingRight = false;
		rayDistance = 2f;
		layerMask = LayerMask.GetMask("Soils");
	}

	public void Update()
	{
		LaunchRays();
	}

	private void LaunchRays()
	{
		Ray ray = new Ray(base.transform.position, base.transform.up);
		Ray ray2 = new Ray(base.transform.position, -base.transform.up);
		Ray ray3 = new Ray(base.transform.position, -base.transform.right);
		Ray ray4 = new Ray(base.transform.position, base.transform.right);
		RaycastHit hitInfo;
		collidingUp = Physics.Raycast(ray, out hitInfo, rayDistance, layerMask);
		collidingDown = Physics.Raycast(ray2, out hitInfo, rayDistance, layerMask);
		collidingLeft = Physics.Raycast(ray3, out hitInfo, rayDistance, layerMask);
		collidingRight = Physics.Raycast(ray4, out hitInfo, rayDistance, layerMask);
	}
}
