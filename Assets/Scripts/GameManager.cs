using Gerard.CherryPickGames.UI;
using UnityEngine;

namespace Gerard.CherrypickGames
{
    // Used to deserialize the json data
    internal struct GridData
    {
        public int Width;
        public int Height;
    }

    public class GameManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private UIManager uiManager;
        [SerializeField] private GridManager gridManager;
        [SerializeField] private CameraController cameraController;
        [Header("Data")]
        [SerializeField] private GameObject spawnerPrefab;

        private SpawnerController _spawner;
        private bool _isDraggingSpawner;
        private Coroutine _spawnCoroutine = null;

        private void Start()
        {
            if (!LoadGridConfig(out var gridData)) return;
        #if UNITY_ANDROID
            // Force width and height to max 25x25 for performance reasons
            gridData.Width = (int)Mathf.Clamp(gridData.Width, 2f, 25f);
            gridData.Height = (int)Mathf.Clamp(gridData.Height, 2f, 25f);
        #endif
            gridManager.Initialize(gridData.Width, gridData.Height);
            UpdateCameraZoomLimits(gridData);
            SpawnSpawner();
        }

        private void OnEnable()
        {
            uiManager.SpawnButton.OnButtonStateChanged += StartStopSpawnerButtonHandler;
            uiManager.ClearingButton.OnButtonStateChanged += OnClearButtonPressedHandler;
        }

        private void OnDisable()
        {
            uiManager.SpawnButton.OnButtonStateChanged -= StartStopSpawnerButtonHandler;
            uiManager.ClearingButton.OnButtonStateChanged -= OnClearButtonPressedHandler;
        }

        private void UpdateCameraZoomLimits(GridData gridData)
        {
            var gridWidth = gridData.Width * gridManager.CellSize;
            var gridHeight = gridData.Height * gridManager.CellSize;
            cameraController.UpdateZoomLimits(gridWidth, gridHeight);
        }

        private void SpawnSpawner()
        {
            var startingGridPosition = gridManager.GetCenterCoordinates();
            var worldPosition = gridManager.GetWorldPositionFromCell(startingGridPosition);
            _spawner = Instantiate(spawnerPrefab, worldPosition, Quaternion.identity, transform)
                .GetComponent<SpawnerController>();

            // Inject dependencies to the SpawnController
            _spawner.Initialize(gridManager, cameraController.MainCamera, OnSpawnerMovedHandler);
        }

        private bool LoadGridConfig(out GridData gridData)
        {
            gridData = default;
            var json = Resources.Load<TextAsset>("gridConfig");
            if (json == null)
            {
                Debug.LogError("gridConfig.json resource not found!");
                return false;
            }
            gridData = JsonUtility.FromJson<GridData>(json.ToString());

            if (gridData.Width != gridData.Height)
            {
                Debug.LogErrorFormat("Width and Height values must be equal");
                return false;
            }
            if (gridData.Width < 2 || gridData.Height < 2)
            {
                Debug.LogErrorFormat($"Minimum grid size must be 2x2, current values are [{gridData.Width}, {gridData.Height}]");
                return false;
            }

            return true;
        }

        #region Action Handlers

        private void OnSpawnerMovedHandler(Vector2Int newCoordinate)
        {
            gridManager.CalculateSpiralGridPath(newCoordinate);
        }

        private void StartStopSpawnerButtonHandler(bool isButtonPressed)
        {
            if (isButtonPressed)
            {
                _spawnCoroutine ??= StartCoroutine(_spawner.SpawnCoroutine());
            }
            else
            {
                if (_spawnCoroutine != null)
                {
                    StopCoroutine(_spawnCoroutine);
                    _spawnCoroutine = null;
                }
            }
        }

        private void OnClearButtonPressedHandler(bool state)
        {
            gridManager.ClearNeighbouringColourCells();
        }

        #endregion
    }
}