using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Containers;

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
    public float HealPerSecond = 300f;

    [DataField]
    public float UnpoweredBonus = 2f;

    [DataField]
    public float BaseDraw = 60000f;

    /// <summary>
    /// On power up, players for all on vessel, pitched down.
    /// </summary>
    [DataField]
    public SoundSpecifier PowerUpSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");

    [DataField]
    public SoundSpecifier PowerDownSound = new SoundPathSpecifier("/Audio/Effects/teleport_departure.ogg");
}
