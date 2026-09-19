using UnityEngine;

public class PlayerLightVisibility : MonoBehaviour
{
    [SerializeField] private GameObject playerLight;

    private void OnEnable()
    {
        GameProgressManager.Instance.OnWentOutside += ShowLight;
        GameProgressManager.Instance.OnDayStarted += HideLight;

        Refresh();
    }

    private void OnDisable()
    {
        GameProgressManager.Instance.OnWentOutside -= ShowLight;
        GameProgressManager.Instance.OnDayStarted -= HideLight;
    }

    private void Refresh()
    {
        playerLight.SetActive(GameProgressManager.Instance.IsOutside);
    }

    private void ShowLight() => playerLight.SetActive(true);
    private void HideLight() => playerLight.SetActive(false);
}