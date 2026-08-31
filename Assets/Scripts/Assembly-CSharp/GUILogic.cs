using Unibill;
using UnityEngine;

public abstract class GUILogic : MonoBehaviour
{
	public GameObject uiRoot;

	public GameObject pnProgressPanelReference;

	public GameObject pnAchievements;

	public TweenScale cupButtonTweenScale;

	private bool reverseAnimation;

	private BackButtonStore backbutton;

	protected virtual void OnEnable()
	{
		MySingleton<Manager>.Instance.guiCurrentLogic = this;
		uiRoot = base.transform.root.gameObject;
		if (!uiRoot.GetComponent<UIRoot>())
		{
			Debug.LogError(base.gameObject.name + " no tiene como padre raiz un componente UI Root.\ufffdEsto deberia pasar?");
		}
		else
		{
			MySingleton<Manager>.Instance.currentUIRoot = uiRoot;
		}
		Resources.UnloadUnusedAssets();
	}

	public virtual void OnTaskFinished(Task finishedTask)
	{
	}

	public virtual void OnRewardClaimed(RewardItem reward)
	{
	}

	public virtual void OnVideoRewardsAvailabilityChanged(bool isAvailable)
	{
	}

	public virtual void OnVideoRewardsFinished(Task task)
	{
	}

	public virtual void BillerInitialized(UnibillState state)
	{
	}

	public virtual void UpdateLocalizeLabels()
	{
	}

	public virtual void PurchasesRestored(bool success)
	{
	}

	public virtual void PurchaseCompleted(PurchaseEvent purchase)
	{
	}

	public virtual void PurchaseCanceled(PurchasableItem item)
	{
	}

	public virtual void PurchaseFailed(PurchasableItem item)
	{
	}

	public void OpenStore(PurchaseEvent purchase = null)
	{
		if (!MySingleton<Manager>.Instance.storeInitData.storeAlreadyLoaded)
		{
			MySingleton<Manager>.Instance.storeInitData.storeAlreadyLoaded = true;
			if (purchase != null)
			{
				decimal creditsAmount = MySingleton<Manager>.Instance.billingManager.GetCreditsAmount(purchase.PurchasedItem.LocalId);
				MySingleton<Manager>.Instance.SetStoreReward((int)creditsAmount);
			}
			MySingleton<Manager>.Instance.storeInitData.lastSceneLoaded = LegacySceneManager.LoadedLevel;
			LegacySceneManager.LoadLevelAdditiveAsync(4);
		}
	}

	public void ExitStore()
	{
		if (MySingleton<Manager>.Instance.guiGameLogic != null)
		{
			MySingleton<Manager>.Instance.guiGameLogic.ActivePauseMenu(true);
		}
		if ((bool)MySingleton<Manager>.Instance.guiStoreLogic.mainCamera)
		{
			NGUITools.SetActive(MySingleton<Manager>.Instance.guiStoreLogic.mainCamera.gameObject, true);
		}
		MySingleton<Manager>.Instance.storeInitData.storeAlreadyLoaded = false;
		if ((bool)MySingleton<Manager>.Instance.guiStoreLogic.mainCamera)
		{
			MySingleton<Manager>.Instance.guiStoreLogic.mainCamera.enabled = true;
			MySingleton<Manager>.Instance.guiStoreLogic.mainCamera = null;
		}
		GameObject gameObject = GameObject.Find("4Store");
		if ((bool)gameObject)
		{
			Object.Destroy(gameObject);
		}
		MySingleton<Manager>.Instance.storeInitData.storeFromCarPacks = false;
		MySingleton<Manager>.Instance.alreadyLoaded = false;
	}

	public virtual void ShowAchievementsPanel(AchievementManager.AchievementCategory category)
	{
		switch (category)
		{
		case AchievementManager.AchievementCategory.MULTIPLAYER:
			if (!MySingleton<Manager>.Instance.multiplayerShowedAchievements && !MySingleton<AchievementManager>.Instance.IsCategoryCompleted(AchievementManager.AchievementCategory.MULTIPLAYER))
			{
				NGUITools.SetActive(pnAchievements, true);
				pnAchievements.GetComponent<AchievementsPanel>().Init(category);
				MySingleton<Manager>.Instance.multiplayerShowedAchievements = true;
			}
			break;
		case AchievementManager.AchievementCategory.GENERAL:
			if (!MySingleton<Manager>.Instance.generalShowedAchievements && !MySingleton<AchievementManager>.Instance.IsCategoryCompleted(AchievementManager.AchievementCategory.GENERAL))
			{
				NGUITools.SetActive(pnAchievements, true);
				pnAchievements.GetComponent<AchievementsPanel>().Init(category);
				MySingleton<Manager>.Instance.generalShowedAchievements = true;
			}
			break;
		}
	}

	public void DelayReverseAnimation()
	{
		cupButtonTweenScale.enabled = false;
		cupButtonTweenScale.delay = 2f;
		cupButtonTweenScale.PlayForward();
	}
}
