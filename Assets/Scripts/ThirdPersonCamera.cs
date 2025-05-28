using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform player;
    public Vector3 positionOffset = new Vector3(0.0f, 2.0f, -3.0f);
    public Vector3 angleOffset = Vector3.zero;
    public float damping = 10f;

    void CameraMove_Follow(bool allowRotationTracking = true)
    {
        Quaternion initialRotation = Quaternion.Euler(angleOffset);
        if (allowRotationTracking)
        {
            Quaternion rot = Quaternion.Lerp(transform.rotation, player.rotation * initialRotation, damping * Time.deltaTime);
            transform.rotation = rot;
        }
        else
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, initialRotation, damping * Time.deltaTime);
        }

        Vector3 forward = transform.rotation * Vector3.forward;
        Vector3 right = transform.rotation * Vector3.right;
        Vector3 up = transform.rotation * Vector3.up;


        Vector3 targetPos = player.position;
        Vector3 desiredPos = targetPos +
                             forward * positionOffset.z +
                             right * positionOffset.x +
                             up * positionOffset.y;


        Vector3 position = Vector3.Lerp(transform.position, desiredPos, damping * Time.deltaTime);
        transform.position = position;
    }

    private void LateUpdate()
    {
        CameraMove_Follow();
    }

}
