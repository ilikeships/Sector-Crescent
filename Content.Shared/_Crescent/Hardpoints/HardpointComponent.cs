namespace Content.Shared._Crescent.Hardpoints;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class HardpointComponent : Component
{
    public EntityUid? anchoring;
    public int CannonRangeCheckRange = 25;
}

[Flags]
public enum weaponTypes
{
    Small = 1<<0,
    Medium = 1<<1,
    Large = 1<<2,
    Energy = 1<<3,
    Ballistic = 1<<4,
    Missile = 1<<5
}

public class HardpointCannonAnchoredEvent : EntityEventArgs
{
    public EntityUid cannonUid;
    public EntityUid gridUid;
}

public class HardpointCannonDeanchoredEvent : EntityEventArgs
{
    public EntityUid CannonUid;
    public EntityUid gridUid;
}
