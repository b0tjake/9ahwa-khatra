using UnityEngine;
using UnityEngine.EventSystems;

namespace QahwaKhatra.Player
{
    public class FloatingJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("References")]
        [SerializeField] private RectTransform _background;
        [SerializeField] private RectTransform _handle;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Settings")]
        [SerializeField] private float _handleRange = 60f;
        [SerializeField] private PlayerController _playerController;

        private Vector2 _inputVector = Vector2.zero;
        private Vector2 _originalPosition;
        private bool _isDragging = false;

        private void Start()
        {
            if (_background != null) _originalPosition = _background.anchoredPosition;
            SetAlpha(0.3f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isDragging = true;
            SetAlpha(0.9f);

            if (_background != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    transform.parent as RectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPos);

                _background.anchoredPosition = localPos;
            }

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_background == null || _handle == null) return;

            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _background,
                eventData.position,
                eventData.pressEventCamera,
                out position);

            position = Vector2.ClampMagnitude(position, _handleRange);
            _handle.anchoredPosition = position;

            _inputVector = position / _handleRange;
            if (_playerController != null)
            {
                _playerController.SetJoystickInput(_inputVector);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isDragging = false;
            _inputVector = Vector2.zero;
            if (_handle != null) _handle.anchoredPosition = Vector2.zero;
            if (_background != null) _background.anchoredPosition = _originalPosition;
            SetAlpha(0.3f);

            if (_playerController != null)
            {
                _playerController.SetJoystickInput(Vector2.zero);
            }
        }

        private void SetAlpha(float alpha)
        {
            if (_canvasGroup != null) _canvasGroup.alpha = alpha;
        }
    }
}
