using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vanilla
{
    public class ObstacleManager : MonoBehaviour
    {
        public static ObstacleManager Instance;

        public List<Transform> Obstacles;
        public float obstacleRadius;

        private void Awake()
        {
            Instance = this;
        }
    }
}
