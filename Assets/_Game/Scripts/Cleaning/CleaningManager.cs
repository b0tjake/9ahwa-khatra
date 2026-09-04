using UnityEngine;
using System;
using QahwaKhatra.Core;
using QahwaKhatra.Utils;

namespace QahwaKhatra.Cleaning
{
    public enum Day1Phase
    {
        SellingJunk,            // Step 1: Pick up all junk and sell in Derb Sultan
        NeedsMop,               // Step 2: Buy Mop & Bucket from Windows XP laptop
        WaitingMopDelivery,     // Step 3: Go outside garage and pick up mop delivery box
        CleaningFloor,          // Step 4: Sweep all dust patches off the floor
        FloorCleanedBuyMachine, // Step 5: Floor clean. Buy Espresso Machine from laptop
        WaitingMachineDelivery, // Step 6: Go outside and open espresso delivery box
        Day1Completed           // Step 7: Everything complete!
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
            if (_currentPhase == Day1Phase.SellingJunk)
            {
                var junkGroup = GameObject.Find("Day1_Junk_Group");
                if (junkGroup == null || junkGroup.transform.childCount == 0)
                {
                    SetPhase(Day1Phase.NeedsMop);
                    Debug.Log("[CleaningManager] All junk cleared! Now buy Mop & Bucket from Windows XP Laptop.");
                }
            }
        }

        public void NotifyOrderedMop()
        {
            SetPhase(Day1Phase.WaitingMopDelivery);
            Debug.Log("[CleaningManager] Mop ordered! Check outside the garage in the derb for the delivery box.");
        }

        public void AcquireMop()
        {
            _hasMop = true;
            SetPhase(Day1Phase.CleaningFloor);
            Debug.Log("[CleaningManager] Mop & Bucket unboxed! Now clean all dust patches off the floor.");
        }

        private void HandleDustCleaned()
        {
            _totalDustCleaned++;
            Debug.Log($"[CleaningManager] Cleaned dust patch {_totalDustCleaned}/{_totalDustTarget}");

            if (_totalDustCleaned >= _totalDustTarget)
            {
                SetPhase(Day1Phase.FloorCleanedBuyMachine);
                Debug.Log("[CleaningManager] Floor is 100% clean! Return to the Laptop to buy the Espresso Machine.");
            }
        }

        public void NotifyOrderedEspresso()
        {
            SetPhase(Day1Phase.WaitingMachineDelivery);
            Debug.Log("[CleaningManager] Espresso machine ordered! Check outside in the street for delivery.");
        }

        public void CompleteEspressoPurchase()
        {
            _hasEspressoMachine = true;
            SetPhase(Day1Phase.Day1Completed);

            Debug.Log("[CleaningManager] Espresso machine unboxed and installed! Day 1 is complete!");

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
                case Day1Phase.WaitingMopDelivery:
                    return "📦 3. خرج للزنقة حل كرتونة السطل والجفاف (Derb Delivery)";
                case Day1Phase.CleaningFloor:
                    return $"4. سيّق الغبرة كاملة من الأرض ({_totalDustCleaned}/{_totalDustTarget})";
                case Day1Phase.FloorCleanedBuyMachine:
                    return "5. شري آلة القهوة من البيسي (100 DH)";
                case Day1Phase.WaitingMachineDelivery:
                    return "📦 6. خرج للزنقة استلم كرتونة آلة القهوة (Espresso Delivery)";
                case Day1Phase.Day1Completed:
                    return "✅ كملتي كلشي! الكراج نقي وآلة القهوة واجدة";
                default:
                    return "";
            }
        }

        private void OnGUI()
        {
            // Only draw checklist if no full screen UI (like laptop) is open
            var laptop = FindFirstObjectByType<QahwaKhatra.Cafe.LaptopShop>();
            if (laptop != null && laptop.IsOpen) return;

            if (!IsDayOneCompleted)
            {
                float w = 360f;
                float h = 80f;
                float x = 20f;
                float y = 50f;

                GUI.Box(new Rect(x, y, w, h), ArabicFixer.Fix("📋 أهداف اليوم الأول (Day 1 Tasks)"));
                GUI.Label(new Rect(x + 10, y + 25, w - 20, 50), ArabicFixer.Fix(GetCurrentObjectiveText()));
            }
        }
    }
}
