using Content.Shared.ArachnidChaos;
using Content.Shared.Armor;
using Content.Shared.Chasm;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Robust.Shared.Serialization;

namespace Content.Shared._Crescent.DegradeableArmor;

[Serializable, NetSerializable]
public enum ArmorRepairMaterial
{
    PlasteelPlate = 1 << 0,
    NTPolymer = 1 << 1,
    CeramicPlate = 1 << 2,
    SteelPlate = 1 << 3,
    DuraThread = 1 << 4,
    PlasmaGlass = 1 << 5,
    Plastic = 1 << 6,
    HomelandAlloy = 1 << 7,
    Kevlar = 1 << 8,
    PlasteelEncasedKevlar = 1 << 9,
    NTCeramic = 1 << 10

}
[Serializable, NetSerializable]
public partial class ArmorRepairDoAfterEvent : SimpleDoAfterEvent
{
    
}
/// <summary>
/// This handles...
/// </summary>
public sealed class DegradeableArmorSystem : EntitySystem
{
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedDoAfterSystem _doing = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<DegradeableArmorComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<DegradeableArmorComponent, InventoryRelayedEvent<DamageModifyEvent>>(OnDamageModify);
        SubscribeLocalEvent<ArmorRepairKitComponent, AfterInteractEvent>(OnArmorKitUse);
        SubscribeLocalEvent<ArmorRepairKitComponent, ArmorRepairDoAfterEvent>(OnArmorDoAfter);
    }

    private void OnArmorDoAfter(EntityUid uid, ArmorRepairKitComponent component, ref ArmorRepairDoAfterEvent args)
    {
        Logger.Error($"Target  is {args.Target}");
        if (args.Cancelled || args.Handled)
            return;
        Logger.Error($"Target  is {args.Target}");
        if (!TryComp<DegradeableArmorComponent>(args.Target, out var targetComponent))
            return;
        if (targetComponent.armorHealth >= targetComponent.armorMaxHealth)
        {
            _popup.PopupClient("This is already in pristine condition!", args.User, args.User, PopupType.Medium);
            return;
        }

        if (_inventory.TryGetContainingSlot(args.Target.Value, out var def))
        {
            _popup.PopupClient("You can't repair this piece whilst wearing it!", args.User, args.User, PopupType.Medium);
            return;
        }
        var usedArmor = Math.Min(component.repairHealth, targetComponent.armorMaxHealth - targetComponent.armorHealth);
        component.repairHealth -= usedArmor;
        if (component.repairHealth <= 0)
        {
            QueueDel(uid);
        }

        targetComponent.armorHealth += usedArmor;
        _popup.PopupClient(
            $"You use the armor kit. The armor on the target is now at {100 * (int) targetComponent.armorHealth / targetComponent.armorMaxHealth}% health",
            args.User, args.User, PopupType.Medium);
    }
    private void OnArmorKitUse(EntityUid uid, ArmorRepairKitComponent component, ref AfterInteractEvent args)
    {
        if (!TryComp<DegradeableArmorComponent>(args.Target, out var targetComponent))
            return;
        if (targetComponent.armorRepair != component.materialType)
        {
            _popup.PopupClient("You can't use this material to repair this!", args.User, args.User, PopupType.Medium);
            return;
        }

        if (targetComponent.armorHealth >= targetComponent.armorMaxHealth)
        {
            _popup.PopupClient("This is already in pristine condition!", args.User, args.User, PopupType.Medium);
            return;
        }

        if (_inventory.TryGetContainingSlot(args.Target.Value, out var def))
        {
            _popup.PopupClient("You can't repair this piece whilst wearing it!",args.User,args.User, PopupType.Medium);
            return;
        }
        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, 5f, new ArmorRepairDoAfterEvent(), uid, target: args.Target, used: uid)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            BlockDuplicate = true
        };
        _doing.TryStartDoAfter(doAfterEventArgs);


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
