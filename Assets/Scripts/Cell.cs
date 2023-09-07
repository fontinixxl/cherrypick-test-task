using UnityEngine;

namespace Gerard.CherrypickGames
{
    public class Cell : MonoBehaviour
    {
        public bool IsBlocked { get; private set; }
        public Vector2Int GridPosition { get; private set; }

        public void Initialize(Vector2Int gridPosition, bool isBlocked)
        {
            GridPosition = gridPosition;
            IsBlocked = isBlocked;
            
            // Update grid color based on state
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (isBlocked)
            {
                spriteRenderer.color = Color.black;
            }
            else
            {
                spriteRenderer.color = (GridPosition.x + GridPosition.y) % 2 == 0 ? Color.white : Color.gray;
            }
        }
    }
}
