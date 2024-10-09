using System.Diagnostics.CodeAnalysis;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Random;
using Robust.Shared.Toolshed.Commands.Math;

namespace Content.Shared._Crescent.Intelligence;

public sealed partial class IntelligenceSystem : EntitySystem
{
    // Yaqubian intelligence
    private const byte Max = byte.MaxValue;

    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IntelligenceThresholdComponent, InteractionAttemptEvent>(OnAttemptInteract);
        SubscribeLocalEvent<IntelligenceThresholdComponent, ShotAttemptedEvent>(OnAttemptShoot);
    }

    /// <summary>
    /// Do stupid shit instead of firing
    /// </summary>
    private void OnAttemptShoot(EntityUid ent, IntelligenceThresholdComponent component, ref ShotAttemptedEvent args)
    {
        var user = args.User;

        // No shooting for you.
        if (!TryGetIntelligence(user, out var intelligence))
        {
            args.Cancel();
            return;
        }

        var canUse = IqInteractionCheck(intelligence.Value, component, out _);

        if (canUse)
            return;

        // Evil ass bitwise trick - if AvailableModes AND AvailableModes-1 is non-zero - it has 2 or mode fire modes.
        // This works because subtracting 1 flips all the bits after the rightmost set one.
        // So if there's more than one - AND will be non-zero.
        var hasFireModes = (args.Used.Comp.AvailableModes & (args.Used.Comp.AvailableModes - 1)) != 0;
        // Check if we have a slot
        _slots.TryGetSlot(ent, "magazine", out var slot);
        var hasMag = _slots.GetItemOrNull(ent, "magazine") is not null;


        // Check if this has a bolt
        if (TryComp<ChamberMagazineAmmoProviderComponent>(ent, out var chamberAmmoProviderComp)
            && RollIntelligenceProbability(intelligence.Value, true))
        {
            _popup.PopupClient("You pull on the bolt hoping it would slingshot the bullet.", args.User);
            _gun.ToggleBolt(ent, chamberAmmoProviderComp, args.User);
            return;
        }

        if (hasFireModes
            && RollIntelligenceProbability(intelligence.Value, true))
        {
            _popup.PopupClient("You fumble with the fire control switch.", args.User);
            _gun.CycleFire(ent, args.Used.Comp, args.User);
            return;
        }

        if (slot is not null && hasMag
            && RollIntelligenceProbability(intelligence.Value, true))
        {
            _popup.PopupClient("You accidentally press the magazine release button.", args.User);
            _slots.TryEject(ent, slot, args.User, out _);
            return;
        }
    }


    /// <summary>
    /// Roll intelligence check
    /// </summary>
    /// <param name="intelligence"></param>
    /// <param name="inverted">Invert result</param>
    /// <returns>True if probability passed. Yaqubian intelligence always passes this.</returns>
    private bool RollIntelligenceProbability(byte intelligence, bool inverted = false)
    {
        var chance = intelligence / Max;

        var result = _rand.Prob(chance);

        if (inverted)
            return !result;

        return result;
    }

    private bool TryGetIntelligence(EntityUid uid, [NotNullWhen(true)] out byte? intelligence, IntelligenceComponent? comp = null)
    {
        intelligence = null;
        if (!Resolve(uid, ref comp))
            return false;

        intelligence = comp.Intelligence;
        return true;
    }

    private void OnAttemptInteract(EntityUid ent, IntelligenceThresholdComponent component, CancellableEntityEventArgs args)
    {
        if (IqInteractionCheck(ent, component, out var reply))
            return;

        _popup.PopupClient(reply, ent);
        args.Cancel();
    }

    private bool IqInteractionCheck(Entity<IntelligenceComponent?> entity, IntelligenceThresholdComponent threshold,
        [NotNullWhen(false)] out string? reply)
    {
        reply = null;

        if (!Resolve(entity.Owner, ref entity.Comp))
        {
            reply = "You appear to have no intelligence.";
            return false;
        }

        return IqInteractionCheck(entity.Comp.Intelligence, threshold, out reply);
    }

    private bool IqInteractionCheck(byte entityIntel, IntelligenceThresholdComponent threshold,
        [NotNullWhen(false)] out string? reply)
    {
        reply = null;

        if (entityIntel < threshold.Required && threshold.Inverted == false)
        {
            reply = "You appear to be lacking intelligence to use this.";
            return false;
        }

        if (threshold.Inverted && entityIntel > threshold.Required)
        {
            reply = "After thinking about it, you still don't really understand how this works.";
            return false;
        }

        return true;
    }
}
