using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AxesInMotion.Network.Minigames
{
	public class GUIRacingMiniGameLogic : MonoBehaviour
	{
		private const string WAIT_COUNTDOWN_TEXT = "NEXT RACE IN {0}";

		private const string FINAL_START_COUNTDOWN_TEXT = "GO!";

		private const string PLAYER_WON_TEXT = "{0} WON THE RACE";

		private const string FINAL_RESULT_TEXT = "Congratulations!\nYou finished the race in {0}th place.\nHere is your reward:";

		private const string REWARD_TEXT = "{0}";

		private const string RESET_COUNTDOWN_TEXT = "00:{0}";

		public UIWidget spWaitingForPlayers;

		public UIWidget spWaitCountdown;

		public UILabel lbWaitCountdownText;

		public UIWidget spStartCountdown;

		public UILabel lbStartCountdownText;

		public UIWidget spFinalCountdown;

		public UILabel lbFinalCountdownText;

		public UIWidget spPlayerWon;

		public UILabel lbPlayerWonText;

		public UIWidget spSpectatorMode;

		public UIPanel pnFinalResults;

		public UIGrid rankingGrid;

		public UILabel lbResultMessage;

		public UILabel lbReward;

		public UILabel lbResetCountdown;

		public GameObject rankingPositionPrefab;

		[HideInInspector]
		public bool arrowIsActive;

		public Transform wayPointArrow;

		[HideInInspector]
		public Transform currentCameraArrowTransform;

		[HideInInspector]
		public Transform currentTargetArrowTransform;

		private void OnEnable()
		{
			MySingleton<Manager>.Instance.guiGameLogic.racingGUILogic = this;
			rankingGrid.sorting = UIGrid.Sorting.None;
			NGUITools.SetActive(MySingleton<Manager>.Instance.guiGameLogic.pnNetworkRadar, false);
			HideAll();
		}

		private void OnDisable()
		{
			if ((bool)MySingleton<Manager>.Instance)
			{
				HideAll();
				NGUITools.SetActive(MySingleton<Manager>.Instance.guiGameLogic.pnNetworkRadar, true);
			}
		}

		private void LateUpdate()
		{
			if (arrowIsActive)
			{
				wayPointArrow.localRotation = currentCameraArrowTransform.localRotation;
			}
		}

		public void ShowWaitingForPlayers()
		{
			HideAll();
			NGUITools.SetActive(spWaitingForPlayers.gameObject, true);
		}

		public void HideWaitingForPlayers()
		{
			NGUITools.SetActive(spWaitingForPlayers.gameObject, false);
		}

		public void SetWaitCountdown(int value)
		{
			if (!spWaitCountdown.gameObject.activeSelf)
			{
				HideAll();
				NGUITools.SetActive(spWaitCountdown.gameObject, true);
			}
			lbWaitCountdownText.text = string.Format(Localization.Get("multiplayer_racing_countdown_wait"), value);
		}

		public void HideWaitCountdown()
		{
			NGUITools.SetActive(spWaitCountdown.gameObject, false);
		}

		public void SetStartCountdown(int value)
		{
			if (!spStartCountdown.gameObject.activeSelf)
			{
				HideAll();
				NGUITools.SetActive(spStartCountdown.gameObject, true);
			}
			if (value == 0)
			{
				lbStartCountdownText.text = Localization.Get("multiplayer_racing_countdown_start_finish");
			}
			else
			{
				lbStartCountdownText.text = value.ToString();
			}
		}

		public void HideStartCountdown()
		{
			NGUITools.SetActive(spStartCountdown.gameObject, false);
		}

		public void SetFinalCountdown(int value)
		{
			if (!spFinalCountdown.gameObject.activeSelf)
			{
				NGUITools.SetActive(spFinalCountdown.gameObject, true);
			}
			string empty = string.Empty;
			empty = ((value <= 9) ? ("0" + value) : value.ToString());
			lbFinalCountdownText.text = string.Format(Localization.Get("multiplayer_racing_countdown_finish"), empty);
		}

		public void HideFinalCountdown()
		{
			NGUITools.SetActive(spFinalCountdown.gameObject, false);
		}

		public void ShowPlayerWon(string playerName)
		{
			if (!spPlayerWon.gameObject.activeSelf)
			{
				NGUITools.SetActive(spPlayerWon.gameObject, true);
			}
			lbPlayerWonText.text = string.Format(Localization.Get("multiplayer_racing_player_won"), playerName);
			StartCoroutine("WaitAndHidePlayerWon", 2f);
		}

		private IEnumerator WaitAndHidePlayerWon(float duration)
		{
			yield return new WaitForSeconds(duration);
			HidePlayerWon();
		}

		public void HidePlayerWon()
		{
			NGUITools.SetActive(spPlayerWon.gameObject, false);
		}

		public void ShowFinalResultPanel()
		{
			HideAll();
			NGUITools.SetActive(pnFinalResults.gameObject, true);
		}

		public void HideFinalResultPanel()
		{
			NGUITools.SetActive(pnFinalResults.gameObject, false);
		}

		public void ShowSpectatorMode()
		{
			NGUITools.SetActive(spSpectatorMode.gameObject, true);
		}

		public void HideSpectatorMode()
		{
			NGUITools.SetActive(spSpectatorMode.gameObject, false);
		}

		public void SetFinalResultTextData(int position, int reward)
		{
			lbResultMessage.text = string.Format(Localization.Get("multiplayer_racing_final_result"), position.ToString());
			lbReward.text = string.Format("{0}", reward.ToString());
		}

		public void SetResetCountdown(int value)
		{
			string arg = ((value < 10) ? ("0" + value) : value.ToString());
			lbResetCountdown.text = string.Format("00:{0}", arg);
		}

		public void SetRankingData(List<NetworkPlayer> playerRanking)
		{
			StartCoroutine("ClearRankingWaitOneFrameAndSetData", playerRanking);
		}

		private IEnumerator ClearRankingWaitOneFrameAndSetData(List<NetworkPlayer> playerRanking)
		{
			ClearRanking();
			yield return null;
			int position = 0;
			foreach (NetworkPlayer player in playerRanking)
			{
				RankingPosition rankingPosition = NGUITools.AddChild(rankingGrid.gameObject, rankingPositionPrefab).GetComponent<RankingPosition>();
				rankingPosition.SetNameAndPosition(player, position + 1);
				position++;
			}
			rankingGrid.Reposition();
		}

		public void ClearRanking()
		{
			foreach (Transform child in rankingGrid.GetChildList())
			{
				NGUITools.Destroy(child.gameObject);
			}
		}

		private void HideAll()
		{
			NGUITools.SetActive(spWaitingForPlayers.gameObject, false);
			NGUITools.SetActive(spWaitCountdown.gameObject, false);
			NGUITools.SetActive(spStartCountdown.gameObject, false);
			NGUITools.SetActive(spFinalCountdown.gameObject, false);
			NGUITools.SetActive(spPlayerWon.gameObject, false);
			NGUITools.SetActive(wayPointArrow.gameObject, false);
			NGUITools.SetActive(pnFinalResults.gameObject, false);
			NGUITools.SetActive(spSpectatorMode.gameObject, false);
		}
	}
}
