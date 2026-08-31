using UnityEngine;

public class CarDamage : MonoBehaviour
{
	public float minForce = 1f;

	public float multiplier = 0.1f;

	public float deformRadius = 1f;

	public float deformNoise = 0.1f;

	public float deformNodeRadius = 0.5f;

	public float maxDeform = 0.5f;

	public float maxNodeRotationStep = 10f;

	public float maxNodeRotation = 14f;

	public float bounceSpeed = 0.1f;

	public float bounceThreshold = 0.002f;

	public bool autoBounce;

	public bool showRepairingLabel = true;

	public MeshFilter[] DeformMeshes;

	public Transform[] DeformNodes;

	public MeshFilter[] DeformColliders;

	private CarVisuals m_CarVisuals;

	private VertexData[] m_meshData;

	private VertexData[] m_colliderData;

	private Vector3[] m_permaNodes;

	private Quaternion[] m_permaNodeAngles;

	private bool m_doBounce;

	private void OnEnable()
	{
		m_CarVisuals = GetComponent<CarVisuals>();
	}

	private void Start()
	{
		m_meshData = new VertexData[DeformMeshes.Length];
		for (int i = 0; i < DeformMeshes.Length; i++)
		{
			m_meshData[i] = new VertexData();
			m_meshData[i].permaVerts = DeformMeshes[i].mesh.vertices;
		}
		m_colliderData = new VertexData[DeformColliders.Length];
		for (int j = 0; j < DeformColliders.Length; j++)
		{
			m_colliderData[j] = new VertexData();
			m_colliderData[j].permaVerts = DeformColliders[j].sharedMesh.vertices;
		}
		m_permaNodes = new Vector3[DeformNodes.Length];
		m_permaNodeAngles = new Quaternion[DeformNodes.Length];
		for (int k = 0; k < DeformNodes.Length; k++)
		{
			m_permaNodes[k] = DeformNodes[k].transform.localPosition;
			m_permaNodeAngles[k] = DeformNodes[k].transform.localRotation;
		}
	}

	private void DeformMesh(Mesh mesh, Vector3[] originalMesh, Transform localTransform, Vector3 contactPoint, Vector3 contactForce)
	{
		Vector3[] vertices = mesh.vertices;
		float num = deformRadius * deformRadius;
		float num2 = maxDeform * maxDeform;
		Vector3 vector = localTransform.InverseTransformPoint(contactPoint);
		Vector3 vector2 = localTransform.InverseTransformDirection(contactForce);
		for (int i = 0; i < vertices.Length; i++)
		{
			float sqrMagnitude = (vector - vertices[i]).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				vertices[i] += vector2 * (deformRadius - Mathf.Sqrt(sqrMagnitude)) / deformRadius + Random.onUnitSphere * deformNoise;
				Vector3 vector3 = vertices[i] - originalMesh[i];
				if (vector3.sqrMagnitude > num2)
				{
					vertices[i] = originalMesh[i] + vector3.normalized * maxDeform;
				}
			}
		}
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}

	private void DeformNode(Transform T, Vector3 originalLocalPos, Quaternion originalLocalRot, Vector3 contactPoint, Vector3 contactVector)
	{
		float sqrMagnitude = (contactPoint - T.position).sqrMagnitude;
		float num = 0f;
		if (sqrMagnitude < deformRadius * deformRadius)
		{
			num = (deformRadius - Mathf.Sqrt(sqrMagnitude)) / deformRadius;
			T.position += contactVector * num + Random.onUnitSphere * deformNoise;
			Vector3 vector = T.localPosition - originalLocalPos;
			if (vector.sqrMagnitude > maxDeform * maxDeform)
			{
				T.localPosition = originalLocalPos + vector.normalized * maxDeform;
			}
		}
		if (sqrMagnitude < deformNodeRadius * deformNodeRadius)
		{
			Vector3 vector2 = AnglesToVector(T.localEulerAngles);
			Vector3 vector3 = new Vector3(maxNodeRotation, maxNodeRotation, maxNodeRotation);
			Vector3 vector4 = vector2 + vector3;
			Vector3 vector5 = vector2 - vector3;
			vector2 += num * Random.onUnitSphere * maxNodeRotationStep;
			T.localEulerAngles = new Vector3(Mathf.Clamp(vector2.x, vector5.x, vector4.x), Mathf.Clamp(vector2.y, vector5.y, vector4.y), Mathf.Clamp(vector2.z, vector5.z, vector4.z));
		}
	}

	private bool BounceMesh(Mesh mesh, Vector3[] originalMesh, float maxSpeed, float sqrThreshold)
	{
		bool result = true;
		Vector3[] vertices = mesh.vertices;
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] = Vector3.MoveTowards(vertices[i], originalMesh[i], maxSpeed);
			if ((originalMesh[i] - vertices[i]).sqrMagnitude >= sqrThreshold)
			{
				result = false;
			}
		}
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return result;
	}

	private bool BounceNode(Transform T, Vector3 originalLocalPosition, Quaternion originalLocalRotation, float maxSpeed, float sqrThreshold)
	{
		T.localPosition = Vector3.MoveTowards(T.localPosition, originalLocalPosition, maxSpeed);
		T.localRotation = Quaternion.RotateTowards(T.localRotation, originalLocalRotation, maxSpeed * 50f);
		return (originalLocalPosition - T.localPosition).sqrMagnitude < sqrThreshold && Quaternion.Angle(originalLocalRotation, T.localRotation) < sqrThreshold;
	}

	private void RestoreNode(Transform T, Vector3 originalLocalPos, Quaternion originalLocalAngles)
	{
		T.localPosition = originalLocalPos;
		T.localRotation = originalLocalAngles;
	}

	private Vector3 AnglesToVector(Vector3 Angles)
	{
		if (Angles.x > 180f)
		{
			Angles.x = -360f + Angles.x;
		}
		if (Angles.y > 180f)
		{
			Angles.y = -360f + Angles.y;
		}
		if (Angles.z > 180f)
		{
			Angles.z = -360f + Angles.z;
		}
		return Angles;
	}

	private void RestoreColliders()
	{
		if (DeformColliders.Length > 0)
		{
			Vector3 centerOfMass = GetComponent<Rigidbody>().centerOfMass;
			for (int i = 0; i < DeformColliders.Length; i++)
			{
				Mesh mesh = new Mesh();
				mesh.vertices = m_colliderData[i].permaVerts;
				mesh.triangles = DeformColliders[i].sharedMesh.triangles;
				mesh.RecalculateNormals();
				mesh.RecalculateBounds();
				DeformColliders[i].sharedMesh = mesh;
			}
			GetComponent<Rigidbody>().centerOfMass = centerOfMass;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (autoBounce)
		{
			m_doBounce = true;
			Debug.Log("Reparando por collision");
		}
	}

	public void DoBounce()
	{
		m_doBounce = true;
	}

	private void Update()
	{
		Vector3 contactForce = Vector3.zero;
		if (m_CarVisuals.localImpactVelocity.sqrMagnitude > minForce * minForce)
		{
			contactForce = base.transform.TransformDirection(m_CarVisuals.localImpactVelocity) * multiplier * 0.2f;
		}
		else if (m_CarVisuals.localDragVelocityDiscrete.sqrMagnitude > minForce * minForce)
		{
			contactForce = base.transform.TransformDirection(m_CarVisuals.localDragVelocityDiscrete) * multiplier * 0.01f;
		}
		if (contactForce.sqrMagnitude > 0f)
		{
			Vector3 contactPoint = base.transform.TransformPoint(m_CarVisuals.localImpactPosition);
			for (int i = 0; i < DeformMeshes.Length; i++)
			{
				DeformMesh(DeformMeshes[i].mesh, m_meshData[i].permaVerts, DeformMeshes[i].transform, contactPoint, contactForce);
			}
			if (DeformColliders.Length > 0)
			{
				Vector3 centerOfMass = GetComponent<Rigidbody>().centerOfMass;
				for (int i = 0; i < DeformColliders.Length; i++)
				{
					Mesh mesh = new Mesh();
					mesh.vertices = DeformColliders[i].sharedMesh.vertices;
					mesh.triangles = DeformColliders[i].sharedMesh.triangles;
					DeformMesh(mesh, m_colliderData[i].permaVerts, DeformColliders[i].transform, contactPoint, contactForce);
					DeformColliders[i].sharedMesh = mesh;
				}
				GetComponent<Rigidbody>().centerOfMass = centerOfMass;
			}
			contactForce *= 0.5f;
		}
		if (m_doBounce)
		{
			Repair(Time.unscaledDeltaTime);
		}
	}

	public bool Repair(float deltaTime)
	{
		float maxSpeed = bounceSpeed * deltaTime;
		float sqrThreshold = bounceThreshold * bounceThreshold;
		bool flag = true;
		for (int i = 0; i < DeformMeshes.Length; i++)
		{
			flag = BounceMesh(DeformMeshes[i].mesh, m_meshData[i].permaVerts, maxSpeed, sqrThreshold) && flag;
		}
		if (flag)
		{
			m_doBounce = false;
			for (int j = 0; j < DeformNodes.Length; j++)
			{
				RestoreNode(DeformNodes[j], m_permaNodes[j], m_permaNodeAngles[j]);
			}
			RestoreColliders();
		}
		return flag;
	}
}
