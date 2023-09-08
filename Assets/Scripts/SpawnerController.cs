using System.Collections;
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
        private bool _isSpawning = false;

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

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _isSpawning = !_isSpawning;
            }

            if (_isSpawning)
            {
                SpawnColoredItems();
            }
            else
            {
                HandleDrag();
            }
        }

        private void SpawnColoredItems()
        {
            if (_gridManager.OrderedStack == null || _gridManager.OrderedStack.Count == 0) return;

            while (_gridManager.OrderedStack.Count > 0)
            {
                var targetGridPosition = _gridManager.OrderedStack.Pop();
                if (_gridManager.IsValidPosition(targetGridPosition))
                {
                    var targetPosition = _gridManager.GetCell(targetGridPosition).transform.position;
                    var item = Instantiate(itemPrefab, transform.position, Quaternion.identity, _gridManager.transform);
                    item.GetComponent<SpriteRenderer>().color = possibleColors[Random.Range(0, possibleColors.Count)];

                    // Start animation coroutine
                    StartCoroutine(MoveToPosition(item.transform, targetPosition, .1f));
                    return;
                }
            }

            _isSpawning = false;
        }

        private IEnumerator MoveToPosition(Transform itemTransform, Vector3 targetPosition, float duration)
        {
            var startPosition = itemTransform.position;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / duration; // value between 0 and 1
                itemTransform.position = Vector3.Lerp(startPosition, targetPosition, normalizedTime);
                yield return null;
            }

            itemTransform.position = targetPosition; // ensure the item reaches the exact target position
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

            if (Input.GetMouseButtonUp(0) && _isDragging)
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