using UnityEngine;

public class RandomVisualVariant : MonoBehaviour
{
    [SerializeField] private GameObject[] possibleModels;
    [SerializeField] private InteractableOutline outline;
    [SerializeField] private bool randomizeRotation;

    private void Awake()
    {
        if (possibleModels == null || possibleModels.Length == 0) return;

        GameObject chosen = possibleModels[Random.Range(0, possibleModels.Length)];

        Quaternion rotation = transform.rotation;
        if (randomizeRotation)
            rotation *= Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        GameObject instance = Instantiate(chosen, transform.position, rotation, transform);

        if (outline != null && instance.TryGetComponent(out Renderer renderer))
            outline.SetTargetRenderer(renderer);
    }
}