using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace QahwaKhatra.Player
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 12f;

        [Header("Camera Reference")]
        [SerializeField] private Camera _mainCamera;

        private NavMeshAgent _agent;
        private Vector2 _moveInput;
        private Vector3 _targetDirection;

        public bool IsMoving => _agent != null && _agent.velocity.sqrMagnitude > 0.1f;
        public float MovementSpeedPercent => _agent != null ? _agent.velocity.magnitude / _agent.speed : 0f;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = _moveSpeed;
            _agent.updateRotation = false; // Custom smooth rotation

            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
        }

        private void Update()
        {
            ReadMovementInput();
            HandleMovement();
            HandleRotation();
        }

        private void ReadMovementInput()
        {
            // Keyboard (WASD / Arrows) & Gamepad
            float horizontal = 0f;
            float vertical = 0f;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
            }

            _moveInput = new Vector2(horizontal, vertical).normalized;
        }

        public void SetJoystickInput(Vector2 input)
        {
            if (input.sqrMagnitude > 0.01f)
            {
                _moveInput = Vector2.ClampMagnitude(input, 1f);
            }
        }

        private void HandleMovement()
        {
            if (_moveInput.sqrMagnitude > 0.01f)
            {
                // Calculate camera-relative movement direction
                Vector3 forward = _mainCamera != null ? _mainCamera.transform.forward : Vector3.forward;
                Vector3 right = _mainCamera != null ? _mainCamera.transform.right : Vector3.right;

                forward.y = 0f;
                right.y = 0f;
                forward.Normalize();
                right.Normalize();

                _targetDirection = (forward * _moveInput.y + right * _moveInput.x).normalized;

                Vector3 targetPosition = transform.position + _targetDirection * _moveSpeed * Time.deltaTime;
                _agent.Move(_targetDirection * _moveSpeed * Time.deltaTime);
            }
        }

        private void HandleRotation()
        {
            if (_targetDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(_targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _rotationSpeed * Time.deltaTime);
            }
        }

        public void UpgradeSpeed(float amount)
        {
            _moveSpeed += amount;
            if (_agent != null) _agent.speed = _moveSpeed;
        }
    }
}
