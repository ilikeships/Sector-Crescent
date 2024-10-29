using Content.Shared._Crescent.ShipShields;
using Content.Server.Power.Components;
using Content.Shared.Projectiles;
using Robust.Shared.Physics.Components;

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
        if (TryComp<ProjectileComponent>(args.Deflected, out var proj))
            component.Damage += (float) proj.Damage.GetTotal();
        else if (TryComp<PhysicsComponent>(args.Deflected, out var phys))
            component.Damage += phys.FixturesMass;
    }

    private void AdjustEmitterLoad(EntityUid uid, ShipShieldEmitterComponent? emitter = null, ApcPowerReceiverComponent? receiver = null)
    {
        if (!Resolve(uid, ref emitter, ref receiver))
            return;

        /// Raise damage to the power of the growth exponent
        var additionalLoad = (float) Math.Pow(emitter.Damage, emitter.DamageExp);

        receiver.Load = emitter.BaseDraw + additionalLoad;
    }
}
