using JetBrains.Annotations;
using UnityEngine;

namespace Vanilla
{
    public class KinematicBehaviour : MonoBehaviour
    {
        private Kinematic kinematic = new Kinematic();

        public Kinematic Kinematic
        {
            get
            {
                if (kinematic == null)
                {
                    kinematic = new Kinematic();
                }

                return kinematic;
            }
        }

        private void OnEnable()
        { 
            InitalizeKinematic();
        }

        public void InitalizeKinematic() 
        {
            Kinematic.position = transform.position;
            Kinematic.orientationInDegrees = Vector3.SignedAngle(transform.forward, Vector3.right, Vector3.up);
            Kinematic.orientation = Mathf.Deg2Rad * Kinematic.orientationInDegrees;
            Kinematic.velocity = Vector3.zero;
            Kinematic.rotationSpeed = 0f;
        }
        
        public void UpdateKinematicTransform()
        {
            transform.position = Kinematic.position;
            transform.forward = new Vector3(Mathf.Cos(Kinematic.orientation), 0, Mathf.Sin(Kinematic.orientation));
        }
    }
}