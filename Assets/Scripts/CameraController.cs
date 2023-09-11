using System;
using Gerard.CherryPickGames.Input;
using Gerard.CherryPickGames.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Gerard.CherrypickGames
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private UIManager uiManager;
        [FormerlySerializedAs("panSpeed")]
        [Header("Camera Movement")]
        [SerializeField] private float minPanSpeed = 2.5f;
        [SerializeField] private float maxPanSpeed = 8f;
        [SerializeField] private float smoothFactor = 0.1f;
        [Header("Zoom")]
        [SerializeField] private float padding;

        private float _minZoom = 5f;
        private float _maxZoom = 20f;

        private Slider _zoomSlider;
        private Vector3 _targetPosition;
        private float _initialZValue;
        private bool _shouldMoveCamera;

        private void Awake()
        {
            MainCamera = GetComponent<Camera>();
            _initialZValue = transform.position.z;
        }

        private void OnEnable()
        {
            InputManager.Instance.CameraMoveEvent += MoveCamera;
        }

        private void OnDisable()
        {
            InputManager.Instance.CameraMoveEvent -= MoveCamera;
        }

        private void Start()
        {
            _zoomSlider = uiManager.ZoomSlider;
            _zoomSlider.minValue = _minZoom;
            _zoomSlider.maxValue = _maxZoom;
            _zoomSlider.value = MainCamera.orthographicSize;
            _zoomSlider.onValueChanged.AddListener(AdjustZoom);
        }

        private void MoveCamera(Vector2 movement, bool isTouchDevice)
        {
            if (uiManager.IsUIBeingInteracted) return;

            if(isTouchDevice)
            {
                movement *= 0.1f;
            }

            var speedFactor = Mathf.InverseLerp(_minZoom, _maxZoom, MainCamera.orthographicSize);
            var adjustedSpeed = Mathf.Lerp(minPanSpeed, maxPanSpeed, speedFactor);

            var delta = new Vector3(movement.x, movement.y, 0);
            _targetPosition += new Vector3(-delta.x * adjustedSpeed, -delta.y * adjustedSpeed, 0);
            _targetPosition.z = _initialZValue;
            _shouldMoveCamera = true;
        }

        private void Update()
        {
            if(!_shouldMoveCamera) return;

            // Smooth camera movement
            transform.position = Vector3.Lerp(transform.position, _targetPosition, smoothFactor);
            // Check if the camera is close enough to the target position
            if ((transform.position - _targetPosition).sqrMagnitude < 0.001f)
            {
                transform.position = _targetPosition; // Ensure exact position
                _shouldMoveCamera = false;
            }
        }

        private void AdjustZoom(float zoomValue)
        {
            MainCamera.orthographicSize = zoomValue;
        }

        public void UpdateZoomLimits(float gridWidth, float gridHeight)
        {
            var desiredZoomForWidth = gridWidth / (2f * MainCamera.aspect);
            var desiredZoomForHeight = gridHeight / 2f;

            // Set the max zoom to whichever dimension is larger
            _maxZoom = Mathf.Max(desiredZoomForWidth, desiredZoomForHeight) * padding;

            // Optionally, set a minimum zoom - here, I'm setting it to show at least 4 rows or columns
            _minZoom = Mathf.Min(gridWidth / 4f / MainCamera.aspect, gridHeight / 4f);

            _zoomSlider.minValue = _minZoom;
            _zoomSlider.maxValue = _maxZoom;
            _zoomSlider.value = _maxZoom;
            AdjustZoom(_maxZoom);
        }

        public Camera MainCamera { get; private set; }
    }
}