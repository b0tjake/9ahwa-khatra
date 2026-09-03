using UnityEngine;

namespace QahwaKhatra.Data
{
    [CreateAssetMenu(fileName = "NewDayObjective", menuName = "QahwaKhatra/DayObjective")]
    public class DayObjectiveSO : ScriptableObject
    {
        [Header("Day Setup")]
        public int dayNumber = 1;
        public string dayTitle = "اليوم الأول: بداية المقهى";
        public float targetDirhams = 200f;
        public int targetCustomers = 5;
        
        [Header("Narrative")]
        public string narrativePrompt = "اخدم القهوة وربح الفلوس باش تطور المحل!";
    }
}
