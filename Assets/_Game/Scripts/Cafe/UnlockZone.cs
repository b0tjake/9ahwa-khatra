using UnityEngine;
using System.Collections;
using QahwaKhatra.Player;
using QahwaKhatra.Core;
using QahwaKhatra.Utils;

namespace QahwaKhatra.Cafe
{
    public class UnlockZone : MonoBehaviour
    {
        [Header("Unlock Settings")]
        [SerializeField] private string _zoneName = "Terrasse Seating";
        [SerializeField] private float _cost = 150f;
        [SerializeField] private float _currentPaid = 0f;
        [SerializeField] private GameObject _targetToActivate;
        [SerializeField] private bool _isUnlocked = false;

        private float _paymentInterval = 0.12f;
        private Coroutine _paymentCoroutine;

        public bool IsUnlocked => _isUnlocked;
        public float RemainingCost => Mathf.Max(0f, _cost - _currentPaid);

        private void Start()
        {
            if (_targetToActivate != null)
            {
                _targetToActivate.SetActive(_isUnlocked);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isUnlocked) return;

            if (other.TryGetComponent<PlayerController>(out var player))
            {
                if (_paymentCoroutine != null) StopCoroutine(_paymentCoroutine);
                _paymentCoroutine = StartCoroutine(PayForUnlockRoutine());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<PlayerController>(out var player))
            {
                if (_paymentCoroutine != null)
                {
                    StopCoroutine(_paymentCoroutine);
                    _paymentCoroutine = null;
                }
            }
        }

        private IEnumerator PayForUnlockRoutine()
        {
            while (!_isUnlocked)
            {
                if (CurrencyManager.Instance != null && CurrencyManager.Instance.CurrentDirhams >= 10f)
                {
                    float payChunk = Mathf.Min(10f, RemainingCost);
                    if (CurrencyManager.Instance.SpendDirhams(payChunk))
                    {
                        _currentPaid += payChunk;

                        if (_currentPaid >= _cost)
                        {
                            CompleteUnlock();
                            yield break;
                        }
                    }
                }
                yield return new WaitForSeconds(_paymentInterval);
            }
        }

        private void CompleteUnlock()
        {
            _isUnlocked = true;
            if (_targetToActivate != null)
            {
                _targetToActivate.SetActive(true);
            }

            Debug.Log($"[UnlockZone] Unlocked: {_zoneName}!");
            gameObject.SetActive(false); // Hide unlock circle
        }

        private void OnGUI()
        {
            if (_isUnlocked) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 screenPos = cam.WorldToScreenPoint(transform.position + Vector3.up * 0.8f);
            if (screenPos.z > 0)
            {
                float w = 150f;
                float h = 45f;
                float x = screenPos.x - w / 2f;
                float y = Screen.height - screenPos.y - h;

                GUI.Box(new Rect(x, y, w, h), "");
                GUI.Label(new Rect(x + 5, y + 4, w - 10, 20), ArabicFixer.Fix(_zoneName));
                GUI.Label(new Rect(x + 5, y + 22, w - 10, 20), $"{RemainingCost:F0} DH");
            }
        }
    }
}
