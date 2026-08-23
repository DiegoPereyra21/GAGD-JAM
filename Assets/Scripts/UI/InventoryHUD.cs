using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using Game.Collectibles;
//Hud q por ahora solamente muestra la cantidad de items, pero luego seria el menu interactivo con canasto
public class InventoryHUD : MonoBehaviour
{
    [SerializeField] private PlayerInventory inventory;
    private Label label;
    private void Awake()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        label = root.Q<Label>("InventoryLabel");
    }
    private void OnEnable()
    {
        inventory.OnInventoryChanged += UpdateLabel;
        UpdateLabel();
    }
    private void OnDisable()
    {
        inventory.OnInventoryChanged -= UpdateLabel;
    }
    private void UpdateLabel()
    {
        var sb = new StringBuilder();
        foreach (CollectibleType type in System.Enum.GetValues(typeof(CollectibleType)))
            sb.AppendLine($"{type}: {inventory.GetCount(type)}");

        label.text = sb.ToString();
    }
}