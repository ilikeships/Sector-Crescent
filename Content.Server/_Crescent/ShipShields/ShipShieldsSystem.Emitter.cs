using Content.Shared._Crescent.ShipShields;
using Content.Server.Power.Components;
using Content.Shared.Projectiles;
using Robust.Shared.Physics.Components;
using Content.Server.Emp;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Station.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;

namespace Content.Server._Crescent.ShipShields;
public partial class ShipShieldsSystem
{
    private const float MAX_EMP_DAMAGE = 10000f;
    [Dependency] private readonly TriggerSystem _trigger = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
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

        var filter = _station.GetInOwningStation(uid);

        if (args.Powered)
        {
            var shield = ShieldEntity(parent.Value, source: uid);
            if (shield != EntityUid.Invalid)
            {
                component.Shield = shield;
                component.Shielded = parent.Value;
            }

            _audio.PlayGlobal(component.PowerUpSound, filter, true, component.PowerUpSound.Params);
        }
        else
        {
            UnshieldEntity(parent.Value);
            component.Shield = null;
            component.Shielded = null;

            _audio.PlayGlobal(component.PowerDownSound, filter, true, component.PowerUpSound.Params);
        }
    }

    private void OnShieldDeflected(EntityUid uid, ShipShieldEmitterComponent component, ShieldDeflectedEvent args)
    {
        if (TryComp<EmpOnTriggerComponent>(args.Deflected, out var emp))
        {
            component.Damage += Math.Clamp(emp.EnergyConsumption, 0f, MAX_EMP_DAMAGE);
            _trigger.Trigger(args.Deflected);
            QueueDel(args.Deflected);
            return;
        }

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
