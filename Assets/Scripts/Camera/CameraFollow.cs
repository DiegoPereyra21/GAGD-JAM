using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 10f;
    [SerializeField] private float angle = 30f;
    [SerializeField] private float smoothSpeed = 10f;

    private Vector3 offset;
    private void Start()
    {
        float rad = angle * Mathf.Deg2Rad;
        offset = new Vector3(0f, distance * Mathf.Sin(rad), -distance * Mathf.Cos(rad));
        transform.rotation = Quaternion.Euler(angle, 0f, 0f);
    }
    private void LateUpdate()
    {
        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}