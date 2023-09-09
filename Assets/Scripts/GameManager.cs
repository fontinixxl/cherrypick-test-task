using UnityEngine;

namespace Gerard.CherrypickGames
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private CameraPan cameraPan;
        [SerializeField] private GridController gridController;
        [SerializeField] private GameObject spawnerPrefab;

        private SpawnerController _spawnerController;
        private bool _isSpawning;

        private void Start()
        {
            gridController.Initialize();
            SpawnSpawner();
        }

        private void Update()
        {
            if (!_isSpawning && Input.GetKeyDown(KeyCode.Backspace))
            {
                gridController.ClearNeighbouringColourCells();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _isSpawning = !_isSpawning;
            }

            if (_isSpawning)
            {
                _spawnerController.SpawnItems();
                // _isSpawning = false; // Debug: Spawn one each time space is pressed
                return;
            }

            // If we are hovering over the spawner, block camera panning
            if (IsMouseOverSpawner())
            {
                _spawnerController.HandleDrag(gridController.CalculateSpiralGridPath);
            }
            else
            {
                cameraPan.HandleCameraMovement();
            }
        }

        private void SpawnSpawner()
        {
            var startingGridPosition = gridController.GetCenterCoordinates();
            var worldPosition = gridController.GetWorldPositionFromCell(startingGridPosition);
            _spawnerController = Instantiate(spawnerPrefab, worldPosition, Quaternion.identity, transform)
                .GetComponent<SpawnerController>();

            // Inject dependencies to the SpawnController
            _spawnerController.Initialize(gridController, OnSpawnerSpawningCompleted);
        }

        private void OnSpawnerSpawningCompleted()
        {
            _isSpawning = false;
        }

        private bool IsMouseOverSpawner()
        {
            var mousePosition = cameraPan.MainCamera.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(mousePosition, Vector2.zero);
            return hit.collider != null ? hit.collider.GetComponent<SpawnerController>() : false;
        }
    }
}