using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gerard.CherrypickGames
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private GameObject spawnerPrefab;

        public Cell[,] Cells { get; private set; }
        public float XOffset { get; private set; }
        public float YOffset { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Stack<Vector2Int> OrderedStack { get; private set; }

        private SpawnerController _spawnController;
        private Vector2Int _startingPosition;

        private void Start()
        {
            if (!LoadGridConfig()) return;

            GenerateGrid();
            GenerateClockWiseOrderedGridPositionsList();
            // StartCoroutine(ColorGridInSpiralOrderStack());
            SpawnSpawnerItem();
        }

        private bool LoadGridConfig()
        {
            var json = Resources.Load<TextAsset>("gridConfig");
            if (json == null)
            {
                Debug.LogError("gridConfig.json resource not found!");
                return false;
            }

            var gridData = JsonUtility.FromJson<GridData>(json.ToString());
            Width = gridData.Width;
            Height = gridData.Height;

            return true;
        }

        private void GenerateGrid()
        {
            Cells = new Cell[Width, Height];
            XOffset = Width * 0.5f - 0.5f;
            YOffset = Height * 0.5f - 0.5f;

            var centerCoordinates = GetCenterCoordinates();
            for (var y = Height - 1; y >= 0; y--) // Start from the top
            {
                for (var x = 0; x < Width; x++)
                {
                    var cellPosition = new Vector3(x, Height - 1 - y, 0); // Adjust y-coordinate
                    var cell = Instantiate(cellPrefab, cellPosition, Quaternion.identity, transform)
                        .GetComponent<Cell>();

                    // If we are NOT on the center cell, 25% chance the cell will be blocked
                    var isBlocked = (x != centerCoordinates.x || y != centerCoordinates.y) && Random.value < .25f;
                    cell.Initialize(new Vector2Int(x, y), isBlocked);

                    Cells[x, y] = cell;
                }
            }

            // Center the entire grid in Unity.
            transform.position = new Vector3(-XOffset, -YOffset, 0);
        }

        // Based on the Width and Height of a two dimensional grid, this method will generate
        // a list containing the grid coordinates ordered following an anti-clock wise pattern
        // Algorithm based on the following article:
        // https://javaconceptoftheday.com/how-to-create-spiral-of-numbers-matrix-in-java/
        private void GenerateClockWiseOrderedGridPositionsList()
        {
            OrderedStack = new Stack<Vector2Int>(Width * Height);
            var value = 1;
            var minCol = 0;
            var maxCol = Width - 1;
            var minRow = 0;
            var maxRow = Height - 1;

            while (value <= Width * Height)
            {
                for (var i = minRow; i <= maxRow; i++)
                {
                    OrderedStack.Push(new Vector2Int(minCol, i));
                    value++;
                }

                for (var i = minCol + 1; i <= maxCol; i++)
                {
                    OrderedStack.Push(new Vector2Int(i, maxRow));
                    value++;
                }

                for (var i = maxRow - 1; i >= minRow; i--)
                {
                    OrderedStack.Push(new Vector2Int(maxCol, i));
                    value++;
                }

                for (var i = maxCol - 1; i >= minCol + 1; i--)
                {
                    OrderedStack.Push(new Vector2Int(i, minRow));
                    value++;
                }

                minCol++;
                minRow++;
                maxCol--;
                maxRow--;
            }

            // Discard the first one since is where the spawner is
            OrderedStack.Pop();
        }

        private void SpawnSpawnerItem()
        {
            _startingPosition = GetCenterCoordinates();
            var worldPosition = GetWorldPositionFromCell(_startingPosition);
            var spawnedGo = Instantiate(spawnerPrefab, worldPosition, Quaternion.identity, transform);
            _spawnController = spawnedGo.GetComponent<SpawnerController>();

            // Inject dependencies to the SpawnController
            _spawnController.Initialize(this, _startingPosition);
        }

        private Vector2Int GetCenterCoordinates()
        {
            int centerX = Width / 2;
            int centerY = Height / 2;
            Vector2Int centerCoordinates;
            // If the width and height are both even, take the bottom-left cell of the four centers.
            if (Width % 2 == 0 && Height % 2 == 0)
            {
                centerCoordinates = new Vector2Int(centerX - 1, centerY - 1);
            }
            else
            {
                centerCoordinates = new Vector2Int(centerX, centerY);
            }

            return centerCoordinates;
        }

        # region Helpers

        public Cell GetCell(Vector2Int gridPos) => Cells[gridPos.x, gridPos.y];

        public Vector3 GetWorldPositionFromCell(Vector2Int gridPos)
        {
            return GetCell(gridPos).GetComponent<Transform>().position;
        }

        public bool IsCellWithinBounds(Vector2Int cellPos)
        {
            return cellPos.x >= 0 && cellPos.x < Width && cellPos.y >= 0 && cellPos.y < Height;
        }

        public bool IsCellEmpty(Vector2Int cellPos)
        {
            return GetCell(cellPos).IsCellEmpty;
        }

        public bool IsValidPosition(Vector2Int cellPos)
        {
            return IsCellWithinBounds(cellPos) && IsCellEmpty(cellPos) && !GetCell(cellPos).IsBlocked;
        }

        private IEnumerator ColorGridInSpiralOrderStack()
        {
            while (OrderedStack.Count > 0)
            {
                Vector2Int position = OrderedStack.Pop();
                if (IsValidPosition(position))
                {
                    var cell = GetCell(position);
                    cell.SetColor(Color.yellow);
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }

        #endregion
    }

    internal struct GridData
    {
        public int Width;
        public int Height;
    }
}