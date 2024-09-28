using Content.Shared.Mobs;
using Content.Shared.Crescent.CCvar;
using Robust.Shared.Timing;
using Robust.Shared.Configuration;
using Content.Shared.Mind.Components;
using Content.Shared.Mind;
using Content.Server.Mind;

namespace Content.Server.Crescent.Respawn;

/// TRIGGER WARNING: NOT REALLY ECS! CARGO CULT RUSTROONS AVERT YOUR GAZE!
/// So, when you die there's a bit of a problem; there's not really a reliable
/// entity to track you by anymore. This means that the only logical solution for
/// tracking your respawn time is to do it by your username/GUID.

public sealed class RespawnTrackerSystem : EntitySystem
{

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;


    /// <summary>
    /// Matches username to death time and respawn time.
    /// </summary>
    public Dictionary<Guid, (TimeSpan, TimeSpan)> RespawnTrackers = new Dictionary<Guid, (TimeSpan, TimeSpan)>();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MindContainerComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<PlayerSessionEntityDeletedEvent>(OnEntityDeleted);
    }

    private void OnMobStateChanged(EntityUid uid, MindContainerComponent component, MobStateChangedEvent args)
    {
        // User ID is stored in the mind, even if they are disconnected or w/e
        TryComp<MindComponent>(component.Mind, out var mind);

        if (mind == null || mind.UserId == null)
            return;

        var guid = (Guid) mind.UserId;

        // erm, you're dead
        if (args.NewMobState == MobState.Dead)
            AddEntry(guid);

        // erm, you're not dead anymore somehow
        if (args.OldMobState == MobState.Dead && args.NewMobState != MobState.Dead)
            RemoveEntry(guid);
    }

    private void OnEntityDeleted(ref PlayerSessionEntityDeletedEvent args)
    {
        AddEntry(args.Guid);
    }

    private void AddEntry(Guid guid)
    {
        Logger.Error("Adding entry for " + guid);
        // delete your old entry if you have one
        RemoveEntry(guid);

        // add new entry
        RespawnTrackers.Add(guid, (_timing.CurTime, _timing.CurTime + TimeSpan.FromSeconds(_cfg.GetCVar(CrescentCVars.RespawnTime))));
    }

    /// <summary>
    /// Removes an entry from the respawn tracker
    /// </summary>
    /// <param name="guid">Player guid</param>
    /// <returns>Whether an entry was removed</returns>
    private bool RemoveEntry(Guid guid)
    {
        if (RespawnTrackers.Keys.Contains(guid))
        {
            RespawnTrackers.Remove(guid);
            return true;
        }

        return false;
    }
}
