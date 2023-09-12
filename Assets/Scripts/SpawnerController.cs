using System;
using System.Collections;
using Gerard.CherryPickGames.Input;
using Gerard.CherryPickGames.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gerard.CherrypickGames
{
    [RequireComponent(typeof(ButtonPressRelease))]
    public class SpawnerController : MonoBehaviour
    {
        [SerializeField] private Item itemPrefab;

        private GridManager _gridManager;
        private bool _isDragging;
        private Camera _mainCamera;
        private Vector3 _originalPositionBeforeDrag;

        private Action<Vector2Int> _onSpawnerMoved;
        private ButtonPressRelease _draggable;
        private InputManager _inputManger;

        public Vector2Int CurrentGridPosition { get; private set; }

        public void Initialize(GridManager gridManager, Camera mainCamera, Action<Vector2Int> onSpawnerMoved)
        {
            _gridManager = gridManager;
            _mainCamera = mainCamera;
            _onSpawnerMoved = onSpawnerMoved;
        }

        private void Awake()
        {
            _draggable = GetComponent<ButtonPressRelease>();
        }

        private void OnEnable()
        {
            _draggable.OnButtonStateChanged += OnObjectInteracted;
        }

        private void Start()
        {
            _inputManger = InputManager.Instance;
        }

        private void Update()
        {
            if (!_isDragging) return;

            var position = GetPositionWorldSpace();
            position.z = 0;
            transform.position = position;
        }

        private void OnDisable()
        {
            _draggable.OnButtonStateChanged -= OnObjectInteracted;
        }

        public IEnumerator SpawnCoroutine()
        {
            if (_gridManager.SpawningCoordinates == null) yield break;

            var possibleColors = _gridManager.PossibleColors;
            while (_gridManager.SpawningCoordinates.Count > 0)
            {
                var targetGridPosition = _gridManager.SpawningCoordinates.Pop();
                if (_gridManager.IsValidPosition(targetGridPosition))
                {
                    var targetPosition = _gridManager.GetCell(targetGridPosition).transform.position;
                    var item = Instantiate(itemPrefab, transform.position, Quaternion.identity, _gridManager.transform);
                    var color = possibleColors[Random.Range(0, possibleColors.Count)];
                    _gridManager.GetCell(targetGridPosition).AddItem(item, color);

                    // Start animation towards target cell
                    StartCoroutine(MoveItemToTargetPosition(item.transform, targetPosition, .1f));
                    yield return null;
                }
            }
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

        private void OnObjectInteracted(bool isObjectSelected)
        {
            if (isObjectSelected)
            {
                _originalPositionBeforeDrag = transform.position;
                _inputManger.EnableDraggingControls();
                _isDragging = true;
            }
            else
            {
                if (TrySnapToCell(GetPositionWorldSpace(), out var newGridPosition))
                {
                    CurrentGridPosition = newGridPosition;
                    _onSpawnerMoved?.Invoke(newGridPosition);
                }
                _isDragging = false;
                _inputManger.EnableCameraControls();
            }
        }

        private Vector3 GetPositionWorldSpace()
        {
            var currentPos = _inputManger.GetCurrentMouseOrTouchPosition();
            return _mainCamera.ScreenToWorldPoint(currentPos);
        }

        private bool TrySnapToCell(Vector3 mousePosition, out Vector2Int newGridPosition)
        {
            var closestX = Mathf.RoundToInt(mousePosition.x + _gridManager.XOffset);
            var closestY =
                Mathf.RoundToInt(_gridManager.Height - 1 - mousePosition.y -
                                 _gridManager.YOffset); // Adjust Y calculation

            // Ensure we are within grid boundaries
            closestX = Mathf.Clamp(closestX, 0, _gridManager.Width - 1);
            closestY = Mathf.Clamp(closestY, 0, _gridManager.Height - 1);

            newGridPosition = new Vector2Int(closestX, closestY);
            var targetCell = _gridManager.GetCell(newGridPosition);

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