using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gerard.CherrypickGames
{
    public class SpawnerController : MonoBehaviour
    {
        [SerializeField] private GameObject itemPrefab;

        private GridController _gridController;
        private bool _isDragging;
        private Camera _mainCamera;
        private Collider2D _collider2D;
        private Vector3 _originalPositionBeforeDrag;

        private Action _onSpawningCompletedCallback;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _collider2D = GetComponent<Collider2D>();
        }

        public void Initialize(GridController gridController, Action callBack)
        {
            _gridController = gridController;
            _onSpawningCompletedCallback = callBack;
        }

        public void SpawnItems()
        {
            if (_gridController.OrderedStack == null) return;

            var possibleColors = _gridController.PossibleColors;
            while (_gridController.OrderedStack.Count > 0)
            {
                var targetGridPosition = _gridController.OrderedStack.Pop();
                if (_gridController.IsValidPosition(targetGridPosition))
                {
                    var targetPosition = _gridController.GetCell(targetGridPosition).transform.position;
                    var item = Instantiate(itemPrefab, transform.position, Quaternion.identity, _gridController.transform)
                        .GetComponent<Item>();
                    var color = possibleColors[Random.Range(0, possibleColors.Count)];
                    _gridController.GetCell(targetGridPosition).AddItem(item, color);

                    // Start animation towards target cell
                    StartCoroutine(MoveItemToTargetPosition(item.transform, targetPosition, .1f));
                    return;
                }
            }

            _onSpawningCompletedCallback.Invoke();
        }

        private IEnumerator MoveItemToTargetPosition(Transform itemTransform, Vector3 targetPosition, float duration)
        {
            var startPosition = itemTransform.position;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var normalizedTime = elapsed / duration; // value between 0 and 1
                itemTransform.position = Vector3.Lerp(startPosition, targetPosition, normalizedTime);
                yield return null;
            }

            itemTransform.position = targetPosition; // ensure the item reaches the exact target position
        }

        public void HandleDrag(Action<Vector2Int> onSpawnerMovedCallback)
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
                if (SnapToCell(mouseWorldPosition, out var newGridPosition))
                {
                    onSpawnerMovedCallback?.Invoke(newGridPosition);
                }

                _isDragging = false;
            }

            if (_isDragging)
            {
                mouseWorldPosition.z = 0; // Ensure the z-position remains consistent
                transform.position = mouseWorldPosition;
            }
        }

        private bool SnapToCell(Vector3 mousePosition, out Vector2Int newGridPosition)
        {
            var closestX = Mathf.RoundToInt(mousePosition.x + _gridController.XOffset);
            var closestY =
                Mathf.RoundToInt(_gridController.Height - 1 - mousePosition.y -
                                 _gridController.YOffset); // Adjust Y calculation

            // Ensure we are within grid boundaries
            closestX = Mathf.Clamp(closestX, 0, _gridController.Width - 1);
            closestY = Mathf.Clamp(closestY, 0, _gridController.Height - 1);

            newGridPosition = new Vector2Int(closestX, closestY);
            var targetCell = _gridController.GetCell(newGridPosition);

            if (targetCell.IsBlocked || !targetCell.IsEmpty)
            {
                transform.position = _originalPositionBeforeDrag;
            }
            else
            {
                transform.position = targetCell.transform.position;
            }

            // If current position is different than original means the snapping was successful
            return transform.position != _originalPositionBeforeDrag;
        }
    }
}