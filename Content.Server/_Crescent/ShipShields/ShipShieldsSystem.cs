using System.Numerics;
using Content.Shared._Crescent.ShipShields;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;

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

        InitializeCommands();
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

        _fixtureSystem.TryCreateFixture(shield, chain, "shield");

        _physicsSystem.WakeBody(shield, body: shieldPhysics);

        return shield;
    }
}
