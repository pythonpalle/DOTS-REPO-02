using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vanilla
{
    public class PlayerSingleton : MonoBehaviour
    {
        public static PlayerSingleton Instance;
        public Vector3 Position { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            Position = transform.position;
        }
    }

}