using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Weather;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Console;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Player;
using System.Numerics;
using Content.Shared.Damage;
using System.Net.NetworkInformation;
using Content.Shared._Crescent.PassiveRegeneration;
using Robust.Server.GameObjects;
using Robust.Server.Player;

namespace Content.Server.Weather;

public sealed class WeatherSystem : SharedWeatherSystem
{
    [Dependency] private readonly IConsoleHost _console = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WeatherComponent, ComponentGetState>(OnWeatherGetState);
        _console.RegisterCommand("weather",
            Loc.GetString("cmd-weather-desc"),
            Loc.GetString("cmd-weather-help"),
            WeatherTwo,
            WeatherCompletion);
    }

    protected override void Run(EntityUid uid, WeatherData weather, WeatherPrototype weatherProto, float frameTime)
    {
        base.Run(uid, weather, weatherProto, frameTime);
        if (weatherProto.Damage is null)
            return;
        if (!TryComp<MapGridComponent>(uid, out var mapComp))
            return;
        var mapTransform = Transform(uid);
        HashSet<Entity<DamageableComponent, PassiveRegenerationComponent>> targets = new();
        _lookup.GetEntitiesOnMap(mapTransform.MapID, targets);
        foreach (var entity in targets)
        {
            if(!TryComp<TransformComponent>(entity, out var transformComp))
                continue;
            if(!_transform.TryGetGridTilePosition(new Entity<TransformComponent?>(entity.Owner, transformComp), out var position, mapComp))
                continue;
            var tile = _mapSystem.GetTileRef(new Entity<MapGridComponent>(entity.Owner, mapComp), position);
            if (!CanWeatherAffect(uid, mapComp, tile))
                continue;
            _damage.TryChangeDamage(entity, weatherProto.Damage, true, false, entity.Comp1, uid);

        }

    }

    private void OnWeatherGetState(EntityUid uid, WeatherComponent component, ref ComponentGetState args)
    {
        args.State = new WeatherComponentState(component.Weather);
    }

    [AdminCommand(AdminFlags.Fun)]
    private void WeatherTwo(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteError(Loc.GetString("cmd-weather-error-no-arguments"));
            return;
        }

        if (!int.TryParse(args[0], out var mapInt))
            return;

        var mapId = new MapId(mapInt);

        if (!MapManager.MapExists(mapId))
            return;

        if (!_mapSystem.TryGetMap(mapId, out var mapUid))
            return;

        var weatherComp = EnsureComp<WeatherComponent>(mapUid.Value);

        //Weather Proto parsing
        WeatherPrototype? weather = null;
        if (!args[1].Equals("null"))
        {
            if (!ProtoMan.TryIndex(args[1], out weather))
            {
                shell.WriteError(Loc.GetString("cmd-weather-error-unknown-proto"));
                return;
            }
        }

        //Time parsing
        TimeSpan? endTime = null;
        if (args.Length == 3)
        {
            var curTime = Timing.CurTime;
            if (int.TryParse(args[2], out var durationInt))
            {
                endTime = curTime + TimeSpan.FromSeconds(durationInt);
            }
            else
            {
                shell.WriteError(Loc.GetString("cmd-weather-error-wrong-time"));
            }
        }

        SetWeather(mapId, weather, endTime);
    }

    private CompletionResult WeatherCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
            return CompletionResult.FromHintOptions(CompletionHelper.MapIds(EntityManager), "Map Id");

        var a = CompletionHelper.PrototypeIDs<WeatherPrototype>(true, ProtoMan);
        return CompletionResult.FromHintOptions(a, Loc.GetString("cmd-weather-hint"));
    }
}
