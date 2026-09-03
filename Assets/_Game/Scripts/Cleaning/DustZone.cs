using UnityEngine;
using System;
using QahwaKhatra.Player;

namespace QahwaKhatra.Cleaning
{
    public class DustZone : MonoBehaviour, IInteractable
    {
        [Header("Dust Settings")]
        [SerializeField] private float _cleanTimeRequired = 1.5f;
        [SerializeField] private Renderer _dustRenderer;

        private float _currentCleanProgress = 0f;
        private bool _isCleaned = false;

        public bool IsCleaned => _isCleaned;
        public string PromptMessage
        {
            get
            {
                if (_isCleaned) return "";
                if (CleaningManager.Instance != null && !CleaningManager.Instance.HasMop)
                {
                    return "خاصك جفاف من البيسي باش تسيق! (Need Mop & Bucket)";
                }
                return "Sweep Floor (سيّق الغبرة)";
            }
        }

        public static event Action OnDustCleaned;

        private void Awake()
        {
            if (_dustRenderer == null)
            {
                _dustRenderer = GetComponent<Renderer>();
            }
        }

        public void OnInteract(PlayerInteraction interactor)
        {
            if (_isCleaned) return;

            // Must have mop purchased first!
            if (CleaningManager.Instance != null && !CleaningManager.Instance.HasMop)
            {
                Debug.Log("[DustZone] You need to buy a Mop & Bucket from the Laptop before you can clean the floor!");
                return;
            }

            _currentCleanProgress += 0.5f;

            if (_dustRenderer != null && _dustRenderer.material != null)
            {
                Color c = _dustRenderer.material.color;
                c.a = Mathf.Clamp01(1f - (_currentCleanProgress / _cleanTimeRequired));
                _dustRenderer.material.color = c;
            }

            if (_currentCleanProgress >= _cleanTimeRequired)
            {
                _isCleaned = true;
                if (TryGetComponent<Collider>(out var col)) col.enabled = false;
                gameObject.SetActive(false);
                OnDustCleaned?.Invoke();
            }
        }
    }
}
