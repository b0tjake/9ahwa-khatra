using UnityEngine;
using System;
using QahwaKhatra.Core;
using QahwaKhatra.Utils;

namespace QahwaKhatra.Cleaning
{
    public enum Day1Phase
    {
        SellingJunk,         // Step 1: Pick up all junk and sell in derb
        NeedsMop,            // Step 2: All junk gone, need to buy mop & bucket from PC
        CleaningFloor,       // Step 3: Mop acquired, sweep all dust patches
        FloorCleanedBuyMachine, // Step 4: Floor sparkling clean, go to PC to buy espresso machine
        CafeReady            // Step 5: Espresso machine placed, day 1 complete, clients may now arrive
    }

    public class CleaningManager : Singleton<CleaningManager>
    {
        [Header("Day 1 State Flow")]
        [SerializeField] private Day1Phase _currentPhase = Day1Phase.SellingJunk;
        [SerializeField] private bool _hasMop = false;
        [SerializeField] private int _totalDustCleaned = 0;
        [SerializeField] private int _totalDustTarget = 3;

        public Day1Phase CurrentPhase => _currentPhase;
        public bool HasMop => _hasMop;
        public bool IsDayOneCompleted => _currentPhase == Day1Phase.CafeReady;

        public static event Action<Day1Phase> OnPhaseChanged;

        private void OnEnable()
        {
            DustZone.OnDustCleaned += HandleDustCleaned;
        }

        private void OnDisable()
        {
            DustZone.OnDustCleaned -= HandleDustCleaned;
        }

        private void Update()
        {
            // Auto detect when all junk has been removed
            if (_currentPhase == Day1Phase.SellingJunk)
            {
                var junkGroup = GameObject.Find("Day1_Junk_Group");
                if (junkGroup == null || junkGroup.transform.childCount == 0)
                {
                    SetPhase(Day1Phase.NeedsMop);
                    Debug.Log("[CleaningManager] All junk cleared! Now buy Mop & Bucket from the Laptop.");
                }
            }
        }

        public void AcquireMop()
        {
            _hasMop = true;
            SetPhase(Day1Phase.CleaningFloor);
            Debug.Log("[CleaningManager] Mop & Bucket acquired! Now clean the dust off the floor.");
        }

        private void HandleDustCleaned()
        {
            _totalDustCleaned++;
            if (_totalDustCleaned >= _totalDustTarget)
            {
                SetPhase(Day1Phase.FloorCleanedBuyMachine);
                Debug.Log("[CleaningManager] Floor is sparkling clean! Go to the Laptop to buy your first Espresso Machine.");
            }
        }

        public void CompleteCafeSetup()
        {
            SetPhase(Day1Phase.CafeReady);
            EventBus.TriggerDayEnded(1, true);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.CafeOpen);
            }
            Debug.Log("[CleaningManager] Cafe is ready! Clients will now start arriving.");
        }

        public void SetPhase(Day1Phase newPhase)
        {
            _currentPhase = newPhase;
            OnPhaseChanged?.Invoke(_currentPhase);
        }
    }
}
