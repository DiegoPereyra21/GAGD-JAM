using UnityEngine;

public class PlayerSpawnHandler : MonoBehaviour
{
    [SerializeField] private CharacterController playerController;
    [SerializeField] private Transform doorInsideSpawnPoint;
    [SerializeField] private CameraTransition cameraTransition;
    [SerializeField] private Transform houseViewAnchor;

    private void Start()
    {
        if (!GameProgressManager.Instance.ShouldSpawnAtDoor) return;

        playerController.enabled = false;
        playerController.transform.position = doorInsideSpawnPoint.position;
        playerController.enabled = true;

        cameraTransition.TransitionTo(houseViewAnchor);
    }
}