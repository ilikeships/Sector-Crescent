using System.Numerics;
using Content.Shared._Crescent.ShipShields;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Events;
using Content.Shared.Physics;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged;
using Robust.Shared.Spawners;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Server.GameStates;

namespace Content.Server._Crescent.ShipShields;
public sealed partial class ShipShieldsSystem : EntitySystem
{
    private const string ShipShieldPrototype = "ShipShield";
    private const float Padding = 6f;

    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    [Dependency] private readonly FixtureSystem _fixtureSystem = default!;

    [Dependency] private readonly PhysicsSystem _physicsSystem = default!;

    [Dependency] private readonly SharedGunSystem _gun = default!;

    [Dependency] private readonly PvsOverrideSystem _pvsSys = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShipShieldComponent, StartCollideEvent>(OnCollide);

        InitializeCommands();
    }

    private void OnCollide(EntityUid uid, ShipShieldComponent component, StartCollideEvent args)
    {
        if (Transform(args.OtherEntity).Anchored)
            return;

        if (!TryComp<PhysicsComponent>(Transform(uid).GridUid, out var ourPhysics) || !TryComp<PhysicsComponent>(args.OtherEntity, out var theirPhysics))
            return;

        var ourVelocity = ourPhysics.LinearVelocity;
        var velocity = theirPhysics.LinearVelocity;

        Logger.Error("Our velocity: " + ourVelocity);
        Logger.Error("Their velocity: " + velocity);

        var collisionSpeedVector = Vector2.Subtract(ourVelocity, velocity);

        Logger.Error("Registering collision with " + args.OtherEntity + " at speed " + collisionSpeedVector);

        if (Math.Abs(collisionSpeedVector.Length()) < 20)
            return;

        Logger.Error("Fast enough for me!");

        if (TryComp<TimedDespawnComponent>(args.OtherEntity, out var despawn))
            despawn.Lifetime += despawn.Lifetime;

        _gun.ShootProjectile(args.OtherEntity, -velocity, _physicsSystem.GetMapLinearVelocity(uid), uid, null, velocity.Length());
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

        _pvsSys.AddGlobalOverride(shield);

        return shield;
    }
}
