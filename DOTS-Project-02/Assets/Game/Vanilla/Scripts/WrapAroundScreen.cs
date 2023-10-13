using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Vanilla
{
    public class WrapAroundScreen : MonoBehaviour
    {
        private List<KinematicBehaviour> _kinematicBehaviours = new List<KinematicBehaviour>();

        // Start is called before the first frame update
        void Start()
        {
            Invoke("GetAllKinematics", 0.01f);
        }

        void GetAllKinematics()
        {
            _kinematicBehaviours = GameObject.FindObjectsByType<KinematicBehaviour>(FindObjectsSortMode.None).ToList();
        }

        // Update is called once per frame
        void Update()
        {
            foreach (var kinematicBehaviour in _kinematicBehaviours)
            {
                if (ScreenManager.OutsideOfScreen(kinematicBehaviour.Kinematic.position, out Vector3 newPos))
                {
                    kinematicBehaviour.Kinematic.position = newPos;
                    kinematicBehaviour.UpdateKinematicTransform();
                }
            }
        }
    }

}
