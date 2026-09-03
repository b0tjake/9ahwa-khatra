using UnityEngine;

namespace QahwaKhatra.Data
{
    [CreateAssetMenu(fileName = "NewDrinkRecipe", menuName = "QahwaKhatra/DrinkRecipe")]
    public class DrinkRecipeSO : ScriptableObject
    {
        [Header("Drink Info")]
        public string drinkId = "espresso";
        public string drinkName = "قهوة خاترة (Espresso)";
        public float basePrice = 10f; // in DH
        public float targetGrindGrams = 13f;
        public float targetFillPercent = 0.25f; // 1/4 by default for khatra!
        public int targetSugarCubes = 1;
        public Color liquidColor = new Color(0.18f, 0.10f, 0.05f); // Dark rich coffee
    }
}
