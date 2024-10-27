using Content.Server.NPC.Components;

namespace Content.Server._Crescent.Heat.EntitySystems;

/// <summary>
/// This handles...
/// </summary>
public sealed class HeatGiverSystem : EntitySystem
{
    private EntityQuery<HeatGiverComponent> _gQuery;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _gQuery = GetEntityQuery<HeatGiverComponent>();

        SubscribeLocalEvent<HeatStorageComponent, ComponentRemove>(checkGivers);
        SubscribeLocalEvent<HeatGiverComponent, ComponentStartup>(prepareReceiver);

    }

    private void prepareReceiver(EntityUid uid, HeatGiverComponent comp, ref ComponentStartup args)
    {
        HeatReceiverComponent heatRev = new HeatReceiverComponent(comp);
        EntityUid targUid = EntityManager.ComponentO
        EntityManager.AddComponent<HeatReceiverComponent>(comp.tak)
    }

    private void checkGivers(EntityUid uid, HeatStorageComponent comp, ref ComponentRemove args)
    {
        if (!_gQuery.HasComponent(uid))
            return;
        HeatGiverComponent giverComp = _gQuery.GetComponent(uid);
        EntityManager.RemoveComponent(uid, giverComp);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<HeatGiverComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            var transferCoeff = Math.Pow(comp.giver.TransferRate * (comp.taker.TransferRate / 3 + 0.7), comp.TransferExponentialMultiplier);
            var transferring = Math.Clamp(transferCoeff * comp.MedianTransferRate, comp.giver.curHeat, Math.Min(comp.TransferCap, comp.taker.maxHeat - comp.taker.curHeat)) * frameTime;
            comp.giver.curHeat -= (float) transferring;
            comp.taker.curHeat += (float) transferring
        }
    }
}
