using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float gravity = -9.81f;
    private CharacterController controller;
    private InputAction moveAction;
    private Vector3 velocity;
    private int freezeCount;
    private bool IsFrozen => freezeCount > 0;//para pausar al pj cuando este recolectando hongos, la ides es q sea reutilizable para distintas situaciones donde necesite una pausa
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        moveAction = GetComponent<PlayerInput>().actions["Move"];
    }

    private void Update()
    {
        Vector2 input = IsFrozen ? Vector2.zero : moveAction.ReadValue<Vector2>();
        Vector3 direction = new Vector3(input.x, 0f, input.y);

        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        controller.Move(direction * moveSpeed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // Freeze temporal por duración fija (ej. animación de recolección)
    public void FreezeMovement(float duration)
    {
        StartCoroutine(TimedFreezeRoutine(duration));
    }

    private IEnumerator TimedFreezeRoutine(float duration)
    {
        freezeCount++;
        yield return new WaitForSeconds(duration);
        freezeCount--;
    }

    // Freeze indefinido, controlado manualmente (ej. mientras el canasto está abierto)
    public void SetFrozen(bool frozen)
    {
        freezeCount += frozen ? 1 : -1;
        freezeCount = Mathf.Max(0, freezeCount);
    }
}