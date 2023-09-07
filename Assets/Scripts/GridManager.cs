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

        private void Start()
        {
            if (LoadGridConfig())
            {
                GenerateGrid();
                SpawnSpawner();
            }
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

            Vector2Int centerCoordinates = GetGridCenterCoordinates();
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    var cellPosition = new Vector3(j - XOffset, i - YOffset, 0);
                    var cell = Instantiate(cellPrefab, cellPosition, Quaternion.identity, transform)
                        .GetComponent<Cell>();

                    // If we are NOT on the center cell, 25% chance of making the cell blocked
                    var isBlocked = (j != centerCoordinates.x || i != centerCoordinates.y) && Random.value < .25f;
                    cell.Initialize(new Vector2Int(j, i), isBlocked);
                    
                    Cells[j, i] = cell;
                }
            }
        }

        private void SpawnSpawner()
        {
            var middleCoord = GetGridCenterCoordinates();
            var cell = Cells[middleCoord.x, middleCoord.y];
            var middleCellTransform = cell.GetComponent<Transform>();
            Instantiate(spawnerPrefab, middleCellTransform.position, Quaternion.identity);
        }

        private Vector2Int GetGridCenterCoordinates()
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
    }

    internal struct GridData
    {
        public int Width;
        public int Height;
    }
}
