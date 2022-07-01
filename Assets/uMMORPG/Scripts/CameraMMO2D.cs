// Simple MMO camera that always follows the player.
using UnityEngine;

public class CameraMMO2D : MonoBehaviour
{
    [Header("Target Follow")]
    public Transform target;
    // the target position can be adjusted by an offset in order to foucs on a
    // target's head for example
    public Vector2 offset = Vector2.zero;

    // smooth the camera movement
    [Header("Dampening")]
    public float damp = 2;

    void LateUpdate()
    {
        if (!target) return;

        // calculate goal position
        Vector2 goal = (Vector2)target.position + offset;

        // interpolate
        Vector2 position = Vector2.Lerp(transform.position, goal, Time.deltaTime * damp);

        // convert to 3D but keep Z to stay in front of 2D plane
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }
}
