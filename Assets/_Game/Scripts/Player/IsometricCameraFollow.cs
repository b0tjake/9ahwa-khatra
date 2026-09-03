using UnityEngine;

namespace QahwaKhatra.Player
{
    public class IsometricCameraFollow : MonoBehaviour
    {
        [Header("Target & Offsets")]
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 10f, -8f);
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private float _cameraPitch = 50f;

        private void Start()
        {
            transform.rotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desiredPosition = _target.position + _offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed * Time.deltaTime);
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }
    }
}
