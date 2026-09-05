using UnityEngine;

public class DayWarningNotifier : MonoBehaviour
{
    [SerializeField] private string speakerName = "Ofelia";
    [SerializeField, TextArea(2, 4)]
    private string message = "Se hizo de día... los ingredientes que junté ya no sirven de nada. Tengo que volver a casa.";
    [SerializeField] private float displayDuration = 4f;

    private void OnEnable()
    {
        GameProgressManager.Instance.OnNightTimeExpired += ShowWarning;
    }

    private void OnDisable()
    {
        GameProgressManager.Instance.OnNightTimeExpired -= ShowWarning;
    }

    private void ShowWarning()
    {
        DialogueUI.Instance.ShowMessage(speakerName, message, displayDuration);
    }
}