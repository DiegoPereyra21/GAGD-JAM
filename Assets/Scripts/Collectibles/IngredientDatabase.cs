using System.Collections.Generic;
using UnityEngine;

namespace Game.Collectibles
{
    [CreateAssetMenu(fileName = "IngredientDatabase", menuName = "Collectibles/Ingredient Database")]
    public class IngredientDatabase : ScriptableObject
    {
        [SerializeField] private List<IngredientType> allIngredients = new List<IngredientType>();

        public IReadOnlyList<IngredientType> AllIngredients => allIngredients;

        public IngredientType FindById(string ingredientId)
        {
            foreach (IngredientType ingredient in allIngredients)
            {
                if (ingredient.ingredientId == ingredientId)
                    return ingredient;
            }

            return null;
        }
    }
}