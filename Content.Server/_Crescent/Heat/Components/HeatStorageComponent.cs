namespace Content.Server._Crescent.Heat;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class HeatStorageComponent : Component
{
    public float maxHeat = 50000f;

    public float curHeat = 0f;

    public float TransferRate
    {
        get => float.Clamp((curHeat + maxHeat/10) / maxHeat, 0.1f, 1.2f);
    }
}
