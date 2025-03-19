using System;
using UnityEngine;

namespace DragAndDropTestCase
{
    public interface IInput
    {
        public event Action<Vector2> OnClickDown;
        public event Action<Vector2> OnClickUp;
        public event Action<Vector2> OnMovedToPosition;
        public event Action<Vector2> OnMovedToDirection;
    }
}