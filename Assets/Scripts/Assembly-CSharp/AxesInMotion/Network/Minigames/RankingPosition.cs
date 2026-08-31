using UnityEngine;

namespace AxesInMotion.Network.Minigames
{
	public class RankingPosition : MonoBehaviour
	{
		public UILabel lbPosition;

		public UILabel lbPlayerName;

		public UISprite spBackground;

		public NetworkPlayer player;

		public Color myPositionColor;

		public Color otherPositionColor;

		public void SetNameAndPosition(NetworkPlayer player, int position)
		{
			this.player = player;
			lbPlayerName.text = player.name;
			if (player.isLocal)
			{
				spBackground.color = myPositionColor;
			}
			else
			{
				spBackground.color = otherPositionColor;
			}
			lbPosition.text = position + ".";
		}
	}
}
