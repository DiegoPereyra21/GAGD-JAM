using UnityEngine;
using UnityEngine.UIElements;

public class HUDPanelVisibility : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private CraftingRoom craftingRoom;

    private void OnEnable()
    {
        craftingRoom.OnEntered += Hide;
        craftingRoom.OnExited += Show;
    }

    private void OnDisable()
    {
        craftingRoom.OnEntered -= Hide;
        craftingRoom.OnExited -= Show;
    }

    private void Hide() => document.enabled = false;
    private void Show() => document.enabled = true;
}