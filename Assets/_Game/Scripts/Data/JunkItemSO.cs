using UnityEngine;

namespace QahwaKhatra.Data
{
    [CreateAssetMenu(fileName = "NewJunkItem", menuName = "QahwaKhatra/JunkItem")]
    public class JunkItemSO : ScriptableObject
    {
        [Header("Item Info")]
        public string itemName = "Old Junk";
        public float sellPrice = 15f; // in DH
        public Color itemColor = Color.gray;
        public Vector3 itemScale = Vector3.one * 0.5f;
    }
}
