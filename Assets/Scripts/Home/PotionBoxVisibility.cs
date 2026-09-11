using UnityEngine;

public class PotionBoxVisibility : MonoBehaviour
{
    [SerializeField] private GameObject potionBoxVisual;

    private void OnEnable()
    {
        GameProgressManager.Instance.OnDayStarted += ShowBox;
        GameProgressManager.Instance.OnWentOutside += HideBox;

        Refresh();
    }

    private void OnDisable()
    {
        GameProgressManager.Instance.OnDayStarted -= ShowBox;
        GameProgressManager.Instance.OnWentOutside -= HideBox;
    }

    private void Refresh()
    {
        potionBoxVisual.SetActive(!GameProgressManager.Instance.IsOutside);
    }

    private void ShowBox() => potionBoxVisual.SetActive(true);
    private void HideBox() => potionBoxVisual.SetActive(false);
}