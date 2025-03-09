using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Crescent.DegradeableArmor;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DegradeableArmorComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float armorDegradationCoefficient = 1;
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float armorMaxHealth = 500;
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float armorHealth = 500;
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ArmorDegradation armorType = ArmorDegradation.Plastic;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public DamageModifierSet initialModifiers = default!;
}
[Serializable, NetSerializable]
public enum ArmorDegradation
{
    Ceramic = 1, // blocks damage but decay is exponential to the damage. 
    Metallic = 1<<1, // degradation is reduced for stuff that is far too weak, degradation is calculated after damage is adjusted. Linear scaling
    Plastic = 1<<2, // always degradates by the total damage amount BEFORE reductions are applied. Linear scaling
}
