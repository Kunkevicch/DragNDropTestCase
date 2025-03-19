using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DragAndDropTestCase
{
    public class TouchInput :
        MonoBehaviour
        , IInput
        , IPointerDownHandler
        , IBeginDragHandler
        , IDragHandler
        , IEndDragHandler
        , IPointerMoveHandler
    {

        private bool _isDragging;
        private Vector2 _startDragPosition;

        public event Action<Vector2> OnClickDown;
        public event Action<Vector2> OnClickUp;
        public event Action<Vector2> OnMovedToPosition;
        public event Action<Vector2> OnMovedToDirection;

        public void OnPointerDown(PointerEventData eventData)
        => OnClickDown?.Invoke(GetWorldPositionByClickPosition(eventData.position));

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _startDragPosition = eventData.position;
            OnMovedToPosition?.Invoke(GetWorldPositionByClickPosition(eventData.position));
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            _startDragPosition = Vector2.zero;
            OnClickUp?.Invoke(GetWorldPositionByClickPosition(eventData.position));
        }

        private Vector2 GetWorldPositionByClickPosition(Vector3 clickPosition)
        => Camera.main.ScreenToWorldPoint(clickPosition);

        public void OnPointerMove(PointerEventData eventData)
        {
            if (!_isDragging)
                return;
            OnMovedToDirection?.Invoke((eventData.position - _startDragPosition).normalized);
        }
    }
}