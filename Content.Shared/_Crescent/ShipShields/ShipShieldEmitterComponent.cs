namespace Content.Shared._Crescent.ShipShields;

[RegisterComponent]
public sealed partial class ShipShieldEmitterComponent : Component
{
    public EntityUid? Shield;
    public EntityUid? Shielded;
}
