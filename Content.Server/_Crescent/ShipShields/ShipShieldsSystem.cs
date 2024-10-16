using System.Numerics;
using Content.Shared._Crescent.ShipShields;
using Content.Shared.Physics;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Random;
using Robust.Shared.Spawners;
using Robust.Server.GameStates;

namespace Content.Server._Crescent.ShipShields;
public sealed partial class ShipShieldsSystem : EntitySystem
{
    private const string ShipShieldPrototype = "ShipShield";
    private const float Padding = 10f;
    private const float CollisionThreshold = 20f;
    private const float DeflectionSpread = 25f;

    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    [Dependency] private readonly FixtureSystem _fixtureSystem = default!;

    [Dependency] private readonly PhysicsSystem _physicsSystem = default!;

    [Dependency] private readonly SharedGunSystem _gun = default!;

    [Dependency] private readonly PvsOverrideSystem _pvsSys = default!;
    [Dependency] private readonly IRobustRandom _random = default!;


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

        var collisionSpeedVector = Vector2.Subtract(ourVelocity, velocity);

        if (Math.Abs(collisionSpeedVector.Length()) < CollisionThreshold)
            return;

        if (TryComp<TimedDespawnComponent>(args.OtherEntity, out var despawn))
            despawn.Lifetime += despawn.Lifetime;

        // I originally tried reflection but the math is too hard with the fucked coordinate system in this game (WorldRotation can be negative. Vector to Angle conversion loses information. Etc etc.)
        // Might try again at some point using just vector math with this (https://math.stackexchange.com/questions/13261/how-to-get-a-reflection-vector)
        var deflectionVector = Transform(args.OtherEntity).WorldPosition - Transform(uid).WorldPosition;
        var angle = _random.NextFloat(DeflectionSpread);

        if (_random.Prob(0.5f))
            angle = -angle;

        deflectionVector = new Vector2((float) (Math.Cos(angle) * deflectionVector.X - Math.Sin(angle) * deflectionVector.Y), (float) (Math.Sin(angle) * deflectionVector.X - Math.Cos(angle) * deflectionVector.Y));

        _gun.ShootProjectile(args.OtherEntity, deflectionVector, _physicsSystem.GetMapLinearVelocity(uid), uid, null, velocity.Length());
    }

    private EntityUid ShieldEntity(EntityUid entity, MapGridComponent? mapGrid = null)
    {
        if (!Resolve(entity, ref mapGrid, false))
            return EntityUid.Invalid;

        var prototype = ShipShieldPrototype;

        var shield = Spawn(prototype, Transform(entity).Coordinates);
        var shieldPhysics = AddComp<PhysicsComponent>(shield);

        _transformSystem.SetLocalPosition(shield, mapGrid.LocalAABB.Center);
        _transformSystem.SetParent(shield, entity);

        GenerateOvalFixture(shield, "shield", shieldPhysics, mapGrid);
        GenerateOvalFixture(shield, "inner1", shieldPhysics, mapGrid, Padding - 0.33f);
        GenerateOvalFixture(shield, "inner2", shieldPhysics, mapGrid, Padding - 0.63f);
        GenerateOvalFixture(shield, "inner3", shieldPhysics, mapGrid, Padding - 1f);

        _physicsSystem.WakeBody(shield, body: shieldPhysics);

        _pvsSys.AddGlobalOverride(shield);

        return shield;
    }

    private void GenerateOvalFixture(EntityUid uid, string name, PhysicsComponent physics, MapGridComponent mapGrid, float padding = Padding)
    {
        float radius;
        float scale;
        var scaleX = true;

        var height = mapGrid.LocalAABB.Height + padding;
        var width = mapGrid.LocalAABB.Width + padding;

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

        _fixtureSystem.TryCreateFixture(uid, chain, name,
            hard: false,
            collisionLayer: (int) CollisionGroup.FullTileLayer,
            body: physics);
    }
}
