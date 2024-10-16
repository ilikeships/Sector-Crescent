using Content.Shared._Crescent.ShipShields;
using Robust.Client.ResourceManagement;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Physics;
using Content.Shared.Books;
using Robust.Shared.Physics.Collision.Shapes;
using System.Numerics;
using Content.Client.Resources;

namespace Content.Client._Crescent.ShipShields;

public sealed class ShipShieldOverlay : Overlay
{
    private readonly IResourceCache _resourceCache;
    private readonly IEntityManager _entManager;
    private readonly SharedTransformSystem _transform;
    private readonly FixtureSystem _fixture;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public ShipShieldOverlay(IEntityManager entityManager, IResourceCache resourceCache)
    {
        _resourceCache = resourceCache;
        _entManager = entityManager;
        _transform = _entManager.EntitySysManager.GetEntitySystem<SharedTransformSystem>();
        _fixture = _entManager.EntitySysManager.GetEntitySystem<FixtureSystem>();

        ZIndex = 8;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;

        var enumerator = _entManager.AllEntityQueryEnumerator<ShipShieldVisualsComponent, FixturesComponent, TransformComponent>();
        while (enumerator.MoveNext(out var uid, out var visuals, out var fixtures, out var xform))
        {
            // VANTABLACK OVERDRAW FROM THE DEPTHS OF LAGHELL
            if (xform.MapID != args.MapId)
                continue;

            // TODO: We can probably at least test its parent grid is in PVS range...?

            var fixture = _fixture.GetFixtureOrNull(uid, "shield", fixtures);

            if (fixture == null || fixture.Shape is not ChainShape)
                continue;

            var chain = (ChainShape) fixture.Shape;

            var (_, _, matrix) = _transform.GetWorldPositionRotationMatrix(uid);

            var texture = _resourceCache.GetTexture("/Textures/_Crescent/ShipShields/shieldtex.png");

            DrawShield(handle, chain, xform, matrix, texture);
        }
    }

    private void DrawShield(DrawingHandleWorld handle, ChainShape chain, TransformComponent xform, Matrix3x2 matrix, Texture tex)
    {
        List<DrawVertexUV2D> verts = new List<DrawVertexUV2D>();

        // The vertices of this fixture are defined relative to local position,
        // so we'll have to add them to this and then use the matrix to put them back in world position.
        var localPos = xform.LocalPosition;

        for (int i = 1; i <= chain.Count; i++)
        {
            // top left corner
            var leftVertex = VertexToWorldPos(localPos, chain.Vertices[i - 1], matrix);

            // top right corner
            var rightVertex = VertexToWorldPos(localPos, chain.Vertices[i], matrix);

            // bottom left corner
            var leftCorner = Corner(localPos, leftVertex);

            // bottom right corner
            var rightCorner = Corner(localPos, rightVertex);

            // Assemble 2 triangles.

            // Triangle one: top left, top right, bottom left
            verts.Add(new DrawVertexUV2D(leftVertex, new Vector2(0, 1)));
            verts.Add(new DrawVertexUV2D(rightVertex, new Vector2(1, 1)));
            verts.Add(new DrawVertexUV2D(leftCorner, Vector2.Zero));

            // Triangle two: top right, bottom left, bottom right
            verts.Add(new DrawVertexUV2D(rightVertex, new Vector2(1, 1)));
            verts.Add(new DrawVertexUV2D(leftCorner, Vector2.Zero));
            verts.Add(new DrawVertexUV2D(rightCorner, new Vector2(1, 0)));
        }

        handle.DrawPrimitives(DrawPrimitiveTopology.TriangleList, texture: tex, verts.ToArray().AsSpan(), Color.White);
    }

    private Vector2 VertexToWorldPos(Vector2 localPos, Vector2 vertexPos, Matrix3x2 matrix)
    {
        var vertLocation = Vector2.Add(localPos, vertexPos);
        Vector2.Transform(vertLocation, matrix);

        return vertLocation;
    }

    private Vector2 Corner(Vector2 localPos, Vector2 vertexPos, float radius = 1.3f)
    {
        var cornerPos = Vector2.Subtract(vertexPos, localPos);
        cornerPos.Normalize();
        cornerPos *= radius;

        return Vector2.Subtract(vertexPos, cornerPos);
    }
}
