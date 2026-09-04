using UnityEngine;

public class IndoorFogDisabler : MonoBehaviour
{
    private void Update()
    {
        RenderSettings.fog = GameProgressManager.Instance.IsOutside;
    }
}