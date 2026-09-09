using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Batch entry point: -executeMethod HandbrakeHudScaleDiagnostic.Run
// Uses the actual 2Game handbrake anchors with the real NGUI layout code. The
// shipping scene is read-only; a unique temporary scene is saved and cleaned
// up because Unity disallows additive creation beside an untitled scene.
// No gameplay singleton is required.
public static class HandbrakeHudScaleDiagnostic
{
    private const string ScenePath = "Assets/Scenes/Common/2Game.unity";

    [Serializable]
    private sealed class Sample
    {
        public string phase;
        public float aspect;
        public float panelScale;
        public int width;
        public int height;
        public Vector2 colliderSize;
    }

    [Serializable]
    private sealed class Report
    {
        public string source = ScenePath;
        public string method = "Real NGUI UISprite anchors from the shipping scene; scale, press/release, additive-scene and repeated HUD-enable lifecycle.";
        public bool completed;
        public string error;
        public int legacyHeightAfterScale;
        public List<Sample> samples = new List<Sample>();
    }

    public static void Run()
    {
        Report report = new Report();
        int exitCode = 0;
        string temporaryScenePath = null;
        UICamera.MouseOrTouch previousTouch = UICamera.currentTouch;
        UICamera.ControlScheme previousScheme = UICamera.currentScheme;
        try
        {
            string sceneText = File.ReadAllText(ScenePath);
            string[] spriteBlocks = Regex.Split(sceneText, @"(?m)^--- ")
                .Where(block => Regex.IsMatch(block, @"(?m)^  mSpriteName: handbrake\s*$"))
                .ToArray();
            Require(spriteBlocks.Length == 1, "Expected one handbrake sprite in 2Game.");
            string source = spriteBlocks[0];
            int expectedWidth = ReadInt(source, "mWidth");
            int expectedHeight = ReadInt(source, "mHeight");
            Require(expectedWidth == 102 && expectedHeight == 102, "Unexpected handbrake design size.");

            var fixtureScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            temporaryScenePath = AssetDatabase.GenerateUniqueAssetPath(
                "Assets/Editor/HandbrakeHudScaleDiagnosticFixture.unity");
            Require(EditorSceneManager.SaveScene(fixtureScene, temporaryScenePath), "Could not save the temporary layout fixture scene.");
            GameObject cameraObject = new GameObject("HandbrakeLayoutCamera", typeof(Camera));
            Camera camera = cameraObject.GetComponent<Camera>();
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographic = true;
            camera.orthographicSize = 360f;
            camera.cullingMask = 1 << 5;

            GameObject panelObject = new GameObject("PnHUD", typeof(UIPanel));
            panelObject.layer = 5;
            UIPanel panel = panelObject.GetComponent<UIPanel>();
            panel.clipping = UIDrawCall.Clipping.None;

            GameObject buttonObject = new GameObject("BtHandBrake", typeof(BoxCollider2D));
            buttonObject.layer = 5;
            buttonObject.transform.SetParent(panelObject.transform, false);
            UISprite sprite = buttonObject.AddComponent<UISprite>();
            sprite.width = expectedWidth;
            sprite.height = expectedHeight;
            sprite.autoResizeBoxCollider = true;
            sprite.updateAnchors = UIRect.AnchorUpdate.OnEnable;
            SetAnchor(source, "leftAnchor", sprite.leftAnchor, panelObject.transform);
            SetAnchor(source, "rightAnchor", sprite.rightAnchor, panelObject.transform);
            SetAnchor(source, "bottomAnchor", sprite.bottomAnchor, panelObject.transform);
            SetAnchor(source, "topAnchor", sprite.topAnchor, panelObject.transform);
            UIButton button = buttonObject.AddComponent<UIButton>();
            UISlideButton slideButton = buttonObject.AddComponent<UISlideButton>();
            button.tweenTarget = buttonObject;
            slideButton.tweenTarget = buttonObject;
            UICamera.currentScheme = UICamera.ControlScheme.Touch;
            UICamera.currentTouch = new UICamera.MouseOrTouch { current = buttonObject };
            panel.ResetAnchors();
            sprite.ResetAnchors();

            foreach (float aspect in new[] { 16f / 9f, 20f / 9f, 21f / 9f })
            {
                SetViewportAspect(camera, aspect);
                panelObject.transform.localScale = Vector3.one;
                Check(report, "initial", aspect, sprite, expectedWidth, expectedHeight);
                float initialCenterY = sprite.transform.localPosition.y;
                Require(Mathf.Abs(initialCenterY - 57f) < 0.01f, "Initial handbrake anchor position moved.");

                float scale = aspect >= 2f ? 0.68f : 1f;
                panelObject.transform.localScale = new Vector3(scale, scale, 1f);
                for (int cycle = 0; cycle < 5; cycle++)
                {
                    panelObject.SetActive(false);
                    var additive = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                    panelObject.SetActive(true);
                    Check(report, "zone-enable-" + cycle, aspect, sprite, expectedWidth, expectedHeight);
                    UICamera.currentTouch.pressed = buttonObject;
                    buttonObject.SendMessage("OnPress", true, SendMessageOptions.DontRequireReceiver);
                    Require(button.state == UIButtonColor.State.Pressed && slideButton.state == UIButtonColor.State.Pressed,
                        "Touch press did not reach both handbrake button components.");
                    Check(report, "pressed-" + cycle, aspect, sprite, expectedWidth, expectedHeight);
                    buttonObject.SendMessage("OnPress", false, SendMessageOptions.DontRequireReceiver);
                    UICamera.currentTouch.pressed = null;
                    Require(button.state == UIButtonColor.State.Normal && slideButton.state == UIButtonColor.State.Normal,
                        "Touch release did not reset both handbrake button components.");
                    Check(report, "released-" + cycle, aspect, sprite, expectedWidth, expectedHeight);
                    EditorSceneManager.CloseScene(additive, true);
                }
            }

            // Negative control: the recovered mismatched anchor fractions must
            // reproduce the growth, proving this test exercises the reported bug.
            SetViewportAspect(camera, 20f / 9f);
            panelObject.transform.localScale = new Vector3(0.68f, 0.68f, 1f);
            sprite.bottomAnchor.relative = 0f;
            sprite.bottomAnchor.absolute = 366;
            sprite.ResetAnchors();
            sprite.UpdateAnchors();
            report.legacyHeightAfterScale = sprite.height;
            Require(sprite.height > expectedHeight * 2, "Legacy anchor negative control did not reproduce growth.");
            report.completed = true;
            Debug.Log("[HandbrakeHudScale] PASS: 102x102 after 15 zone cycles and 30 press/release events; old anchors grow to " + sprite.height + "px.");
        }
        catch (Exception exception)
        {
            exitCode = 1;
            report.error = exception.ToString();
            Debug.LogException(exception);
        }
        finally
        {
            UICamera.currentTouch = previousTouch;
            UICamera.currentScheme = previousScheme;
            try
            {
                if (!string.IsNullOrEmpty(temporaryScenePath))
                {
                    // Unload only these throwaway test scenes before removing
                    // the uniquely generated fixture asset and its metadata.
                    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    if (File.Exists(temporaryScenePath))
                        Require(AssetDatabase.DeleteAsset(temporaryScenePath), "Could not remove the temporary layout fixture scene.");
                }
            }
            catch (Exception exception)
            {
                exitCode = 1;
                report.completed = false;
                report.error = (report.error ?? string.Empty) + "\nFixture cleanup: " + exception;
                Debug.LogException(exception);
            }
            string output = Environment.GetEnvironmentVariable("RACING_HUD_DIAGNOSTIC_OUTPUT");
            string json = JsonUtility.ToJson(report, true);
            if (!string.IsNullOrEmpty(output))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));
                File.WriteAllText(output, json);
            }
            Debug.Log("[HandbrakeHudScale] " + json);
            EditorApplication.Exit(exitCode);
        }
    }

    private static int ReadInt(string source, string name)
    {
        Match match = Regex.Match(source, @"(?m)^  " + name + @":\s*(\d+)\s*$");
        Require(match.Success, "Missing " + name + " in handbrake sprite.");
        return int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
    }

    private static void SetViewportAspect(Camera camera, float aspect)
    {
        // This NGUI version uses Screen size and camera.rect rather than
        // camera.aspect when calculating orthographic camera sides.
        Vector2 screen = NGUITools.screenSize;
        Require(screen.x > 0f && screen.y > 0f, "Editor did not provide a usable viewport size.");
        float ratio = aspect / (screen.x / screen.y);
        camera.rect = new Rect(0f, 0f, Mathf.Min(1f, ratio), Mathf.Min(1f, 1f / ratio));
        Vector3[] sides = camera.GetSides(10f);
        float actualAspect = (sides[2].x - sides[0].x) / (sides[1].y - sides[3].y);
        Require(Mathf.Abs(actualAspect - aspect) < 0.001f, "NGUI viewport aspect did not match the test case.");
    }

    private static void SetAnchor(string source, string name, UIRect.AnchorPoint anchor, Transform target)
    {
        Match match = Regex.Match(source, @"(?m)^  " + name + @":\r?\n    target:[^\r\n]+\r?\n    relative: ([^\r\n]+)\r?\n    absolute: ([^\r\n]+)");
        Require(match.Success, "Missing " + name + " in handbrake sprite.");
        anchor.Set(target, float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
            int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture));
    }

    private static void Check(Report report, string phase, float aspect, UISprite sprite, int width, int height)
    {
        sprite.ResetAnchors();
        sprite.UpdateAnchors();
        BoxCollider2D collider = sprite.GetComponent<BoxCollider2D>();
        sprite.ResizeCollider();
        report.samples.Add(new Sample { phase = phase, aspect = aspect, panelScale = sprite.transform.parent.localScale.x,
            width = sprite.width, height = sprite.height, colliderSize = collider.size });
        Require(sprite.width == width && sprite.height == height, phase + ": handbrake dimensions changed.");
        Require((collider.size - new Vector2(width, height)).sqrMagnitude < 0.01f, phase + ": touch target dimensions changed.");
        Require((sprite.transform.localScale - Vector3.one).sqrMagnitude < 0.0001f, phase + ": button scale changed.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
