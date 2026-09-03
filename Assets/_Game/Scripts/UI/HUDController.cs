using UnityEngine;
using TMPro;
using QahwaKhatra.Utils;

namespace QahwaKhatra.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("UI Text References")]
        [SerializeField] private TextMeshProUGUI _currencyText;
        [SerializeField] private TextMeshProUGUI _dayText;
        [SerializeField] private TextMeshProUGUI _interactionPromptText;
        [SerializeField] private GameObject _interactionButton;

        private void OnEnable()
        {
            EventBus.OnCurrencyChanged += UpdateCurrencyDisplay;
            EventBus.OnDayStarted += UpdateDayDisplay;
        }

        private void OnDisable()
        {
            EventBus.OnCurrencyChanged -= UpdateCurrencyDisplay;
            EventBus.OnDayStarted -= UpdateDayDisplay;
        }

        private void Start()
        {
            if (_interactionButton != null) _interactionButton.SetActive(false);
        }

        private void UpdateCurrencyDisplay(float newAmount)
        {
            if (_currencyText != null)
            {
                _currencyText.text = $"{newAmount:F0} DH";
            }
        }

        private void UpdateDayDisplay(int dayNumber)
        {
            if (_dayText != null)
            {
                _dayText.text = $"Day {dayNumber}";
            }
        }

        public void ShowInteractionPrompt(string message)
        {
            if (_interactionPromptText != null)
            {
                _interactionPromptText.text = message;
            }
            if (_interactionButton != null)
            {
                _interactionButton.SetActive(true);
            }
        }

        public void HideInteractionPrompt()
        {
            if (_interactionButton != null)
            {
                _interactionButton.SetActive(false);
            }
        }
    }
}
