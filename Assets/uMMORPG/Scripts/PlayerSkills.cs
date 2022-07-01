using UnityEngine;
using Mirror;

[RequireComponent(typeof(Level))]
[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(PlayerParty))]
[DisallowMultipleComponent]
public class PlayerSkills : Skills
{
    [Header("Components")]
    public Level level;
    public Movement movement;
    public PlayerParty party;

    [Header("Skill Experience")]
    [SyncVar] public long skillExperience = 0;

    // always store lookDirection at the time of casting.
    // this is the only 100% accurate way since player movement is only synced
    // in intervals and look direction from velocity is never 100% accurate.
    // fixes https://github.com/vis2k/uMMORPG2D/issues/19
    // => only necessary for players. all other entities are server controlled.
    Vector2 _currentSkillDirection = Vector2.down;
    protected override Vector2 currentSkillDirection => _currentSkillDirection;

    void Start()
    {
        // do nothing if not spawned (=for character selection previews)
        if (!isServer && !isClient) return;

        // spawn effects for any buffs that might still be active after loading
        // (OnStartServer is too early)
        // note: no need to do that in Entity.Start because we don't load them
        //       with previously casted skills
        if (isServer)
            for (int i = 0; i < buffs.Count; ++i)
                if (buffs[i].BuffTimeRemaining() > 0)
                    buffs[i].data.SpawnEffect(entity, entity);
    }

    // IMPORTANT
    // for targetless skills we always need look direction at the exact moment
    // when the skill is casted on the client.
    //
    // using the look direction on server is never 100% accurate.
    // it's assumed from movement.velocity, but we only sync client position to
    // server every 'interval'. it's never 100%.
    //
    // for example, consider this movement on client:
    //    ----------|
    //              |
    //
    // which might be come this movement on server:
    //    --------\
    //             \
    //
    // if the server's destination is set to the last position while it hasn't
    // reached the second last position yet (it'll just go diagonal, hence not
    // change the move direction from 'right' to 'down'.
    //
    // the only 100% accurate solution is to always pass direction at the exact
    // moment of the cast.
    //
    // see also: https://github.com/vis2k/uMMORPG2D/issues/19
    [Command]
    public void CmdUse(int skillIndex, Vector2 direction)
    {
        // validate
        if ((entity.state == "IDLE" || entity.state == "MOVING" || entity.state == "CASTING") &&
            0 <= skillIndex && skillIndex < skills.Count)
        {
            // skill learned and can be casted?
            if (skills[skillIndex].level > 0 && skills[skillIndex].IsReady())
            {
                // set skill index to cast next.
                currentSkill = skillIndex;

                // set look direction to use when the cast starts.
                // DO NOT set entity.lookDirection instead. it would be over-
                // written by Entity.Update before the actual cast starts!
                // fixes https://github.com/vis2k/uMMORPG2D/issues/19
                _currentSkillDirection = direction;

                // let's set it anyway for visuals.
                // even if it might be overwritten.
                entity.lookDirection = direction;
            }
        }
    }

    // helper function: try to use a skill and walk into range if necessary
    [Client]
    public void TryUse(int skillIndex, bool ignoreState=false)
    {
        // only if not casting already
        // (might need to ignore that when coming from pending skill where
        //  CASTING is still true)
        if (entity.state != "CASTING" || ignoreState)
        {
            Skill skill = skills[skillIndex];
            if (CastCheckSelf(skill) && CastCheckTarget(skill))
            {
                // check distance between self and target
                Vector2 destination;
                if (CastCheckDistance(skill, out destination))
                {
                    // cast
                    CmdUse(skillIndex, ((Player)entity).lookDirection);
                }
                else
                {
                    // move to the target first
                    // (use collider point(s) to also work with big entities)
                    float stoppingDistance = skill.castRange * ((Player)entity).attackToMoveRangeRatio;
                    movement.Navigate(destination, stoppingDistance);

                    // use skill when there
                    ((Player)entity).useSkillWhenCloser = skillIndex;
                }
            }
        }
        else
        {
            ((Player)entity).pendingSkill = skillIndex;
        }
    }

    public bool HasLearned(string skillName)
    {
        // has this skill with at least level 1 (=learned)?
        return HasLearnedWithLevel(skillName, 1);
    }

    public bool HasLearnedWithLevel(string skillName, int skillLevel)
    {
        // (avoid Linq because it is HEAVY(!) on GC and performance)
        foreach (Skill skill in skills)
            if (skill.level >= skillLevel && skill.name == skillName)
                return true;
        return false;
    }

    // helper function for command and UI
    // -> this is for learning and upgrading!
    public bool CanUpgrade(Skill skill)
    {
        return skill.level < skill.maxLevel &&
               level.current >= skill.upgradeRequiredLevel &&
               skillExperience >= skill.upgradeRequiredSkillExperience &&
               (skill.predecessor == null || (HasLearnedWithLevel(skill.predecessor.name, skill.predecessorLevel)));
    }

    // -> this is for learning and upgrading!
    [Command]
    public void CmdUpgrade(int skillIndex)
    {
        // validate
        if ((entity.state == "IDLE" || entity.state == "MOVING" || entity.state == "CASTING") &&
            0 <= skillIndex && skillIndex < skills.Count)
        {
            // can be upgraded?
            Skill skill = skills[skillIndex];
            if (CanUpgrade(skill))
            {
                // decrease skill experience
                skillExperience -= skill.upgradeRequiredSkillExperience;

                // upgrade
                ++skill.level;
                skills[skillIndex] = skill;
            }
        }
    }

    // events //////////////////////////////////////////////////////////////////
    [Server]
    public void OnKilledEnemy(Entity victim)
    {
        // killed a monster
        if (victim is Monster monster)
        {
            // gain exp if not in a party or if in a party without exp share
            if (!party.InParty() || !party.party.shareExperience)
                skillExperience += Experience.BalanceExperienceReward(monster.rewardSkillExperience, level.current, monster.level.current);
        }
    }
}
