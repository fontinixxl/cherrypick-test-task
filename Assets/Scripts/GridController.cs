using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gerard.CherrypickGames
{
    public class GridController : MonoBehaviour
    {
        // Used to deserialize the json data
        private struct GridData
        {
            public int Width;
            public int Height;
        }

        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private List<Color> possibleColors = new();

        private Vector2Int _startingGridPosition;
        private bool[,] _visitedCells;

        public void Initialize()
        {
            if (!LoadGridConfig()) return;
            OrderedStack = new Stack<Vector2Int>(Width * Height);
            _visitedCells = new bool[Width, Height];
            GenerateGrid();
            CalculateSpiralGridPath(_startingGridPosition);
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
        // a Stack containing the grid coordinates in a clock wise pattern.
        // Algorithm based on the following article:
        // https://javaconceptoftheday.com/how-to-create-spiral-of-numbers-matrix-in-java/
        public void CalculateSpiralGridPath(Vector2Int spawnerGridPosition)
        {
            OrderedStack.Clear();

            var value = 1;
            var minCol = 0;
            var maxCol = Width - 1;
            var minRow = 0;
            var maxRow = Height - 1;

            while (value <= Width * Height)
            {
                for (var i = minRow; i <= maxRow; i++)
                {
                    var gridPos = new Vector2Int(minCol, i);
                    OrderedStack.Push(gridPos);
                    value++;
                }

                for (var i = minCol + 1; i <= maxCol; i++)
                {
                    var gridPos = new Vector2Int(i, maxRow);
                    OrderedStack.Push(gridPos);
                    value++;
                }

                for (var i = maxRow - 1; i >= minRow; i--)
                {
                    var gridPos = new Vector2Int(maxCol, i);
                    OrderedStack.Push(gridPos);
                    value++;
                }

                for (var i = maxCol - 1; i >= minCol + 1; i--)
                {
                    var gridPos = new Vector2Int(i, minRow);
                    OrderedStack.Push(gridPos);
                    value++;
                }

                minCol++;
                minRow++;
                maxCol--;
                maxRow--;
            }

            // Discard the first one since it's where the spawner is
            OrderedStack.Pop();
        }

        # region Clear_Neighbours

        public void ClearNeighbouringColourCells()
        {
            Array.Clear(_visitedCells, 0, _visitedCells.Length);
            ClearNeighbourCellsSameColor();
            CalculateSpiralGridPath(_startingGridPosition);
        }

        private void ClearNeighbourCellsSameColor()
        {
            foreach (var color in PossibleColors)
            {
                for (var x = 0; x < Width; x++)
                {
                    for (var y = 0; y < Height; y++)
                    {
                        var cell = GetCell(new Vector2Int(x, y));
                        if (!_visitedCells[x, y] && !cell.IsEmpty && cell.ItemColor == color)
                        {
                            List<Cell> sameColoredCells = new List<Cell>();
                            DFS(cell.GridPosition, color, sameColoredCells);

                            // Clear if 2 or more
                            if (sameColoredCells.Count >= 2)
                            {
                                foreach (var sameColorCell in sameColoredCells)
                                {
                                    sameColorCell.ClearCell();
                                }
                            }
                        }
                    }
                }
            }
        }

        // Depth-First Search Algorithm: https://en.wikipedia.org/wiki/Depth-first_search
        private void DFS(Vector2Int position, Color targetColor, List<Cell> sameColoredCells)
        {
            // Base conditions for out-of-bounds
            if (position.x < 0 || position.x >= Width || position.y < 0 || position.y >= Height)
                return;

            var cell = GetCell(position);

            // Check if cell is null, already visited, or color mismatch
            if (cell == null || cell.IsEmpty || _visitedCells[position.x, position.y] || cell.ItemColor != targetColor)
                return;

            // Mark as visited
            _visitedCells[position.x, position.y] = true;

            // Add to the same colored cells list
            sameColoredCells.Add(cell);

            // Go in all directions: up, down, left, right
            DFS(new Vector2Int(position.x + 1, position.y), targetColor, sameColoredCells);
            DFS(new Vector2Int(position.x - 1, position.y), targetColor, sameColoredCells);
            DFS(new Vector2Int(position.x, position.y + 1), targetColor, sameColoredCells);
            DFS(new Vector2Int(position.x, position.y - 1), targetColor, sameColoredCells);
        }

        #endregion

        # region Helpers

        public Vector2Int GetCenterCoordinates()
        {
            int centerX = Width / 2;
            int centerY = Height / 2;
            Vector2Int centerCoordinates;
            // If the width and height are both even, take the bottom-left cell of the four centers.
            if (Width % 2 == 0 && Height % 2 == 0)
            {
                centerCoordinates = new Vector2Int(centerX, centerY - 1);
            }
            else
            {
                centerCoordinates = new Vector2Int(centerX, centerY);
            }

            return centerCoordinates;
        }

        public Cell GetCell(Vector2Int gridPos) => Cells[gridPos.x, gridPos.y];

        public bool IsValidPosition(Vector2Int cellPos) =>
            IsCellWithinBounds(cellPos) && IsCellEmpty(cellPos) && !GetCell(cellPos).IsBlocked;

        public Vector3 GetWorldPositionFromCell(Vector2Int gridPos) =>
            GetCell(gridPos).GetComponent<Transform>().position;

        private bool IsCellWithinBounds(Vector2Int cellPos) =>
            cellPos.x >= 0 && cellPos.x < Width && cellPos.y >= 0 && cellPos.y < Height;

        private bool IsCellEmpty(Vector2Int cellPos) => GetCell(cellPos).IsEmpty;

        #endregion

        #region Autoproperties

        private Cell[,] Cells { get; set; }
        public float XOffset { get; private set; }
        public float YOffset { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Stack<Vector2Int> OrderedStack { get; private set; }
        public List<Color> PossibleColors => possibleColors;

        #endregion
    }
}