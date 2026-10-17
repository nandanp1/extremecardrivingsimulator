using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Batch entry: -executeMethod OffroadLoadingDiagnostic.Run (omit -quit).
// Runs real PlayMode scenes and their coroutines; never saves scenes or prefabs.
public static class OffroadLoadingDiagnostic
{
    private const string Forest = "2Game_Forest";
    private const string InstalledKey = "key2Game_Forestinstalled";
    private enum Stage { None, Menu, Game, Loading, Unloading, Settling, Stopping }

    [Serializable]
    private sealed class SettleSample
    {
        public double simulationSeconds;
        public float bodyHeight;
        public float verticalVelocity;
        public int forestGroundedWheels;
    }

    [Serializable]
    private sealed class SettleResult
    {
        public string method = "Move actual local car to Forest spawn, restore its normal constraints, invoke real GameLogic.Reset upright-and-drop, then observe at least five seconds of normal fixed-step physics without further position or velocity writes.";
        public Vector3 authoredSpawn;
        public float referenceGroundHeight;
        public float initialBodyHeight;
        public float minimumBodyHeight;
        public float finalBodyHeight;
        public float finalVerticalVelocity;
        public int finalForestGroundedWheels;
        public int wheelCount;
        public double simulationSeconds;
        public bool remainedDynamic = true;
        public bool passed;
        public List<SettleSample> samples = new List<SettleSample>();
    }

    [Serializable]
    private sealed class GroundProbe
    {
        public string name;
        public Vector3 origin;
        public bool requireForestCollider;
        public bool hit;
        public string collider;
        public string scene;
        public Vector3 point;
    }

    [Serializable]
    private sealed class Visit
    {
        public string name;
        public bool installedBefore;
        public bool installedAfter;
        public string loadTrigger;
        public string localCollider;
        public bool rejectedForeignCar;
        public bool observedLoading;
        public bool observedPausedLimit;
        public bool hudRestored;
        public bool loadingPanelsHidden;
        public float restoredTimeScale;
        public bool rootPresent;
        public int rootCount;
        public int renderers;
        public int invalidRenderBounds;
        public int oversizedRenderBounds;
        public float maximumRendererExtent;
        public int terrainColliders;
        public int meshColliders;
        public int groundHits;
        public string groundCollider;
        public Vector3 groundPoint;
        public List<GroundProbe> requiredGroundProbes = new List<GroundProbe>();
        public double elapsedSeconds;
        public bool passed;
    }

    [Serializable]
    private sealed class LogEntry
    {
        public string stage;
        public string type;
        public string message;
        public string stack;
    }

    [Serializable]
    private sealed class Report
    {
        public string unityVersion;
        public string method = "Real PlayMode: 0Start -> real menu GoToGame -> Free mode local car. Dispatch existing Forest load/limit/unload ProcessTriggerEvent handlers with the real local BasicCollider. First visit forces only Forest installed=false; repeat visit uses the persisted installed=true path. Finally move the real car to Forest spawn and exercise Reset followed by at least five seconds of unconstrained settling and Forest wheel-contact checks.";
        public string limitations = "Trigger callbacks are injected, not reached by driving. Verifies real loading coroutines, scene geometry, collider raycasts, HUD object state and stationary vehicle support after a natural reset drop; does not prove driving across triggers, every terrain location, touch input, camera appearance, Android rendering or frame pacing. Startup warnings/errors are retained for review; errors during Forest transitions or settling fail the run.";
        public bool enteredPlayMode;
        public bool enteredMenu;
        public bool enteredFreeGame;
        public bool unloadedBetweenVisits;
        public bool preferencesRestored;
        public bool editorSettingsRestored;
        public bool completed;
        public bool passed;
        public string error;
        public string lastStage;
        public int loadingErrors;
        public int omittedLogs;
        public string screenshotPath;
        public string screenshotNote;
        public List<Visit> visits = new List<Visit>();
        public SettleResult settle;
        public List<LogEntry> logs = new List<LogEntry>();
    }

    private static Report report;
    private static Stage stage;
    private static double deadline;
    private static double stageStarted;
    private static double readySince;
    private static double loadFinishedAt;
    private static string output;
    private static bool previousOptionsEnabled;
    private static EnterPlayModeOptions previousOptions;
    private static float previousTimeScale;
    private static bool settingsCaptured;
    private static bool preferenceCaptured;
    private static string encryptedInstalledKey;
    private static bool previousPlainExists;
    private static int previousPlainValue;
    private static bool previousEncryptedExists;
    private static string previousEncryptedValue;
    private static LevelZone forest;
    private static Collider localCollider;
    private static Rigidbody localBody;
    private static RigidbodyConstraints previousConstraints;
    private static LevelZoneTrigger loadTrigger;
    private static LevelZoneTrigger limitTrigger;
    private static LevelZoneTrigger unloadTrigger;
    private static Visit visit;
    private static bool writingReport;
    private static double settleStarted;
    private static double lastSettleSample;
    private static WheelCollider[] settleWheels;
    private static GameObject settleForestRoot;

    public static void Run()
    {
        if (!Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Run OffroadLoadingDiagnostic only in a fresh batchmode Editor process outside PlayMode.");
        report = new Report { unityVersion = Application.unityVersion };
        output = Environment.GetEnvironmentVariable("RACING_OFFROAD_DIAGNOSTIC_OUTPUT");
        if (string.IsNullOrEmpty(output)) output = Path.GetFullPath("Builds/Diagnostics/offroad-loading.json");
        try
        {
            previousOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            previousOptions = EditorSettings.enterPlayModeOptions;
            previousTimeScale = Time.timeScale;
            settingsCaptured = true;
            // Preserve this editor-only controller across PlayMode entry/exit.
            // Scene reload remains enabled, so gameplay Awake/Start run normally.
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            Application.logMessageReceived += OnLog;
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorSceneManager.OpenScene("Assets/Scenes/Common/0Start.unity", OpenSceneMode.Single);
            SetStage(Stage.Menu, 90);
            EditorApplication.isPlaying = true;
        }
        catch (Exception exception) { Fail(exception); }
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode) report.enteredPlayMode = true;
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            if (stage != Stage.Stopping) report.error = "PlayMode ended before the diagnostic completed.";
            Finish();
        }
    }

    private static void Tick()
    {
        if (stage == Stage.None) return;
        try
        {
            if (EditorApplication.timeSinceStartup > deadline)
                throw new TimeoutException("Timed out in " + stage + "; Forest state=" + (forest == null ? "missing" : forest.statusLevel.ToString()) + ", timeScale=" + Time.timeScale);
            if (stage == Stage.Stopping) return;
            if (!EditorApplication.isPlaying) return;
            EditorApplication.QueuePlayerLoopUpdate();
            if (stage == Stage.Menu)
            {
                GUIMenuLogic menu = UnityEngine.Object.FindFirstObjectByType<GUIMenuLogic>();
                if (menu == null || menu.currentCar == null) { readySince = 0; return; }
                if (!ReadyFor(1)) return;
                report.enteredMenu = true;
                menu.GoToGame();
                SetStage(Stage.Game, 90);
            }
            else if (stage == Stage.Game)
            {
                Manager manager = UnityEngine.Object.FindFirstObjectByType<Manager>();
                if (manager == null || manager.gameLogic == null || manager.gameLogic.instantiatedCar == null || manager.guiGameLogic == null || !manager.guiGameLogic.pnHud.gameObject.activeInHierarchy || (manager.gameLogic.rootLoading != null && manager.gameLogic.rootLoading.activeInHierarchy))
                { readySince = 0; return; }
                if (!ReadyFor(3)) return;
                Require(manager.gameMode == Manager.GameMode.Free, "Menu did not enter Free mode.");
                report.enteredFreeGame = true;
                forest = UnityEngine.Object.FindObjectsByType<LevelZone>(FindObjectsSortMode.None).Single(value => value.nameSceneZone == Forest);
                Require(forest.needInstall, "Forest no longer uses an installed-first-visit path; diagnostic needs updating.");
                Require(forest.isUnloaded, "Forest was already loaded before its first trigger.");
                LevelZoneTrigger[] triggers = forest.GetComponentsInChildren<LevelZoneTrigger>(true);
                loadTrigger = triggers.First(value => value.isTriggerForLoad);
                limitTrigger = triggers.First(value => value.isTriggerForLimitLoadingZone);
                unloadTrigger = triggers.First(value => value.isTriggerForUnload);
                localBody = manager.gameLogic.instantiatedCar.GetComponent<Rigidbody>();
                localCollider = manager.gameLogic.instantiatedCar.GetComponentsInChildren<Collider>(true).First(value => value.enabled && value.gameObject.activeInHierarchy && value.gameObject.layer == LayerMask.NameToLayer("BasicCollider") && value.attachedRigidbody == localBody);
                previousConstraints = localBody.constraints;
                localBody.linearVelocity = Vector3.zero;
                localBody.angularVelocity = Vector3.zero;
                localBody.constraints = RigidbodyConstraints.FreezeAll;
                CapturePreference();
                SettingsManager.SetBool(InstalledKey, false);
                BeginVisit("first-install");
            }
            else if (stage == Stage.Loading)
            {
                if (!forest.isLoaded) return;
                if (loadFinishedAt == 0) loadFinishedAt = EditorApplication.timeSinceStartup;
                if (EditorApplication.timeSinceStartup - loadFinishedAt < 1) return;
                ObserveVisit();
                Require(visit.passed, "Forest " + visit.name + " failed; see visit fields and transition logs.");
                if (report.visits.Count == 1)
                {
                    forest.ProcessTriggerEvent(unloadTrigger, localCollider, false);
                    SetStage(Stage.Unloading, 30);
                }
                else
                {
                    BeginSettling();
                }
            }
            else if (stage == Stage.Unloading)
            {
                if (!forest.isUnloaded || GameObject.Find(Forest) != null) return;
                if (!ReadyFor(0.5)) return;
                report.unloadedBetweenVisits = true;
                Require(SettingsManager.GetBool(InstalledKey, false), "First visit did not persist the installed flag.");
                BeginVisit("repeat-async");
            }
            else if (stage == Stage.Settling)
            {
                ObserveSettling();
            }
        }
        catch (Exception exception) { Fail(exception); }
    }

    private static bool ReadyFor(double seconds)
    {
        if (readySince == 0) readySince = EditorApplication.timeSinceStartup;
        return EditorApplication.timeSinceStartup - readySince >= seconds;
    }

    private static void BeginSettling()
    {
        GameLogic logic = MySingleton<Manager>.Instance.gameLogic;
        Require(localBody != null && forest.spawnTransformZone != null, "Local car or Forest spawn disappeared before settling.");
        Require((previousConstraints & RigidbodyConstraints.FreezePositionY) == 0, "Saved gameplay constraints would prevent a natural fall.");
        settleForestRoot = GameObject.Find(Forest);
        Require(settleForestRoot != null, "Forest root disappeared before settling.");
        SetStage(Stage.Settling, 45);
        report.settle = new SettleResult
        {
            authoredSpawn = forest.spawnTransformZone.position,
            referenceGroundHeight = visit.requiredGroundProbes.Single(probe => probe.name == "forest-spawn").point.y
        };
        localBody.constraints = previousConstraints;
        Require(!localBody.isKinematic && localBody.useGravity, "The local car must already be dynamic with gravity before its reset drop.");
        localBody.transform.SetPositionAndRotation(forest.spawnTransformZone.position, forest.spawnTransformZone.rotation);
        Physics.SyncTransforms();
        // Reset is the shipping upright-and-drop action. It cancels the spawn
        // stabilizer and raises the body three meters; it does not freeze it.
        logic.Reset();
        localBody.WakeUp();
        settleWheels = localBody.GetComponentsInChildren<WheelCollider>(true);
        report.settle.wheelCount = settleWheels.Length;
        Require(settleWheels.Length >= 3, "Local car does not have enough WheelColliders for the support check.");
        report.settle.initialBodyHeight = localBody.position.y;
        report.settle.minimumBodyHeight = localBody.position.y;
        settleStarted = Time.fixedTimeAsDouble;
        lastSettleSample = -1;
    }

    private static void ObserveSettling()
    {
        SettleResult result = report.settle;
        Require(localBody != null && settleForestRoot != null, "Local car or Forest root disappeared while settling.");
        result.simulationSeconds = Time.fixedTimeAsDouble - settleStarted;
        result.finalBodyHeight = localBody.position.y;
        result.minimumBodyHeight = Mathf.Min(result.minimumBodyHeight, result.finalBodyHeight);
        result.finalVerticalVelocity = localBody.linearVelocity.y;
        result.remainedDynamic &= !localBody.isKinematic && localBody.useGravity && (localBody.constraints & RigidbodyConstraints.FreezePositionY) == 0;
        Require(result.remainedDynamic, "Settling car became kinematic, lost gravity or froze its vertical position.");
        Require(Finite(localBody.position) && Finite(localBody.linearVelocity), "Settling car position or velocity is not finite.");
        Require(result.finalBodyHeight >= result.referenceGroundHeight - 1f, "Local car fell below the Forest ground during the natural reset drop.");
        result.finalForestGroundedWheels = 0;
        foreach (WheelCollider wheel in settleWheels)
        {
            if (wheel.enabled && wheel.GetGroundHit(out WheelHit hit) && hit.collider != null && hit.collider.transform.IsChildOf(settleForestRoot.transform))
                result.finalForestGroundedWheels++;
        }
        if (result.simulationSeconds - lastSettleSample >= 0.5 || result.simulationSeconds >= 5)
        {
            result.samples.Add(new SettleSample
            {
                simulationSeconds = result.simulationSeconds,
                bodyHeight = result.finalBodyHeight,
                verticalVelocity = result.finalVerticalVelocity,
                forestGroundedWheels = result.finalForestGroundedWheels
            });
            lastSettleSample = result.simulationSeconds;
        }
        if (result.simulationSeconds < 5) return;
        // The prefab root is not the chassis bottom, and the sloping ground at
        // the four tires differs from the center spawn ray. Actual tire contact
        // proves support; retain the per-frame fall guard instead of requiring
        // the root pivot to sit above that single center-ground sample.
        result.passed = result.remainedDynamic && result.finalForestGroundedWheels >= 3 && Mathf.Abs(result.finalVerticalVelocity) < 2f && result.finalBodyHeight >= result.referenceGroundHeight - 1f && result.finalBodyHeight < result.referenceGroundHeight + 4f && report.loadingErrors == 0;
        Debug.Log("[OffroadLoadingDiagnostic] Settling " + JsonUtility.ToJson(result));
        Require(result.passed, "Real local car did not settle with at least three wheels supported by Forest ground; see settle evidence.");
        CaptureSettledCamera();
        report.completed = true;
        Stop();
    }

    private static void CaptureSettledCamera()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            report.screenshotNote = "Skipped: null graphics device (-nographics).";
            return;
        }
        Camera camera = Camera.main;
        if (camera == null) { report.screenshotNote = "Skipped: no main camera."; return; }
        RenderTexture oldTarget = camera.targetTexture;
        RenderTexture oldActive = RenderTexture.active;
        RenderTexture target = null;
        Texture2D pixels = null;
        try
        {
            target = RenderTexture.GetTemporary(1280, 720, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            pixels.Apply();
            string path = Path.ChangeExtension(Path.GetFullPath(output), ".png");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, pixels.EncodeToPNG());
            report.screenshotPath = path;
            report.screenshotNote = "Main camera after the real car's five-second Forest reset drop; offscreen 1280x720 render, no camera or scene assets saved.";
        }
        catch (Exception exception) { report.screenshotNote = "Optional capture failed: " + exception.Message; }
        finally
        {
            camera.targetTexture = oldTarget;
            RenderTexture.active = oldActive;
            if (pixels != null) UnityEngine.Object.DestroyImmediate(pixels);
            if (target != null) RenderTexture.ReleaseTemporary(target);
        }
    }

    private static void SetStage(Stage next, double seconds)
    {
        stage = next;
        stageStarted = EditorApplication.timeSinceStartup;
        deadline = stageStarted + seconds;
        readySince = loadFinishedAt = 0;
        report.lastStage = next.ToString();
        Debug.Log("[OffroadLoadingDiagnostic] " + next);
    }

    private static void BeginVisit(string name)
    {
        visit = new Visit { name = name, installedBefore = SettingsManager.GetBool(InstalledKey, false), loadTrigger = loadTrigger.name, localCollider = localCollider.name };
        report.visits.Add(visit);
        SetStage(Stage.Loading, 90);
        // Negative control: a different rigidbody on the correct layer must not
        // own zone loading. This exercises the actual local-player guard.
        var foreign = new GameObject("OffroadDiagnostic_ForeignCar");
        try
        {
            foreign.layer = localCollider.gameObject.layer;
            var body = foreign.AddComponent<Rigidbody>();
            body.isKinematic = true;
            Collider collider = foreign.AddComponent<BoxCollider>();
            forest.ProcessTriggerEvent(loadTrigger, collider, true);
            visit.rejectedForeignCar = forest.isUnloaded;
        }
        finally { UnityEngine.Object.Destroy(foreign); }
        Require(visit.rejectedForeignCar, "Foreign-rigidbody negative control incorrectly started Forest loading.");
        forest.ProcessTriggerEvent(loadTrigger, localCollider, true);
        visit.observedLoading = forest.isLoading;
        Require(visit.observedLoading, "Real local-car collider did not start the loading coroutine.");
        // Exercise the actual wait-at-boundary path, including its unscaled wait.
        forest.ProcessTriggerEvent(limitTrigger, localCollider, true);
        visit.observedPausedLimit = Time.timeScale == 0f && !MySingleton<Manager>.Instance.guiGameLogic.pnHud.gameObject.activeInHierarchy;
        Require(visit.observedPausedLimit, "Limit-loading trigger did not pause gameplay and hide the HUD.");
    }

    private static void ObserveVisit()
    {
        GUIGameLogic gui = MySingleton<Manager>.Instance.guiGameLogic;
        visit.elapsedSeconds = EditorApplication.timeSinceStartup - stageStarted;
        visit.installedAfter = SettingsManager.GetBool(InstalledKey, false);
        visit.hudRestored = gui.pnHud.gameObject.activeInHierarchy;
        visit.loadingPanelsHidden = !gui.pnChangeZone.gameObject.activeInHierarchy && !gui.pnLoadingBg.gameObject.activeInHierarchy && !gui.spMiniLoading.gameObject.activeInHierarchy;
        visit.restoredTimeScale = Time.timeScale;
        GameObject root = GameObject.Find(Forest);
        visit.rootPresent = root != null && root.activeInHierarchy;
        visit.rootCount = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Count(value => value.name == Forest);
        if (root != null)
        {
            Physics.SyncTransforms();
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (!renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                visit.renderers++;
                Bounds bounds = renderer.bounds;
                if (!Finite(bounds.center) || !Finite(bounds.extents)) { visit.invalidRenderBounds++; continue; }
                float extent = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
                visit.maximumRendererExtent = Mathf.Max(visit.maximumRendererExtent, extent);
                if (extent > 20000f) visit.oversizedRenderBounds++;
            }
            foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
            {
                if (!collider.enabled || !collider.gameObject.activeInHierarchy || collider.isTrigger) continue;
                if (collider is TerrainCollider terrain && terrain.terrainData != null) visit.terrainColliders++;
                else if (collider is MeshCollider mesh && mesh.sharedMesh != null) visit.meshColliders++;
                else continue;
                Bounds bounds = collider.bounds;
                if (!Finite(bounds.center) || !Finite(bounds.extents) || bounds.size.sqrMagnitude < 0.01f) continue;
                foreach (float x in new[] { 0.25f, 0.5f, 0.75f })
                foreach (float z in new[] { 0.25f, 0.5f, 0.75f })
                {
                    Vector3 origin = new Vector3(Mathf.Lerp(bounds.min.x, bounds.max.x, x), bounds.max.y + 20f, Mathf.Lerp(bounds.min.z, bounds.max.z, z));
                    if (!collider.Raycast(new Ray(origin, Vector3.down), out RaycastHit hit, bounds.size.y + 40f) || hit.normal.y < 0.25f || !Finite(hit.point)) continue;
                    visit.groundHits++;
                    if (visit.groundCollider == null) { visit.groundCollider = collider.name; visit.groundPoint = hit.point; }
                }
            }
            Require(forest.spawnTransformZone != null, "Forest spawn transform is missing.");
            Vector3 spawn = forest.spawnTransformZone.position;
            AddGroundProbe("forest-spawn", spawn + Vector3.up * 5f, root, true);
            AddGroundProbe("forest-spawn-west", spawn + new Vector3(-3f, 5f, 0f), root, true);
            AddGroundProbe("forest-spawn-east", spawn + new Vector3(3f, 5f, 0f), root, true);
            AddGroundProbe("forest-spawn-north", spawn + new Vector3(0f, 5f, 3f), root, true);
            AddGroundProbe("forest-spawn-south", spawn + new Vector3(0f, 5f, -3f), root, true);
            // The connecting tunnel can belong to the common gameplay scene.
            // Require real ground there, while spawn probes specifically require
            // collision geometry owned by the newly streamed Forest root.
            Vector3 entrance = limitTrigger.transform.position;
            // This tall trigger's center is around y=44, far above the road.
            // Use the authored Forest spawn elevation (about -3.21) for the
            // entrance ray origins; trigger-center height is not road height.
            entrance.y = spawn.y;
            AddGroundProbe("entrance-limit", entrance + Vector3.up * 5f, root, false);
            AddGroundProbe("entrance-west-10m", entrance + new Vector3(-10f, 5f, 0f), root, false);
            AddGroundProbe("entrance-west-30m", entrance + new Vector3(-30f, 5f, 0f), root, false);
        }
        visit.passed = visit.rejectedForeignCar && visit.observedLoading && visit.observedPausedLimit && visit.installedAfter && visit.hudRestored && visit.loadingPanelsHidden && Mathf.Abs(visit.restoredTimeScale - 1f) < 0.001f && visit.rootPresent && visit.rootCount == 1 && visit.renderers > 0 && visit.invalidRenderBounds == 0 && visit.oversizedRenderBounds == 0 && visit.groundHits > 0 && visit.requiredGroundProbes.Count == 8 && visit.requiredGroundProbes.All(probe => probe.hit) && report.loadingErrors == 0;
        Debug.Log("[OffroadLoadingDiagnostic] " + JsonUtility.ToJson(visit));
    }

    private static void AddGroundProbe(string name, Vector3 origin, GameObject root, bool requireForest)
    {
        var probe = new GroundProbe { name = name, origin = origin, requireForestCollider = requireForest };
        visit.requiredGroundProbes.Add(probe);
        // A floor more than 15 m below these authored driveable positions is a
        // missing playable surface, even if some unrelated geometry exists below.
        foreach (RaycastHit hit in Physics.RaycastAll(origin, Vector3.down, 20f, ~0, QueryTriggerInteraction.Ignore).OrderBy(value => value.distance))
        {
            if (hit.collider == null || hit.normal.y < 0.25f || !Finite(hit.point)) continue;
            if (localBody != null && hit.transform.IsChildOf(localBody.transform)) continue;
            if (requireForest && !hit.transform.IsChildOf(root.transform)) continue;
            if (!(hit.collider is MeshCollider) && !(hit.collider is TerrainCollider)) continue;
            probe.hit = true;
            probe.collider = hit.collider.name;
            probe.scene = hit.collider.gameObject.scene.name;
            probe.point = hit.point;
            break;
        }
    }

    private static bool Finite(Vector3 value)
    {
        return !float.IsNaN(value.x) && !float.IsInfinity(value.x) && !float.IsNaN(value.y) && !float.IsInfinity(value.y) && !float.IsNaN(value.z) && !float.IsInfinity(value.z);
    }

    private static void CapturePreference()
    {
        // Save the exact backing values before ObscuredPrefs can migrate a legacy
        // plain key. No profile-wide delete/reset is used.
        MethodInfo encrypt = typeof(ObscuredPrefs).GetMethod("EncryptKey", BindingFlags.NonPublic | BindingFlags.Static);
        Require(encrypt != null, "ObscuredPrefs.EncryptKey was not found.");
        encryptedInstalledKey = (string)encrypt.Invoke(null, new object[] { InstalledKey });
        previousPlainExists = PlayerPrefs.HasKey(InstalledKey);
        previousPlainValue = PlayerPrefs.GetInt(InstalledKey, 0);
        previousEncryptedExists = PlayerPrefs.HasKey(encryptedInstalledKey);
        previousEncryptedValue = PlayerPrefs.GetString(encryptedInstalledKey, string.Empty);
        preferenceCaptured = true;
    }

    private static void OnLog(string message, string stack, LogType type)
    {
        if (writingReport || report == null || type == LogType.Log) return;
        if ((stage == Stage.Loading || stage == Stage.Unloading || stage == Stage.Settling) && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)) report.loadingErrors++;
        if (report.logs.Count >= 200) { report.omittedLogs++; return; }
        report.logs.Add(new LogEntry { stage = stage.ToString(), type = type.ToString(), message = message, stack = stack });
    }

    private static void Fail(Exception exception)
    {
        report.error = (report.error == null ? "" : report.error + "\n") + exception;
        if (stage == Stage.Stopping) { Finish(); return; }
        Stop();
    }

    private static void Stop()
    {
        if (localBody != null) localBody.constraints = previousConstraints;
        SetStage(Stage.Stopping, 30);
        if (EditorApplication.isPlayingOrWillChangePlaymode) EditorApplication.isPlaying = false;
        else Finish();
    }

    private static void Finish()
    {
        stage = Stage.None;
        EditorApplication.update -= Tick;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        Application.logMessageReceived -= OnLog;
        try
        {
            if (preferenceCaptured)
            {
                if (previousPlainExists) PlayerPrefs.SetInt(InstalledKey, previousPlainValue); else PlayerPrefs.DeleteKey(InstalledKey);
                if (previousEncryptedExists) PlayerPrefs.SetString(encryptedInstalledKey, previousEncryptedValue); else PlayerPrefs.DeleteKey(encryptedInstalledKey);
                PlayerPrefs.Save();
            }
            report.preferencesRestored = true;
            if (settingsCaptured)
            {
                EditorSettings.enterPlayModeOptions = previousOptions;
                EditorSettings.enterPlayModeOptionsEnabled = previousOptionsEnabled;
                Time.timeScale = previousTimeScale;
            }
            report.editorSettingsRestored = true;
        }
        catch (Exception exception) { report.error = (report.error ?? "") + "\nCleanup: " + exception; }
        report.passed = report.completed && report.error == null && report.loadingErrors == 0 && report.unloadedBetweenVisits && report.visits.Count == 2 && report.visits.All(value => value.passed) && report.settle != null && report.settle.passed && report.preferencesRestored && report.editorSettingsRestored;
        int exitCode = report.passed ? 0 : 2;
        try
        {
            writingReport = true;
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));
            File.WriteAllText(output, JsonUtility.ToJson(report, true));
            Debug.Log("[OffroadLoadingDiagnostic] completed=" + report.completed + ", passed=" + report.passed + ", output=" + output);
            if (report.error != null) Debug.LogError("[OffroadLoadingDiagnostic] " + report.error);
        }
        catch (Exception exception) { Debug.LogError("[OffroadLoadingDiagnostic] Could not write report: " + exception); exitCode = 2; }
        EditorApplication.Exit(exitCode);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
