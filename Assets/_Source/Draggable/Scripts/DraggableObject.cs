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

        private Rigidbody2D _rb;
        private Coroutine _changeScaleCoroutine;
        private Vector3 _baseScale;

        private readonly Collider2D[] _groundHitsCache = new Collider2D[3];

        private const float MIN_DISTANCE_TO_NEAREST_POINT = 0.1f;
        private const float TIME_TO_CHANGE_SCALE = 0.2f;
        private const float SCALE_UP_MODIFIER = 0.03f;

        private bool _isDragged;
        private bool _isLocked;
        private bool _isFindFloor;

        private void Awake()
        {
            _baseScale = transform.localScale;
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

        public void StartDrag(Vector2 startedDragPosition)
        {
            _isDragged = true;
            StartChangeScale(true);
            _rb.MovePosition(startedDragPosition);
        }

        public void Drag(Vector2 position)
        {
            _rb.MovePosition(position);
        }

        public void Drop()
        {
            _isDragged = false;
            StartChangeScale(false);
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
        => Physics2D.Raycast(origin, direction, maxDistance, layerMask).point;

        private void StartChangeScale(bool isScaleUp)
        {
            if (_changeScaleCoroutine != null)
            {
                StopCoroutine(_changeScaleCoroutine);
                _changeScaleCoroutine = null;
            }

            _changeScaleCoroutine = StartCoroutine(ChangeScaleRoutine(isScaleUp));
        }

        private IEnumerator ChangeScaleRoutine(bool isScaleUp)
        {
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = isScaleUp ?
                _baseScale + Vector3.one * SCALE_UP_MODIFIER :
                _baseScale;

            float elapsedTime = 0f;

            while (elapsedTime < TIME_TO_CHANGE_SCALE)
            {
                float progress = elapsedTime / TIME_TO_CHANGE_SCALE;
                transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.localScale = targetScale;
            _changeScaleCoroutine = null;
        }
    }
}

