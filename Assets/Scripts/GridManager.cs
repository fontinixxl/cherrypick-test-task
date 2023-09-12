using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gerard.CherrypickGames
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private Cell cellPrefab;
        [SerializeField] private List<Color> possibleColors = new();
        [SerializeField] private int cellSize = 1;

        private bool[,] _visitedCells;

        public void Initialize(int width, int height)
        {
            Width = width;
            Height = height;
            _visitedCells = new bool[Width, Height];
            GenerateGrid();
            var newPosition = GetCenterCoordinates();
            GenerateSpawningCoordinates(newPosition.x, newPosition.y);
        }

        private void GenerateGrid()
        {
            Cells = new Cell[Width, Height];
            XOffset = Width * 0.5f - CellSize;
            YOffset = Height * 0.5f - CellSize;

            var centerCoordinates = GetCenterCoordinates();
            for (var y = Height - 1; y >= 0; y--) // Start from the top
            {
                for (var x = 0; x < Width; x++)
                {
                    var cellPosition = new Vector3(x, Height - 1 - y, 0); // Adjust y-coordinate
                    var cell = Instantiate(cellPrefab, cellPosition, Quaternion.identity, transform);

                    // If we are NOT on the center cell, 25% chance the cell will be blocked
                    var isBlocked = (x != centerCoordinates.x || y != centerCoordinates.y) && Random.value < .25f;
                    cell.Initialize(new Vector2Int(x, y), isBlocked);

                    Cells[x, y] = cell;
                }
            }

            // Center the entire grid in Unity.
            transform.position = new Vector3(-XOffset, -YOffset, 0);
        }

        public void GenerateSpawningCoordinates(int startX, int startY)
        {
            var tmpStack = new Stack<Vector2Int>(Width * Height - 1);

            // Calculate virtual grid size
            var maxRowDist = Mathf.Max(Height - startX, startX + 1);
            var maxColDist = Mathf.Max(Width - startY, startY + 1);
            var virtualSize = 2 * Mathf.Max(maxRowDist, maxColDist) - 1;

            var visited = new bool[virtualSize, virtualSize];

            var center = virtualSize / 2;
            var offsetX = center - startX;
            var offsetY = center - startY;

            var virtualX = startX + offsetX;
            var virtualY = startY + offsetY;

            int[,] directions = { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 } };

            var currentDir = 0;
            var stepsInCurrentDirection = 1;
            var directionChanges = 0;

            while (tmpStack.Count < Height * Width)
            {
                for (var j = 0; j < stepsInCurrentDirection; j++)
                {
                    if (virtualX >= 0 && virtualX < virtualSize && virtualY >= 0 && virtualY < virtualSize &&
                        !visited[virtualX, virtualY])
                    {
                        visited[virtualX, virtualY] = true;

                        var mappedX = virtualX - offsetX;
                        var mappedY = virtualY - offsetY;

                        if (mappedX >= 0 && mappedX < Height && mappedY >= 0 && mappedY < Width)
                        {
                            tmpStack.Push(new Vector2Int(mappedX, mappedY));
                        }

                        virtualX += directions[currentDir, 0];
                        virtualY += directions[currentDir, 1];
                    }
                    else
                    {
                        break;
                    }
                }

                directionChanges++;
                if (directionChanges == 2)
                {
                    directionChanges = 0;
                    stepsInCurrentDirection++;
                }

                currentDir = (currentDir + 1) % 4;
            }

            // Reverse Stack : https://rb.gy/py862
            SpawningCoordinates = new Stack<Vector2Int>(tmpStack);
            // Remove the starting cell since it is where the spawner is placed
            SpawningCoordinates.Pop();
        }

        # region Clear_Neighbours

        public void ClearNeighbouringColourCells()
        {
            Array.Clear(_visitedCells, 0, _visitedCells.Length);
            ClearNeighbourCellsSameColor();
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
                            var sameColoredCells = new List<Cell>();
                            DFS(cell.GridPosition, color, sameColoredCells);

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
        public Stack<Vector2Int> SpawningCoordinates { get; private set; }
        public float XOffset { get; private set; }
        public float YOffset { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public List<Color> PossibleColors => possibleColors;
        public int CellSize => cellSize;

        #endregion
    }
}