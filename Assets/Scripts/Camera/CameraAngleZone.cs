using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CameraAngleZone : MonoBehaviour
{
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private float zoneAngle = 45f;
    [SerializeField] private float defaultAngle = 30f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            cameraFollow.SetAngle(zoneAngle);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            cameraFollow.SetAngle(defaultAngle);
    }
}