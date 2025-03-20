using UnityEngine;

namespace DragAndDropTestCase
{
    public interface IDraggable
    {
        public void StartDrag(Vector2 direction);
        public void Drag(Vector2 direction);
        public void Drop();
    }
}