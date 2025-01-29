namespace Content.Shared._Crescent.Hardpoints;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class HardpointComponent : Component
{
    public EntityUid? anchoring;
}

public class HardpointCannonAnchoredEvent : EntityEventArgs
{
    public EntityUid cannonUid;
}
