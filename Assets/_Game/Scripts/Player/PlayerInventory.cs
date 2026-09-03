using UnityEngine;
using System.Collections.Generic;

namespace QahwaKhatra.Player
{
    public class PlayerInventory : MonoBehaviour
    {
        [Header("Stacking Setup")]
        [SerializeField] private Transform _stackRoot;
        [SerializeField] private float _itemVerticalSpacing = 0.35f;
        [SerializeField] private int _maxCapacity = 4;

        [Header("Sway Juice")]
        [SerializeField] private float _swayIntensity = 5f;
        [SerializeField] private float _swaySmoothness = 10f;

        private readonly List<Transform> _carriedItems = new List<Transform>();
        private Vector3 _lastPosition;
        private Vector3 _currentVelocity;

        public int CurrentCount => _carriedItems.Count;
        public bool HasCapacity => _carriedItems.Count < _maxCapacity;
        public bool IsEmpty => _carriedItems.Count == 0;

        private void Start()
        {
            _lastPosition = transform.position;

            if (_stackRoot == null)
            {
                var go = new GameObject("StackRoot");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(0f, 1.8f, 0.2f);
                _stackRoot = go.transform;
            }
        }

        private void Update()
        {
            // Calculate velocity for procedural sway
            Vector3 worldDelta = transform.position - _lastPosition;
            _currentVelocity = Vector3.Lerp(_currentVelocity, worldDelta / Mathf.Max(Time.deltaTime, 0.001f), _swaySmoothness * Time.deltaTime);
            _lastPosition = transform.position;

            ApplyStackSway();
        }

        public bool TryAddItem(Transform itemTransform)
        {
            if (!HasCapacity) return false;

            _carriedItems.Add(itemTransform);
            itemTransform.SetParent(_stackRoot);

            // Position stacked item
            float heightOffset = (_carriedItems.Count - 1) * _itemVerticalSpacing;
            itemTransform.localPosition = new Vector3(0f, heightOffset, 0f);
            itemTransform.localRotation = Quaternion.identity;

            return true;
        }

        public Transform RemoveTopItem()
        {
            if (IsEmpty) return null;

            int lastIdx = _carriedItems.Count - 1;
            Transform item = _carriedItems[lastIdx];
            _carriedItems.RemoveAt(lastIdx);
            item.SetParent(null);
            return item;
        }

        private void ApplyStackSway()
        {
            if (IsEmpty) return;

            // Invert velocity in local space to create lean/drag effect
            Vector3 localVel = transform.InverseTransformDirection(_currentVelocity);
            float tiltZ = Mathf.Clamp(-localVel.x * _swayIntensity, -25f, 25f);
            float tiltX = Mathf.Clamp(localVel.z * _swayIntensity, -25f, 25f);

            for (int i = 0; i < _carriedItems.Count; i++)
            {
                // Items higher up sway more
                float factor = (i + 1) * 0.4f;
                Quaternion targetRot = Quaternion.Euler(tiltX * factor, 0f, tiltZ * factor);
                _carriedItems[i].localRotation = Quaternion.Slerp(_carriedItems[i].localRotation, targetRot, _swaySmoothness * Time.deltaTime);
            }
        }

        public void UpgradeCapacity(int extraSlots)
        {
            _maxCapacity += extraSlots;
        }
    }
}
