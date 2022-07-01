// NavMesh + NavMeshAgent movement for monsters, pets, etc.
// => abstract because Warp needs to call RpcWarp in NetworkNavMeshAgent or
//    NetworkNavMeshAgentRubberbanding depending on the implementation
// => players can use it to but need to inherit and implement their own WASD
//    movement for it
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NavMeshAgent2D))]
[DisallowMultipleComponent]
//[RequireComponent(typeof(NetworkNavMeshAgent2D))] => players use a different sync method than monsters. can't require it.
public abstract class NavMeshMovement2D : Movement
{
    [Header("Components")]
    public NavMeshAgent2D agent;

    public override Vector2 GetVelocity() =>
        agent.velocity;

    // IsMoving:
    // -> agent.hasPath will be true if stopping distance > 0, so we can't
    //    really rely on that.
    // -> pathPending is true while calculating the path, which is good
    // -> remainingDistance is the distance to the last path point, so it
    //    also works when clicking somewhere onto a obstacle that isn't
    //    directly reachable.
    // -> velocity is the best way to detect WASD movement
    public override bool IsMoving() =>
        agent.pathPending ||
        agent.remainingDistance > agent.stoppingDistance ||
        agent.velocity != Vector2.zero;

    public override void SetSpeed(float speed)
    {
        agent.speed = speed;
    }

    public override bool CanNavigate()
    {
        return true;
    }

    public override void Navigate(Vector2 destination, float stoppingDistance)
    {
        agent.stoppingDistance = stoppingDistance;
        agent.destination = destination;
    }

    // when spawning we need to know if the last saved position is still valid
    // for this type of movement.
    public override bool IsValidSpawnPoint(Vector2 position)
    {
        return NavMesh2D.SamplePosition(position, out NavMeshHit2D _, 0.1f, NavMesh2D.AllAreas);
    }

    public override Vector2 NearestValidDestination(Vector2 destination)
    {
        return agent.NearestValidDestination(destination);
    }

    [Server]
    public void OnDeath()
    {
        // reset movement. don't slide to a destination if we die while moving.
        Reset();
    }
}
