using UnityEngine;

namespace DragAndDropTestCase
{
    public class LockArea : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out ILockable lockable))
            {
                lockable.LockPosition();
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out ILockable lockable))
            {
                lockable.UnlockPosition();
            }
        }
    }
}
