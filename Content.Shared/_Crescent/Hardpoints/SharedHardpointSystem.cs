using Content.Shared.Construction.Components;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;

namespace Content.Shared._Crescent.Hardpoints;

/// <summary>
/// This handles...
/// </summary>
public sealed class SharedHardpointSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<HardpointAnchorableOnlyComponent, AnchorAttemptEvent>(OnAnchorTry);
        SubscribeLocalEvent<FixturesComponent, AnchorStateChangedEvent>(OnFixtureAnchor);
    }

    public void OnFixtureAnchor(EntityUid uid, FixturesComponent comp, ref AnchorStateChangedEvent args)
    {
        if (args.Transform.GridUid is null)
            return;
        Logger.Error($"new BB detected on {MetaData(args.Transform.GridUid.Value).EntityName}");
    }

    private void OnAnchorTry(EntityUid uid, HardpointAnchorableOnlyComponent component, ref AnchorAttemptEvent args)
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
            return;
        }

        args.Cancel();
    }

    private void AnchorEntityToHardpoint(EntityUid target, EntityUid anchor, HardpointComponent hardpoint)
    {

    }
}
