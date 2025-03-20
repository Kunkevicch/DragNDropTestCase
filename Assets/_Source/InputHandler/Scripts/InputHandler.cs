using System;
using UnityEngine;
using Zenject;

namespace DragAndDropTestCase
{
    public class InputHandler : IInitializable, IDisposable
    {
        private readonly IInput _input;
        private readonly LayerMask _draggableLayer;
        private readonly SpriteRenderer _backGround;
        private readonly Collider2D[] _cachedDraggableObject = new Collider2D[5];

        private const float SPHERECAST_RADIUS = 0.025f;

        private IDraggable _currentDraggableObject;

        private float _cameraHalfWidth;
        private float _minX, _maxX;

        private Camera Camera => Camera.main;

        public InputHandler(IInput input, LayerMask draggableLayer, SpriteRenderer backGround)
        {
            _input = input;
            _draggableLayer = draggableLayer;
            _backGround = backGround;
        }

        public void Initialize()
        {
            // В иной ситуации я бы не менял фреймрейт тут, просто тестовое не настолько большое, чтобы создавать глобальную входную точку в проект.
            // Лок фпс нужен для того, чтобы при 100+ фпс заряд батареи не опустошался со сверхсветовой скоростью
            Application.targetFrameRate = 30;
            CalculateWorldBounds();

            Subscribe();
        }

        public void Dispose()
        {
            Unsubscribe();
        }

        private void CalculateWorldBounds()
        {
            float orthographicSize = Camera.orthographicSize;
            _cameraHalfWidth = orthographicSize * Camera.aspect;

            Bounds bgBounds = _backGround.bounds;
            _minX = bgBounds.min.x + _cameraHalfWidth;
            _maxX = bgBounds.max.x - _cameraHalfWidth;
        }

        private void Subscribe()
        {
            _input.OnClickDown += OnClickDown;
            _input.OnClickUp += OnClickUp;
            _input.OnMovedToPosition += OnMovedToPosition;
            _input.OnMovedToDirection += OnMovedToDirection;
        }

        private void Unsubscribe()
        {
            _input.OnClickDown -= OnClickDown;
            _input.OnClickUp -= OnClickUp;
            _input.OnMovedToPosition -= OnMovedToPosition;
            _input.OnMovedToDirection -= OnMovedToDirection;
        }

        private void OnClickDown(Vector2 clickDownPosition)
        {
            var draggableObjectsCount = Physics2D.OverlapCircleNonAlloc(clickDownPosition, SPHERECAST_RADIUS, _cachedDraggableObject, _draggableLayer);

            if (draggableObjectsCount == 0)
                return;

            float minYDelta = float.PositiveInfinity;
            IDraggable nearestDraggableObject = null;

            foreach (var draggableObjectCollider in _cachedDraggableObject)
            {
                if (draggableObjectCollider == null)
                    continue;

                if (draggableObjectCollider.TryGetComponent(out IDraggable draggable))
                {
                    float yDelta = Mathf.Abs(draggableObjectCollider.transform.position.y - clickDownPosition.y);
                    if (yDelta < minYDelta)
                    {
                        minYDelta = yDelta;
                        nearestDraggableObject = draggable;
                    }

                }
            }

            if (nearestDraggableObject != null)
            {
                _currentDraggableObject = nearestDraggableObject;
                _currentDraggableObject.StartDrag(clickDownPosition);
            }

            for (int i = 0; i < _cachedDraggableObject.Length; i++)
            {
                _cachedDraggableObject[i] = null;
            }
        }

        private void OnClickUp(Vector2 clickUpPosition)
        {
            _currentDraggableObject?.Drop();
            _currentDraggableObject = null;
        }

        private void OnMovedToPosition(Vector2 dragPosition)
        => _currentDraggableObject?.Drag(dragPosition);


        private void OnMovedToDirection(Vector2 moveDirection)
        {
            if (_currentDraggableObject != null)
                return;

            Vector3 targetPosition = Camera.transform.position + moveDirection.x * Time.deltaTime * Vector3.left;
            float clampedX = Mathf.Clamp(targetPosition.x, _minX, _maxX);

            Camera.transform.position = new Vector3(clampedX, Camera.transform.position.y, Camera.transform.position.z);
        }
    }
}
