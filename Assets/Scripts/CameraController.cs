using UnityEngine;

namespace DG
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float _minZ = -150f;
        [SerializeField] private float _maxZ = -8.5f;
        [SerializeField] private float _panVelocity = 10f;
        [SerializeField] private float _zoomVelocity = 1000f;
        [SerializeField] private float _screenEdgeBorder = 200f;

        private Transform _transform;
        private float _originalPanVelocity;
        private float _originalZoomVelocity;

        private void Awake()
        {
            _transform = transform;
            _originalPanVelocity = _panVelocity;
            _originalZoomVelocity = _zoomVelocity;
        }

        private void Update()
        {
            CheckSpeedBoost();
            Zoom();
            // Move camera with keyboard or with screen edge / mouse (can be additive for more speed)
            KeyboardMove(GetDirection());
            EdgeScreenMouseMove();
        }

        private void CheckSpeedBoost()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _panVelocity *= 2;
                _zoomVelocity *= 2;
            }

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                _panVelocity = _originalPanVelocity;
                _zoomVelocity = _originalZoomVelocity;
            }
        }

        private void Zoom()
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            _transform.Translate(Vector3.forward * (scroll * _zoomVelocity * Time.deltaTime));
            // Clamp Z
            var position = _transform.position;
            var clampedZ = Mathf.Clamp(position.z, _minZ, _maxZ);
            position.z = clampedZ;
            _transform.position = position;
        }

        private Vector2 GetDirection()
        {
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");
            // Clamp to prevent diagonal movement being faster than pure horizontal or vertical
            var direction = new Vector2(horizontal, vertical);
            return Vector2.ClampMagnitude(direction, 1f);
        }

        private void KeyboardMove(Vector3 direction)
        {
            _transform.Translate(direction * (_panVelocity * Time.deltaTime));
        }
        
        private void EdgeScreenMouseMove()
        {
            var mousePosition = Input.mousePosition;

            var leftRect = new Rect(0, 0, _screenEdgeBorder, Screen.height);
            var rightRect = new Rect(Screen.width - _screenEdgeBorder, 0, _screenEdgeBorder, Screen.height);
            var upRect = new Rect(0, Screen.height - _screenEdgeBorder, Screen.width, _screenEdgeBorder);
            var downRect = new Rect(0, 0, Screen.width, _screenEdgeBorder);

            var direction = new Vector2
            {
                x = leftRect.Contains(mousePosition) ? -1 : rightRect.Contains(mousePosition) ? 1 : 0,
                y = upRect.Contains(mousePosition) ? 1 : downRect.Contains(mousePosition) ? -1 : 0
            };

            _transform.Translate(direction * (_panVelocity * Time.deltaTime));
        }
    }
}