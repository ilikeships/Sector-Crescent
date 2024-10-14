using System.Numerics;
using Content.Shared._Crescent.ShipShields;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Events;
using Content.Shared.Physics;
using FastAccessors;
using Robust.Shared.Spawners;
using Content.Shared.Projectiles;

namespace Content.Server._Crescent.ShipShields;
public sealed partial class ShipShieldsSystem : EntitySystem
{
    private const string ShipShieldPrototype = "ShipShield";
    private const float Padding = 6f;

    [Dependency]
    private readonly SharedTransformSystem _transformSystem = default!;

    [Dependency]
    private readonly FixtureSystem _fixtureSystem = default!;

    [Dependency]
    private readonly PhysicsSystem _physicsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShipShieldComponent, StartCollideEvent>(OnCollide);

        InitializeCommands();
    }

    private void OnCollide(EntityUid uid, ShipShieldComponent component, StartCollideEvent args)
    {
        if (TryComp<TimedDespawnComponent>(args.OtherEntity, out var despawn))
            despawn.Lifetime += despawn.Lifetime;

        if (TryComp<ProjectileComponent>(args.OtherEntity, out var projectile))
            projectile.Weapon = uid;

        if (!TryComp<PhysicsComponent>(args.OtherEntity, out var physics))
            return;

        _physicsSystem.SetLinearVelocity(args.OtherEntity, -physics.LinearVelocity);
    }

    public EntityUid ShieldEntity(EntityUid entity, MapGridComponent? mapGrid = null)
    {
        if (!Resolve(entity, ref mapGrid, false))
            return EntityUid.Invalid;

        var shield = Spawn(ShipShieldPrototype, Transform(entity).Coordinates);
        var shieldPhysics = AddComp<PhysicsComponent>(shield);

        _transformSystem.SetLocalPosition(shield, mapGrid.LocalAABB.Center);
        _transformSystem.SetParent(shield, entity);

        var radius = 0f;
        var scale = 1f;
        var scaleX = true;

        var height = mapGrid.LocalAABB.Height + Padding;
        var width = mapGrid.LocalAABB.Width + Padding;

        if (width > height)
        {
            radius = 0.5f * height;
            scale = width / height;
        }
        else
        {
            radius = 0.5f * width;
            scale = height / width;
            scaleX = false;
        }

        var chain = new ChainShape();

        chain.CreateLoop(Vector2.Zero, radius);

        for (int i = 0; i < chain.Vertices.Length; i++)
        {
            if (scaleX)
            {
                chain.Vertices[i].X *= scale;
            }
            else
            {
                chain.Vertices[i].Y *= scale;
            }
        }

        _fixtureSystem.TryCreateFixture(shield, chain, "shield",
            hard: false,
            collisionLayer: (int) CollisionGroup.FullTileLayer,
            body: shieldPhysics);

        _physicsSystem.WakeBody(shield, body: shieldPhysics);

        return shield;
    }
}
