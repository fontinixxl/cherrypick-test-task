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
        private bool _isSpawningButtonPressed;

        private void OnEnable()
        {
            uiManager.SpawnButton.OnButtonStateChanged += StartStopSpawnerButtonHandler;
            uiManager.ClearingButton.OnButtonPressed += OnClearButtonPressedHandler;
        }

        private void OnDisable()
        {
            uiManager.SpawnButton.OnButtonStateChanged -= StartStopSpawnerButtonHandler;
            uiManager.ClearingButton.OnButtonPressed -= OnClearButtonPressedHandler;
        }

        private void Start()
        {
            if (!LoadGridConfig(out var gridData)) return;
            gridManager.Initialize(gridData.Width, gridData.Height);
            UpdateCameraZoomLimits(gridData);
            SpawnSpawner();
        }

        private void Update()
        {
            if (_isSpawningButtonPressed)
            {
                _spawner.Spawn();
                return;
            }

            // Spawner has movement priority whether the pointer is on the spawner or we are already dragging it
            if (_spawner.IsMouseOverOrDragging())
            {
                _spawner.HandleDrag();
            }
            else
            {
                cameraController.HandleMovement();
            }
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
            _spawner.Initialize(gridManager, cameraController.MainCamera, OnAllItemsSpawnHandler, OnSpawnerMovedHandler);
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
            return true;
        }

        #region Action Handlers

        private void OnSpawnerMovedHandler(Vector2Int newCoordinate)
        {
            gridManager.CalculateSpiralGridPath(newCoordinate);
        }

        private void OnAllItemsSpawnHandler()
        {
            _isSpawningButtonPressed = false;
        }

        private void StartStopSpawnerButtonHandler(bool state)
        {
            _isSpawningButtonPressed = state;
        }

        private void OnClearButtonPressedHandler()
        {
            gridManager.ClearNeighbouringColourCells();
        }

        #endregion
    }
}