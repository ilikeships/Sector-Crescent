using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Station.Systems;
using Content.Shared.ActionBlocker;
using Content.Shared.Alert;
using Content.Shared.Popups;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Events;
using Content.Shared.Shuttles.Systems;
using Content.Shared.Tag;
using Content.Shared.Movement.Systems;
using Content.Shared.Shuttles.UI.MapObjects;
using Content.Shared.Timing;
using Content.Shared.Crescent.Radar;
using Robust.Server.GameObjects;
using Robust.Shared.Collections;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Utility;
using Content.Shared.UserInterface;
using Content.Server.DeviceLinking.Systems;
using Content.Server.PointCannons;
using Content.Shared.NamedModules.Components;

namespace Content.Server.Shuttles.Systems;

public sealed partial class ShuttleConsoleSystem : SharedShuttleConsoleSystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly TagSystem _tags = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedContentEyeSystem _eyeSystem = default!;
    [Dependency] private readonly DeviceLinkSystem _link = default!;

    private EntityQuery<MetaDataComponent> _metaQuery;
    private EntityQuery<TransformComponent> _xformQuery;

    private readonly HashSet<Entity<ShuttleConsoleComponent>> _consoles = new();

    public override void Initialize()
    {
        base.Initialize();

        _metaQuery = GetEntityQuery<MetaDataComponent>();
        _xformQuery = GetEntityQuery<TransformComponent>();

        SubscribeLocalEvent<ShuttleConsoleComponent, ComponentShutdown>(OnConsoleShutdown);
        SubscribeLocalEvent<ShuttleConsoleComponent, PowerChangedEvent>(OnConsolePowerChange);
        SubscribeLocalEvent<ShuttleConsoleComponent, AnchorStateChangedEvent>(OnConsoleAnchorChange);
        SubscribeLocalEvent<ShuttleConsoleComponent, ActivatableUIOpenAttemptEvent>(OnConsoleUIOpenAttempt);
        Subs.BuiEvents<ShuttleConsoleComponent>(ShuttleConsoleUiKey.Key, subs =>
        {
            subs.Event<ShuttleConsoleFTLBeaconMessage>(OnBeaconFTLMessage);
            subs.Event<ShuttleConsoleFTLPositionMessage>(OnPositionFTLMessage);
            subs.Event<BoundUIClosedEvent>(OnConsoleUIClose);
        });

        SubscribeLocalEvent<DroneConsoleComponent, ConsoleShuttleEvent>(OnCargoGetConsole);
        SubscribeLocalEvent<DroneConsoleComponent, AfterActivatableUIOpenEvent>(OnDronePilotConsoleOpen);
        Subs.BuiEvents<DroneConsoleComponent>(ShuttleConsoleUiKey.Key, subs =>
        {
            subs.Event<BoundUIClosedEvent>(OnDronePilotConsoleClose);
        });

        SubscribeLocalEvent<DockEvent>(OnDock);
        SubscribeLocalEvent<UndockEvent>(OnUndock);

        SubscribeLocalEvent<PilotComponent, ComponentGetState>(OnGetState);

        SubscribeLocalEvent<FTLDestinationComponent, ComponentStartup>(OnFtlDestStartup);
        SubscribeLocalEvent<FTLDestinationComponent, ComponentShutdown>(OnFtlDestShutdown);
        SubscribeLocalEvent<ShuttleConsoleComponent, NavConsoleGroupPressedMessage>(OnGroupPressed);
        SubscribeLocalEvent<NamedModulesComponent, ModuleNamingChangeEvent>(OnNameChange);

        InitializeFTL();
    }

    private void OnNameChange(EntityUid consoleUid, NamedModulesComponent comp, ModuleNamingChangeEvent args)
    {
        foreach (var (index, name) in args.NewNames)
        {
            comp.ButtonNames[index] = name;
        }
        Dirty(consoleUid, comp);
    }

    private void OnFtlDestStartup(EntityUid uid, FTLDestinationComponent component, ComponentStartup args)
    {
        RefreshShuttleConsoles();
    }

    private void OnFtlDestShutdown(EntityUid uid, FTLDestinationComponent component, ComponentShutdown args)
    {
        RefreshShuttleConsoles();
    }

    private void OnDock(DockEvent ev)
    {
        RefreshShuttleConsoles();
    }

    private void OnUndock(UndockEvent ev)
    {
        RefreshShuttleConsoles();
    }

    /// <summary>
    /// Refreshes all the shuttle console data for a particular grid.
    /// </summary>
    public void RefreshShuttleConsoles(EntityUid gridUid)
    {
        var exclusions = new List<ShuttleExclusionObject>();
        GetExclusions(ref exclusions);
        _consoles.Clear();
        _lookup.GetChildEntities(gridUid, _consoles);
        DockingInterfaceState? dockState = null;
        IFFInterfaceState? iffState = null;

        foreach (var entity in _consoles)
        {
            UpdateState(entity, entity.Comp, ref dockState, ref iffState);
        }
    }

    /// <summary>
    /// Refreshes all of the data for shuttle consoles.
    /// </summary>
    public void RefreshShuttleConsoles()
    {
        var exclusions = new List<ShuttleExclusionObject>();
        GetExclusions(ref exclusions);
        var query = AllEntityQuery<ShuttleConsoleComponent>();
        DockingInterfaceState? dockState = null;
        IFFInterfaceState? iffState = null;

        while (query.MoveNext(out var uid, out var console))
        {
            UpdateState(uid, console, ref dockState, ref iffState);
        }
    }

    public void RefreshIFFState()
    {
        var query = AllEntityQuery<ShuttleConsoleComponent>();
        while (query.MoveNext(out var uid, out var console))
        {
            if (console.LastUpdatedState == null || console.LastUpdatedState.IFFState == null)
            {
                continue;
            }

            console.LastUpdatedState.IFFState.Turrets = GetAllTurrets(uid);
        }
    }

    /// <summary>
    /// Stop piloting if the window is closed.
    /// </summary>
    private void OnConsoleUIClose(EntityUid uid, ShuttleConsoleComponent component, BoundUIClosedEvent args)
    {
        if ((ShuttleConsoleUiKey)args.UiKey != ShuttleConsoleUiKey.Key)
        {
            return;
        }

        RemovePilot(args.Actor);
    }

    private void OnConsoleUIOpenAttempt(EntityUid uid, ShuttleConsoleComponent component,
        ActivatableUIOpenAttemptEvent args)
    {
        if (!TryPilot(args.User, uid))
            args.Cancel();
    }

    private void OnConsoleAnchorChange(EntityUid uid, ShuttleConsoleComponent component,
        ref AnchorStateChangedEvent args)
    {
        DockingInterfaceState? dockState = null;
        IFFInterfaceState? iffState = null;
        UpdateState(uid, component, ref dockState, ref iffState);
    }

    private void OnConsolePowerChange(EntityUid uid, ShuttleConsoleComponent component, ref PowerChangedEvent args)
    {
        DockingInterfaceState? dockState = null;
        IFFInterfaceState? iffState = null;
        UpdateState(uid, component, ref dockState, ref iffState);
    }

    private bool TryPilot(EntityUid user, EntityUid uid)
    {
        if (!_tags.HasTag(user, "CanPilot") ||
            !TryComp<ShuttleConsoleComponent>(uid, out var component) ||
            !this.IsPowered(uid, EntityManager) ||
            !Transform(uid).Anchored ||
            !_blocker.CanInteract(user, uid))
        {
            return false;
        }

        var pilotComponent = EnsureComp<PilotComponent>(user);
        var console = pilotComponent.Console;

        if (console != null)
        {
            RemovePilot(user, pilotComponent);

            // This feels backwards; is this intended to be a toggle?
            if (console == uid)
                return false;
        }

        AddPilot(uid, user, component);
        return true;
    }

    private void OnGetState(EntityUid uid, PilotComponent component, ref ComponentGetState args)
    {
        args.State = new PilotComponentState(GetNetEntity(component.Console));
    }

    /// <summary>
    /// Returns the position and angle of all dockingcomponents.
    /// </summary>
    public Dictionary<NetEntity, List<DockingPortState>> GetAllDocks()
    {
        // TODO: NEED TO MAKE SURE THIS UPDATES ON ANCHORING CHANGES!
        var result = new Dictionary<NetEntity, List<DockingPortState>>();
        var query = AllEntityQuery<DockingComponent, TransformComponent, MetaDataComponent>();

        while (query.MoveNext(out var uid, out var comp, out var xform, out var metadata))
        {
            if (xform.ParentUid != xform.GridUid)
                continue;

            var gridDocks = result.GetOrNew(GetNetEntity(xform.GridUid.Value));

            var state = new DockingPortState()
            {
                Name = metadata.EntityName,
                Coordinates = GetNetCoordinates(xform.Coordinates),
                Angle = xform.LocalRotation,
                Entity = GetNetEntity(uid),
                GridDockedWith =
                    _xformQuery.TryGetComponent(comp.DockedWith, out var otherDockXform) ?
                    GetNetEntity(otherDockXform.GridUid) :
                    null,
            };

            gridDocks.Add(state);
        }

        return result;
    }

    private void UpdateState(EntityUid consoleUid, ShuttleConsoleComponent console, ref DockingInterfaceState? dockState, ref IFFInterfaceState? iffState)
    {
        EntityUid? entity = consoleUid;

        var getShuttleEv = new ConsoleShuttleEvent
        {
            Console = entity,
        };

        RaiseLocalEvent(entity.Value, ref getShuttleEv);
        entity = getShuttleEv.Console;

        TryComp<TransformComponent>(entity, out var consoleXform);
        var shuttleGridUid = consoleXform?.GridUid;

        NavInterfaceState navState;
        ShuttleMapInterfaceState mapState;
        dockState ??= GetDockState();
        iffState ??= GetIFFState(consoleUid, consoleXform, null);

        if (shuttleGridUid != null && entity != null)
        {
            navState = GetNavState(entity.Value, dockState.Docks);
            mapState = GetMapState(shuttleGridUid.Value);
        }
        else
        {
            navState = new NavInterfaceState(0f, null, null, new Dictionary<NetEntity, List<DockingPortState>>());
            mapState = new ShuttleMapInterfaceState(
                FTLState.Invalid,
                default,
                new List<ShuttleBeaconObject>(),
                new List<ShuttleExclusionObject>());
        }

        if (_ui.HasUi(consoleUid, ShuttleConsoleUiKey.Key))
        {
            var state = new ShuttleBoundUserInterfaceState(navState, mapState, dockState);
            state.IFFState = iffState;
            console.LastUpdatedState = state;
            _ui.SetUiState(consoleUid, ShuttleConsoleUiKey.Key, state);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var toRemove = new ValueList<(EntityUid, PilotComponent)>();
        var query = EntityQueryEnumerator<PilotComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Console == null)
                continue;

            if (!_blocker.CanInteract(uid, comp.Console))
            {
                toRemove.Add((uid, comp));
            }
        }

        foreach (var (uid, comp) in toRemove)
        {
            RemovePilot(uid, comp);
        }

        var consoleQuery = EntityQueryEnumerator<ShuttleConsoleComponent, TransformComponent>();
        while (consoleQuery.MoveNext(out var uid, out var console, out var transform))
        {
            if (console.LastUpdatedState == null || !_ui.IsUiOpen(uid, ShuttleConsoleUiKey.Key))
            {
                continue;
            }

            var turrets = console.LastUpdatedState.IFFState.Turrets;
            var iffState = GetIFFState(uid, transform, turrets);
            var state = new ShuttleBoundUserInterfaceState(console.LastUpdatedState);
            state.IFFState = iffState;

            if (state.DirtyFlags < ShuttleBoundUserInterfaceState.StateDirtyFlags.IFF)
            {
                state.DirtyFlags |= ShuttleBoundUserInterfaceState.StateDirtyFlags.IFF;
            }
            else if (state.DirtyFlags > ShuttleBoundUserInterfaceState.StateDirtyFlags.IFF)
            {
                state.DirtyFlags = ShuttleBoundUserInterfaceState.StateDirtyFlags.IFF;
            }

            console.LastUpdatedState = state;
            _ui.SetUiState(uid, ShuttleConsoleUiKey.Key, state);
        }
    }

    protected override void HandlePilotShutdown(EntityUid uid, PilotComponent component, ComponentShutdown args)
    {
        base.HandlePilotShutdown(uid, component, args);
        RemovePilot(uid, component);
    }

    private void OnConsoleShutdown(EntityUid uid, ShuttleConsoleComponent component, ComponentShutdown args)
    {
        ClearPilots(component);
    }

    public void AddPilot(EntityUid uid, EntityUid entity, ShuttleConsoleComponent component)
    {
        if (!EntityManager.TryGetComponent(entity, out PilotComponent? pilotComponent)
        || component.SubscribedPilots.Contains(entity))
        {
            return;
        }

        _eyeSystem.SetZoom(entity, component.Zoom, ignoreLimits: true);

        component.SubscribedPilots.Add(entity);

        _alertsSystem.ShowAlert(entity, AlertType.PilotingShuttle);

        pilotComponent.Console = uid;
        ActionBlockerSystem.UpdateCanMove(entity);
        pilotComponent.Position = EntityManager.GetComponent<TransformComponent>(entity).Coordinates;
        Dirty(entity, pilotComponent);
    }

    public void RemovePilot(EntityUid pilotUid, PilotComponent pilotComponent)
    {
        var console = pilotComponent.Console;

        if (!TryComp<ShuttleConsoleComponent>(console, out var helm))
            return;

        pilotComponent.Console = null;
        pilotComponent.Position = null;
        _eyeSystem.ResetZoom(pilotUid);

        if (!helm.SubscribedPilots.Remove(pilotUid))
            return;

        _alertsSystem.ClearAlert(pilotUid, AlertType.PilotingShuttle);

        _popup.PopupEntity(Loc.GetString("shuttle-pilot-end"), pilotUid, pilotUid);

        if (pilotComponent.LifeStage < ComponentLifeStage.Stopping)
            EntityManager.RemoveComponent<PilotComponent>(pilotUid);
    }

    public void RemovePilot(EntityUid entity)
    {
        if (!EntityManager.TryGetComponent(entity, out PilotComponent? pilotComponent))
            return;

        RemovePilot(entity, pilotComponent);
    }

    public void ClearPilots(ShuttleConsoleComponent component)
    {
        var query = GetEntityQuery<PilotComponent>();
        while (component.SubscribedPilots.TryGetValue(0, out var pilot))
        {
            if (query.TryGetComponent(pilot, out var pilotComponent))
                RemovePilot(pilot, pilotComponent);
        }
    }

    /// <summary>
    /// Specific for a particular shuttle.
    /// </summary>
    public NavInterfaceState GetNavState(Entity<RadarConsoleComponent?, TransformComponent?> entity, Dictionary<NetEntity, List<DockingPortState>> docks)
    {
        if (!Resolve(entity, ref entity.Comp1, ref entity.Comp2))
            return new NavInterfaceState(SharedRadarConsoleSystem.DefaultMaxRange, null, null, docks);

        return GetNavState(
            entity,
            docks,
            entity.Comp2.Coordinates,
            entity.Comp2.LocalRotation);
    }

    public NavInterfaceState GetNavState(
        Entity<RadarConsoleComponent?, TransformComponent?> entity,
        Dictionary<NetEntity, List<DockingPortState>> docks,
        EntityCoordinates coordinates,
        Angle angle)
    {
        if (!Resolve(entity, ref entity.Comp1, ref entity.Comp2))
            return new NavInterfaceState(SharedRadarConsoleSystem.DefaultMaxRange, GetNetCoordinates(coordinates), angle, docks);

        return new NavInterfaceState(
            entity.Comp1.MaxRange,
            GetNetCoordinates(coordinates),
            angle,
            docks);
    }

    /// <summary>
    /// Global for all shuttles.
    /// </summary>
    /// <returns></returns>
    public DockingInterfaceState GetDockState()
    {
        var docks = GetAllDocks();
        return new DockingInterfaceState(docks);
    }

    /// <summary>
    /// Specific to a particular shuttle.
    /// </summary>
    public ShuttleMapInterfaceState GetMapState(Entity<FTLComponent?> shuttle)
    {
        FTLState ftlState = FTLState.Available;
        StartEndTime stateDuration = default;

        if (Resolve(shuttle, ref shuttle.Comp, false) && shuttle.Comp.LifeStage < ComponentLifeStage.Stopped)
        {
            ftlState = shuttle.Comp.State;
            stateDuration = _shuttle.GetStateTime(shuttle.Comp);
        }

        List<ShuttleBeaconObject>? beacons = null;
        List<ShuttleExclusionObject>? exclusions = null;
        GetBeacons(ref beacons);
        GetExclusions(ref exclusions);

        return new ShuttleMapInterfaceState(
            ftlState,
            stateDuration,
            beacons ?? new List<ShuttleBeaconObject>(),
            exclusions ?? new List<ShuttleExclusionObject>());
    }

    public void OnGroupPressed(EntityUid consoleUid, ShuttleConsoleComponent shuttleConsole, NavConsoleGroupPressedMessage args)
    {
        switch (args.Payload)
        {
            case 1: _link.InvokePort(consoleUid, "Group1"); break;
            case 2: _link.InvokePort(consoleUid, "Group2"); break;
            case 3: _link.InvokePort(consoleUid, "Group3"); break;
            case 4: _link.InvokePort(consoleUid, "Group4"); break;
            case 5: _link.InvokePort(consoleUid, "Group5"); break;
            default:
                break;
        };
    }


    public IFFInterfaceState GetIFFState(EntityUid consoleUid, TransformComponent? consoleTransform, Dictionary<NetEntity, List<TurretState>>? turrets)
    {
        var projectiles = GetProjectilesInRange(consoleUid, consoleTransform);
        turrets ??= GetAllTurrets(consoleUid);
        return new IFFInterfaceState(projectiles, turrets);
    }

    public List<ProjectileState> GetProjectilesInRange(EntityUid consoleUid, TransformComponent? consoleTransform)
    {
        var projectiles = new List<ProjectileState>();

        if (!Resolve(consoleUid, ref consoleTransform))
        {
            return projectiles;
        }

        var consolePosition = _transform.GetMapCoordinates(consoleTransform);
        var range = SharedRadarConsoleSystem.DefaultMaxRange;

        var query = EntityQueryEnumerator<ProjectileIFFComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var projectileIFF, out var transform))
        {
            if (!consolePosition.InRange(_transform.GetMapCoordinates(transform), range))
            {
                continue;
            }

            var projectile = new ProjectileState { Coordinates = GetNetCoordinates(_transform.GetMoverCoordinates(uid, transform)) };
            projectiles.Add(projectile);
        }

        return projectiles;
    }

    public Dictionary<NetEntity, List<TurretState>> GetAllTurrets(EntityUid consoleUid)
    {
        List<EntityUid>? controlledUids = null;
        if (TryComp<TargetingConsoleComponent>(consoleUid, out var targCon))
            controlledUids = targCon.CurrentGroup;

        var turrets = new Dictionary<NetEntity, List<TurretState>>();
        var query = EntityQueryEnumerator<TurretIFFComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var turretIFF, out var transform))
        {
            if (transform?.GridUid == null)
                continue;

            var netEntity = GetNetEntity(transform.GridUid.Value);
            var turret = new TurretState
            {
                IsControlled = controlledUids == null || controlledUids.Contains(uid),
                Coordinates = GetNetCoordinates(transform.Coordinates)
            };

            if (turrets.TryGetValue(netEntity, out var gridTurrets))
            {
                gridTurrets.Add(turret);
            }
            else
            {
                gridTurrets = new List<TurretState> { turret };
                turrets.Add(netEntity, gridTurrets);
            }
        }

        return turrets;
    }
}
