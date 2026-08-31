using UnityEngine;

public class qb_Group : MonoBehaviour
{
	public string groupName;

	private bool visible;

	private bool frozen;

	public void AddObject(GameObject newObject)
	{
		newObject.transform.parent = base.transform;
	}

	public void Hide()
	{
		visible = false;
	}

	public void Show()
	{
		visible = true;
	}

	public void Freeze()
	{
		frozen = true;
	}

	public void UnFreeze()
	{
		frozen = false;
	}

	public void CleanUp()
	{
		Object.DestroyImmediate(base.gameObject);
	}
}
