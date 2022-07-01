// simple movement wrapper for mount.
// only needs to set transform.position.
using UnityEngine;

public class MountMovement : Movement
{
    // components to assign in Inspector
    public Mount mount;

    public override Vector2 GetVelocity() => mount.owner.movement.GetVelocity();
    public override bool IsMoving() => true;
    public override void SetSpeed(float speed) {}
    public override void Reset() {}
    public override void Warp(Vector2 destination) => transform.position = destination;
    public override bool CanNavigate() => false;
    public override void Navigate(Vector2 destination, float stoppingDistance) {}
    public override bool IsValidSpawnPoint(Vector2 position) => true;
    public override Vector2 NearestValidDestination(Vector2 destination) => Vector2.zero;
}
