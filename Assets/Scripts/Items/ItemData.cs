using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Game Data/Item")]
public class ItemData : GameDataAsset
{
    [TextArea(2, 4)]
    [SerializeField] private string description;

    [Header("Mesa")]
    [SerializeField] private TableType table;
    [Tooltip("Si está desactivado, el ítem solo existe crudo.")]
    [SerializeField] private bool processable = true;
    [Tooltip("No se puede tirar del canasto.")]
    [SerializeField] private bool isKeyItem;

    [Header("Crudo")]
    [SerializeField] private Sprite rawIcon;
    [Tooltip("Modelo en la mesa y el caldero.")]
    [SerializeField] private GameObject rawModel;
    [Tooltip("Escala del modelo crudo dentro del canasto.")]
    [SerializeField, Min(0.01f)] private float basketScale = 1f;

    [Header("Procesado (solo si es procesable)")]
    [SerializeField] private string processedName;
    [SerializeField] private Sprite processedIcon;
    [SerializeField] private GameObject processedModel;

    public string Description => description;
    public TableType Table => table;
    public bool Processable => processable;
    public bool IsKeyItem => isKeyItem;
    public float BasketScale => basketScale;

    public bool IsGrindable => processable && table == TableType.Mortar;
    public bool IsCuttable => processable && table == TableType.CuttingBoard;

    public bool HasState(ItemState state) => state == ItemState.Raw || processable;

    public string GetName(ItemState state)
    {
        if (state == ItemState.Raw) return DisplayName;
        return processable ? processedName : null;
    }

    public Sprite GetIcon(ItemState state)
    {
        if (state == ItemState.Raw) return rawIcon;
        return processable ? processedIcon : null;
    }

    public GameObject GetModel(ItemState state)
    {
        if (state == ItemState.Raw) return rawModel;
        return processable ? processedModel : null;
    }
}