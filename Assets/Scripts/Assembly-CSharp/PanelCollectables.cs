using System.Collections.Generic;
using UnityEngine;

public class PanelCollectables : MonoBehaviour
{
	public UIGrid collectionCategoryGrid;

	public GameObject collectionCategoryGloval;

	public GameObject collectionCategorySource;

	public GameObject[] collectionCategoryArray;

	private List<List<CollectablesEntryInfo>> collectablesList;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Init(List<List<CollectablesEntryInfo>> collectablesList)
	{
		int count = collectablesList.Count;
		collectionCategoryArray = new GameObject[count];
		List<CollectablesEntryInfo> list;
		for (int i = 2; i < count; i++)
		{
			collectionCategoryArray[i] = Object.Instantiate(collectionCategorySource) as GameObject;
			collectionCategoryArray[i].transform.parent = collectionCategoryGrid.transform;
			collectionCategoryArray[i].transform.localScale = Vector3.one;
			list = collectablesList[i];
			collectionCategoryArray[i].GetComponent<CollectablesCategoryBehaviour>().Init(collectablesList[i], list[0].categoryIconName);
		}
		if (count > 1 && collectionCategorySource != null)
		{
			list = collectablesList[1];
			collectionCategoryArray[1] = collectionCategorySource;
			collectionCategoryArray[1].GetComponent<CollectablesCategoryBehaviour>().Init(collectablesList[1], list[0].categoryIconName);
		}
		else if (collectionCategorySource != null)
		{
			collectionCategorySource.SetActive(false);
		}
		list = collectablesList[0];
		collectionCategoryArray[0] = collectionCategoryGloval;
		collectionCategoryArray[0].GetComponent<CollectablesCategoryBehaviour>().Init(collectablesList[0], list[0].categoryIconName);
		collectionCategoryGrid.repositionNow = true;
	}
}
