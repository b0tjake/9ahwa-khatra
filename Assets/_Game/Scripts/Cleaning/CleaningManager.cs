using UnityEngine;
using System;
using QahwaKhatra.Core;
using QahwaKhatra.Utils;

namespace QahwaKhatra.Cleaning
{
    public class CleaningManager : Singleton<CleaningManager>
    {
        [Header("Objective")]
        [SerializeField] private float _targetEarnings = 100f; // Target DH to finish Day 1
        [SerializeField] private bool _dayOneCompleted = false;

        public bool IsDayOneCompleted => _dayOneCompleted;

        public static event Action OnDayOneObjectiveMet;

        private void OnEnable()
        {
            EventBus.OnCurrencyChanged += CheckObjective;
        }

        private void OnDisable()
        {
            EventBus.OnCurrencyChanged -= CheckObjective;
        }

        private void CheckObjective(float currentDirhams)
        {
            if (!_dayOneCompleted && currentDirhams >= _targetEarnings)
            {
                _dayOneCompleted = true;
                Debug.Log("[CleaningManager] Day 1 Objective Met! You earned 100 DH. Ready to buy espresso machine!");
                OnDayOneObjectiveMet?.Invoke();
                EventBus.TriggerDayEnded(1, true);

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetState(GameState.CafeOpen);
                }
            }
        }
    }
}
