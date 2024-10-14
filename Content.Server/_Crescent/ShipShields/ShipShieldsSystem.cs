using System.Numerics;
using Content.Shared._Crescent.ShipShields;
using Robust.Shared.Physics.Systems;
using System.Numerics;
using Content.Shared.Movement.Components;
using Content.Shared.Physics;
using Content.Shared.Salvage;
using Robust.Shared.Map;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Server.GameObjects;
using System.Linq;


namespace Content.Server._Crescent.ShipShields;
public sealed partial class ShipShieldsSystem : EntitySystem
{
    public const string ShipShieldPrototype = "ShipShield";

    [Dependency]
    private readonly SharedTransformSystem _transformSystem = default!;

    [Dependency]
    private readonly FixtureSystem _fixtureSystem = default!;

    [Dependency]
    private readonly PhysicsSystem _physicsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeCommands();
    }

    public EntityUid ShieldEntity(EntityUid entity)
    {
        var shield = Spawn(ShipShieldPrototype, Transform(entity).Coordinates);
        var shieldPhysics = AddComp<PhysicsComponent>(shield);

        _transformSystem.SetParent(shield, entity);

        var chain = new ChainShape();

        chain.CreateLoop(Vector2.Zero, 1f);

        for (int i = 0; i < chain.Vertices.Length; i++)
        {
            chain.Vertices[i].X *= 2;
        }

        _fixtureSystem.TryCreateFixture(shield, chain, "shield");

        _physicsSystem.WakeBody(shield, body: shieldPhysics);

        return shield;
    }
}
