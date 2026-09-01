using UnityEngine;
using TMPro;

public class MailboxIndicatorTMP : MonoBehaviour
{
    [SerializeField] private Mailbox mailbox;
    [SerializeField] private TextMeshPro label;
    [SerializeField] private float referenceDistance = 10f;

    private Camera cam;
    private Vector3 baseScale;

    private void Awake()
    {
        cam = Camera.main;
        baseScale = label.transform.localScale;
    }

    private void LateUpdate()
    {
        int remaining = mailbox.RemainingCount;

        if (remaining <= 0)
        {
            label.text = "";
            return;
        }

        label.text = $"{remaining} cartas";
    }
}