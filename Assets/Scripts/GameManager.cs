using Gerard.CherryPickGames.UI;
using UnityEngine;

namespace Gerard.CherrypickGames
{
    public class GameManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private UIManager uiManager;
        [SerializeField] private GridManager gridManager;
        [SerializeField] private CameraController cameraController;
        [Header("Data")]
        [SerializeField] private TextAsset gridConfigFile;
        [SerializeField] private SpawnerController spawnerPrefab;

        private SpawnerController _spawner;
        private bool _isDraggingSpawner;
        private Coroutine _spawnCoroutine;

        private void OnEnable()
        {
            uiManager.SpawnButton.OnButtonStateChanged += StartStopSpawnerButtonHandler;
            uiManager.ClearingButton.OnButtonStateChanged += OnClearButtonPressed;
        }

        private void OnDisable()
        {
            uiManager.SpawnButton.OnButtonStateChanged -= StartStopSpawnerButtonHandler;
            uiManager.ClearingButton.OnButtonStateChanged -= OnClearButtonPressed;
        }

        private void Start()
        {
            if (!LoadGridConfig(out var gridData)) return;
            gridManager.Initialize(gridData.Width, gridData.Height);
            UpdateCameraZoomLimits(gridData);
            SpawnSpawner();
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
            _spawner = Instantiate(spawnerPrefab, worldPosition, Quaternion.identity, transform);
            _spawner.Initialize(gridManager, cameraController.MainCamera, OnSpawnerChangedPosition);
        }

        private bool LoadGridConfig(out GridData gridData)
        {
            var logMsg = $"Loading JSON config: ";
            gridData = default;
            if (gridConfigFile == null)
            {
                Debug.LogError($"{logMsg} gridConfig.json resource not found!");
                return false;
            }
            gridData = JsonUtility.FromJson<GridData>(gridConfigFile.ToString());

            if (gridData.Width != gridData.Height)
            {
                Debug.LogErrorFormat($"{logMsg} Width and Height values must be equal");
                return false;
            }
            if (gridData.Width < 2 || gridData.Height < 2)
            {
                Debug.LogErrorFormat($"{logMsg} Minimum grid size must be 2x2, current values are [{gridData.Width}, {gridData.Height}]");
                return false;
            }

#if UNITY_ANDROID
            // Force width and height to max 250x250 for performance reasons
            gridData.Width = (int)Mathf.Clamp(gridData.Width, 2f, 250f);
            gridData.Height = (int)Mathf.Clamp(gridData.Height, 2f, 250f);
#endif
            return true;
        }

        #region Action Handlers

        private void OnSpawnerChangedPosition(Vector2Int newCoordinate)
        {
            gridManager.GenerateSpawningCoordinates(newCoordinate.x, newCoordinate.y);
        }

        private void OnClearButtonPressed(bool _)
        {
            gridManager.ClearNeighbouringColourCells();
            gridManager.GenerateSpawningCoordinates(_spawner.CurrentGridPosition.x, _spawner.CurrentGridPosition.y);
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

        #endregion
    }

    // Used to deserialize json data
    public struct GridData
    {
        public int Width;
        public int Height;
    }
}