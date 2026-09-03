using UnityEngine;

namespace QahwaKhatra.Data
{
    [CreateAssetMenu(fileName = "NewCustomerType", menuName = "QahwaKhatra/CustomerType")]
    public class CustomerTypeSO : ScriptableObject
    {
        [Header("Profile")]
        public string customerTypeName = "Regular (زبون عادي)";
        public Color clothingColor = new Color(0.2f, 0.4f, 0.8f);
        
        [Header("Patience & Payout")]
        public float basePatience = 45f; // Seconds before leaving angry
        public float tipMultiplier = 1.0f;
        
        [Header("Drink Preference")]
        public DrinkRecipeSO preferredDrink;
        public float preferredFill = 0.25f; // e.g. 1/4 (khatra), 1/2, full
        public int preferredSugar = 1;
    }
}
