using System.Collections;
using UnityEngine;

namespace DragAndDropTestCase
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class DraggableObject : MonoBehaviour, IDraggable, ILockable
    {
        [SerializeField] private float _groundCheckDistance;
        [SerializeField] private float _searchRadius;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private LayerMask _groundLayer;

        private readonly Collider2D[] _groundHitsCache = new Collider2D[3];

        private const float MIN_DISTANCE_TO_NEAREST_POINT = 0.1f;

        private Rigidbody2D _rb;

        private bool _isDragged;
        private bool _isLocked;
        private bool _isFindFloor;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (_isDragged || _isLocked || _isFindFloor)
                return;

            if (IsValidPath())
            {
                _rb.MovePosition(_rb.position + Vector2.down * Time.fixedDeltaTime);
            }
            else
            {
                _isFindFloor = true;
                MoveToNearestFloor();
            }
        }

        public void Drag(Vector2 position)
        {
            _isDragged = true;
            _rb.MovePosition(position);
        }

        public void Drop()
        {
            _isDragged = false;
        }

        public void LockPosition()
        => _isLocked = true;

        public void UnlockPosition()
        => _isLocked = false;

        private bool IsValidPath()
        => Physics2D.Raycast(_rb.position, Vector2.down, _groundCheckDistance, _groundLayer);

        private void MoveToNearestFloor()
        {
            Physics2D.OverlapCircleNonAlloc(_rb.position, _searchRadius, _groundHitsCache, _groundLayer);
            Vector2 closestPoint = Vector2.zero;
            for (int i = 0; i < _groundHitsCache.Length; i++)
            {
                if (_groundHitsCache[i] != null)
                {
                    closestPoint = FindEdgePoint(_rb.position, (new Vector2(_groundHitsCache[i].transform.position.x, _groundHitsCache[i].transform.position.y) - _rb.position), _groundCheckDistance, _groundLayer);
                    break;
                }
            }
            StartCoroutine(MoveToPointRoutine(closestPoint));
        }

        private IEnumerator MoveToPointRoutine(Vector2 targetPosition)
        {
            while (Vector2.Distance(_rb.position, targetPosition) > MIN_DISTANCE_TO_NEAREST_POINT)
            {
                Vector2 newPosition = Vector2.MoveTowards(
                    _rb.position,
                    targetPosition,
                    _moveSpeed * Time.fixedDeltaTime
                );

                _rb.MovePosition(newPosition);

                yield return new WaitForFixedUpdate();
            }
            _rb.MovePosition(targetPosition);
            _isFindFloor = false;
            _isLocked = true;
        }

        private Vector2 FindEdgePoint(Vector2 origin, Vector2 direction, float maxDistance, LayerMask layerMask)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxDistance, layerMask);
            return hit.point;
        }
    }
}

