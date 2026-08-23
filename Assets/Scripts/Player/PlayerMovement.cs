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
    private bool isFrozen;//para pausar al pj cuando este recolectando hongos, la ides es q sea reutilizable para distintas situaciones donde necesite una pausa
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        moveAction = GetComponent<PlayerInput>().actions["Move"];
    }
    private void Update()
    {
        Vector2 input = isFrozen ? Vector2.zero : moveAction.ReadValue<Vector2>();
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
    //freeze
        public void FreezeMovement(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(FreezeRoutine(duration));
    }
    private IEnumerator FreezeRoutine(float duration)
    {
        isFrozen = true;
        yield return new WaitForSeconds(duration);
        isFrozen = false;
    }
}