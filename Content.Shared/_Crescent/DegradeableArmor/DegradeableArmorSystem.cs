using Content.Shared.Armor;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;

namespace Content.Shared._Crescent.DegradeableArmor;

/// <summary>
/// This handles...
/// </summary>
public sealed class DegradeableArmorSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<DegradeableArmorComponent, InventoryRelayedEvent<DamageModifyEvent>>(OnDamageModify);
    }

    private void OnDamageModify(EntityUid uid, DegradeableArmorComponent component, InventoryRelayedEvent<DamageModifyEvent> args)
    {
        if (component.armorHealth == 0)
            return;
        DamageSpecifier adjustedDamage = new();
        adjustedDamage.DamageDict.EnsureCapacity(args.Args.Damage.DamageDict.Count);
        var armorDamage = 0f;



        foreach (var (type, value) in args.Args.Damage.DamageDict)
        {
            if (!adjustedDamage.DamageDict.TryGetValue(type, out var _))
                continue;
            var trueReduction = component.InitialModifiers.FlatReduction[type];
            var adjustedValue = value;
            if (trueReduction == 0)
                continue;
            switch (component.armorType)
            {
                case ArmorDegradation.Ceramic:
                {
                    trueReduction = component.armorHealth / component.armorMaxHealth;
                    trueReduction *= trueReduction;
                    armorDamage += (float) value * (float) value / (6f + 1f/Math.Min(0.10f,1f - trueReduction));
                    adjustedValue -= FixedPoint2.Abs(trueReduction * adjustedValue);
                    break;
                }
                case ArmorDegradation.Metallic:
                {
                    trueReduction = component.armorHealth / component.armorMaxHealth;
                    armorDamage += (float) value * component.armorHealth / (component.armorHealth + component.armorMaxHealth) - trueReduction / 2f * (float)value;
                    adjustedValue -= FixedPoint2.Abs(trueReduction * adjustedValue);
                    break;
                }
                case ArmorDegradation.Plastic:
                {
                    trueReduction = (component.armorHealth + (float) value*2) / component.armorMaxHealth;
                    armorDamage += trueReduction * (float) value * 1.1f;
                    adjustedValue -= FixedPoint2.Abs(trueReduction * value);
                    break;
                }
            }

            adjustedDamage.DamageDict[type] = adjustedValue;

        }

        component.armorHealth = Math.Max(0, component.armorHealth - armorDamage);
        args.Args.Damage = adjustedDamage;
    }
}
