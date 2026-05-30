using UnityEngine;
using UHFPS.Runtime;

namespace AfterShift.UHFPS
{
    public class DynamicObjectExtensions : MonoBehaviour
    {
        [SerializeField] private DynamicObject dynamicObject;
        [SerializeField] private float lockDelay = 0.5f;

        private void Reset()
        {
            dynamicObject = GetComponent<DynamicObject>();
        }

        public void SetOpenAndLock()
        {
            if (dynamicObject == null) return;
            if (dynamicObject.IsLocked || dynamicObject.IsJammed) return;

            dynamicObject.SetOpenState();
            Invoke(nameof(LockAfter), lockDelay);
        }

        public void SetCloseAndLock()
        {
            if (dynamicObject == null) return;
            if (dynamicObject.IsLocked || dynamicObject.IsJammed) return;

            dynamicObject.SetCloseState();
            Invoke(nameof(LockAfter), lockDelay);
        }

        private void LockAfter()
        {
            if (dynamicObject == null) return;
            dynamicObject.SetLockedStatus(true);
        }
    }
}