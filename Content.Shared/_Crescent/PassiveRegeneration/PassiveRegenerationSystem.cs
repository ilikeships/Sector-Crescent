using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Prototypes;

namespace Content.Shared._Crescent.PassiveRegeneration;

/// <summary>
/// This handles...
/// </summary>
public sealed class PassiveRegenerationSystem : EntitySystem
{

    [Dependency] private readonly ThirstSystem _thirst = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly PrototypeManager _prototype = default!;

    EntityQuery<PassiveRegenerationComponent> _componentQuery;

    private float accumulator = 0f;
  

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        accumulator += frameTime;
        if (accumulator > 15f)
        {
            var query = EntityQueryEnumerator<PassiveRegenerationComponent>();
            while (query.MoveNext(out var uid, out var regenComp))
            {
                if(!TryComp<DamageableComponent>(uid, out var damageable))
                    continue;
                if(damageable.TotalDamage == 0)
                    continue;
                if(!TryComp<ThirstComponent>(uid, out var thirst) || !TryComp<HungerComponent>(uid, out var hunger));
                    continue;
                    var totalHeal = (thirst.CurrentThirst / thirst.ThirstThresholds[thirst.LastThirstThreshold]);
                    totalHeal *= (hunger.CurrentHunger / hunger.Thresholds[hunger.CurrentThreshold]);
                    totalHeal *= 4f;
                    foreach(var (damageType, damageAmount) in damageable.Damage.GetDamagePerGroup(_prototype))
                    {
                        
                    }

            }
        }
    }
}
