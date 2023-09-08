using UnityEngine;

namespace Gerard.CherrypickGames
{
    public class Cell : MonoBehaviour
    {
        public bool IsBlocked { get; private set; }
        public Vector2Int GridPosition { get; private set; }
        public bool IsCellEmpty { get; set; }

        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(Vector2Int gridPosition, bool isBlocked)
        {
            GridPosition = gridPosition;
            IsBlocked = isBlocked;

            if (isBlocked)
            {
                _spriteRenderer.color = Color.black;
            }
            else
            {
                IsCellEmpty = true;
                _spriteRenderer.color = (GridPosition.x + GridPosition.y) % 2 == 0 ? Color.white : Color.gray;
            }
        }

        public void SetColor(Color color) => _spriteRenderer.color = color;
    }
}