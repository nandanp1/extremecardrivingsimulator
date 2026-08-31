using UnityEngine;

public interface GUIFacebookScreenshotModeExtension
{
	void SetBackButtonFunctionToNone();

	void SetBackButtonFunctionToMinimizeResumeScreenshot();

	void SetBackButtonFunctionToCancelScreenshot();

	void SetBackButtonFunctionToLast();

	void SetBackButtonFunctionToResumeScreenshot();

	void PreInitMode();

	void PostFinalizeMode();

	void BeforePerformCameraRotation();

	Camera GetCurrentCamera();
}
