using UnityEngine;
//vista que pidieron, no parece que sepan muy bien la diferencia entre las perspectivas pero bueno, les convencio esta vista
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 10f;
    [SerializeField] private float angle = 30f;
    [SerializeField] private float smoothSpeed = 10f;

    private Vector3 offset;
    private Quaternion fixedRotation;

    private void Awake()
    {
        float rad = angle * Mathf.Deg2Rad;
        offset = new Vector3(0f, distance * Mathf.Sin(rad), -distance * Mathf.Cos(rad));
        fixedRotation = Quaternion.Euler(angle, 0f, 0f);
        transform.rotation = fixedRotation;
    }

    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, GetDesiredPosition(), smoothSpeed * Time.deltaTime);
    }

    public Vector3 GetDesiredPosition() => target.position + offset;
    public Quaternion FixedRotation => fixedRotation;
}