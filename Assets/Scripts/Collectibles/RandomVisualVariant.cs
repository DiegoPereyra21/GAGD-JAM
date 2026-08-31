using System;
using Game.Collectibles;
using UnityEngine;

[Serializable]
public class VisualVariant
{
    public GameObject fullModel;
    public GameObject depletedModel; // opcional, dejalo vacío si no aplica
}

public class RandomVisualVariant : MonoBehaviour
{
    [SerializeField] private VisualVariant[] possibleModels;
    [SerializeField] private InteractableOutline outline;
    [SerializeField] private bool randomizeRotation;

    private GameObject currentInstance;
    private VisualVariant chosenVariant;
    private Collectible collectible;

    private void Awake()
    {
        if (possibleModels == null || possibleModels.Length == 0) return;

        if (randomizeRotation)
            transform.Rotate(0f, UnityEngine.Random.Range(0f, 360f), 0f);

        chosenVariant = possibleModels[UnityEngine.Random.Range(0, possibleModels.Length)];
        SpawnModel(chosenVariant.fullModel);

        collectible = GetComponent<Collectible>();
        if (collectible != null)
            collectible.OnCollected += HandleCollected;
    }

    private void OnDestroy()
    {
        if (collectible != null)
            collectible.OnCollected -= HandleCollected;
    }

    private void HandleCollected()
    {
        if (chosenVariant.depletedModel == null) return;
        SpawnModel(chosenVariant.depletedModel);
    }

    private void SpawnModel(GameObject model)
    {
        if (currentInstance != null)
            Destroy(currentInstance);

        currentInstance = Instantiate(model, transform.position, transform.rotation, transform);

        if (outline != null && currentInstance.TryGetComponent(out Renderer renderer))
            outline.SetTargetRenderer(renderer);
    }
}