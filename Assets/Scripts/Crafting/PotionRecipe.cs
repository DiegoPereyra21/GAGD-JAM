using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPotionRecipe", menuName = "Crafting/Potion Recipe")]
public class PotionRecipe : ScriptableObject
{
    public string potionId;
    public string potionName;
    public GameObject visualPrefab;
    public List<RecipeIngredient> ingredients;
    public string description;
}