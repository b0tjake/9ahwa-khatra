using UnityEngine;
using QahwaKhatra.Player;
using QahwaKhatra.Data;

namespace QahwaKhatra.Cleaning
{
    public class JunkItem : MonoBehaviour, IInteractable
    {
        [Header("Data")]
        [SerializeField] private JunkItemSO _itemData;

        public JunkItemSO ItemData => _itemData;
        public string PromptMessage => $"Pick up {_itemData?.itemName ?? "Junk"}";

        private Collider _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            ApplyVisuals();
        }

        public void SetData(JunkItemSO data)
        {
            _itemData = data;
            ApplyVisuals();
        }

        private void ApplyVisuals()
        {
            if (_itemData == null) return;

            if (TryGetComponent<Renderer>(out var rend))
            {
                rend.material.color = _itemData.itemColor;
            }
            transform.localScale = _itemData.itemScale;
        }

        public void OnInteract(PlayerInteraction interactor)
        {
            if (interactor.TryGetComponent<PlayerInventory>(out var inventory))
            {
                if (inventory.HasCapacity)
                {
                    if (_collider != null) _collider.enabled = false;
                    inventory.TryAddItem(transform);
                }
            }
        }
    }
}
