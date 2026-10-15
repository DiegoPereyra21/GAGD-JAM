using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Object = UnityEngine.Object;

public class GameDatabaseValidator : IPreprocessBuildWithReport
{
    private const string ExpectedPathSuffix = "/Resources/" + GameDatabase.ResourcesPath + ".asset";

    public int callbackOrder => 0;

    public class Result
    {
        public int Errors { get; private set; }
        public int Warnings { get; private set; }

        public void Error(string message, Object context)
        {
            Errors++;
            Debug.LogError("[Validador] " + message, context);
        }

        public void Warning(string message, Object context)
        {
            Warnings++;
            Debug.LogWarning("[Validador] " + message, context);
        }
    }

    [MenuItem("Tools/Validar datos del juego")]
    private static void ValidateFromMenu()
    {
        Result result = Validate();
        string summary = $"[Validador] Terminado: {result.Errors} error(es), {result.Warnings} advertencia(s).";

        if (result.Errors > 0) Debug.LogError(summary);
        else if (result.Warnings > 0) Debug.LogWarning(summary);
        else Debug.Log(summary);
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        Result result = Validate();
        if (result.Errors > 0)
            throw new BuildFailedException($"[Validador] Hay {result.Errors} error(es) en los datos del juego. Revisá la consola.");
    }

    public static Result Validate()
    {
        Result result = new Result();

        GameDatabase db = FindDatabase(result);
        if (db == null) return result;

        ValidateIdentity(db.Items, "ítems", db, result);
        ValidateIdentity(db.Potions, "pociones", db, result);
        ValidateIdentity(db.Quests, "quests", db, result);
        ValidateItems(db, result);
        ValidatePotions(db, result);
        ValidateQuests(db, result);
        ValidateSpecialReferences(db, result);
        ValidateUnlisted(db.Items, result);
        ValidateUnlisted(db.Potions, result);
        ValidateUnlisted(db.Quests, result);

        return result;
    }

    private static GameDatabase FindDatabase(Result result)
    {
        string[] guids = AssetDatabase.FindAssets("t:" + nameof(GameDatabase));

        if (guids.Length == 0)
        {
            result.Error($"No existe ningún GameDatabase. Crealo en 'Assets/Resources/{GameDatabase.ResourcesPath}.asset'.", null);
            return null;
        }

        if (guids.Length > 1)
            result.Warning($"Hay {guids.Length} GameDatabase en el proyecto. El juego solo usa el que termina en '{ExpectedPathSuffix}'.", null);

        GameDatabase fallback = null;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameDatabase db = AssetDatabase.LoadAssetAtPath<GameDatabase>(path);

            if (path.EndsWith(ExpectedPathSuffix)) return db;
            if (fallback == null) fallback = db;
        }

        result.Error($"El GameDatabase tiene que estar en una carpeta 'Resources' y llamarse '{GameDatabase.ResourcesPath}.asset', si no el juego no lo encuentra.", fallback);
        return fallback;
    }

    private static void ValidateIdentity<T>(IReadOnlyList<T> list, string label, Object owner, Result result) where T : GameDataAsset
    {
        HashSet<T> seenAssets = new HashSet<T>();
        Dictionary<string, T> idOwners = new Dictionary<string, T>();

        for (int i = 0; i < list.Count; i++)
        {
            T entry = list[i];

            if (entry == null)
            {
                result.Error($"La lista de {label} tiene un elemento vacío (posición {i}).", owner);
                continue;
            }

            if (!seenAssets.Add(entry))
            {
                result.Warning($"'{entry.name}' está repetido en la lista de {label}.", entry);
                continue;
            }

            if (string.IsNullOrEmpty(entry.Id))
                result.Error($"'{entry.name}' no tiene id. Menú ⋮ del Inspector > 'Generar ID desde el nombre del asset'.", entry);
            else if (!IdUtility.IsValid(entry.Id))
                result.Error($"'{entry.name}': el id '{entry.Id}' no es válido (sugerido: '{IdUtility.Sanitize(entry.Id)}').", entry);
            else if (idOwners.TryGetValue(entry.Id, out T other))
                result.Error($"'{entry.name}' y '{other.name}' comparten el id '{entry.Id}'.", entry);
            else
                idOwners[entry.Id] = entry;

            if (string.IsNullOrWhiteSpace(entry.DisplayName))
                result.Error($"'{entry.name}' no tiene nombre visible.", entry);
        }
    }

    private static void ValidateItems(GameDatabase db, Result result)
    {
        foreach (ItemData item in db.Items)
        {
            if (item == null) continue;

            if (item.GetModel(ItemState.Raw) == null)
                result.Error($"'{item.name}' no tiene modelo crudo.", item);

            if (item.GetIcon(ItemState.Raw) == null)
                result.Warning($"'{item.name}' no tiene ícono crudo.", item);

            if (item.Processable)
            {
                if (string.IsNullOrWhiteSpace(item.GetName(ItemState.Processed)))
                    result.Error($"'{item.name}' es procesable pero no tiene nombre procesado.", item);

                if (item.GetModel(ItemState.Processed) == null)
                    result.Error($"'{item.name}' es procesable pero no tiene modelo procesado.", item);

                if (item.GetIcon(ItemState.Processed) == null)
                    result.Warning($"'{item.name}' es procesable pero no tiene ícono procesado.", item);
            }
            else if (HasProcessedData(item))
            {
                result.Warning($"'{item.name}' no es procesable pero tiene datos de procesado cargados (se ignoran).", item);
            }
        }
    }

    private static bool HasProcessedData(ItemData item)
    {
        SerializedObject so = new SerializedObject(item);
        return !string.IsNullOrEmpty(so.FindProperty("processedName").stringValue)
            || so.FindProperty("processedIcon").objectReferenceValue != null
            || so.FindProperty("processedModel").objectReferenceValue != null;
    }

    private static void ValidatePotions(GameDatabase db, Result result)
    {
        HashSet<ItemData> dbItems = new HashSet<ItemData>(db.Items.Where(i => i != null));
        Dictionary<string, PotionData> signatures = new Dictionary<string, PotionData>();

        foreach (PotionData potion in db.Potions)
        {
            if (potion == null) continue;

            if (potion.Model == null)
                result.Error($"'{potion.name}' no tiene modelo.", potion);

            if (potion.Image == null)
                result.Warning($"'{potion.name}' no tiene imagen.", potion);

            if (potion.Ingredients.Count == 0)
            {
                result.Error($"'{potion.name}' no tiene ingredientes.", potion);
                continue;
            }

            HashSet<ItemKey> seenKeys = new HashSet<ItemKey>();
            bool valid = true;

            for (int i = 0; i < potion.Ingredients.Count; i++)
            {
                PotionIngredient ingredient = potion.Ingredients[i];
                string where = $"'{potion.name}', ingrediente {i + 1}";

                if (ingredient == null || ingredient.Item == null)
                {
                    result.Error($"{where}: no tiene ítem asignado.", potion);
                    valid = false;
                    continue;
                }

                if (ingredient.Amount < 1)
                {
                    result.Error($"{where} ({ingredient.Item.name}): la cantidad tiene que ser 1 o más.", potion);
                    valid = false;
                }

                if (!dbItems.Contains(ingredient.Item))
                {
                    result.Error($"{where}: '{ingredient.Item.name}' no está en el GameDatabase.", potion);
                    valid = false;
                }

                if (!ingredient.Item.HasState(ingredient.State))
                {
                    result.Error($"{where}: '{ingredient.Item.name}' no es procesable y la receta lo pide procesado.", potion);
                    valid = false;
                }

                if (!seenKeys.Add(ingredient.Key))
                    result.Warning($"{where}: '{ingredient.Key.DisplayName}' aparece más de una vez; las cantidades se suman.", potion);
            }

            if (!valid) continue;

            string signature = BuildSignature(potion);
            if (signatures.TryGetValue(signature, out PotionData other))
                result.Error($"'{potion.name}' y '{other.name}' tienen exactamente los mismos ingredientes: el caldero no puede distinguirlas.", potion);
            else
                signatures[signature] = potion;
        }
    }

    private static string BuildSignature(PotionData potion)
    {
        return string.Join("|", potion.GetRequirements()
            .Select(pair => $"{pair.Key.Item.GetHashCode()}:{(int)pair.Key.State}:{pair.Value}")
            .OrderBy(s => s, StringComparer.Ordinal));
    }

    private static void ValidateQuests(GameDatabase db, Result result)
    {
        HashSet<PotionData> dbPotions = new HashSet<PotionData>(db.Potions.Where(p => p != null));

        foreach (QuestData quest in db.Quests)
        {
            if (quest == null) continue;

            if (string.IsNullOrWhiteSpace(quest.VillagerName))
                result.Warning($"'{quest.name}' no tiene aldeano.", quest);

            if (quest.Potion == null)
                result.Error($"'{quest.name}' no tiene poción asignada.", quest);
            else if (!dbPotions.Contains(quest.Potion))
                result.Error($"'{quest.name}': la poción '{quest.Potion.name}' no está en el GameDatabase.", quest);
            else if (quest.Potion == db.SleepPotion)
                result.Warning($"'{quest.name}' pide la poción del sueño.", quest);
        }
    }

    private static void ValidateSpecialReferences(GameDatabase db, Result result)
    {
        HashSet<ItemData> dbItems = new HashSet<ItemData>(db.Items.Where(i => i != null));
        HashSet<PotionData> dbPotions = new HashSet<PotionData>(db.Potions.Where(p => p != null));

        if (db.SleepPotion == null)
            result.Error("No está asignada la poción del sueño (Sleep Potion).", db);
        else if (!dbPotions.Contains(db.SleepPotion))
            result.Error($"La poción del sueño '{db.SleepPotion.name}' no está en la lista de pociones.", db);

        if (db.SleepIngredient == null)
        {
            result.Error("No está asignado el ingrediente del sueño (Sleep Ingredient).", db);
            return;
        }

        if (!dbItems.Contains(db.SleepIngredient))
            result.Error($"El ingrediente del sueño '{db.SleepIngredient.name}' no está en la lista de ítems.", db);

        if (!db.SleepIngredient.IsKeyItem)
            result.Warning($"'{db.SleepIngredient.name}' debería tener 'Is Key Item' activado para que no se pueda tirar del canasto.", db.SleepIngredient);

        if (db.SleepPotion != null && !db.SleepPotion.Ingredients.Any(i => i != null && i.Item == db.SleepIngredient))
            result.Warning($"La poción del sueño no usa '{db.SleepIngredient.name}' como ingrediente.", db.SleepPotion);
    }

    private static void ValidateUnlisted<T>(IReadOnlyList<T> listed, Result result) where T : GameDataAsset
    {
        HashSet<T> set = new HashSet<T>(listed.Where(x => x != null));

        foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(T).Name))
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
            if (asset != null && !set.Contains(asset))
                result.Warning($"'{asset.name}' existe pero no está en el GameDatabase: el juego no lo va a usar.", asset);
        }
    }
}