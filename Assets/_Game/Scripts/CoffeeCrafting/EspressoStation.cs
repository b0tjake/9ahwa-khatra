using UnityEngine;
using UnityEngine.InputSystem;
using QahwaKhatra.Player;
using QahwaKhatra.Data;
using QahwaKhatra.Core;
using QahwaKhatra.Utils;

namespace QahwaKhatra.CoffeeCrafting
{
    public class EspressoStation : MonoBehaviour, IInteractable
    {
        [Header("Drink Configuration")]
        [SerializeField] private DrinkRecipeSO _recipe;

        [Header("Crafting Runtime State")]
        [SerializeField] private bool _isCrafting = false;
        [SerializeField] private float _currentGrindGrams = 0f;
        [SerializeField] private float _currentFill = 0f;
        [SerializeField] private int _currentSugar = 0;
        [SerializeField] private int _craftingStep = 0; // 0: Grind, 1: Pour, 2: Sugar, 3: Done

        public string PromptMessage => ArabicFixer.Fix("صاوب قهوة (Make Coffee)");
        public bool IsCrafting => _isCrafting;

        private CraftingResult _lastResult;

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
            _currentGrindGrams = 0f;
            _currentFill = 0f;
            _currentSugar = 0;
        }

        private void Update()
        {
            if (!_isCrafting) return;

            // Step 0: Grind Beans (Hold Space / Click / Touch to grind)
            if (_craftingStep == 0)
            {
                bool isHolding = false;
                if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed) isHolding = true;
                if (Mouse.current != null && Mouse.current.leftButton.isPressed) isHolding = true;
                if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) isHolding = true;

                if (isHolding)
                {
                    _currentGrindGrams += 8f * Time.deltaTime;
                    if (_currentGrindGrams > 20f) _currentGrindGrams = 20f;
                }

                if (!isHolding && _currentGrindGrams > 2f)
                {
                    _craftingStep = 1;
                }
            }
            // Step 1: Pull Espresso Shot (Hold to fill cup)
            else if (_craftingStep == 1)
            {
                bool isHolding = false;
                if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed) isHolding = true;
                if (Mouse.current != null && Mouse.current.leftButton.isPressed) isHolding = true;
                if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) isHolding = true;

                if (isHolding)
                {
                    _currentFill += 0.35f * Time.deltaTime;
                    if (_currentFill > 1f) _currentFill = 1f;
                }

                if (!isHolding && _currentFill > 0.05f)
                {
                    _craftingStep = 2;
                }
            }
            // Step 2: Sugar
            else if (_craftingStep == 2)
            {
                if (Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame)
                {
                    _currentSugar = (_currentSugar + 1) % 4;
                }

                if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    FinishCrafting();
                }
            }
        }

        private void FinishCrafting()
        {
            _craftingStep = 3;
            _isCrafting = false;

            float targetGrind = _recipe != null ? _recipe.targetGrindGrams : 13f;
            float targetFill = _recipe != null ? _recipe.targetFillPercent : 0.25f;
            int targetSugar = _recipe != null ? _recipe.targetSugarCubes : 1;
            float basePrice = _recipe != null ? _recipe.basePrice : 10f;

            _lastResult = CraftingScorer.CalculateScore(
                _currentGrindGrams, targetGrind,
                _currentFill, targetFill,
                _currentSugar, targetSugar,
                basePrice);

            Debug.Log($"[EspressoStation] Crafted! Score: {_lastResult.finalScore:F0}%. Result: {_lastResult.feedbackMessage}. Earned: {_lastResult.finalPayout} DH");

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddDirhams(_lastResult.finalPayout, "Coffee Served");
            }
        }

        private void OnGUI()
        {
            if (!_isCrafting) return;

            float w = 380f;
            float h = 260f;
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - h) / 2f;

            GUI.Box(new Rect(x, y, w, h), ArabicFixer.Fix("☕ قهوة خاترة — تحضير القهوة"));

            if (_craftingStep == 0)
            {
                GUI.Label(new Rect(x + 20, y + 40, w - 40, 30), ArabicFixer.Fix("1. طحن الحبوب (Grind to 13g):"));
                GUI.Label(new Rect(x + 20, y + 70, w - 40, 30), ArabicFixer.Fix($"الوزن: {_currentGrindGrams:F1} g / 13.0 g"));
                GUI.HorizontalSlider(new Rect(x + 20, y + 105, w - 40, 30), _currentGrindGrams, 0f, 20f);
                GUI.Label(new Rect(x + 20, y + 135, w - 40, 40), ArabicFixer.Fix("اضغط وماتطلقش باش تطحن!\n(Hold SPACE / Click to Grind, Release at 13g)"));
            }
            else if (_craftingStep == 1)
            {
                GUI.Label(new Rect(x + 20, y + 40, w - 40, 30), ArabicFixer.Fix("2. هبط القهوة (Target: 1/4 كاس):"));
                GUI.Label(new Rect(x + 20, y + 70, w - 40, 30), ArabicFixer.Fix($"الكاس: {(_currentFill * 100f):F0}% مليان (الهدف: 25%)"));
                GUI.HorizontalSlider(new Rect(x + 20, y + 105, w - 40, 30), _currentFill, 0f, 1f);
                GUI.Label(new Rect(x + 20, y + 135, w - 40, 40), ArabicFixer.Fix("اضغط وماتطلقش باش تعمر الكاس!\n(Hold SPACE / Click to Pour, Release at 1/4)"));
            }
            else if (_craftingStep == 2)
            {
                GUI.Label(new Rect(x + 20, y + 40, w - 40, 30), ArabicFixer.Fix("3. السكر (Sugar):"));
                GUI.Label(new Rect(x + 20, y + 70, w - 40, 30), ArabicFixer.Fix($"السكر الحالي: {_currentSugar} سكرات (الهدف: 1)"));

                if (GUI.Button(new Rect(x + 20, y + 110, 150, 40), ArabicFixer.Fix("+ سكرة (Tap S)")))
                {
                    _currentSugar = (_currentSugar + 1) % 4;
                }

                if (GUI.Button(new Rect(x + 190, y + 110, 160, 40), ArabicFixer.Fix("سالي وقدم (Serve!)")))
                {
                    FinishCrafting();
                }
            }
        }
    }
}
