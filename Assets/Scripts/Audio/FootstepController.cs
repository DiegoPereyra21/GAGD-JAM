using UnityEngine;

public class FootstepController : MonoBehaviour
{
    [Header("Evento de pasos")]
    [SerializeField] private AK.Wwise.Event footstepEvent;

    [Header("Configuración de pasos")]
    [SerializeField] private float distancePerStep = 5f;

    private CharacterController characterController;
    private Vector3 lastPosition;
    private float distanceWalked;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        lastPosition = transform.position;
    }

    private void Update()
    {
        Vector3 movement = transform.position - lastPosition;

        // Ignora el movimiento vertical
        movement.y = 0f;

        float distance = movement.magnitude;
        lastPosition = transform.position;

        if (distance <= 0f)
            return;

        // No reproduce pasos mientras está en el aire
        if (!characterController.isGrounded)
            return;

        distanceWalked += distance;

        if (distanceWalked >= distancePerStep)
        {
            PlayFootstep();
            distanceWalked = 0f;
        }
    }

    private void PlayFootstep()
    {
        footstepEvent.Post(gameObject);
    }
}