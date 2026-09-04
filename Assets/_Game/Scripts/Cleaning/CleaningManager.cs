using UnityEngine;
using System;
using QahwaKhatra.Core;
using QahwaKhatra.Utils;

namespace QahwaKhatra.Cleaning
{
    public enum Day1Phase
    {
        SellingJunk,            // Step 1: Pick up all junk and sell in Derb Sultan
        NeedsMop,               // Step 2: Junk cleared. Must buy Mop & Bucket from Windows XP laptop
        CleaningFloor,          // Step 3: Mop acquired. Clean all dust patches
        FloorCleanedBuyMachine, // Step 4: Floor 100% clean. Return to laptop to buy Espresso Machine
        Day1Completed           // Step 5: Everything complete! Day 1 finishes properly.
    }

    public class CleaningManager : Singleton<CleaningManager>
    {
        [Header("Day 1 State Flow")]
        [SerializeField] private Day1Phase _currentPhase = Day1Phase.SellingJunk;
        [SerializeField] private bool _hasMop = false;
        [SerializeField] private bool _hasEspressoMachine = false;
        [SerializeField] private int _totalDustCleaned = 0;
        [SerializeField] private int _totalDustTarget = 3;

        public Day1Phase CurrentPhase => _currentPhase;
        public bool HasMop => _hasMop;
        public bool HasEspressoMachine => _hasEspressoMachine;
        public bool IsDayOneCompleted => _currentPhase == Day1Phase.Day1Completed;
        public int TotalDustCleaned => _totalDustCleaned;
        public int TotalDustTarget => _totalDustTarget;

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
            // Auto detect when all junk has been removed from garage
            if (_currentPhase == Day1Phase.SellingJunk)
            {
                var junkGroup = GameObject.Find("Day1_Junk_Group");
                if (junkGroup == null || junkGroup.transform.childCount == 0)
                {
                    SetPhase(Day1Phase.NeedsMop);
                    Debug.Log("[CleaningManager] Step 1 Complete: All junk cleared! Now buy Mop & Bucket from Windows XP Laptop.");
                }
            }
        }

        public void AcquireMop()
        {
            _hasMop = true;
            SetPhase(Day1Phase.CleaningFloor);
            Debug.Log("[CleaningManager] Step 2 Complete: Mop & Bucket acquired! Now sweep and clean all dust patches off the floor.");
        }

        private void HandleDustCleaned()
        {
            _totalDustCleaned++;
            Debug.Log($"[CleaningManager] Cleaned dust patch {_totalDustCleaned}/{_totalDustTarget}");

            if (_totalDustCleaned >= _totalDustTarget)
            {
                SetPhase(Day1Phase.FloorCleanedBuyMachine);
                Debug.Log("[CleaningManager] Step 3 Complete: Garage floor is 100% sparkling clean! Return to the Laptop to buy the Espresso Machine.");
            }
        }

        public void CompleteEspressoPurchase()
        {
            _hasEspressoMachine = true;
            SetPhase(Day1Phase.Day1Completed);

            Debug.Log("[CleaningManager] Step 4 Complete: Garage is clean, Mop is bought, Espresso machine is set up! Day 1 is COMPLETE!");

            // Notify DayManager to show the completion summary screen
            if (DayManager.Instance != null)
            {
                DayManager.Instance.EndDay(true);
            }
        }

        public void SetPhase(Day1Phase newPhase)
        {
            _currentPhase = newPhase;
            OnPhaseChanged?.Invoke(_currentPhase);
        }

        public string GetCurrentObjectiveText()
        {
            switch (_currentPhase)
            {
                case Day1Phase.SellingJunk:
                    var junkGroup = GameObject.Find("Day1_Junk_Group");
                    int remaining = junkGroup != null ? junkGroup.transform.childCount : 0;
                    return $"1. خوي الكراج من الخردة ({remaining} باقين)";
                case Day1Phase.NeedsMop:
                    return "2. شري سطل وجفاف من البيسي (Windows XP)";
                case Day1Phase.CleaningFloor:
                    return $"3. سيّق الغبرة كاملة من الأرض ({_totalDustCleaned}/{_totalDustTarget})";
                case Day1Phase.FloorCleanedBuyMachine:
                    return "4. شري آلة القهوة من البيسي (100 DH)";
                case Day1Phase.Day1Completed:
                    return "✅ كملتي كلشي! الكراج نقي وآلة القهوة واجدة";
                default:
                    return "";
            }
        }

        private void OnGUI()
        {
            // Display Day 1 checklist in top-left corner
            if (!IsDayOneCompleted)
            {
                float w = 320f;
                float h = 75f;
                float x = 20f;
                float y = 50f;

                GUI.Box(new Rect(x, y, w, h), ArabicFixer.Fix("📋 أهداف اليوم الأول (Day 1 Tasks)"));
                GUI.Label(new Rect(x + 10, y + 25, w - 20, 45), ArabicFixer.Fix(GetCurrentObjectiveText()));
            }
        }
    }
}
