using UnityEngine;

namespace Gerard.CherrypickGames
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private GameObject cellPrefab;
        
        private int _width;
        private int _height;
        
        private void Start()
        {
            if (LoadGridConfig())
            {
                GenerateGrid();
            }
        }

        private bool LoadGridConfig()
        {
            var json = Resources.Load<TextAsset>("gridConfig");
            if(json == null)
            {
                Debug.LogError("gridConfig.json resource not found!");
                return false;
            }
            var gridData = JsonUtility.FromJson<GridData>(json.ToString());
            _width = gridData.Width;
            _height = gridData.Height;

            return true;
        }
        
        private void GenerateGrid()
        {
            var xOffset = _width * 0.5f - 0.5f;
            var yOffset = _height * 0.5f - 0.5f;

            for (var i = 0; i < _height; i++)
            {
                for (var j = 0; j < _width; j++)
                {
                    var cell = Instantiate(cellPrefab, new Vector3(j - xOffset, i - yOffset, 0), Quaternion.identity, this.transform);
                    var cellRenderer = cell.GetComponent<SpriteRenderer>();
                    if (Random.value < 0.25f)
                    {
                        cellRenderer.color = Color.black;
                    }
                    else
                    {
                        cellRenderer.color = (i + j) % 2 == 0 ? Color.white : Color.gray;
                    }
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