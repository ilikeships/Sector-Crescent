using Content.Shared.Damage;

namespace Content.Shared._Crescent.DegradeableArmor;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class DegradeableArmorComponent : Component
{
    public float armorDegradationCoefficient = 1;
    public float armorMaxHealth = 500;
    public float armorHealth = 500;
    public ArmorDegradation armorType = ArmorDegradation.Plastic;

    [DataField]
    public DamageModifierSet InitialModifiers = default!;
}

public enum ArmorDegradation
{
    Ceramic = 1, // blocks damage but decay is exponential to the damage. 
    Metallic = 1<<1, // degradation is reduced for stuff that is far too weak, degradation is calculated after damage is adjusted. Linear scaling
    Plastic = 1<<2, // always degradates by the total damage amount BEFORE reductions are applied. Linear scaling
}
