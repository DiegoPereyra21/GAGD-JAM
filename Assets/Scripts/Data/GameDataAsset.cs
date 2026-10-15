using UnityEngine;

public abstract class GameDataAsset : ScriptableObject
{
    [Header("Identidad")]
    [Tooltip("Clave única que usan los saves. Solo minúsculas, números y '_'. Cambiarla invalida los saves existentes.")]
    [SerializeField] private string id;
    [SerializeField] private string displayName;

    public string Id => id;
    public string DisplayName => displayName;

#if UNITY_EDITOR
    [ContextMenu("Generar ID desde el nombre del asset")]
    protected void GenerateIdFromAssetName()
    {
        UnityEditor.Undo.RecordObject(this, "Generar ID");
        id = IdUtility.Sanitize(name);
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}