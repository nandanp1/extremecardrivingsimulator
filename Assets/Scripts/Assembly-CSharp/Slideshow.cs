using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIGrid))]
public class Slideshow : MonoBehaviour
{
	public float slideTime = 1f;

	private UICenterOnChild centerer;

	public List<Transform> gridElements = new List<Transform>();

	private int shownElementIndex;

	private UIDragScrollView dragScrollView;

	public UIGrid bulletsGrid;

	public GameObject bulletPrefab;

	private bool isValidElement;

	private bool justReleased;

	private void OnEnable()
	{
		UIGrid component = GetComponent<UIGrid>();
		component.Reposition();
		gridElements = component.GetChildList();
		centerer = base.gameObject.AddMissingComponent<UICenterOnChild>();
		UICenterOnChild uICenterOnChild = centerer;
		uICenterOnChild.onCenter = (UICenterOnChild.OnCenterCallback)Delegate.Combine(uICenterOnChild.onCenter, new UICenterOnChild.OnCenterCallback(OnCentered));
		StartCoroutine("DoSlide");
	}

	private void OnDisable()
	{
		StopCoroutine("DoSlide");
		UICenterOnChild uICenterOnChild = centerer;
		uICenterOnChild.onCenter = (UICenterOnChild.OnCenterCallback)Delegate.Remove(uICenterOnChild.onCenter, new UICenterOnChild.OnCenterCallback(OnCentered));
	}

	public void Refresh()
	{
		StopCoroutine("DoSlide");
		UIGrid component = GetComponent<UIGrid>();
		component.Reposition();
		gridElements = component.GetChildList();
		StartCoroutine("DoSlide");
	}

	private IEnumerator DoSlide()
	{
		while (this != null && base.enabled)
		{
			if (isValidElement)
			{
				yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(slideTime));
				isValidElement = false;
			}
			else
			{
				yield return null;
			}
			shownElementIndex = ++shownElementIndex % gridElements.Count;
			Transform element = gridElements[shownElementIndex];
			if ((bool)element && element.gameObject.activeSelf)
			{
				isValidElement = true;
				centerer.CenterOn(element);
			}
		}
	}

	public void OnElementPress()
	{
		StopCoroutine("DoSlide");
	}

	public void OnElementRelease()
	{
		justReleased = true;
		StopCoroutine("OnReleaseLogic");
		StartCoroutine("OnReleaseLogic");
	}

	private IEnumerator OnReleaseLogic()
	{
		yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(slideTime * 2f));
		StartCoroutine("DoSlide");
	}

	public void OnElementClick(InfoCar.PurchasablePack carPackId)
	{
		MySingleton<Manager>.Instance.storeInitData.packID = carPackId;
		MySingleton<Manager>.Instance.storeInitData.packStoreFromPause = true;
		MySingleton<Manager>.Instance.guiGameLogic.OpenStore();
	}

	public void OnElementClickPack(InfoPack pack)
	{
		MySingleton<Manager>.Instance.storeInitData.packID = pack.packId;
		MySingleton<Manager>.Instance.storeInitData.pack = pack;
		MySingleton<Manager>.Instance.storeInitData.packStoreFromPause = true;
		MySingleton<Manager>.Instance.guiGameLogic.OpenStore();
	}

	private void OnCentered(GameObject element)
	{
		if (justReleased)
		{
			justReleased = false;
			shownElementIndex = gridElements.IndexOf(element.transform);
		}
	}

	private void SetupBullets()
	{
	}

	private void UpdateBullets()
	{
	}
}
