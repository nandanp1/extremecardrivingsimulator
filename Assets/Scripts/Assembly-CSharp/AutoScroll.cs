using UnityEngine;

[RequireComponent(typeof(UIScrollView))]
public class AutoScroll : MonoBehaviour
{
	private UIScrollView mScrollView;

	public float scrollSpeed = 0.5f;

	private Vector3 scrollDirection = Vector3.zero;

	private void Start()
	{
		mScrollView = GetComponent<UIScrollView>();
		if (mScrollView.dragEffect == UIScrollView.DragEffect.MomentumAndSpring)
		{
			mScrollView.dragEffect = UIScrollView.DragEffect.Momentum;
		}
		if (mScrollView.movement == UIScrollView.Movement.Horizontal)
		{
			scrollDirection = Vector3.right;
		}
		else
		{
			scrollDirection = Vector3.down;
		}
	}

	private void LateUpdate()
	{
		AutoScrollPanel();
	}

	private void AutoScrollPanel()
	{
		if (mScrollView.RestrictWithinBounds(false, mScrollView.canMoveHorizontally, mScrollView.canMoveVertically))
		{
			InvertScrollDirection();
		}
		mScrollView.MoveRelative(scrollSpeed * scrollDirection);
	}

	private void InvertScrollDirection()
	{
		scrollSpeed *= -1f;
	}
}
