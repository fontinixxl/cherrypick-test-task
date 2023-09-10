using UnityEngine;

namespace Gerard.CherrypickGames
{
    public class Cell : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private Item _item;

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
                _spriteRenderer.color = (GridPosition.x + GridPosition.y) % 2 == 0 ? Color.white : Color.gray;
            }
        }

        public void AddItem(Item item, Color color)
        {
            _item = item;
            _item.Color = color;
        }

        public void ClearCell()
        {
            if (_item != null)
            {
                Destroy(_item.gameObject);
            }
        }

        public bool IsBlocked { get; private set; }
        public Vector2Int GridPosition { get; private set; }
        public bool IsEmpty => _item == null;
        public Color ItemColor => _item.Color;
    }
}