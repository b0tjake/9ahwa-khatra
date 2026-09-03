using UnityEngine;
using QahwaKhatra.Utils;

namespace QahwaKhatra.Core
{
    public class CurrencyManager : Singleton<CurrencyManager>
    {
        [Header("Starting Currency")]
        [SerializeField] private float _currentDirhams = 0f;

        public float CurrentDirhams => _currentDirhams;

        private void Start()
        {
            EventBus.TriggerCurrencyChanged(_currentDirhams);
        }

        public void AddDirhams(float amount, string reason = "")
        {
            if (amount <= 0) return;

            _currentDirhams += amount;
            EventBus.TriggerCurrencyChanged(_currentDirhams);
            EventBus.TriggerCurrencyEarned(amount, reason);
        }

        public bool SpendDirhams(float amount)
        {
            if (amount <= 0) return true;

            if (_currentDirhams >= amount)
            {
                _currentDirhams -= amount;
                EventBus.TriggerCurrencyChanged(_currentDirhams);
                return true;
            }

            return false;
        }

        public bool HasEnough(float amount)
        {
            return _currentDirhams >= amount;
        }
    }
}
