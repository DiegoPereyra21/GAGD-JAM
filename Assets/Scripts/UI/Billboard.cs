using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private float tiltOffset = 0f; //por si querés inclinarlo un poco además de mirar a cámara

    private Transform cam;

    private void LateUpdate()
    {
        if (cam == null)
        {
            if (Camera.main == null) return;
            cam = Camera.main.transform;
        }

        transform.rotation = Quaternion.LookRotation(transform.position - cam.position)
            * Quaternion.Euler(tiltOffset, 0f, 0f);
    }
}