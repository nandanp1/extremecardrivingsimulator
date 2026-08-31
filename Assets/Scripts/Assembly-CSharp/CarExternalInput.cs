using UnityEngine;

public class CarExternalInput : MonoBehaviour
{
	protected CarControl m_CarControl;

	private void Start()
	{
		m_CarControl = GetComponent<CarControl>();
	}
}
