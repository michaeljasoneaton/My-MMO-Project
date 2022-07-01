// we want to support different movement types:
//   * Navmesh for Monsters
//   * Navmesh Rubberbanding for Players
//   etc.
//
// => Entity.cs needs some common functionality to work with all of them.
// => this makes swapping out movement systems very easy!
using UnityEngine;
using Mirror;

public abstract class Movement : NetworkBehaviour
{
    // velocity is useful for animations etc.
    // => not a property because most movement systems manage their own
    //    'velocity' variable internally, and set them too (we want readonly)
    public abstract Vector2 GetVelocity();

    // currently moving? important for certain skills that can't be casted
    // while moving, etc.
    public abstract bool IsMoving();

    // .speed lives in Entity and depends on level, skills, equip, etc.
    // => in here we simply apply it (e.g. to NavMeshAgent.speed)
    public abstract void SetSpeed(float speed);

    // reset all movement. just stop and stand.
    public abstract void Reset();

    // warp to a different area
    // => setting transform.position isn't good enough. for example,
    //    NavMeshAgent movement always needs to call agent.Warp. otherwise the
    //    agent might get stuck on a tree inbetween position and destination etc
    public abstract void Warp(Vector2 destination);

    // does this movement system support navigation / pathfinding?
    // -> some systems might not support it never
    // -> some might support it while grounded, etc.
    public abstract bool CanNavigate();

    // navigate along a path to a destination
    public abstract void Navigate(Vector2 destination, float stoppingDistance);

    // when spawning we need to know if the last saved position is still valid
    // for this type of movement.
    // * NavMesh movement should only spawn on the NavMesh
    // * CharacterController movement should spawn on a Mesh, etc.
    public abstract bool IsValidSpawnPoint(Vector2 position);

    // sometimes we need to know the nearest valid destination for a point that
    // might be behind a wall, etc.
    public abstract Vector2 NearestValidDestination(Vector2 destination);
}
