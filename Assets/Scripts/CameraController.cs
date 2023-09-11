using Gerard.CherryPickGames.Input;
using Gerard.CherryPickGames.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gerard.CherrypickGames
{
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(CameraPan))]
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private UIManager uiManager;
        [SerializeField] private float padding;

        private CameraPan _cameraPan;
        private Slider _zoomSlider;

        private readonly float _minZoom = 5f;
        private readonly float _maxZoom = 20f;

        private void Awake()
        {
            MainCamera = GetComponent<Camera>();
            _cameraPan = GetComponent<CameraPan>();
        }

        private void OnEnable()
        {
            InputManager.Instance.CameraMoveEvent += HandleMovement;
        }

        private void OnDisable()
        {
            InputManager.Instance.CameraMoveEvent -= HandleMovement;
        }

        private void Start()
        {
            _zoomSlider = uiManager.ZoomSlider;
            _zoomSlider.minValue = _minZoom;
            _zoomSlider.maxValue = _maxZoom;
            _zoomSlider.value = MainCamera.orthographicSize;
            _zoomSlider.onValueChanged.AddListener(AdjustZoom);
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
            var maxZoom = Mathf.Max(desiredZoomForWidth, desiredZoomForHeight) * padding;

            // Optionally, set a minimum zoom - here, I'm setting it to show at least 4 rows or columns
            var minZoom = Mathf.Min(gridWidth / 4f / MainCamera.aspect, gridHeight / 4f);

            _zoomSlider.minValue = minZoom;
            _zoomSlider.maxValue = maxZoom;
            _zoomSlider.value = maxZoom;
            AdjustZoom(maxZoom);
        }

        private void HandleMovement(Vector2 movement, bool isTouchScreen)
        {
            if (uiManager.IsUIBeingInteracted) return;
            _cameraPan.HandleCameraMovement(movement, isTouchScreen);
        }

        public Camera MainCamera { get; private set; }
    }
}