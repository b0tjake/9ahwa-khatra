using System;

namespace QahwaKhatra.Utils
{
    public static class EventBus
    {
        // Currency Events
        public static event Action<float> OnCurrencyChanged;
        public static event Action<float, string> OnCurrencyEarned;
        public static void TriggerCurrencyChanged(float newAmount) => OnCurrencyChanged?.Invoke(newAmount);
        public static void TriggerCurrencyEarned(float amount, string reason) => OnCurrencyEarned?.Invoke(amount, reason);

        // Day Cycle Events
        public static event Action<int> OnDayStarted;
        public static event Action<int, bool> OnDayEnded;
        public static void TriggerDayStarted(int day) => OnDayStarted?.Invoke(day);
        public static void TriggerDayEnded(int day, bool success) => OnDayEnded?.Invoke(day, success);

        // Customer & Order Events
        public static event Action<string> OnCustomerOrdered;
        public static event Action<float, int> OnCustomerServed;
        public static event Action OnCustomerLeftAngry;
        public static void TriggerCustomerOrdered(string drinkName) => OnCustomerOrdered?.Invoke(drinkName);
        public static void TriggerCustomerServed(float score, int tip) => OnCustomerServed?.Invoke(score, tip);
        public static void TriggerCustomerLeftAngry() => OnCustomerLeftAngry?.Invoke();
    }
}
