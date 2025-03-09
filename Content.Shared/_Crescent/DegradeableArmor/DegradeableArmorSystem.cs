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
        Logger.Error("-----------------------------------");
        if (component.armorHealth == 0)
            return;
        var armorDamage = 0f;


        Dictionary<string, FixedPoint2> adjustedDamage = new();
        adjustedDamage.EnsureCapacity(args.Args.Damage.DamageDict.Count);
        foreach (var (type, value) in args.Args.Damage.DamageDict)
        {
            if (!component.initialModifiers.FlatReduction.ContainsKey(type))
                continue;
            adjustedDamage.Add(type, value );
            
            var trueReduction = component.initialModifiers.FlatReduction[type];
            var adjustedValue = value;
            if (trueReduction == 0)
                continue;
            switch (component.armorType)
            {
                case ArmorDegradation.Ceramic:
                {
                    trueReduction *= component.armorHealth / component.armorMaxHealth;
                    trueReduction *= component.armorHealth / component.armorMaxHealth;
                    armorDamage += (float) value * (float) value / 8f;
                    break;
                }
                case ArmorDegradation.Metallic:
                {
                    trueReduction *= component.armorHealth / component.armorMaxHealth;
                    armorDamage +=  (float) value * (float) value * (float) value * args.Args.armorDamageMultiplier /
                                   ((float)(value) + component.armorMaxHealth);
                    break;
                }
                case ArmorDegradation.Plastic:
                {
                    trueReduction *= (component.armorHealth + (float) value*2) / component.armorMaxHealth;
                    armorDamage += trueReduction * (float) value * 1.1f;
                    break;
                }
            }
            adjustedValue = Math.Max(0f, (float) value - trueReduction);
            Logger.Error(
                $"Damage adjusted for type {type}, old {value} , new {adjustedValue}. Armor damage {armorDamage}. Armor Health {component.armorHealth}");
            adjustedDamage[type] = adjustedValue;

        }

        component.armorHealth = Math.Max(0, component.armorHealth - armorDamage);
        args.Args.Damage.DamageDict = adjustedDamage;
        Dirty(uid, component);
    }
}
