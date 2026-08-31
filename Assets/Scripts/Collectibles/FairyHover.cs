using UnityEngine;

public class FairyHover : MonoBehaviour
{
    [SerializeField] private float wanderRadius = 0.5f;
    [SerializeField] private float orbitSpeed = 60f;
    [SerializeField] private float bobHeight = 0.15f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float rotationSpeed = 40f;
    [SerializeField] private float rotationWobble = 15f;

    private Vector3 origin;
    private float angle;
    private float rotationOffset;

    private void Start()
    {
        origin = transform.position;
        angle = Random.Range(0f, 360f);
        rotationOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        angle += orbitSpeed * Time.deltaTime;

        float rad = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * wanderRadius;
        offset.y = Mathf.Sin(Time.time * bobSpeed) * bobHeight;

        transform.position = origin + offset;

        float yaw = angle + 90f + Mathf.Sin(Time.time * rotationSpeed * 0.1f + rotationOffset) * rotationWobble;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
}