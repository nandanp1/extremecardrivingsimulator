using UnityEngine;

[RequireComponent(typeof(UILabel))]
public class UIAutoResizeLabel : MonoBehaviour
{
	private UILabel label;

	[SerializeField]
	private UIWidget sprite;

	[SerializeField]
	private UIWidget centerIn;

	public int spacing = 16;

	public void RecenterInParent()
	{
		if (!label)
		{
			label = GetComponent<UILabel>();
		}
		label.overflowMethod = UILabel.Overflow.ResizeFreely;
		sprite.transform.parent = label.transform;
		float num = centerIn.width;
		float num2 = label.width + spacing + sprite.width;
		float num3 = (num - num2) * 0.5f;
		float x = centerIn.transform.localPosition.x - num * 0.5f + num3 + (float)label.width * 0.5f;
		float x2 = (float)label.width * 0.5f + (float)spacing + (float)sprite.width * 0.5f;
		label.transform.localPosition = new Vector3(x, label.transform.localPosition.y);
		sprite.transform.localPosition = new Vector3(x2, sprite.transform.localPosition.y);
		label.MakePixelPerfect();
	}
}
