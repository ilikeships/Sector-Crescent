using Content.Shared.Construction.Components;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;

namespace Content.Shared._Crescent.Hardpoints;

/// <summary>
/// This handles...
/// </summary>
public class SharedHardpointSystem : EntitySystem
{
    [Dependency] public readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] public readonly EntityLookupSystem _lookupSystem = default!;
    [Dependency] public readonly SharedMapSystem _mapSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<HardpointAnchorableOnlyComponent, AnchorAttemptEvent>(OnAnchorTry);
        SubscribeLocalEvent<HardpointAnchorableOnlyComponent, AnchorStateChangedEvent>(OnAnchorChange);
    }

    public void OnAnchorChange(EntityUid uid, HardpointAnchorableOnlyComponent component, ref AnchorStateChangedEvent args)
    {
        if (args.Anchored == true)
            return;
        if (component.anchoredTo is null)
        {
            Logger.Error($"SharedHardpointSystem had a anchored entity that wasn't attached to a hardpoint!");
            return;
        }

        var gridUid = Transform(component.anchoredTo.Value).GridUid;
        if (gridUid is null)
            return;
        Comp<HardpointComponent>(component.anchoredTo.Value).anchoring = null;
        HardpointCannonDeanchoredEvent arg = new();
        arg.CannonUid = uid;
        arg.gridUid = gridUid.Value;
        RaiseLocalEvent(component.anchoredTo.Value, arg);
        component.anchoredTo = null;
    }
    public void OnAnchorTry(EntityUid uid, HardpointAnchorableOnlyComponent component, ref AnchorAttemptEvent args)
    {
        var gridUid = Transform(uid).GridUid;
        if (gridUid is null)
            return;
        if (!TryComp<MapGridComponent>(gridUid, out var gridComp))
            return;
        if (!_transformSystem.TryGetGridTilePosition(uid, out var indice, gridComp))
        {
            args.Cancel();
            return;
        }

        foreach (var entity in _mapSystem.GetAnchoredEntities(new Entity<MapGridComponent>(gridUid.Value, gridComp ), indice))
        {
            if (!TryComp<HardpointComponent>(entity, out var hardComp))
                continue;
            if (hardComp.anchoring is not null)
                continue;
            AnchorEntityToHardpoint(uid, entity,component ,hardComp, gridUid.Value);
            return;
        }

        args.Cancel();
    }

    public void AnchorEntityToHardpoint(EntityUid target, EntityUid anchor,HardpointAnchorableOnlyComponent targetComp, HardpointComponent hardpoint, EntityUid grid)
    {
        hardpoint.anchoring = target;
        targetComp.anchoredTo = anchor;
        HardpointCannonAnchoredEvent arg = new();
        arg.cannonUid = target;
        arg.gridUid = grid;
        RaiseLocalEvent(anchor, arg);
    }
}
