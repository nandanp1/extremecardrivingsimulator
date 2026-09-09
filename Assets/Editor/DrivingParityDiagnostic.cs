using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Editor-only integration diagnostic. Runs the recovered vehicle's own methods
// against Unity's native WheelCollider simulation; it never saves a scene/prefab.
// The manual update order is explicit, so final Android playtesting is still needed.
public static class DrivingParityDiagnostic
{
    private const float Dt = 0.02f;
    private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private const string PrefabPath = "Assets/Resources/Coche1.prefab";
    private static readonly Dictionary<string, MethodInfo> Methods = new Dictionary<string, MethodInfo>();
    private static StreamWriter csv;
    private static readonly List<Result> Results = new List<Result>();
    private static readonly List<FixtureStatus> Fixtures = new List<FixtureStatus>();

    [Serializable]
    private class FixtureStatus
    {
        public string name;
        public bool usable;
        public string note;
    }

    [Serializable]
    private class Report
    {
        public string unityVersion;
        public string method = "EditMode manual lifecycle + native Physics.Simulate; actual Coche1, CarMain, CarSettings, CarControl, CarWheel, CarAntiRollBar; no scene/prefab saves";
        public string limitations = "Explicit Update->CarControl.FixedUpdate->CarWheel.FixedUpdate->anti-roll order; no rendered HUD, frame pacing, collision damage, scene streaming, or audio; seeded-speed tests allow tire spin-up before measurement.";
        public float fixedDeltaTime = Dt;
        public bool completed;
        public string error;
        public Result[] scenarios;
        public bool naturalAirportRunupRequested;
        public string naturalAirportRunupStatus = "Not run; natural approach route remains unverified. Optional fixture requires RACING_PARITY_NATURAL_LOOP=1.";
        public FixtureStatus[] fixtures;
    }

    [Serializable]
    private class Result
    {
        public string name;
        public float initialDisplayKmh;
        public float[] speedAt123510Seconds = new float[5];
        public float endDisplayKmh;
        public float minimumDisplayKmh = float.MaxValue;
        public float maximumDisplayKmh;
        public float headingChangeDegrees;
        public float maximumLateralSlipAngleDegrees;
        public float maximumWheelSidewaysSlip;
        public float maximumWheelForwardSlip;
        public float maximumWheelRpm;
        public float maximumBodyTiltDegrees;
        public float minimumUpDot = 1f;
        public float maximumHeight;
        public float longestAirborneSeconds;
        public float firstStopSeconds = -1f;
        public float stopDistance = -1f;
        public float endDistance;
        public float elapsedSeconds;
        public float wheelAngleHalfSecondAfterRelease = -1f;
        public float wheelAngleFourTenthsAfterRelease = -1f;
        public float wheelAngleHalfSecondAfterReversal = -999f;
        public float yawRateHalfSecondAfterReversal = -999f;
        public float naturalLaunchSeconds;
        public float naturalLaunchInitialFilteredGas;
        public float groundedYawTravelAfterHandbrakeRelease;
        public float handbrakeFullyReleasedAt = -1f;
        public float finalBodySideslipDegrees;
        public float finalYawRate;
        public float initialSignedForwardMS;
        public float finalSignedForwardMS;
        public float brake233To126Seconds = -1f;
        public float maximumTransformBodyPositionDifference;
        public float maximumTransformBodyAngleDifference;
        public float finalTwoSecondsBodyHeightRange;
        public float finalTwoSecondsRearContactFraction;
        public float loopRampEntrySeconds = -1f;
        public float loopRampEntryDisplayKmh = -1f;
        public Vector3 loopRampEntryPosition;
        public float loopFirstInversionSeconds = -1f;
        public float loopFirstInversionDisplayKmh = -1f;
        public Vector3 loopFirstInversionPosition;
        public float loopExitSeconds = -1f;
        public float loopExitDisplayKmh = -1f;
        public Vector3 loopExitPosition;
        public float loopRampEntryToExitSeconds = -1f;
        public int groundedSteps;
        public int steps;
        public bool valid = true;
        public string note;
    }

    private sealed class Vehicle : IDisposable
    {
        public GameObject root;
        public Rigidbody body;
        public CarControl control;
        public CarSettings settings;
        public GameLogic logic;
        public CarWheel[] wheels;
        public CarAntiRollBar[] antiRoll;
        public Rigidbody[] bodies;
        private float nextPedalTick;
        private bool lastGas;
        private bool lastBrake;
        private bool lastHandbrake;
        private float elapsed;

        public Vehicle(Vector3 position, Quaternion rotation, GameLogic gameLogic)
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (source == null) throw new InvalidOperationException("Coche1 prefab missing.");
            root = UnityEngine.Object.Instantiate(source, position, rotation);
            root.name = "Coche1_DrivingParity";
            foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true)) behaviour.enabled = false;
            foreach (Transform child in root.transform)
                if (!child.gameObject.activeSelf && !child.CompareTag("InstantiateAI") && !child.CompareTag("FrontLight")) child.gameObject.SetActive(true);
            body = root.GetComponent<Rigidbody>();
            bodies = root.GetComponentsInChildren<Rigidbody>(true);
            control = root.GetComponent<CarControl>();
            settings = root.GetComponent<CarSettings>();
            wheels = new[] { control.WheelFL, control.WheelFR, control.WheelRL, control.WheelRR };
            antiRoll = root.GetComponents<CarAntiRollBar>();
            logic = gameLogic;
            logic.currentCar = root.transform;
            logic.carControl = control;
            logic.gas = logic.brakes = logic.handbreak = 0f;
            logic.ResetCarInput();
            settings.externalInput = false;
            settings.abs = settings.tc = settings.esp = true;
            control.readUserInput = true;
            Invoke(control, "Awake");
            foreach (CarWheel wheel in wheels) Invoke(wheel, "Start");
            Invoke(control, "Start");
            Invoke(settings, "Start");
            Invoke(settings, "Update");
            foreach (Rigidbody childBody in bodies)
            {
                childBody.linearVelocity = Vector3.zero;
                childBody.angularVelocity = Vector3.zero;
            }
            body.constraints = RigidbodyConstraints.None;
            Physics.SyncTransforms();
        }

        public void Step(bool gas = false, bool brake = false, bool handbrake = false, int steering = 0)
        {
            // InvokeRepeating starts immediately and repeats every 0.1 s. Calling
            // those exact methods also exercises their saturation/release behavior.
            if (elapsed + 0.00001f >= nextPedalTick || gas != lastGas || brake != lastBrake || handbrake != lastHandbrake)
            {
                Invoke(logic, gas ? "IncrementGas" : "DecrementGas");
                Invoke(logic, brake ? "IncrementBrake" : "DecrementBrake");
                Invoke(logic, handbrake ? "IncrementHandbrake" : "DecrementHandbrake");
                nextPedalTick = elapsed + 0.1f;
            }
            lastGas = gas;
            lastBrake = brake;
            lastHandbrake = handbrake;
            logic.DirLeft(steering < 0);
            logic.DirRight(steering > 0);
            AdvanceSteering(logic, Dt);
            Invoke(settings, "Update");
            Invoke(control, "Update");
            Invoke(control, "FixedUpdate");
            foreach (CarWheel wheel in wheels) Invoke(wheel, "FixedUpdate");
            foreach (CarAntiRollBar bar in antiRoll) Invoke(bar, "FixedUpdate");
            Physics.Simulate(Dt);
            elapsed += Dt;
        }

        public void Settle()
        {
            for (int i = 0; i < 150; i++) Step();
            if (Grounded() < 3 || body.linearVelocity.magnitude > 0.5f)
                throw new InvalidOperationException("Vehicle did not settle: grounded=" + Grounded() + ", speed=" + body.linearVelocity.magnitude);
        }

        public void SeedSpeed(float displayedKmh)
        {
            Vector3 velocity = root.transform.forward * (displayedKmh / 7.2f);
            // WheelCollider.rpm is read-only. Holding the chassis at the requested
            // speed briefly spins tires up through actual contact forces.
            for (int i = 0; i < 40; i++)
            {
                foreach (Rigidbody rb in bodies) rb.linearVelocity = velocity;
                Step();
            }
            foreach (Rigidbody rb in bodies) rb.linearVelocity = velocity;
            body.angularVelocity = Vector3.zero;
            logic.ResetCarInput();
            logic.handbreak = 0f;
            elapsed = nextPedalTick = 0f;
            Physics.SyncTransforms();
        }

        public int Grounded()
        {
            int count = 0;
            foreach (CarWheel wheel in wheels) if (wheel.getWheelCollider().GetGroundHit(out _)) count++;
            return count;
        }

        public void Dispose()
        {
            logic.currentCar = null;
            logic.carControl = null;
            UnityEngine.Object.DestroyImmediate(root);
            Physics.SyncTransforms();
        }
    }

    public static void Run()
    {
        string directory = Environment.GetEnvironmentVariable("RACING_PARITY_OUTPUT");
        if (string.IsNullOrEmpty(directory)) directory = Path.GetFullPath("Builds/Diagnostics/driving-parity");
        Directory.CreateDirectory(directory);
        Report report = new Report { unityVersion = Application.unityVersion };
        SimulationMode oldMode = Physics.simulationMode;
        float oldDt = Time.fixedDeltaTime;
        int exitCode = 0;
        try
        {
            if (!Application.isBatchMode || EditorApplication.isPlaying)
                throw new InvalidOperationException("Run only in a fresh batchmode Editor process, outside PlayMode.");
            Results.Clear();
            Fixtures.Clear();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Physics.simulationMode = SimulationMode.Script;
            Time.fixedDeltaTime = Dt;
            GameObject stubs = new GameObject("DrivingParity_InactiveLifecycleStubs");
            Manager manager = stubs.AddComponent<Manager>();
            manager.enabled = false;
            manager.controls = Manager.Controls.Buttons;
            // AddComponent<GameLogic> invokes its gameplay Reset as Unity's Editor
            // initialization callback, before currentCar exists. Clone the scene
            // component instead, so no lifecycle callback has to be suppressed.
            Scene activeScene = SceneManager.GetActiveScene();
            Scene preview = EditorSceneManager.OpenPreviewScene("Assets/Scenes/Common/2Game.unity");
            GameLogic sourceLogic = preview.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<GameLogic>(true)).Single();
            GameLogic logic = UnityEngine.Object.Instantiate(sourceLogic);
            SceneManager.MoveGameObjectToScene(logic.gameObject, activeScene);
            EditorSceneManager.ClosePreviewScene(preview);
            logic.enabled = false;
            manager.gameLogic = logic;
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "DrivingParity_FlatGround";
            ground.layer = LayerMask.NameToLayer("Soils");
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.transform.localScale = new Vector3(4000f, 1f, 4000f);
            UnityEngine.Object.DestroyImmediate(ground.GetComponent<MeshRenderer>());
            Physics.SyncTransforms();
            csv = new StreamWriter(Path.Combine(directory, "telemetry.csv"));
            csv.WriteLine("scenario,seconds,display_kmh,world_mps,forward_mps,x,y,z,heading_delta_deg,yaw_rate,body_up_dot,grounded,body_slip_deg,max_wheel_side_slip,gas,brake,handbrake,steer,left_wheel_deg,right_wheel_deg," +
                "fl_forward_slip,fl_rpm,fl_motor_nm,fl_brake_nm,fl_load_n,fr_forward_slip,fr_rpm,fr_motor_nm,fr_brake_nm,fr_load_n,rl_forward_slip,rl_rpm,rl_motor_nm,rl_brake_nm,rl_load_n,rr_forward_slip,rr_rpm,rr_motor_nm,rr_brake_nm,rr_load_n,transform_y,transform_body_distance,transform_body_angle,vertical_mps,body_pitch_deg,body_roll_deg");

            Flat(logic, "throttle_rise", 0f, 15f, t => new InputState(true));
            Flat(logic, "rolling_throttle_95", 95f, 7f, t => new InputState(true), false, true);
            Flat(logic, "rolling_throttle_49_reference_5p80s", 49f, 5.8f, t => new InputState(true), false, true);
            NaturalRolling(logic, "natural_rolling_throttle_95", 95f, 7f);
            NaturalRolling(logic, "natural_rolling_throttle_49_reference_5p80s", 49f, 5.8f);
            NaturalBrakeAndRecovery(logic);
            Flat(logic, "tc_off_launch", 0f, 15f, t => new InputState(true), false, false, false);
            Flat(logic, "tc_off_rolling_throttle_95", 95f, 7f, t => new InputState(true), false, true, false);
            Flat(logic, "coast_220", 220f, 10f, t => new InputState());
            Flat(logic, "coast_253_reference_2p28s", 253f, 2.28f, t => new InputState());
            Flat(logic, "coast_110_reference_1p42s", 110f, 1.42f, t => new InputState());
            foreach (float speed in new[] { 100f, 200f, 300f })
            {
                string suffix = speed.ToString("F0", CultureInfo.InvariantCulture);
                Flat(logic, "coast_" + suffix, speed, 10f, t => new InputState());
                Flat(logic, "brake_" + suffix, speed, 10f, t => new InputState(false, true), true);
                Flat(logic, "handbrake_" + suffix, speed, 10f, t => new InputState(false, false, true), true);
                Flat(logic, "steer_hold_release_" + suffix, speed, 6f, t => new InputState(true, false, false, t < 1.5f ? 1 : 0));
                // A 0.15 s touch is quantized to eight 50 Hz updates (0.16 s),
                // followed by exactly twenty release updates (0.40 s).
                Flat(logic, "steer_tap_release_" + suffix, speed, 0.56f, t => new InputState(true, false, false, t < 0.15f ? 1 : 0));
                Flat(logic, "steer_sustained_" + suffix, speed, 6f, t => new InputState(true, false, false, 1));
                Flat(logic, "steer_reverse_" + suffix, speed, 6f, t => new InputState(true, false, false, t < 1.5f ? 1 : t < 3f ? -1 : 0));
            }
            foreach (float speed in new[] { 100f, 200f })
                Flat(logic, "handbrake_stunt_recovery_" + speed.ToString("F0", CultureInfo.InvariantCulture), speed, 6f,
                    t => new InputState(t >= 0.6f, false, t < 0.6f, t < 0.6f ? -1 : t < 1.2f ? 1 : 0));
            Reset(logic);
            UnityEngine.Object.DestroyImmediate(ground);
            EditorSceneManager.OpenScene("Assets/Scenes/Games/Extreme/AsyncScenes/2Game_Airport.unity", OpenSceneMode.Additive);
            foreach (MonoBehaviour behaviour in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)) behaviour.enabled = false;
            Physics.SyncTransforms();
            if (!UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None).Any(c => c.name == "Airport_Ramp_05_003"))
                throw new InvalidOperationException("Actual airport loop collider was not loaded.");
            foreach (float entrySpeed in new[] { 278f, 300f, 324f })
            {
                using (Vehicle car = new Vehicle(new Vector3(725f, -5.66f, -726.526f), Quaternion.Euler(0f, 270f, 0f), logic))
                {
                    foreach (Rigidbody rb in car.bodies) rb.linearVelocity = Vector3.left * (entrySpeed / 7.2f);
                    car.logic.gas = 1f;
                    Record(car, "airport_loop_" + entrySpeed.ToString("F0", CultureInfo.InvariantCulture), 10f, t => new InputState(true), false);
                }
            }
            report.naturalAirportRunupRequested = Environment.GetEnvironmentVariable("RACING_PARITY_NATURAL_LOOP") == "1";
            if (report.naturalAirportRunupRequested)
            {
                NaturalAirportRunup(logic);
                report.naturalAirportRunupStatus = Fixtures.Last().note;
            }
            report.completed = true;
            if (Results.Any(result => !result.valid) || Fixtures.Any(fixture => !fixture.usable)) exitCode = 2;
        }
        catch (Exception exception)
        {
            exception = exception is TargetInvocationException invocation && invocation.InnerException != null ? invocation.InnerException : exception;
            report.error = exception.ToString();
            Debug.LogException(exception);
            exitCode = 1;
        }
        finally
        {
            csv?.Dispose();
            csv = null;
            Physics.simulationMode = oldMode;
            Time.fixedDeltaTime = oldDt;
            report.scenarios = Results.ToArray();
            report.fixtures = Fixtures.ToArray();
            File.WriteAllText(Path.Combine(directory, "summary.json"), JsonUtility.ToJson(report, true));
            Debug.Log("[DrivingParity] completed=" + report.completed + ", scenarios=" + Results.Count + ", output=" + directory);
            EditorApplication.Exit(exitCode);
        }
    }

    private struct InputState
    {
        public bool gas, brake, handbrake;
        public int steering;
        public InputState(bool gas, bool brake = false, bool handbrake = false, int steering = 0)
        {
            this.gas = gas;
            this.brake = brake;
            this.handbrake = handbrake;
            this.steering = steering;
        }
    }

    private static void Flat(GameLogic logic, string name, float speed, float duration, Func<float, InputState> input, bool stopAtZero = false, bool fullGasStart = false, bool tractionControl = true)
    {
        using (Vehicle car = new Vehicle(new Vector3(0f, 1f, 0f), Quaternion.identity, logic))
        {
            car.Settle();
            if (speed > 0f) car.SeedSpeed(speed);
            if (fullGasStart) car.logic.gas = 1f;
            if (!tractionControl)
            {
                if (logic.ChangeTc()) throw new InvalidOperationException("Actual TC toggle did not disable traction control.");
                Invoke(car.settings, "Update");
                if (car.control.autoMotorMax) throw new InvalidOperationException("CarSettings did not propagate TC off.");
            }
            Record(car, name, duration, input, stopAtZero);
        }
    }

    private static void NaturalRolling(GameLogic logic, string name, float speed, float duration)
    {
        using (Vehicle car = new Vehicle(new Vector3(0f, 1f, 0f), Quaternion.identity, logic))
        {
            car.Settle();
            int launchSteps = 0;
            while (DisplaySpeed(car) < speed && launchSteps < 1500)
            {
                car.Step(true);
                launchSteps++;
            }
            if (DisplaySpeed(car) < speed) throw new InvalidOperationException("Natural launch could not reach " + speed + " HUD km/h within 30 s.");
            float initialGas = car.logic.gas;
            Result result = Record(car, name, duration, t => new InputState(true), false);
            result.naturalLaunchSeconds = launchSteps * Dt;
            result.naturalLaunchInitialFilteredGas = initialGas;
            result.note = "Natural launch to the target, then continuously held accelerator; no body velocity or wheel RPM overrides at any point. Entry overshoot is one 0.02 s physics step; the real pedal filter continues unchanged.";
        }
    }

    private static void NaturalBrakeAndRecovery(GameLogic logic)
    {
        using (Vehicle car = new Vehicle(new Vector3(0f, 1f, 0f), Quaternion.identity, logic))
        {
            car.Settle();
            ReachNaturalSpeed(car, 248f);
            Record(car, "natural_brake_248_reference_233_to_126", 7f, t => new InputState(false, true), true);
        }
        using (Vehicle car = new Vehicle(new Vector3(0f, 1f, 0f), Quaternion.identity, logic))
        {
            car.Settle();
            ReachNaturalSpeed(car, 95f);
            // A bounded vertical disturbance, representing dropping off a curb:
            // displace the body 0.25 m but preserve every existing velocity.
            car.root.transform.position += Vector3.up * 0.25f;
            Physics.SyncTransforms();
            Result result = Record(car, "natural_drop_recovery_95", 8f, t => new InputState(true), false);
            result.valid &= result.finalTwoSecondsBodyHeightRange < 0.06f && result.finalTwoSecondsRearContactFraction > 0.8f;
            result.note += " Natural approach; vertical position displaced +0.25 m with linear/angular velocities untouched. Recovery requires final 2 s heave <0.06 m and rear contact fraction >0.8.";
        }
        using (Vehicle car = new Vehicle(new Vector3(0f, 1f, 0f), Quaternion.identity, logic))
        {
            car.Settle();
            int steps = 0;
            while (Vector3.Dot(car.body.linearVelocity, car.root.transform.forward) > -6f && steps++ < 1000) car.Step(false, true);
            if (steps >= 1000) throw new InvalidOperationException("Actual brake input could not select reverse.");
            Result result = Record(car, "natural_reverse_to_forward", 8f, t => new InputState(true), false);
            result.valid &= result.finalSignedForwardMS > 6f;
            result.note += " Reverse entered using the actual brake control, then held throttle and released brake; must recover to forward >6 m/s.";
        }
        using (Vehicle car = new Vehicle(new Vector3(0f, 1f, 0f), Quaternion.identity, logic))
        {
            car.Settle();
            ReachNaturalSpeed(car, 95f);
            Result result = Record(car, "natural_forward_to_reverse", 10f, t => new InputState(false, true), false);
            result.valid &= result.finalSignedForwardMS < -6f;
            result.note += " Forward speed reached naturally, then throttle released and brake held through stop into reverse; must reach reverse >6 m/s.";
        }
    }

    private static void ReachNaturalSpeed(Vehicle car, float speed)
    {
        int steps = 0;
        while (DisplaySpeed(car) < speed && steps++ < 1500) car.Step(true);
        if (DisplaySpeed(car) < speed) throw new InvalidOperationException("Natural acceleration failed to reach " + speed + " HUD km/h.");
    }

    private static void NaturalAirportRunup(GameLogic logic)
    {
        // Include the original airport's detail zones for the natural approach;
        // the existing seeded loop regressions above retain their exact fixture.
        foreach (string suffix in new[] { "0", "1", "2" })
            EditorSceneManager.OpenScene("Assets/Scenes/Games/Extreme/AsyncScenes/2Game_Airport_Zone" + suffix + ".unity", OpenSceneMode.Additive);
        foreach (MonoBehaviour behaviour in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)) behaviour.enabled = false;
        Physics.SyncTransforms();
        Vector3 spawn = Vector3.zero;
        string floorName = null;
        List<string> blockers = new List<string>();
        foreach (float x in new[] { 1100f, 1050f, 1000f, 950f, 900f })
        {
            RaycastHit hit = Physics.RaycastAll(new Vector3(x, 40f, -726.526f), Vector3.down, 80f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
                .Where(value => value.normal.y > 0.8f && value.point.y > -10f && value.point.y < 0f)
                .OrderBy(value => value.distance).FirstOrDefault();
            if (hit.collider == null)
            {
                blockers.Add("x=" + x + ": no airport floor");
                continue;
            }
            Vector3 candidate = new Vector3(x, hit.point.y + 1f, -726.526f);
            // Check all the way to the actual ramp foot. Stopping this check at
            // x=775 misses the loop's solid back wall at approximately x=752.
            if (Physics.Raycast(candidate, Vector3.left, out RaycastHit obstruction, x - 675f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                blockers.Add("x=" + x + ": " + obstruction.collider.name + " at " + obstruction.point);
                continue;
            }
            spawn = candidate;
            floorName = hit.collider.name;
            break;
        }
        if (floorName == null)
        {
            Fixtures.Add(new FixtureStatus { name = "airport_loop_natural_runup", usable = false, note = "Blocked fixture, not a vehicle-physics result: no unobstructed straight approach from x=1100..900 to the actual ramp foot x=675 at z=-726.526. " + string.Join("; ", blockers) + ". Natural loop approach remains unverified; no synthetic floor or velocity substituted." });
            Debug.LogWarning("[DrivingParity] Natural airport run-up fixture unavailable.");
            return;
        }
        Fixtures.Add(new FixtureStatus { name = "airport_loop_natural_runup", usable = true, note = "Natural run-up fixture available from " + spawn + " on " + floorName + "; see corresponding scenario for vehicle-physics result." });
        using (Vehicle car = new Vehicle(spawn, Quaternion.Euler(0f, 270f, 0f), logic))
        {
            car.Settle();
            Result result = Record(car, "airport_loop_natural_runup", 30f, t => new InputState(true), false);
            result.note = "Natural airport run-up from " + spawn + " on " + floorName + "; starts at rest, actual accelerator held throughout, no seeded linear/angular velocity or wheel RPM. " +
                "Ramp entry is first loop-area up-dot <0.9 with positive vertical velocity; inversion is first up-dot <0; successful exit requires subsequent grounded upright exit. " +
                "Observed entry=" + result.loopRampEntrySeconds + ", inversion=" + result.loopFirstInversionSeconds + ", exit=" + result.loopExitSeconds + " seconds.";
        }
    }

    private static Result Record(Vehicle car, string name, float duration, Func<float, InputState> input, bool stopAtZero)
    {
        Result result = new Result { name = name, initialDisplayKmh = DisplaySpeed(car), initialSignedForwardMS = Vector3.Dot(car.body.linearVelocity, car.root.transform.forward), maximumHeight = car.body.position.y };
        for (int i = 0; i < result.speedAt123510Seconds.Length; i++) result.speedAt123510Seconds[i] = -1f;
        Results.Add(result);
        Vector3 startPosition = car.body.position;
        float previousYaw = car.root.transform.eulerAngles.y;
        float heading = 0f, airborne = 0f;
        bool loop = name.StartsWith("airport_loop");
        bool steeringScenario = name.StartsWith("steer_");
        bool handbrakeStunt = name.StartsWith("handbrake_stunt_recovery");
        bool handbrakeSeen = false;
        bool invertedContact = false;
        bool loopExited = false;
        int previousSteeringTarget = 0;
        float releaseAt = -1f, reversalAt = -1f;
        float crossed233At = -1f;
        float previousSpeed = result.initialDisplayKmh;
        float lateMinimumY = float.MaxValue, lateMaximumY = float.MinValue;
        int lateRearContacts = 0, lateSamples = 0;
        int[] sampleSteps = { 50, 100, 150, 250, 500 };
        for (int step = 1; step <= Mathf.RoundToInt(duration / Dt); step++)
        {
            InputState state = input((step - 1) * Dt);
            if (state.steering == 0 && previousSteeringTarget != 0 && releaseAt < 0f) releaseAt = (step - 1) * Dt;
            if (state.steering != 0 && previousSteeringTarget != 0 && state.steering != previousSteeringTarget && reversalAt < 0f) reversalAt = (step - 1) * Dt;
            previousSteeringTarget = state.steering;
            car.Step(state.gas, state.brake, state.handbrake, state.steering);
            Vector3 velocity = car.body.linearVelocity;
            Vector3 localVelocity = car.root.transform.InverseTransformDirection(velocity);
            float speed = DisplaySpeed(car);
            float poseDifference = Vector3.Distance(car.root.transform.position, car.body.position);
            float angleDifference = Quaternion.Angle(car.root.transform.rotation, car.body.rotation);
            result.maximumTransformBodyPositionDifference = Mathf.Max(result.maximumTransformBodyPositionDifference, poseDifference);
            result.maximumTransformBodyAngleDifference = Mathf.Max(result.maximumTransformBodyAngleDifference, angleDifference);
            if (name.StartsWith("natural_brake_248"))
            {
                if (crossed233At < 0f && previousSpeed > 233f && speed <= 233f)
                    crossed233At = ((step - 1) + Mathf.InverseLerp(previousSpeed, speed, 233f)) * Dt;
                if (crossed233At >= 0f && result.brake233To126Seconds < 0f && previousSpeed > 126f && speed <= 126f)
                    result.brake233To126Seconds = ((step - 1) + Mathf.InverseLerp(previousSpeed, speed, 126f)) * Dt - crossed233At;
            }
            previousSpeed = speed;
            if (float.IsNaN(speed) || float.IsInfinity(speed) || velocity.sqrMagnitude > 1000000f)
                throw new InvalidOperationException("Non-finite or runaway simulation in " + name + " step=" + step);
            float yaw = car.root.transform.eulerAngles.y;
            float yawStep = Mathf.DeltaAngle(previousYaw, yaw);
            heading += yawStep;
            previousYaw = yaw;
            float upDot = Vector3.Dot(car.root.transform.up, Vector3.up);
            int grounded = car.Grounded();
            handbrakeSeen |= car.logic.handbreak > 0.01f;
            if (handbrakeStunt && handbrakeSeen && !state.handbrake && car.logic.handbreak <= 0.001f && result.handbrakeFullyReleasedAt < 0f)
                result.handbrakeFullyReleasedAt = (step - 1) * Dt;
            if (handbrakeStunt && result.handbrakeFullyReleasedAt >= 0f && grounded >= 2)
                result.groundedYawTravelAfterHandbrakeRelease += Mathf.Abs(yawStep);
            invertedContact |= loop && upDot < -0.6f && grounded >= 2 && car.body.position.y > 20f;
            loopExited |= invertedContact && upDot > 0.9f && grounded >= 2 && car.body.position.y < -5f && car.body.position.x < 735f && car.body.position.x > 700f && velocity.x < -5f;
            bool inLoopBounds = car.body.position.x > 657f && car.body.position.x < 752f && Mathf.Abs(car.body.position.z + 726.526f) < 8f;
            if (loop && inLoopBounds && result.loopRampEntrySeconds < 0f && upDot < 0.9f && velocity.y > 0f)
            {
                result.loopRampEntrySeconds = step * Dt;
                result.loopRampEntryDisplayKmh = speed;
                result.loopRampEntryPosition = car.body.position;
            }
            if (loop && inLoopBounds && result.loopFirstInversionSeconds < 0f && upDot < 0f)
            {
                result.loopFirstInversionSeconds = step * Dt;
                result.loopFirstInversionDisplayKmh = speed;
                result.loopFirstInversionPosition = car.body.position;
            }
            if (loopExited && result.loopExitSeconds < 0f)
            {
                result.loopExitSeconds = step * Dt;
                result.loopExitDisplayKmh = speed;
                result.loopExitPosition = car.body.position;
                if (result.loopRampEntrySeconds >= 0f) result.loopRampEntryToExitSeconds = result.loopExitSeconds - result.loopRampEntrySeconds;
            }
            float slip = velocity.magnitude > 2f ? Mathf.Atan2(Mathf.Abs(localVelocity.x), Mathf.Abs(localVelocity.z)) * Mathf.Rad2Deg : 0f;
            float wheelSlip = 0f;
            List<string> wheelColumns = new List<string>();
            foreach (CarWheel wheel in car.wheels)
            {
                WheelCollider collider = wheel.getWheelCollider();
                bool groundedWheel = collider.GetGroundHit(out WheelHit hit);
                if (groundedWheel)
                {
                    wheelSlip = Mathf.Max(wheelSlip, Mathf.Abs(hit.sidewaysSlip));
                    result.maximumWheelForwardSlip = Mathf.Max(result.maximumWheelForwardSlip, Mathf.Abs(hit.forwardSlip));
                }
                result.maximumWheelRpm = Mathf.Max(result.maximumWheelRpm, Mathf.Abs(collider.rpm));
                wheelColumns.AddRange(new[] { F(groundedWheel ? hit.forwardSlip : 0f), F(collider.rpm), F(collider.motorTorque), F(collider.brakeTorque), F(groundedWheel ? hit.force : 0f) });
            }
            wheelColumns.AddRange(new[] { F(car.root.transform.position.y), F(poseDifference), F(angleDifference), F(velocity.y), F(car.body.rotation.eulerAngles.x), F(car.body.rotation.eulerAngles.z) });
            if (step * Dt >= duration - 2f)
            {
                lateMinimumY = Mathf.Min(lateMinimumY, car.body.position.y);
                lateMaximumY = Mathf.Max(lateMaximumY, car.body.position.y);
                lateRearContacts += (car.control.WheelRL.getWheelCollider().isGrounded ? 1 : 0) + (car.control.WheelRR.getWheelCollider().isGrounded ? 1 : 0);
                lateSamples += 2;
            }
            airborne = grounded == 0 ? airborne + Dt : 0f;
            result.steps = step;
            result.minimumDisplayKmh = Mathf.Min(result.minimumDisplayKmh, speed);
            result.maximumDisplayKmh = Mathf.Max(result.maximumDisplayKmh, speed);
            result.maximumLateralSlipAngleDegrees = Mathf.Max(result.maximumLateralSlipAngleDegrees, slip);
            result.maximumWheelSidewaysSlip = Mathf.Max(result.maximumWheelSidewaysSlip, wheelSlip);
            result.maximumBodyTiltDegrees = Mathf.Max(result.maximumBodyTiltDegrees, Mathf.Acos(Mathf.Clamp(upDot, -1f, 1f)) * Mathf.Rad2Deg);
            result.minimumUpDot = Mathf.Min(result.minimumUpDot, upDot);
            result.maximumHeight = Mathf.Max(result.maximumHeight, car.body.position.y);
            result.longestAirborneSeconds = Mathf.Max(result.longestAirborneSeconds, airborne);
            if (grounded >= 3) result.groundedSteps++;
            for (int i = 0; i < sampleSteps.Length; i++) if (step == sampleSteps[i]) result.speedAt123510Seconds[i] = speed;
            result.headingChangeDegrees = heading;
            result.endDisplayKmh = speed;
            result.endDistance = Vector3.Distance(startPosition, car.body.position);
            result.elapsedSeconds = step * Dt;
            result.finalBodySideslipDegrees = slip;
            result.finalYawRate = car.body.angularVelocity.y;
            result.finalSignedForwardMS = localVelocity.z;
            float maxWheelAngle = Mathf.Max(Mathf.Abs(car.control.getSteerL()), Mathf.Abs(car.control.getSteerR()));
            if (releaseAt >= 0f && step * Dt - releaseAt >= 0.4f - 0.0001f && result.wheelAngleFourTenthsAfterRelease < 0f)
                result.wheelAngleFourTenthsAfterRelease = maxWheelAngle;
            if (releaseAt >= 0f && step * Dt - releaseAt >= 0.5f - 0.0001f && result.wheelAngleHalfSecondAfterRelease < 0f)
                result.wheelAngleHalfSecondAfterRelease = maxWheelAngle;
            if (reversalAt >= 0f && step * Dt - reversalAt >= 0.5f - 0.0001f && result.wheelAngleHalfSecondAfterReversal < -998f)
            {
                result.wheelAngleHalfSecondAfterReversal = (car.control.getSteerL() + car.control.getSteerR()) * 0.5f;
                result.yawRateHalfSecondAfterReversal = car.body.angularVelocity.y;
            }
            if (step % 5 == 0 || step == 1 || step == Mathf.RoundToInt(duration / Dt) || loopExited || (stopAtZero && result.initialDisplayKmh > 10f && localVelocity.z < 0.2f))
                csv.WriteLine(string.Join(",", new[] { name, F(step * Dt), F(speed), F(velocity.magnitude), F(localVelocity.z), F(car.body.position.x), F(car.body.position.y), F(car.body.position.z), F(heading), F(car.body.angularVelocity.y), F(upDot), grounded.ToString(), F(slip), F(wheelSlip), F(car.logic.gas), F(car.logic.brakes), F(car.logic.handbreak), F(car.control.steerInput), F(car.control.getSteerL()), F(car.control.getSteerR()) }.Concat(wheelColumns)));
            if (stopAtZero && result.initialDisplayKmh > 10f && localVelocity.z < 0.2f)
            {
                result.firstStopSeconds = step * Dt;
                result.stopDistance = result.endDistance;
                break;
            }
            if (loopExited) break;
        }
        result.valid = loop ? loopExited : car.body.position.y > -3f && result.maximumBodyTiltDegrees < 50f && (!steeringScenario || result.maximumLateralSlipAngleDegrees < 35f);
        if (lateSamples > 0)
        {
            result.finalTwoSecondsBodyHeightRange = lateMaximumY - lateMinimumY;
            result.finalTwoSecondsRearContactFraction = lateRearContacts / (float)lateSamples;
        }
        if (name == "natural_rolling_throttle_49_reference_5p80s") result.valid &= Mathf.Abs(result.endDisplayKmh - 248f) <= 12f;
        if (name == "natural_rolling_throttle_95") result.valid &= Mathf.Abs(result.endDisplayKmh - 276f) <= 12f;
        if (name == "rolling_throttle_49_reference_5p80s" || name == "rolling_throttle_95")
        {
            float target = name == "rolling_throttle_95" ? 276f : 248f;
            result.valid &= Mathf.Abs(result.endDisplayKmh - target) <= 12f &&
                result.finalTwoSecondsRearContactFraction > 0.8f && result.finalTwoSecondsBodyHeightRange < 0.06f;
        }
        if (name == "coast_253_reference_2p28s") result.valid &= Mathf.Abs(result.endDisplayKmh - 177f) <= 5f;
        if (name == "coast_110_reference_1p42s") result.valid &= Mathf.Abs(result.endDisplayKmh - 94f) <= 5f;
        if (name.StartsWith("natural_brake_248")) result.valid &= Mathf.Abs(result.brake233To126Seconds - 1.421f) <= 0.20f;
        if (name.StartsWith("steer_hold_release") && result.initialDisplayKmh > 150f)
            result.valid &= result.wheelAngleHalfSecondAfterRelease >= 0f && result.wheelAngleHalfSecondAfterRelease <= 0.05f;
        if (name.StartsWith("steer_tap_release"))
            result.valid &= result.wheelAngleFourTenthsAfterRelease >= 0f && result.wheelAngleFourTenthsAfterRelease <= 0.05f;
        if (handbrakeStunt)
            result.valid &= result.handbrakeFullyReleasedAt >= 0f && result.groundedYawTravelAfterHandbrakeRelease < 180f;
        result.note = loop ? "Actual airport loop: invertedContact=" + invertedContact + ", groundedExit=" + loopExited + "; initial body speed is seeded, native wheel RPM starts at zero and spins up through contact." : "Synthetic level surface; displayed km/h is actual forward m/s x 7.2, matching GameLogic.GetSpeed. Seeded-speed cases include 0.8 s of chassis-speed-constrained tire spin-up; inspect wheel RPM/contact telemetry for remaining transients.";
        if (steeringScenario)
            result.note += " Flat-road gates: body sideslip <35 deg, tilt <50 deg; high-speed held steering centered within 0.5 s; short tap centered within 0.4 s (wheel tolerance 0.05 deg).";
        if (handbrakeStunt)
            result.note += " Stunt: left + handbrake held 0.6 s, right + throttle held for the next 0.6 s, then neutral steering + throttle. Intentional tire slip is allowed; grounded yaw travel after the actual handbrake filter reaches zero must stay below 180 deg.";
        if (name.StartsWith("tc_off"))
            result.note += " Actual GameLogic.ChangeTc toggled TC off and CarSettings propagated it; wheel slip/RPM are reported but intentionally not capped by a validation gate.";
        if (name.StartsWith("natural_brake_248"))
            result.note += " Real gas-release/brake-press filters after natural acceleration to 248; interpolated 233-to-126 HUD km/h interval reported against video reference 1.421 s.";
        csv.Flush();
        Debug.Log("[DrivingParity] " + name + " start=" + result.initialDisplayKmh.ToString("F1") + " end=" + result.endDisplayKmh.ToString("F1") + " yaw=" + result.headingChangeDegrees.ToString("F1") + " maxSlipDeg=" + result.maximumLateralSlipAngleDegrees.ToString("F1") + " stop=" + result.firstStopSeconds.ToString("F2"));
        return result;
    }

    private static void Reset(GameLogic logic)
    {
        using (Vehicle car = new Vehicle(new Vector3(0f, 1f, 0f), Quaternion.identity, logic))
        {
            car.Settle();
            logic.gas = 1f;
            logic.brakes = -1f;
            logic.handbreak = 1f;
            logic.DirLeft(true);
            AdvanceSteering(logic, 0.5f);
            Invoke(car.control, "Update");
            logic.ResetCarInput();
            Invoke(car.control, "Update");
            bool clear = logic.gas == 0f && logic.brakes == 0f && logic.handbreak == 0f && car.control.motorInput == 0f && car.control.brakeInput == 0f && car.control.handbrakeInput == 0f && car.control.steerInput == 0f;
            Results.Add(new Result { name = "reset_clears_all_inputs", valid = clear, minimumDisplayKmh = 0f, note = "Actual ResetCarInput followed by actual CarControl.Update: gas=" + logic.gas + ", brake=" + logic.brakes + ", handbrake=" + logic.handbreak + ", steer=" + car.control.steerInput });
            Debug.Log("[DrivingParity] reset_clears_all_inputs=" + clear);
        }
    }

    private static void AdvanceSteering(GameLogic logic, float dt)
    {
        MethodInfo method = typeof(GameLogic).GetMethod("UpdateDirectionalInput", InstanceFlags, null, new[] { typeof(float) }, null);
        if (method != null)
        {
            method.Invoke(logic, new object[] { dt });
            return;
        }
        // Before the runtime helper exists, reproduce only its time scheduling;
        // rates and current state are reflected from the unchanged real class.
        FieldInfo state = typeof(GameLogic).GetField("m_directionalInput", InstanceFlags);
        float current = (float)state.GetValue(logic);
        bool left = (bool)typeof(GameLogic).GetField("m_dirLeftPressed", InstanceFlags).GetValue(logic);
        bool right = (bool)typeof(GameLogic).GetField("m_dirRightPressed", InstanceFlags).GetValue(logic);
        float target = left == right ? 0f : left ? -1f : 1f;
        bool reversing = !Mathf.Approximately(target, 0f) && !Mathf.Approximately(current, 0f) && Mathf.Sign(target) != Mathf.Sign(current);
        string field = Mathf.Approximately(target, 0f) || reversing ? "STEER_RETURN_RATE" : "STEER_ENGAGE_RATE";
        float rate = (float)typeof(GameLogic).GetField(field, BindingFlags.NonPublic | BindingFlags.Static).GetRawConstantValue();
        current = Mathf.MoveTowards(current, target, rate * dt);
        state.SetValue(logic, current);
        logic.dirLeft = Mathf.Min(0f, current);
        logic.dirRight = Mathf.Max(0f, current);
    }

    private static float DisplaySpeed(Vehicle car) => Mathf.Abs(Vector3.Dot(car.body.linearVelocity, car.root.transform.forward)) * 7.2f;
    private static string F(float value) => value.ToString("F5", CultureInfo.InvariantCulture);

    private static void Invoke(object target, string name)
    {
        string key = target.GetType().FullName + "." + name;
        if (!Methods.TryGetValue(key, out MethodInfo method))
        {
            method = target.GetType().GetMethod(name, InstanceFlags, null, Type.EmptyTypes, null);
            if (method == null) throw new MissingMethodException(key);
            Methods.Add(key, method);
        }
        method.Invoke(target, null);
    }
}
