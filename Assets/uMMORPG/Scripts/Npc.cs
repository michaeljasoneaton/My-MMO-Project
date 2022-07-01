// The Npc class is rather simple. It contains state Update functions that do
// nothing at the moment, because Npcs are supposed to stand around all day.
//
// Npcs first show the welcome text and then have options for item trading and
// quests.
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkNavMeshAgent2D))]
public partial class Npc : Entity
{
    [Header("Components")]
    public NpcGuildManagement guildManagement;
    public NpcQuests quests;
    public NpcRevive revive;
    public NpcTrading trading;
    public NpcTeleport teleport;

    [Header("Welcome Text")]
    [TextArea(1, 30)] public string welcome;

    // cache all NpcOffers
    [HideInInspector] public NpcOffer[] offers;

    void Awake()
    {
        offers = GetComponents<NpcOffer>();
    }

    // finite state machine states /////////////////////////////////////////////
    [Server] protected override string UpdateServer() { return state; }
    [Client] protected override void UpdateClient() {}

    // skills //////////////////////////////////////////////////////////////////
    public override bool CanAttack(Entity entity) { return false; }

    // interaction /////////////////////////////////////////////////////////////
    protected override void OnInteract()
    {
        Player player = Player.localPlayer;

        // alive, close enough? then talk
        // use collider point(s) to also work with big entities
        if (health.current > 0 &&
            Utils.ClosestDistance(player, this) <= player.interactionRange)
        {
            UINpcDialogue.singleton.Show();
        }
        // otherwise walk there
        // use collider point(s) to also work with big entities
        else
        {
            Vector2 destination = Utils.ClosestPoint(this, player.transform.position);
            player.movement.Navigate(destination, player.interactionRange);
        }
    }
}
