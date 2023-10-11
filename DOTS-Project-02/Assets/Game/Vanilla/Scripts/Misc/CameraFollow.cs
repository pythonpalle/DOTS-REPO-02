using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Vector3 offset;
    public Transform followTransform;

    void Update()
    {
        transform.position = followTransform.position + offset;
    }
}
