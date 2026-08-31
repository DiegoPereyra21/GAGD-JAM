using UnityEngine;

namespace Game.Collectibles
{
    [CreateAssetMenu(fileName = "NewIngredient", menuName = "Collectibles/Ingredient Type")]
    public class IngredientType : ScriptableObject
    {
        public string ingredientId;
        public string displayName;
        public IngredientType rawSource;
    }
}