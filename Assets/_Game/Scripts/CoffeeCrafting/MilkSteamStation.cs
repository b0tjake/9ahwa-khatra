using UnityEngine;
using UnityEngine.InputSystem;
using QahwaKhatra.Player;
using QahwaKhatra.Data;
using QahwaKhatra.Core;
using QahwaKhatra.Utils;

namespace QahwaKhatra.CoffeeCrafting
{
    public class MilkSteamStation : MonoBehaviour, IInteractable
    {
        [Header("Nous-Nous State")]
        [SerializeField] private bool _isCrafting = false;
        [SerializeField] private float _milkTemp = 20f; // Starts at room temp (20°C). Green zone: 65°C - 75°C
        [SerializeField] private float _coffeeFill = 0.5f; // Half espresso
        [SerializeField] private float _milkFill = 0f; // Half milk to make نص نص!
        [SerializeField] private int _craftingStep = 0; // 0: Steam Milk, 1: Pour Milk, 2: Done

        public string PromptMessage => ArabicFixer.Fix("صاوب نص نص (Make Nous-Nous)");
        public bool IsCrafting => _isCrafting;

        public void OnInteract(PlayerInteraction interactor)
        {
            if (!_isCrafting)
            {
                StartCrafting();
            }
        }

        public void StartCrafting()
        {
            _isCrafting = true;
            _craftingStep = 0;
            _milkTemp = 20f;
            _coffeeFill = 0.5f;
            _milkFill = 0f;
        }

        private void Update()
        {
            if (!_isCrafting) return;

            // Step 0: Steam Wand (Hold to heat milk towards green zone: 65°C - 75°C)
            if (_craftingStep == 0)
            {
                bool isHolding = false;
                if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed) isHolding = true;
                if (Mouse.current != null && Mouse.current.leftButton.isPressed) isHolding = true;
                if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) isHolding = true;

                if (isHolding)
                {
                    _milkTemp += 35f * Time.deltaTime;
                    if (_milkTemp > 100f) _milkTemp = 100f;
                }

                if (!isHolding && _milkTemp > 35f)
                {
                    _craftingStep = 1; // Move to pour milk
                }
            }
            // Step 1: Pour Steamed Milk (Hold to fill second half of glass to 100%)
            else if (_craftingStep == 1)
            {
                bool isHolding = false;
                if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed) isHolding = true;
                if (Mouse.current != null && Mouse.current.leftButton.isPressed) isHolding = true;
                if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) isHolding = true;

                if (isHolding)
                {
                    _milkFill += 0.4f * Time.deltaTime;
                    if (_milkFill > 0.5f) _milkFill = 0.5f;
                }

                if (!isHolding && _milkFill > 0.1f)
                {
                    FinishCrafting();
                }
            }
        }

        private void FinishCrafting()
        {
            _craftingStep = 2;
            _isCrafting = false;

            // Score calculation:
            // Temp: target 70°C, tolerance ±8°C = 100%
            float tempDiff = Mathf.Abs(_milkTemp - 70f);
            float tempScore = Mathf.Clamp01(1f - (tempDiff / 20f)) * 100f;

            // Pour: target 0.5 milk fill, tolerance ±0.05 = 100%
            float pourDiff = Mathf.Abs(_milkFill - 0.5f);
            float pourScore = Mathf.Clamp01(1f - (pourDiff / 0.25f)) * 100f;

            float finalScore = (tempScore * 0.5f) + (pourScore * 0.5f);
            float payout = 15f; // Nous-Nous base price = 15 DH

            string feedback = "نص نص عادي (Average)";
            if (finalScore >= 85f)
            {
                feedback = "نص نص رائع! ⭐⭐⭐ (Perfect Nous-Nous!)";
                payout = 20f; // with tip
            }
            else if (finalScore < 50f)
            {
                feedback = "حليب محروق! 😡 (Burnt milk)";
                payout = 8f;
            }

            Debug.Log($"[MilkSteamStation] Crafted Nous-Nous! Score: {finalScore:F0}%. Result: {feedback}. Payout: {payout} DH");

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddDirhams(payout, "Served Nous-Nous (نص نص)");
            }
        }

        private void OnGUI()
        {
            if (!_isCrafting) return;

            float w = 400f;
            float h = 260f;
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - h) / 2f;

            GUI.Box(new Rect(x, y, w, h), ArabicFixer.Fix("☕🥛 تحضير نص نص (Nous-Nous Crafting)"));

            if (_craftingStep == 0)
            {
                GUI.Label(new Rect(x + 20, y + 40, w - 40, 30), ArabicFixer.Fix("1. سخن الحليب بالبخار (Target: 65°C - 75°C):"));
                GUI.Label(new Rect(x + 20, y + 70, w - 40, 30), $"الحرارة: {_milkTemp:F0}°C (المنطقة الخضراء: 70°C)");

                // Temperature gauge
                GUI.HorizontalSlider(new Rect(x + 20, y + 105, w - 40, 30), _milkTemp, 20f, 100f);

                string hint = _milkTemp < 65f ? "اضغط وماتطلقش باش تسخن الحليب!" : (_milkTemp <= 75f ? "🔥 ممتاز! طلق دابا (Release Now!)" : "⚠️ الحليب غادي يتحرق! (Too Hot!)");
                GUI.Label(new Rect(x + 20, y + 140, w - 40, 45), ArabicFixer.Fix(hint));
            }
            else if (_craftingStep == 1)
            {
                GUI.Label(new Rect(x + 20, y + 40, w - 40, 30), ArabicFixer.Fix("2. كب الحليب فوق القهوة (نص قهوة / نص حليب):"));
                GUI.Label(new Rect(x + 20, y + 70, w - 40, 30), $"الحليب: {(_milkFill * 200f):F0}% / 100% (الهدف: نص كاس)");

                GUI.HorizontalSlider(new Rect(x + 20, y + 105, w - 40, 30), _milkFill, 0f, 0.5f);
                GUI.Label(new Rect(x + 20, y + 140, w - 40, 45), ArabicFixer.Fix("اضغط باش تكب الحليب، طلق فاش يعمر الكاس!\n(Hold to pour milk, release when full!)"));
            }
        }
    }
}
