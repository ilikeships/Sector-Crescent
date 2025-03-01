using Content.Shared.Containers.ItemSlots;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.UI.MapObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Content.Shared.Shuttles.Systems;
using Content.Shared.Shuttles;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Content.Shared.Maps;
using Robust.Server.GameObjects;
using Content.Server.Shuttles.Systems;
using Content.Server.Shuttles.Components;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Physics;
using Content.Shared.Roles;
using Content.Server.Administration.Commands;
using Linguini.Shared.Types.Bundle;

namespace Content.Server._Crescent.AbyssalDisks;
public sealed class AbyssalDiskSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private EntityQuery<MapGridComponent> _gridQuery;
    private EntityQuery<PhysicsComponent> _physicsQuery;
    private EntityQuery<TransformComponent> _xformQuery;
    private List<Entity<MapGridComponent>> _grids = new();
    public override void Initialize()
    {
        base.Initialize();
        _gridQuery = GetEntityQuery<MapGridComponent>();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
        _xformQuery = GetEntityQuery<TransformComponent>();
        SubscribeLocalEvent<ItemSlotsComponent, ItemSlotInsertAttemptEvent>(OnDiskInserted);
    }

    private void FTLIntoCoords(EntityUid? ent, EntityCoordinates coords)
    {
        _transform.SetCoordinates((ent, Transform(ent), MetaData(ent)), coords);
    }

    private void OnDiskInserted(EntityUid uid, ItemSlotsComponent component, ref ItemSlotInsertAttemptEvent args)
    {
        if (!TryComp<TransformComponent>(Transform(uid).GridUid, out var shipTransform) || !TryComp<MetaDataComponent>(Transform(uid).GridUid, out var shipMeta))
            return;
        var shipUid = shipTransform.GridUid;
        if (shipUid is null)
            return;
        if (TryComp<FTLComponent>(uid, out var FTLComp)) { FTLComp.State = FTLState.Arriving; }
        if (TryComp<ShuttleComponent>(Transform(uid).GridUid, out var shuttleComp))
        {
            if (args.Item is { Valid: true } disk)
            {
                if (TryComp<AbyssalDiskComponent>(disk, out var aDiskComp))
                    if (aDiskComp.Enabled == false)
                        return;
            }
            else
                return;

            var targetCoordinates = Transform(uid).Coordinates;
            var query = EntityQueryEnumerator<AbyssalFTLDestinationComponent, TransformComponent>();
            while (query.MoveNext(out var tUid, out var tComp, out var xForm))
            {
                if (TryComp<TransformComponent>(Transform(uid).GridUid, out var entXform))
                {
                    return;
                }
                targetCoordinates = xForm.Coordinates;


                if (tComp.Enabled == false && shipUid is not null)
                {
                    FTLIntoCoords(shipTransform.GridUid, targetCoordinates);
                }
                    
            }
            Console.WriteLine($"can FTL: {_shuttle.CanFTL}");
            if (targetCoordinates == Transform(uid).Coordinates)
                return;
            else
                _shuttle.FTLToCoordinates(uid, shuttleComp, targetCoordinates, Transform(uid).LocalRotation, 5.5f, 20f);


            Console.WriteLine($"target position: {targetCoordinates}");
            Console.WriteLine($"shuttle position: {Transform(uid).Coordinates}");

            if (_itemSlots.TryGetSlot(uid, SharedShuttleConsoleComponent.DiskSlotName, out var itemSlot) && itemSlot.HasItem) { EntityManager.DeleteEntity(itemSlot.Item); }
        }
    }
}
