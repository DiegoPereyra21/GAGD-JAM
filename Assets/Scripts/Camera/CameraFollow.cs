using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 10f;
    [SerializeField] private float angle = 30f;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float angleTransitionSpeed = 5f;

    private Vector3 offset;
    private Quaternion fixedRotation;
    private float currentAngle;
    private float targetAngle;

    private void Awake()
    {
        currentAngle = angle;
        targetAngle = angle;
        RecalculateOffset();
    }

    private void LateUpdate()
    {
        if (!Mathf.Approximately(currentAngle, targetAngle))
        {
            currentAngle = Mathf.Lerp(currentAngle, targetAngle, angleTransitionSpeed * Time.deltaTime);
            RecalculateOffset();
        }

        transform.position = Vector3.Lerp(transform.position, GetDesiredPosition(), smoothSpeed * Time.deltaTime);
        transform.rotation = fixedRotation;
    }

    private void RecalculateOffset()
    {
        float rad = currentAngle * Mathf.Deg2Rad;
        offset = new Vector3(0f, distance * Mathf.Sin(rad), -distance * Mathf.Cos(rad));
        fixedRotation = Quaternion.Euler(currentAngle, 0f, 0f);
    }

    public void SetAngle(float newAngle) => targetAngle = newAngle;

    public Vector3 GetDesiredPosition() => target.position + offset;
    public Quaternion FixedRotation => fixedRotation;
}