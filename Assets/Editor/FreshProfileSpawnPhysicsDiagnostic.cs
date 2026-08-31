using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FreshProfileSpawnPhysicsDiagnostic
{
    private static readonly string[] ScenePaths =
    {
        "Assets/Scenes/Common/2Game.unity",
        "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_City.unity",
        "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_Zone1.unity",
        "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_Zone3.unity"
    };

    public static void LogWheelFrictionDefaults()
    {
        try
        {
            GameObject probe = new GameObject("WheelCollider_DefaultProbe");
            WheelCollider fresh = probe.AddComponent<WheelCollider>();
            Debug.Log($"[WheelDefaults] fresh forward=({fresh.forwardFriction.extremumSlip:R}, {fresh.forwardFriction.extremumValue:R}, {fresh.forwardFriction.asymptoteSlip:R}, {fresh.forwardFriction.asymptoteValue:R}, stiffness={fresh.forwardFriction.stiffness:R}), " +
                      $"sideways=({fresh.sidewaysFriction.extremumSlip:R}, {fresh.sidewaysFriction.extremumValue:R}, {fresh.sidewaysFriction.asymptoteSlip:R}, {fresh.sidewaysFriction.asymptoteValue:R}, stiffness={fresh.sidewaysFriction.stiffness:R})");
            UnityEngine.Object.DestroyImmediate(probe);

            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Coche1.prefab");
            foreach (WheelCollider wheel in source.GetComponentsInChildren<WheelCollider>(true))
            {
                Debug.Log($"[WheelDefaults] Coche1/{wheel.name} forward=({wheel.forwardFriction.extremumSlip:R}, {wheel.forwardFriction.extremumValue:R}, {wheel.forwardFriction.asymptoteSlip:R}, {wheel.forwardFriction.asymptoteValue:R}, stiffness={wheel.forwardFriction.stiffness:R}), " +
                          $"sideways=({wheel.sidewaysFriction.extremumSlip:R}, {wheel.sidewaysFriction.extremumValue:R}, {wheel.sidewaysFriction.asymptoteSlip:R}, {wheel.sidewaysFriction.asymptoteValue:R}, stiffness={wheel.sidewaysFriction.stiffness:R})");
            }
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    public static void Run()
    {
        try
        {
            EditorSceneManager.OpenScene(ScenePaths[0], OpenSceneMode.Single);
            for (int i = 1; i < ScenePaths.Length; i++)
            {
                EditorSceneManager.OpenScene(ScenePaths[i], OpenSceneMode.Additive);
            }

            Transform spawn = Resources.FindObjectsOfTypeAll<Transform>()
                .FirstOrDefault(value => value.gameObject.scene.IsValid() && value.name == "FirstSpawnPoint");
            if (spawn == null)
            {
                throw new InvalidOperationException("FirstSpawnPoint was not found.");
            }

            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Coche1.prefab");
            if (source == null)
            {
                throw new InvalidOperationException("Coche1 prefab was not found.");
            }

            GameObject car = UnityEngine.Object.Instantiate(source, spawn.position, spawn.rotation);
            car.name = "Coche1_SpawnDiagnostic";
            foreach (MonoBehaviour behaviour in car.GetComponentsInChildren<MonoBehaviour>(true))
            {
                behaviour.enabled = false;
            }

            Rigidbody body = car.GetComponent<Rigidbody>();
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            Physics.SyncTransforms();

            Debug.Log($"[SpawnPhysics] spawn position={spawn.position:R}, rotation={spawn.rotation.eulerAngles:R}; " +
                      $"car position={car.transform.position:R}, body centerOfMass={body.worldCenterOfMass:R}, mass={body.mass:R}");

            Collider[] carColliders = car.GetComponentsInChildren<Collider>(true);
            Collider[] sceneColliders = UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(value => value != null && value.gameObject.scene.IsValid() && !value.transform.IsChildOf(car.transform))
                .ToArray();
            Debug.Log($"[SpawnPhysics] carColliders={carColliders.Length}, sceneColliders={sceneColliders.Length}");

            foreach (Collider collider in carColliders)
            {
                Debug.Log($"[SpawnPhysics] car collider path={Path(collider.transform)}, type={collider.GetType().Name}, " +
                          $"enabled={collider.enabled}, trigger={collider.isTrigger}, bounds={collider.bounds}");
            }

            int broadphaseIntersections = 0;
            int penetrations = 0;
            foreach (Collider own in carColliders)
            {
                if (!own.enabled || own.isTrigger || own is WheelCollider)
                {
                    continue;
                }

                foreach (Collider other in sceneColliders)
                {
                    if (!other.enabled || other.isTrigger || !own.bounds.Intersects(other.bounds))
                    {
                        continue;
                    }

                    broadphaseIntersections++;
                    Vector3 direction;
                    float distance;
                    bool penetrating = Physics.ComputePenetration(
                        own, own.transform.position, own.transform.rotation,
                        other, other.transform.position, other.transform.rotation,
                        out direction, out distance);
                    Debug.Log($"[SpawnPhysics] overlap own={Path(own.transform)}({own.GetType().Name}) " +
                              $"other={Path(other.transform)}({other.GetType().Name}) scene={other.gameObject.scene.name} " +
                              $"penetrating={penetrating} direction={direction:R} distance={distance:R} otherBounds={other.bounds}");
                    if (penetrating)
                    {
                        penetrations++;
                    }
                }
            }

            float alignOffset = float.NegativeInfinity;
            foreach (WheelCollider wheel in car.GetComponentsInChildren<WheelCollider>(true))
            {
                RaycastHit hit = Physics.RaycastAll(wheel.transform.position + Vector3.up * 2f, Vector3.down, 10f,
                        Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
                    .Where(value => !value.transform.IsChildOf(car.transform))
                    .OrderBy(value => value.distance)
                    .FirstOrDefault();
                bool rayHit = hit.collider != null;
                if (rayHit)
                {
                    alignOffset = Mathf.Max(alignOffset,
                        hit.point.y + wheel.radius + wheel.suspensionDistance + 0.02f - wheel.transform.position.y);
                }
                Debug.Log($"[SpawnPhysics] wheel={Path(wheel.transform)}, position={wheel.transform.position:R}, " +
                          $"radius={wheel.radius:R}, suspensionDistance={wheel.suspensionDistance:R}, " +
                          $"spring={wheel.suspensionSpring.spring:R}, damper={wheel.suspensionSpring.damper:R}, " +
                          $"target={wheel.suspensionSpring.targetPosition:R}, rayHit={rayHit}, " +
                          $"rayDistance={(rayHit ? hit.distance : -1f):R}, rayObject={(rayHit ? Path(hit.transform) : "<none>")}");
            }

            Debug.Log($"[SpawnPhysics] broadphaseIntersections={broadphaseIntersections}, penetrations={penetrations}");
            if (!float.IsNegativeInfinity(alignOffset))
            {
                car.transform.position += Vector3.up * alignOffset;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                Physics.SyncTransforms();
                SimulationMode oldMode = Physics.simulationMode;
                Physics.simulationMode = SimulationMode.Script;
                try
                {
                    Debug.Log($"[SpawnPhysics] sim alignedOffset={alignOffset:R}, start={car.transform.position:R}");
                    for (int step = 1; step <= 100; step++)
                    {
                        Physics.Simulate(0.02f);
                        if (step <= 5 || step % 10 == 0)
                        {
                            Debug.Log($"[SpawnPhysics] sim step={step}, position={car.transform.position:R}, " +
                                      $"rotation={car.transform.rotation.eulerAngles:R}, velocity={body.linearVelocity:R}, " +
                                      $"angularVelocity={body.angularVelocity:R}");
                        }
                    }
                }
                finally
                {
                    Physics.simulationMode = oldMode;
                }
            }
            UnityEngine.Object.DestroyImmediate(car);
            EditorApplication.Exit(penetrations == 0 ? 0 : 2);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    private static string Path(Transform value)
    {
        if (value == null)
        {
            return "<null>";
        }

        string result = value.name;
        while (value.parent != null)
        {
            value = value.parent;
            result = value.name + "/" + result;
        }
        return result;
    }
}
