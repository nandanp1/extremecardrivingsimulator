using UnityEngine;

[RequireComponent(typeof(WheelCollider))]
public sealed class WheelSkidmarks : MonoBehaviour
{
    public GameObject skidCaller;
    public float startSlipValue = 6.5f;

    private Skidmarks skidmarks;
    private WheelCollider wheelCollider;
    private Rigidbody rootBody;
    private int lastSkidmark = -1;

    private void Start()
    {
        skidCaller = transform.root.gameObject;
        wheelCollider = GetComponent<WheelCollider>();
        rootBody = skidCaller.GetComponent<Rigidbody>();
        skidmarks = FindFirstObjectByType<Skidmarks>();

        if (skidmarks == null)
        {
            Debug.LogWarning("No skidmarks object found. Skidmarks will not be drawn", this);
        }
    }

    private void FixedUpdate()
    {
        if (skidmarks == null || wheelCollider == null)
        {
            return;
        }

        WheelHit hit;
        if (!wheelCollider.GetGroundHit(out hit))
        {
            lastSkidmark = -1;
            return;
        }

        float slip = Mathf.Abs(hit.sidewaysSlip);
        if (slip <= startSlipValue)
        {
            lastSkidmark = -1;
            return;
        }

        Vector3 velocity = rootBody != null ? rootBody.linearVelocity : Vector3.zero;
        Vector3 position = hit.point + 2f * velocity * Time.deltaTime;
        lastSkidmark = skidmarks.AddSkidMark(position, hit.normal, slip / 25f, lastSkidmark);
    }
}
