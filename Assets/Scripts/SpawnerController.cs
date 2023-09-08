using System.Collections.Generic;
using UnityEngine;

namespace Gerard.CherrypickGames
{
    public class SpawnerController : MonoBehaviour
    {
        [SerializeField] private GameObject itemPrefab;

        [SerializeField] private List<Color> possibleColors = new()
        {
            Color.blue,
            Color.red,
            Color.green
        };

        private GridManager _gridManager;
        private bool _isDragging;
        private Camera _mainCamera;
        private Collider2D _collider2D;
        private Vector3 _originalPositionBeforeDrag;

        private Vector2Int _currentGridPos;

        public void Initialize(GridManager gridManager, Vector2Int initialGridPosition)
        {
            _gridManager = gridManager;
            _currentGridPos = initialGridPosition;
        }

        private void Awake()
        {
            _mainCamera = Camera.main;
            _collider2D = GetComponent<Collider2D>();
        }

        private void Update()
        {
            if (_gridManager == null) return;

            HandleDrag();
        }


        private void HandleDrag()
        {
            var mouseWorldPosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            if (Input.GetMouseButtonDown(0))
            {
                if (_collider2D.OverlapPoint(mouseWorldPosition))
                {
                    _originalPositionBeforeDrag = transform.position;
                    _isDragging = true;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {   
                _isDragging = false;
                SnapToCell(mouseWorldPosition);
            }

            if (_isDragging)
            {
                mouseWorldPosition.z = 0; // Ensure the z-position remains consistent
                transform.position = mouseWorldPosition;
            }
        }

        private void SnapToCell(Vector3 mousePosition)
        {
            var closestX = Mathf.RoundToInt(mousePosition.x + _gridManager.XOffset);
            var closestY =
                Mathf.RoundToInt(_gridManager.Height - 1 - mousePosition.y -
                                 _gridManager.YOffset); // Adjust Y calculation

            // Ensure we are within grid boundaries
            closestX = Mathf.Clamp(closestX, 0, _gridManager.Width - 1);
            closestY = Mathf.Clamp(closestY, 0, _gridManager.Height - 1);

            var closestCell = _gridManager.Cells[closestX, closestY];
            if (closestCell.IsBlocked)
            {
                transform.position = _originalPositionBeforeDrag;
            }
            else
            {
                transform.position = closestCell.transform.position;
            }
        }
    }
}