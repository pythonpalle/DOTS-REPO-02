using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DOTS
{
    public class ExcecuteAuthroing : MonoBehaviour
    {
        public bool runWithJobs;
        public bool runWithoutJobs;
    
        class ExcecuteBaker : Baker<ExcecuteAuthroing>
        {
            public override void Bake(ExcecuteAuthroing authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                if (authoring.runWithJobs)
                {
                    AddComponent(entity, new RunBoidsWithJobs());
                }
                else if (authoring.runWithoutJobs)
                {
                    AddComponent(entity, new RunBoidsWithoutJobs());
                }
            }
        }
    }
}

