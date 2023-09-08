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

        private SpawnerController _spawnController;
        private Vector2Int _startingPosition;

        private void Start()
        {
            if (!LoadGridConfig()) return;

            GenerateGrid();
            SpawnSpawnerItem();
            var spiralPosOrder = GenerateAntiClockWiseOrderedGridPositionsList();
            StartCoroutine(ColorGridInSpiralOrderClockWise(spiralPosOrder));
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
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    var cellPosition = new Vector3(j - XOffset, i - YOffset, 0);
                    var cell = Instantiate(cellPrefab, cellPosition, Quaternion.identity, transform)
                        .GetComponent<Cell>();

                    // If we are NOT on the center cell, 25% chance the cell will be blocked
                    var isBlocked = (j != centerCoordinates.x || i != centerCoordinates.y) && Random.value < .25f;
                    cell.Initialize(new Vector2Int(j, i), isBlocked);

                    Cells[j, i] = cell;
                }
            }
        }

        // Based on the Width and Height of a two dimensional grid, this method will generate
        // a list containing the grid coordinates ordered following an anti-clock wise pattern
        // Algorithm based on the following article:
        // https://javaconceptoftheday.com/how-to-create-spiral-of-numbers-matrix-in-java/
        private List<Vector2Int> GenerateAntiClockWiseOrderedGridPositionsList()
        {
            var positions = new List<Vector2Int>();

            var value = 1;
            var minCol = 0;
            var maxCol = Width - 1;
            var minRow = 0;
            var maxRow = Height - 1;

            while (value <= Width * Height)
            {
                for (var i = minRow; i <= maxRow; i++)
                {
                    positions.Add(new Vector2Int(minCol, i));
                    value++;
                }

                for (var i = minCol + 1; i <= maxCol; i++)
                {
                    positions.Add(new Vector2Int(i, maxRow));
                    value++;
                }

                for (var i = maxRow - 1; i >= minRow; i--)
                {
                    positions.Add(new Vector2Int(maxCol, i));
                    value++;
                }

                for (var i = maxCol - 1; i >= minCol + 1; i--)
                {
                    positions.Add(new Vector2Int(i, minRow));
                    value++;
                }

                minCol++;
                minRow++;
                maxCol--;
                maxRow--;
            }

            return positions;
        }

        private void SpawnSpawnerItem()
        {
            _startingPosition = GetCenterCoordinates();
            var worldPosition = GetWorldPositionFromCell(_startingPosition);
            var spawnedGo = Instantiate(spawnerPrefab, worldPosition, Quaternion.identity);
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

        public Cell GetCell(Vector2Int gridPos) => Cells[gridPos.x, Height - 1 - gridPos.y];

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

        #endregion

        private IEnumerator ColorGridInSpiralOrderClockWise(List<Vector2Int> antiClockWiseOrderedPositions)
        {
            for (var i = antiClockWiseOrderedPositions.Count - 1; i >= 0; i--)
            {
                var cellPosition = antiClockWiseOrderedPositions[i];
                if (IsValidPosition(cellPosition))
                {
                    var cell = GetCell(cellPosition);
                    cell.SetColor(Color.yellow);
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
    }

    internal struct GridData
    {
        public int Width;
        public int Height;
    }
}