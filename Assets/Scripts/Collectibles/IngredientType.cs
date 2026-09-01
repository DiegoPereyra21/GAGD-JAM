using UnityEngine;

namespace Game.Collectibles
{
    [CreateAssetMenu(fileName = "NewIngredient", menuName = "Collectibles/Ingredient Type")]
    public class IngredientType : ScriptableObject
    {
        public string ingredientId;
        public string displayName;
        public IngredientType rawSource;
        [TextArea(2, 4)]
        public string locationDescription;
    }
}