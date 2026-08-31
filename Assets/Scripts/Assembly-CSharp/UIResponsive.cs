using UnityEngine;

[RequireComponent(typeof(UIWidget))]
public class UIResponsive : MonoBehaviour
{
	private UIWidget uiWidget;

	public int screenWidthLimit = 500;

	[Tooltip("Max value to low resolutions")]
	public AnimationCurve scaleCurve;

	public AnimationCurve adjustAnchorsCurve;

	private UIRect.AnchorPoint originalTopAnchor;

	private UIRect.AnchorPoint originalBottomAnchor;

	private UIRect.AnchorPoint originalLeftAnchor;

	private UIRect.AnchorPoint originalRightAnchor;

	private Vector2 screenSize;

	private float screenDPI;

	[Header("Adjust Widget")]
	public bool adjustToLeft;

	public int leftAnchorAdjust = 90;

	public bool adjustToRight;

	public int rightAnchorAdjust = -30;

	private void Awake()
	{
		uiWidget = GetComponent<UIWidget>();
		uiWidget.keepAspectRatio = UIWidget.AspectRatioSource.BasedOnWidth;
		originalBottomAnchor = uiWidget.bottomAnchor;
		originalLeftAnchor = uiWidget.leftAnchor;
		originalRightAnchor = uiWidget.rightAnchor;
		originalTopAnchor = uiWidget.topAnchor;
		screenSize = NGUITools.screenSize;
		screenDPI = Mathf.Clamp(Screen.dpi, 1f, float.PositiveInfinity);
		screenWidthLimit = Mathf.RoundToInt((float)screenWidthLimit * (screenDPI * 0.01f));
		float num = scaleCurve.Evaluate((float)screenWidthLimit / screenSize.x);
		float num2 = adjustAnchorsCurve.Evaluate((float)screenWidthLimit / screenSize.x);
		int num3 = Mathf.RoundToInt((float)originalLeftAnchor.absolute * num);
		int num4 = Mathf.RoundToInt((float)originalRightAnchor.absolute * num);
		float f = (float)num3 + (float)leftAnchorAdjust * num2;
		float f2 = (float)num4 + (float)rightAnchorAdjust * num2;
		uiWidget.gameObject.transform.localScale = Vector3.one * num;
		uiWidget.leftAnchor.absolute = ((!adjustToLeft) ? num3 : Mathf.RoundToInt(f));
		uiWidget.rightAnchor.absolute = ((!adjustToRight) ? num4 : Mathf.RoundToInt(f2));
		uiWidget.topAnchor.absolute = Mathf.RoundToInt((float)originalTopAnchor.absolute * num);
		uiWidget.bottomAnchor.absolute = Mathf.RoundToInt((float)originalBottomAnchor.absolute * num);
	}
}
