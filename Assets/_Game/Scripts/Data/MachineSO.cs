using UnityEngine;

namespace QahwaKhatra.Data
{
    [CreateAssetMenu(fileName = "NewMachine", menuName = "QahwaKhatra/Machine")]
    public class MachineSO : ScriptableObject
    {
        [Header("Machine Info")]
        public string machineId = "espresso_basic";
        public string machineName = "آلة إسبريسو كلاسيك (Basic Espresso)";
        public float price = 100f; // 100 DH
        public string description = "Makes real قهوة خاترة. The heart of any Moroccan café.";
        public DrinkRecipeSO unlockedDrink;
    }
}
