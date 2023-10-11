using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float sprintSpeedModifer = 2f;
    public float speed = 5f;
    
    void Update()
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        var added = new Vector3(horizontal, 0, vertical);

        if (added.Equals(Vector3.zero))
            return;

        float sprintModifier = Input.GetKey(KeyCode.LeftShift) ? sprintSpeedModifer : 1;
        var input = added.normalized * Time.deltaTime * speed * sprintModifier;

        var currentPosition = transform.position;
        var rotation = Quaternion.LookRotation(input, Vector3.up);
        var newPos = currentPosition + input;
        
        float minDis = ObstacleManager.Instance.obstacleRadius + 1f;
        float minDisSq = minDis*minDis;

        foreach (var obstacle in ObstacleManager.Instance.Obstacles)
        {
            var obstaclePos = obstacle.position;

            float squareDisToObstacle = MathUtility.distancesq(obstaclePos, newPos);
            
            if (squareDisToObstacle < minDisSq)
            {
                newPos = currentPosition;
                break;
            }
        }

        transform.position = newPos;
        transform.rotation = rotation;
    }
}
