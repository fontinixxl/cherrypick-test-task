using UnityEngine;

namespace Gerard.CherrypickGames
{
    public class Item : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private Color _color;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        public Color Color
        {
            get => _color;
            set
            {
                _spriteRenderer.color = value;
                _color = value;
            }
        }
    }
}
