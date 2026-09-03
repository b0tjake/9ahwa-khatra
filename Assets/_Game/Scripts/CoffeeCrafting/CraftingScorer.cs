using UnityEngine;

namespace QahwaKhatra.CoffeeCrafting
{
    public struct CraftingResult
    {
        public float finalScore; // 0 to 100
        public int starRating;   // 1 to 3
        public float finalPayout;
        public string feedbackMessage;
    }

    public static class CraftingScorer
    {
        public static CraftingResult CalculateScore(
            float actualGrindGrams, float targetGrindGrams,
            float actualFill, float targetFill,
            int actualSugar, int targetSugar,
            float basePrice)
        {
            // 1. Grind score (target: 13g, tolerance ±1g = 100%)
            float grindDiff = Mathf.Abs(actualGrindGrams - targetGrindGrams);
            float grindScore = Mathf.Clamp01(1f - (grindDiff / 4f)) * 100f;

            // 2. Fill level score (target: e.g. 0.25, tolerance ±0.05 = 100%)
            float fillDiff = Mathf.Abs(actualFill - targetFill);
            float fillScore = Mathf.Clamp01(1f - (fillDiff / 0.25f)) * 100f;

            // 3. Sugar accuracy (exact match = 100%, off by 1 = 50%, off by 2+ = 0%)
            int sugarDiff = Mathf.Abs(actualSugar - targetSugar);
            float sugarScore = sugarDiff == 0 ? 100f : (sugarDiff == 1 ? 50f : 0f);

            // Weighted total: Fill (40%) + Grind (35%) + Sugar (25%)
            float finalScore = (fillScore * 0.4f) + (grindScore * 0.35f) + (sugarScore * 0.25f);

            int stars = 1;
            string feedback = "عادية (Average)";
            float payout = basePrice;

            if (finalScore >= 90f)
            {
                stars = 3;
                feedback = "خاترة بزاف! ⭐⭐⭐ (Perfect!)";
                payout = basePrice * 1.35f; // 35% tip
            }
            else if (finalScore >= 70f)
            {
                stars = 2;
                feedback = "مزيانة ⭐⭐ (Good!)";
                payout = basePrice * 1.15f; // 15% tip
            }
            else if (finalScore < 50f)
            {
                feedback = "هادي ماشي قهوة! 😡 (Terrible)";
                payout = basePrice * 0.6f;
            }

            return new CraftingResult
            {
                finalScore = finalScore,
                starRating = stars,
                finalPayout = Mathf.Round(payout),
                feedbackMessage = feedback
            };
        }
    }
}
