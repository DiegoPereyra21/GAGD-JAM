using UnityEngine;
using UnityEngine.UIElements;

public class HUDPanelVisibility : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private CauldronCraftingSystem cauldron;

    private void OnEnable()
    {
        cauldron.OnEnteredCrafting += Hide;
        cauldron.OnExitedCrafting += Show;
    }

    private void OnDisable()
    {
        cauldron.OnEnteredCrafting -= Hide;
        cauldron.OnExitedCrafting -= Show;
    }

    private void Hide() => document.enabled = false;
    private void Show() => document.enabled = true;
}