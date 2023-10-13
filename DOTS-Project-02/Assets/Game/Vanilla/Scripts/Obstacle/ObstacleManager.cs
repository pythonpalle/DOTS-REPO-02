using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vanilla
{
    public class ObstacleManager : MonoBehaviour
    {
        public static ObstacleManager Instance;

        [SerializeField] private List<KinematicBehaviour> obastacles;
        public float obstacleRadius;

        private List<Kinematic> obstacleKinematics;

        public List<Kinematic> ObstacleKinematics
        {
            get
            {
                if (obstacleKinematics == null)
                {
                    obstacleKinematics = new List<Kinematic>();

                    foreach (var kinematicBehaviour in obastacles)
                    {
                        obstacleKinematics.Add(kinematicBehaviour.Kinematic);
                    }
                }

                return obstacleKinematics;
            }
        }

        private void Awake()
        {
            Instance = this;
        }

        // private void Update()
        // {
        //     foreach (var obstacle in obastacles)
        //     {
        //         if (ScreenManager.OutsideOfScreen(obstacle.Kinematic.position, out Vector3 newPos))
        //         {
        //             obstacle.Kinematic.position = newPos;
        //             obstacle.UpdateKinematicTransform();
        //         }
        //     }
        // }
    }
}
