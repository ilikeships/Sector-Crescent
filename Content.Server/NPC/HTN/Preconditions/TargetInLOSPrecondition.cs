using Content.Server.Interaction;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;

namespace Content.Server.NPC.HTN.Preconditions;

public sealed partial class TargetInLOSPrecondition : HTNPrecondition
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private InteractionSystem _interaction = default!;
    private NpcFactionSystem _npcFaction = default!;

    [DataField("targetKey")]
    public string TargetKey = "Target";

    [DataField("rangeKey")]
    public string RangeKey = "RangeKey";

    /// <summary>
    /// Ignore the line of sight obstruction if it is marked as hostile.
    /// </summary>
    [DataField("ignoreHostileObstruction")]
    public bool IgnoreHostileObstruction = false;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _interaction = sysManager.GetEntitySystem<InteractionSystem>();
        _npcFaction = sysManager.GetEntitySystem<NpcFactionSystem>();
    }

    public override bool IsMet(NPCBlackboard blackboard)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var target, _entManager))
            return false;

        var range = blackboard.GetValueOrDefault<float>(RangeKey, _entManager);

        if (IgnoreHostileObstruction && _entManager.TryGetComponent<NpcFactionMemberComponent>(owner, out var member))
        {
            var entityMember = new Entity<NpcFactionMemberComponent?>(owner, member);
            return _interaction.InRangeUnobstructed(owner, target, range, predicate: other => _entManager.TryGetComponent<NpcFactionMemberComponent>(other, out var otherMember) && !_npcFaction.IsEntityFriendly(entityMember, (other, otherMember)));
        }
        else
        {
            return _interaction.InRangeUnobstructed(owner, target, range);
        }
    }
}
