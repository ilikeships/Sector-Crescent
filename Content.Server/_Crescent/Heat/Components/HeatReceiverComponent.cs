namespace Content.Server._Crescent.Heat;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class HeatReceiverComponent : Component
{
    public HeatGiverComponent From;

    public HeatReceiverComponent(HeatGiverComponent from)
    {
        From = from;
    }
}
