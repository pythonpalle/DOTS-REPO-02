using Unity.Entities;
using Unity.Transforms;
using UnityEngine;


namespace DOTS
{
    public partial struct WrapAroundSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CameraConfig>();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var cameraConfig = SystemAPI.GetSingleton<CameraConfig>();

            var camera = Camera.main;
            if (!camera)
                return;
            
            // finds all wrap transforms and wraps around screen if necessary
            foreach (var wrapTransform in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<WrapComponent>())
            {
                var position = wrapTransform.ValueRO.Position;
                if (OutsideOfScreen(camera, position, out var newPos))
                {
                    wrapTransform.ValueRW.Position = newPos;
                }
            }
        }
        
        private bool OutsideOfScreen(Camera camera, Vector3 inPosition, out Vector3 newPos)
        {
            var viewportPos = camera.WorldToViewportPoint(inPosition);
            newPos = new Vector3();
            float offset = 0.1f;
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

            newPos = camera.ViewportToWorldPoint(viewportPos);
            return outside;
        }
    }
    
    

}
