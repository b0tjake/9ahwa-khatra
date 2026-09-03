using UnityEngine;
using UnityEngine.InputSystem;
using QahwaKhatra.Player;
using QahwaKhatra.Core;
using QahwaKhatra.Utils;

namespace QahwaKhatra.CoffeeCrafting
{
    public class TeaPourStation : MonoBehaviour, IInteractable
    {
        [Header("Moroccan Mint Tea State")]
        [SerializeField] private bool _isCrafting = false;
        [SerializeField] private float _pourHeight = 0.2f; // Height of the teapot (berrad)
        [SerializeField] private float _foamCrown = 0f; // Foam crown percentage (رڭة)
        [SerializeField] private float _teaVolume = 0f; // Fill level of glass

        public string PromptMessage => ArabicFixer.Fix("عمّر أتاي بنعناع (Pour Mint Tea)");
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
            _pourHeight = 0.2f;
            _foamCrown = 0f;
            _teaVolume = 0f;
        }

        private void Update()
        {
            if (!_isCrafting) return;

            bool isHolding = false;
            if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed) isHolding = true;
            if (Mouse.current != null && Mouse.current.leftButton.isPressed) isHolding = true;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) isHolding = true;

            if (isHolding)
            {
                // Raise the teapot higher (swipe / hold up)
                _pourHeight += 0.35f * Time.deltaTime;
                if (_pourHeight > 1.2f) _pourHeight = 1.2f;

                // Higher pour generates thick foam crown (رڭة)!
                if (_pourHeight > 0.6f)
                {
                    _foamCrown += 0.45f * Time.deltaTime;
                    if (_foamCrown > 1f) _foamCrown = 1f;
                }

                _teaVolume += 0.3f * Time.deltaTime;
                if (_teaVolume > 1f) _teaVolume = 1f;
            }

            // Release when full
            if (!isHolding && _teaVolume > 0.25f)
            {
                FinishCrafting();
            }
        }

        private void FinishCrafting()
        {
            _isCrafting = false;

            // Score based on foam crown (رڭة) and fill volume
            float score = (_foamCrown * 0.6f + _teaVolume * 0.4f) * 100f;
            float payout = 10f; // Base tea price

            string feedback = "أتاي عادي (Average Tea)";
            if (score >= 80f)
            {
                feedback = "أتاي مشحر بالرڭة! ⭐⭐⭐ (Authentic Foamy Moroccan Tea!)";
                payout = 16f; // full price + tip
            }

            Debug.Log($"[TeaPourStation] Brewed Mint Tea! Score: {score:F0}%. Result: {feedback}. Payout: {payout} DH");

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddDirhams(payout, "Served Mint Tea (أتاي بنعناع)");
            }
        }

        private void OnGUI()
        {
            if (!_isCrafting) return;

            float w = 400f;
            float h = 260f;
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - h) / 2f;

            GUI.Box(new Rect(x, y, w, h), ArabicFixer.Fix("🫖 براد أتاي بنعناع (Moroccan Mint Tea)"));

            GUI.Label(new Rect(x + 20, y + 40, w - 40, 30), ArabicFixer.Fix("هز البراد الفوق باش تطلع الرڭة (High Pour):"));

            GUI.Label(new Rect(x + 20, y + 70, w - 40, 25), $"علو البراد: {_pourHeight:F1} m (الهدف: فوق 0.7 متر)");
            GUI.HorizontalSlider(new Rect(x + 20, y + 95, w - 40, 25), _pourHeight, 0.2f, 1.2f);

            GUI.Label(new Rect(x + 20, y + 125, w - 40, 25), ArabicFixer.Fix($"الرڭة (Foam Crown): {(_foamCrown * 100f):F0}% | الكاس: {(_teaVolume * 100f):F0}%"));
            GUI.HorizontalSlider(new Rect(x + 20, y + 150, w - 40, 25), _foamCrown, 0f, 1f);

            GUI.Label(new Rect(x + 20, y + 185, w - 40, 45), ArabicFixer.Fix("اضغط باش تخوي أتاي من الفوق، وطلق فاش يعمر الكاس بالرڭة!"));
        }
    }
}
