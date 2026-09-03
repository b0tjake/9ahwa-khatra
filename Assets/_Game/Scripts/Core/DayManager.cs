using UnityEngine;
using System.Collections.Generic;
using QahwaKhatra.Utils;
using QahwaKhatra.Core;
using QahwaKhatra.Data;

namespace QahwaKhatra.Core
{
    public class DayManager : Singleton<DayManager>
    {
        [Header("Progression State")]
        [SerializeField] private int _currentDay = 1;
        [SerializeField] private float _dailyEarnings = 0f;
        [SerializeField] private int _dailyCustomersServed = 0;
        [SerializeField] private List<DayObjectiveSO> _dayObjectives = new List<DayObjectiveSO>();

        private bool _isDaySummaryOpen = false;

        public int CurrentDay => _currentDay;
        public float DailyEarnings => _dailyEarnings;
        public int DailyCustomersServed => _dailyCustomersServed;
        public DayObjectiveSO CurrentObjective => (_currentDay - 1 >= 0 && _currentDay - 1 < _dayObjectives.Count) ? _dayObjectives[_currentDay - 1] : null;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            EventBus.TriggerDayStarted(_currentDay);
        }

        private void OnEnable()
        {
            EventBus.OnCurrencyEarned += RecordEarnings;
            EventBus.OnCustomerServed += RecordCustomerServed;
        }

        private void OnDisable()
        {
            EventBus.OnCurrencyEarned -= RecordEarnings;
            EventBus.OnCustomerServed -= RecordCustomerServed;
        }

        private void RecordEarnings(float amount, string reason)
        {
            _dailyEarnings += amount;
            CheckObjectiveCompletion();
        }

        private void RecordCustomerServed(float score, int tip)
        {
            _dailyCustomersServed++;
            CheckObjectiveCompletion();
        }

        private void CheckObjectiveCompletion()
        {
            var obj = CurrentObjective;
            if (obj == null) return;

            if (_dailyEarnings >= obj.targetDirhams && !_isDaySummaryOpen)
            {
                EndDay(true);
            }
        }

        public void EndDay(bool success)
        {
            _isDaySummaryOpen = true;
            EventBus.TriggerDayEnded(_currentDay, success);
            Debug.Log($"[DayManager] Day {_currentDay} Ended! Earnings: {_dailyEarnings} DH, Customers: {_dailyCustomersServed}");
        }

        public void StartNextDay()
        {
            _currentDay++;
            _dailyEarnings = 0f;
            _dailyCustomersServed = 0;
            _isDaySummaryOpen = false;

            EventBus.TriggerDayStarted(_currentDay);
            Debug.Log($"[DayManager] Welcome to Day {_currentDay}!");
        }

        private void OnGUI()
        {
            if (!_isDaySummaryOpen) return;

            float w = 380f;
            float h = 260f;
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - h) / 2f;

            GUI.Box(new Rect(x, y, w, h), ArabicFixer.Fix($"🎉 نهاية اليوم {_currentDay} (Day Summary)"));

            GUI.Label(new Rect(x + 20, y + 40, w - 40, 30), ArabicFixer.Fix($"الفلوس المربوحة اليوم: {_dailyEarnings:F0} DH"));
            GUI.Label(new Rect(x + 20, y + 75, w - 40, 30), ArabicFixer.Fix($"الزبناء لي شربو القهوة: {_dailyCustomersServed} زبون"));
            GUI.Label(new Rect(x + 20, y + 110, w - 40, 45), ArabicFixer.Fix("مبروك كملتي الهدف ديال اليوم!\nيمكن ليك دوز لليوم الموالي وتطور المحل."));

            if (GUI.Button(new Rect(x + 20, y + 175, w - 40, 50), ArabicFixer.Fix("دوز لليوم الموالي (Start Next Day ➡️)")))
            {
                StartNextDay();
            }
        }
    }
}
