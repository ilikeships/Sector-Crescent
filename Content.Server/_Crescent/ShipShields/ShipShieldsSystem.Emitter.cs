using Content.Shared._Crescent.ShipShields;
using Content.Server.Power.Components;
using Content.Shared.Projectiles;

namespace Content.Server._Crescent.ShipShields;
public partial class ShipShieldsSystem
{

    public void InitializeEmitters()
    {
        SubscribeLocalEvent<ShipShieldEmitterComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<ShipShieldEmitterComponent, ShieldDeflectedEvent>(OnShieldDeflected);
    }

    private void OnPowerChanged(EntityUid uid, ShipShieldEmitterComponent component, PowerChangedEvent args)
    {
        var parent = Transform(uid).GridUid;

        if (parent == null)
            return;

        if (args.Powered)
        {
            var shield = ShieldEntity(parent.Value, source: uid);
            if (shield != EntityUid.Invalid)
            {
                component.Shield = shield;
                component.Shielded = parent.Value;
            }
        }
        else
        {
            UnshieldEntity(parent.Value);
            component.Shield = null;
            component.Shielded = null;
        }
    }

    private void OnShieldDeflected(EntityUid uid, ShipShieldEmitterComponent component, ShieldDeflectedEvent args)
    {
        if (!TryComp<ProjectileComponent>(args.Deflected, out var proj))
            return;

        component.Damage += (float) proj.Damage.GetTotal();
    }
}
