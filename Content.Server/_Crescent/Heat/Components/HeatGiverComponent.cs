using Content.Shared.Atmos.Piping.Portable.Components;

namespace Content.Server._Crescent.Heat;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class HeatGiverComponent : Component
{
    public HeatStorageComponent giver;

    public HeatStorageComponent taker;

    public EntityUid givingEntity;

    public float TransferExponentialMultiplier = 1f;

    public float TransferCap = 500f;

    public float MedianTransferRate = 100f;

    public HeatGiverComponent(HeatStorageComponent giving, HeatStorageComponent receiving, EntityUid giving, float expMultiplier,
        float TransfCap, float medianTransfer)
    {
        giver = giving;
        taker = receiving;
        givingEntity = giving;

        TransferExponentialMultiplier = expMultiplier;
        TransferCap = TransfCap;
        MedianTransferRate = medianTransfer;
    }
}
