using UnityEngine;
using UnityEngine.InputSystem;
//para eliminar los saves y poder testear como si fuera el dia uno
public class DebugResetSaves : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            PlayerPrefs.DeleteAll();

            HomeStorage.Instance.Load();
            GameProgressManager.Instance.Load();
            GameProgressManager.Instance.StartNight();

            Debug.Log("[Debug] Todos los saves borrados y reseteados.");
        }
    }
}