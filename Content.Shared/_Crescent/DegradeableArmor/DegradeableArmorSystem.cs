using Content.Shared.Armor;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;

namespace Content.Shared._Crescent.DegradeableArmor;

/// <summary>
/// This handles...
/// </summary>
public sealed class DegradeableArmorSystem : EntitySystem
{
    [Dependency] private readonly StaminaSystem _stamina = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<DegradeableArmorComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<DegradeableArmorComponent, InventoryRelayedEvent<DamageModifyEvent>>(OnDamageModify);
    }

    private void OnInit(EntityUid uid, DegradeableArmorComponent component, ref MapInitEvent args)
    {
        if(component.armorHealth == 0)
            component.armorHealth = component.armorMaxHealth;
    }
    private void OnDamageModify(EntityUid uid, DegradeableArmorComponent component, InventoryRelayedEvent<DamageModifyEvent> args)
    {
        if (component.armorHealth == 0)
            return;
        //Logger.Error("-----------------------------------");
        var armorDamage = 0f;


        var damageDictionary = args.Args.Damage.DamageDict;
        foreach (var (type, value) in damageDictionary)
        {
            if (!component.initialModifiers.FlatReduction.ContainsKey(type))
                continue;
            
            var trueReduction = component.initialModifiers.FlatReduction[type];
            if (trueReduction == 0)
                continue;
            switch (component.armorType)
            {
                case ArmorDegradation.Ceramic:
                {
                    trueReduction *= component.armorHealth / component.armorMaxHealth;
                    trueReduction *= component.armorHealth / component.armorMaxHealth;
                    
                    break;
                }
                case ArmorDegradation.Metallic:
                {
                    trueReduction *= component.armorHealth / component.armorMaxHealth;
                    break;
                }
                case ArmorDegradation.Plastic:
                {
                    trueReduction *= (component.armorHealth + (float) value*2) / component.armorMaxHealth;
                    break;
                }
            }

            trueReduction = Math.Clamp(trueReduction, 0, component.maxBlockCoefficients[type] * component.initialModifiers.FlatReduction[type]);
            _stamina.TakeStaminaDamage(uid, trueReduction * component.staminaConversions[type]);
            armorDamage += trueReduction * args.Args.armorDamageMultiplier * component.armorDamageCoefficients[type]; 
            //Logger.Error(
            //    $"Damage adjusted for type {type}, old {value} , new {Math.Max(0f, (float) value - trueReduction)}. Armor damage {armorDamage}. Armor Health {component.armorHealth}. Stamina damage {trueReduction * component.staminaConversions[type]}");
            damageDictionary[type] = Math.Max(0f, (float) value - trueReduction);

        }

        component.armorHealth = Math.Max(0, component.armorHealth - armorDamage);
        Dirty(uid, component);
    }

}
