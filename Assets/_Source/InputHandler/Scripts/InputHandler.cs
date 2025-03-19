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

        private const float RAYCAST_DISTANCE = 1f;

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
            var draggableObject = Physics2D.Raycast(clickDownPosition, Vector2.zero, RAYCAST_DISTANCE, _draggableLayer);

            if (draggableObject)
            {
                if (draggableObject.collider.TryGetComponent(out IDraggable draggable))
                {
                    _currentDraggableObject = draggable;
                }
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
            //TODO: доделать перемещение камеры
            if (_currentDraggableObject != null)
                return;

            Vector3 targetPosition = Camera.transform.position + moveDirection.x * Time.deltaTime * Vector3.left;
            float clampedX = Mathf.Clamp(targetPosition.x, _minX, _maxX);

            Camera.transform.position = new Vector3(clampedX, Camera.transform.position.y, Camera.transform.position.z);
        }

        private bool IsValidMove()
        {

            return false;
        }
    }
}
