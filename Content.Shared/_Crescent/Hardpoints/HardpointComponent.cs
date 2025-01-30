using Robust.Shared.GameStates;

namespace Content.Shared._Crescent.Hardpoints;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HardpointComponent : Component
{
    public EntityUid? anchoring;
    public int CannonRangeCheckRange = 25;
    public weaponTypes CompatibleTypes = weaponTypes.Ballistic;
    public weaponSizes CompatibleSizes = weaponSizes.Small;
}

[Flags]
public enum weaponTypes
{
    Energy = 1<<1,
    Ballistic = 1<<2,
    Missile = 1<<3,
    Universal = Energy | Ballistic | Missile,

}
public enum weaponSizes
{
    Small = 1,
    Medium = 2,
    Large = 3
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
