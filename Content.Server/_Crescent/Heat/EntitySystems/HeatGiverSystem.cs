namespace Content.Server._Crescent.Heat.EntitySystems;

/// <summary>
/// This handles...
/// </summary>
public sealed class HeatGiverSystem : EntitySystem
{
    private EntityQuery<HeatGiverComponent> _cQuery;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        _cQuery = EntityManager.GetEntityQuery<HeatGiverComponent>();

    }

    public override void Update(float frameTime)
    {

    }
}
