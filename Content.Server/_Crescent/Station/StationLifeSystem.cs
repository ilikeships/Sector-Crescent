using Content.Server.Power.Components;
using Content.Server.Shuttles.Components;
using Content.Server.Station.Systems;

namespace Content.Server._Crescent.Station;

/// <summary>
/// Handles the figurative "life and death" of vessels for gameplay purposes.
/// </summary>
public sealed partial class StationLifeSystem : EntitySystem
{
    [Dependency] private readonly StationSystem _station = default!;
    public override void Initialize()
    {
        base.Initialize();
    }

    /// <summary>
    /// Whether the station that owns this entity is alive.
    /// </summary>
    public bool IsAlive(EntityUid uid)
    {
        var station = _station.GetOwningStation(uid);

        if (station == null)
            return false;

        // A good starting heuristic: Does the station contain a powered shuttle console?
        bool poweredConsole = false;

        var consoles = EntityQueryEnumerator<ShuttleConsoleComponent, ApcPowerReceiverComponent>();

        while (consoles.MoveNext(out var consoleUid, out var _, out var receiver))
        {
            if (_station.GetOwningStation(consoleUid) != station)
                continue;

            if (receiver.Powered)
            {
                poweredConsole = true;
                break;
            }
        }

        return poweredConsole;
    }
}
