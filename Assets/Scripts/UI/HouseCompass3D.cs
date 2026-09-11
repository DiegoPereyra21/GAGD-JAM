using UnityEngine;

public class HouseCompass3D : MonoBehaviour
{
    [SerializeField] private Transform houseTarget;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Vector3 screenOffset = new Vector3(0f, -0.3f, 1f);
    [SerializeField] private Renderer[] arrowRenderers;
    [SerializeField] private Vector3 modelForwardCorrection; // ajustá esto si el modelo no apunta con su +Z

    private void LateUpdate()
    {
        bool visible = GameProgressManager.Instance.IsOutside;

        foreach (Renderer r in arrowRenderers)
            r.enabled = visible;

        if (!visible) return;

        transform.position = cameraTransform.TransformPoint(screenOffset);
        transform.LookAt(houseTarget.position, cameraTransform.up);
        transform.rotation *= Quaternion.Euler(modelForwardCorrection);
    }
}