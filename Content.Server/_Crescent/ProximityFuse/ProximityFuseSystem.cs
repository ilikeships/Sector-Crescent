using Content.Server.Explosion.Components;
using Robust.Shared.Map.Components;
using Content.Server.Explosion.EntitySystems;
using System.Numerics;
using Robust.Shared.Random;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Projectiles;
using Content.Shared.Tag;
using Content.Server.Shuttles.Components;


namespace Content.Server._Crescent.ProximityFuse;

public sealed class ProximityFuseSystem : EntitySystem
{
    float safety = 0f;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        
        var query = EntityQueryEnumerator<ProximityFuseComponent, TransformComponent>(); // get all proximity fuse components
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (TryComp<ProjectileComponent>(uid, out var projectile) && TryComp<GunComponent>(projectile.Shooter, out var shooterGunComp) && TryComp<TransformComponent>(projectile.Shooter, out var shooterTransform))
            {
                safety += frameTime;
                float distance = float.MaxValue;
                float closestDistance = float.MaxValue;
                var shipQuery = EntityQueryEnumerator<ShuttleComponent, TransformComponent>();
                while (shipQuery.MoveNext(out var tUid, out var tComp, out var tXform))
                {
                    if (shooterTransform.GridUid == uid) { return; }

                    distance = Vector2.Distance(
                        _transform.ToMapCoordinates(xform.Coordinates).Position,
                        _transform.ToMapCoordinates(tXform.Coordinates).Position
                    );
                    if (distance < closestDistance)
                        closestDistance = distance;
                }
                if (safety >= comp.SafetyTime)
                {
                    if (closestDistance <= comp.MaxRange)
                        comp.Fuse -= frameTime;
                    else
                        comp.Fuse = (comp.MaxRange / shooterGunComp.ProjectileSpeed) * _random.NextFloat(0.5f, 1.5f);
                    if (closestDistance <= comp.MinRange)
                        Detonate(uid);

                    if (comp.Fuse <= 0f)
                        Detonate(uid);
                }
            }
        }
    }
    public void Detonate(EntityUid uid)
    {
        if (TryComp<ExplosiveComponent>(uid, out var explosiveComp))
            _entMan.System<ExplosionSystem>().TriggerExplosive(uid);
        else
            _entMan.DeleteEntity(uid);
    }
}
