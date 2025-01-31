using Content.Server.PointCannons;
using Content.Shared._Crescent.Hardpoints;
using Content.Shared.Construction.Components;
using Content.Shared.PointCannons;
using Robust.Shared.Physics;
using Robust.Shared.Timing;

namespace Content.Server._Crescent.Hardpoint;

/// <summary>
/// This handles...
/// </summary>
public sealed class HardpointSystem : SharedHardpointSystem
{
    [Dependency] private readonly PointCannonSystem _cannonSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    // Explosions can cause a lot of lookups and events to fire. So we time-limit it based on grids
    private const float UpdateDelay = 30f;
    private float InternalTimer = 0f;
    private HashSet<EntityUid> NeedsFiringRangeUpdate = new();
    private HashSet<EntityUid> QueuedGrids = new();
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<FixturesComponent, AnchorStateChangedEvent>(OnFixtureAnchor);
        SubscribeLocalEvent<HardpointComponent, HardpointCannonAnchoredEvent>(OnCannonAnchor);
        SubscribeLocalEvent<HardpointComponent, HardpointCannonDeanchoredEvent>(OnCannonDeanchor);
    }

    public void OnFixtureAnchor(EntityUid uid, FixturesComponent comp, ref AnchorStateChangedEvent args)
    {
        if (args.Transform.GridUid is null)
            return;
        updateAllHardpointsOnGrid(args.Transform.GridUid.Value);
    }

    public void QueueHardpointRefresh(EntityUid cannon, EntityUid grid)
    {
        QueuedGrids.Add(grid);
        NeedsFiringRangeUpdate.Add(cannon);
    }

    public void updateAllHardpointsOnGrid(EntityUid gridUid)
    {
        if (QueuedGrids.Contains(gridUid))
            return;
        HashSet<Entity<HardpointComponent>> lookupList = new();
        _lookupSystem.GetGridEntities(gridUid, lookupList);
        foreach (var entity in lookupList)
        {
            if (entity.Comp.anchoring is null)
                continue;
            QueueHardpointRefresh(entity, gridUid);
        }
    }
    
    public void OnCannonAnchor(EntityUid uid, HardpointComponent comp, ref HardpointCannonAnchoredEvent args)
    {
        _cannonSystem.LinkCannonToAllConsoles(args.cannonUid);
        _cannonSystem.RefreshFiringRanges(args.cannonUid, null, null, null, comp.CannonRangeCheckRange);
    }

    public void OnCannonDeanchor(EntityUid uid, HardpointComponent comp, ref HardpointCannonDeanchoredEvent args)
    {
        _cannonSystem.UnlinkCannon(args.CannonUid);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_gameTiming.IsFirstTimePredicted)
            return;

        InternalTimer += frameTime;
        if (InternalTimer < UpdateDelay)
            return;
        InternalTimer = 0;
        EntityQuery<HardpointComponent> hardpointQuery = GetEntityQuery<HardpointComponent>();
        foreach(var entity in NeedsFiringRangeUpdate)
        {
            if (TerminatingOrDeleted(entity))
                continue;
            var hardpoint = hardpointQuery.GetComponent(entity);
            if (hardpoint.anchoring is null)
                continue;
            if (TerminatingOrDeleted(hardpoint.anchoring.Value))
                continue;
            _cannonSystem.RefreshFiringRanges(hardpoint.anchoring.Value, null, null, null, hardpoint.CannonRangeCheckRange);

        }
        QueuedGrids.Clear();
        NeedsFiringRangeUpdate.Clear();
    }
}
