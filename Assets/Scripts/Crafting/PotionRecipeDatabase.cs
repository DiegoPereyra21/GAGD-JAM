using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PotionRecipeDatabase", menuName = "Crafting/Potion Recipe Database")]
public class PotionRecipeDatabase : ScriptableObject
{
    [SerializeField] private List<PotionRecipe> recipes = new List<PotionRecipe>();

    public IReadOnlyList<PotionRecipe> Recipes => recipes;
    
    public PotionRecipe FindById(string potionId)
    {
        foreach (PotionRecipe recipe in recipes)
        {
            if (recipe.potionId == potionId)
                return recipe;
        }

        return null;
    }
}