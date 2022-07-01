// put all mount control code into a separate component.
// => for consistency with PetControl
// => if we need mount inventories, then we can put the Cmds in here too later
using UnityEngine;
using Mirror;

[DisallowMultipleComponent]
public class PlayerMountControl : NetworkBehaviour
{
    [Header("Mount")]
    public Transform spriteToOffsetWhenMounted;
    public float seatOffsetY = 0.25f;

    [SyncVar, HideInInspector] public Mount activeMount;

    void LateUpdate()
    {
        // follow mount's seat position if mounted
        // (on server too, for correct collider position and calculations)
        ApplyMountSeatOffset();
    }

    public bool IsMounted()
    {
        return activeMount != null && activeMount.health.current > 0;
    }

    void ApplyMountSeatOffset()
    {
        if (spriteToOffsetWhenMounted != null)
        {
            // apply seat offset if on mount (not a dead one), reset otherwise
            if (activeMount != null && activeMount.health.current > 0)
                spriteToOffsetWhenMounted.transform.position = (Vector2)activeMount.seat.position + Vector2.up * seatOffsetY;
            else
                spriteToOffsetWhenMounted.transform.localPosition = Vector2.zero;
        }
    }
}
