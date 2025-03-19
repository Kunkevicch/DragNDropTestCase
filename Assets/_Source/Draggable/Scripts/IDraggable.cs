using UnityEngine;

namespace DragAndDropTestCase
{
    public interface IDraggable
    {
        void Drag(Vector2 direction);
        void Drop();
    }
}