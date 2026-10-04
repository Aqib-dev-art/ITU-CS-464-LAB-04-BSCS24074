using UnityEngine;

// Makes the camera follow the player from behind and above, smoothly.
// Put this on the Main Camera and drag the Player into the Target slot.
public class CameraFollow : MonoBehaviour
{
    public Transform target;                                  // the player
    public Vector3 offset = new Vector3(0f, 6f, -8f);         // position relative to the player
    public float smoothTime = 0.15f;                          // lower = snappier, higher = floatier
    public float lookHeight = 1f;                             // aim slightly above the player's feet

    Vector3 velocity;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        transform.LookAt(target.position + Vector3.up * lookHeight);
    }
}
