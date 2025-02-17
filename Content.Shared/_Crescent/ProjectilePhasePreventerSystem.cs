using System.Linq;
using System.Numerics;
using Content.Shared.Projectiles;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._Crescent;

/// <summary>
/// This handles...
///
/// </summary>
[RegisterComponent]
public sealed partial class ProjectilePhasePreventComponent : Component
{

}
public sealed class ProjectilePhasePreventerSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _phys = default!;
    [Dependency] private readonly SharedTransformSystem _trans = default!;
    /// <inheritdoc/>
    public override void Initialize()

    {
        SubscribeLocalEvent<ProjectilePhasePreventComponent, MoveEvent>(OnMove);
    }
    private void OnMove(EntityUid uid, ProjectilePhasePreventComponent comp, ref MoveEvent args)
    {
        if (!TryComp<PhysicsComponent>(uid, out var physComp))
            return;
        if (!TryComp<FixturesComponent>(uid, out var fixtComp))
            return;
        if (!TryComp<ProjectileComponent>(uid, out var projComp))
            return;
        if (TerminatingOrDeleted(uid))
            return;
        var s = _trans.ToMapCoordinates(args.OldPosition);
        if (s == MapCoordinates.Nullspace)
            return;
        var e = _trans.ToMapCoordinates(args.NewPosition);
        if (e == MapCoordinates.Nullspace)
            return;
        var map = _trans.GetMapId(args.OldPosition);
        var start = s.Position;
        var end = e.Position;
        var angle = (end - start);
        CollisionRay ray = new CollisionRay(start, angle, physComp.CollisionMask);
        foreach (var obj in _phys.IntersectRay(map, ray, angle.Length(), uid, false))
        {
            if (obj.HitEntity == projComp.Shooter)
                continue;
            if (TerminatingOrDeleted(obj.HitEntity))
                continue;
            if (!TryComp<PhysicsComponent>(obj.HitEntity, out var targPhysComp))
                continue;
            if (!TryComp<FixturesComponent>(obj.HitEntity, out var targFixtComp))
                continue;
            var ev = new StartCollideEvent(uid, obj.HitEntity, fixtComp.Fixtures.Keys.First(),
                targFixtComp.Fixtures.Keys.First(), fixtComp.Fixtures.Values.First(),
                targFixtComp.Fixtures.Values.First(), physComp, targPhysComp, obj.HitPos);
            RaiseLocalEvent(uid, ref ev);
        }
        
    }
}
