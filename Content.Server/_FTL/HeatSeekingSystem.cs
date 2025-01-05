using System.Linq;
using System.Numerics;
using Content.Shared.Interaction;
using Content.Shared.Physics;
using Content.Shared.Projectiles;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Random;

namespace Content.Server._FTL.HeatSeeking;

/// <summary>
/// This handles...
/// </summary>
public sealed class HeatSeekingSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly RotateToFaceSystem _rotate = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HeatSeekingComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (comp.TargetEntity is not null)
            {
                var entXform = Transform(comp.TargetEntity.Value);
                var originalAngle = _transform.GetWorldRotation(xform);
                var angle = (
                    _transform.ToMapCoordinates(entXform.Coordinates).Position -
                    _transform.ToMapCoordinates(xform.Coordinates).Position
                ).ToWorldAngle();
                var trueRotationSpeed = comp.RotationSpeed;
                if(trueRotationSpeed is null)
                    trueRotationSpeed = 999;
                trueRotationSpeed *= frameTime;

                if (angle > originalAngle + trueRotationSpeed.Value)
                {
                    angle = originalAngle + trueRotationSpeed.Value;
                }
                else if (angle < originalAngle - trueRotationSpeed.Value)
                {
                    angle = originalAngle - trueRotationSpeed.Value;
                }

                _transform.SetLocalRotationNoLerp(uid, angle, xform);

                _rotate.TryRotateTo(uid, angle, frameTime, comp.WeaponArc, comp.RotationSpeed?.Theta ?? double.MaxValue,
                    xform);
                _physics.SetLinearVelocity(uid, angle.ToWorldVec() * comp.Acceleration);
                //_physics.ApplyForce(uid, xform.LocalRotation.RotateVec(new Vector2(0, 1)) * comp.Acceleration);
            }
            else
                GetNewTarget(uid, comp, xform);
            
        }
    }

    public void GetNewTarget(EntityUid uid, HeatSeekingComponent component, TransformComponent transform)
    {
        var ray = new CollisionRay(_transform.GetMapCoordinates(uid, transform).Position,
            transform.LocalRotation.ToWorldVec(),
            (int) (CollisionGroup.Impassable | CollisionGroup.BulletImpassable));

        var results = _physics.IntersectRay(transform.MapID, ray, component.DefaultSeekingRange, uid).ToList();

        if (results.Count <= 0)
            return; // nothing to heatseek ykwim

        if (component is { LockedIn: true, TargetEntity: not null })
            return; // Don't reassign target entity if we have one AND we have the LockedIn property

        if (TryComp<ProjectileComponent>(uid, out var projectile)
            && TryComp<TransformComponent>(projectile.Shooter, out var shooterTransform))
        {
            var shooterGridUid = shooterTransform.GridUid;
            for (int i = 0; i < results.Count; i++)
            {
                var hitEntity = results[i].HitEntity;
                if (TryComp<TransformComponent>(hitEntity, out var hitTransform))
                {
                    if (shooterGridUid == hitTransform.GridUid)
                    {
                        continue;
                    }

                    component.TargetEntity = hitEntity;
                    break;
                }
            }
        }
        else
        {
            component.TargetEntity = results[0].HitEntity;
        }
    }
}
