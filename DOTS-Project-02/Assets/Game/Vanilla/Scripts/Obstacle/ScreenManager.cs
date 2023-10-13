using UnityEngine;

namespace Vanilla
{
    public static class ScreenManager
    {
        private static Camera mainCamera = Camera.main;
        
        public static bool OutsideOfScreen(Vector3 inPosition, out Vector3 newPos)
        {
            var viewportPos = mainCamera.WorldToViewportPoint(inPosition);
            newPos = new Vector3();
            float offset = 0.05f;
            float halfOffset = offset * 0.5f;

            bool outside = false;
        
            if (viewportPos.x < -offset)
            {
                viewportPos.x = 1+halfOffset;
                outside = true;
            }
            else if (viewportPos.x > 1+offset)
            {
                viewportPos.x = -halfOffset;
                outside = true;
            }

            if (viewportPos.y < -offset)
            {
                viewportPos.y = 1+halfOffset;
                outside = true;
            }
            else if (viewportPos.y > 1+offset)
            {
                viewportPos.y = -halfOffset;
                outside = true;
            }

            newPos = mainCamera.ViewportToWorldPoint(viewportPos);
            return outside;
        }
    }
}