namespace Content.Shared._Crescent.ShipShields;

[RegisterComponent]
public sealed partial class ShipShieldEmitterComponent : Component
{
    public EntityUid? Shield;
    public EntityUid? Shielded;

    [DataField]
    public float Accumulator;

    [DataField]
    public float Damage = 0f;

    [DataField]
    public float DamageExp = 1.2f;

    [DataField]
    public float HealPerSecond = 200f;

    [DataField]
    public float UnpoweredBonus = 2f;

    [DataField]
    public float BaseDraw = 50000f;
}
