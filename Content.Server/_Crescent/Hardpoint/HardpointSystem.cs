using Content.Server.PointCannons;
using Content.Shared._Crescent.Hardpoints;
using Content.Shared.Construction.Components;
using Content.Shared.PointCannons;
using Robust.Shared.Physics;

namespace Content.Server._Crescent.Hardpoint;

/// <summary>
/// This handles...
/// </summary>
public sealed class HardpointSystem : SharedHardpointSystem
{
    [Dependency] private readonly PointCannonSystem _cannonSystem = default!;
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
        var targetCoords = _transformSystem.GetGridTilePositionOrDefault(uid);
        updateAllHardointsOnGridNearPoint(args.Transform.GridUid.Value, targetCoords);
    }

    public void updateAllHardpointsOnGrid(EntityUid gridUid)
    {
        HashSet<Entity<HardpointComponent>> lookupList = new();
        _lookupSystem.GetGridEntities(gridUid, lookupList);
        foreach (var entity in lookupList)
        {
            if (entity.Comp.anchoring is null)
                continue;
            _cannonSystem.RefreshFiringRanges(entity.Comp.anchoring.Value, null, null, null, entity.Comp.CannonRangeCheckRange);
        }
    }

    public void updateAllHardointsOnGridNearPoint(EntityUid gridUid, Vector2i targetCoords)
    {
        HashSet<Entity<HardpointComponent>> lookupList = new();
        _lookupSystem.GetGridEntities(gridUid, lookupList);
        foreach (var entity in lookupList)
        {
            if (entity.Comp.anchoring is null)
                continue;
            var ourCoords = targetCoords - _transformSystem.GetGridTilePositionOrDefault(entity.Owner);
            if (ourCoords.Length < entity.Comp.CannonRangeCheckRange)
                _cannonSystem.RefreshFiringRanges(entity.Comp.anchoring.Value, null, null, null, entity.Comp.CannonRangeCheckRange);
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
}
