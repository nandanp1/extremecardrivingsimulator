using UnityEngine;

public class SpawnerMaster : MonoBehaviour
{
	public bool checkOnce = true;

	public LayerMask layerMaskCompare = -1;

	public string[] tagsCompare;

	[HideInInspector]
	public SpawnerMasterCallbackEvent OnSpawnerMasterEnter;

	[HideInInspector]
	public bool isChecked;

	private bool _desireCollide;

	public void OnSpawnerSensorTriggerEnter(SpawnerSensor spawnerSensor, Collider other)
	{
		if (!IsDesireCollider(other) || OnSpawnerMasterEnter == null)
		{
			return;
		}
		if (checkOnce)
		{
			if (!isChecked)
			{
				OnSpawnerMasterEnter(other.transform.position);
				isChecked = true;
			}
		}
		else
		{
			OnSpawnerMasterEnter(other.transform.position);
		}
	}

	private bool IsDesireCollider(Collider other)
	{
		_desireCollide = false;
		if (tagsCompare != null)
		{
			if (((int)layerMaskCompare & (1 << other.gameObject.layer)) > 0)
			{
				_desireCollide = true;
			}
			if (!_desireCollide)
			{
				string[] array = tagsCompare;
				foreach (string text in array)
				{
					if (other.CompareTag(text))
					{
						_desireCollide = true;
						break;
					}
				}
			}
		}
		return _desireCollide;
	}
}
