using System.Numerics;
using Content.Server.UserInterface;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Systems;
using Content.Shared.PowerCell;
using Content.Shared.Movement.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Content.Shared.PowerCell;
using Content.Shared.Movement.Components;
using Content.Shared.Crescent.Radar;

namespace Content.Server.Shuttles.Systems;

public sealed class RadarConsoleSystem : SharedRadarConsoleSystem
{
    [Dependency] private readonly ShuttleConsoleSystem _console = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RadarConsoleComponent, ComponentStartup>(OnRadarStartup);
    }

    private void OnRadarStartup(EntityUid uid, RadarConsoleComponent component, ComponentStartup args)
    {
        UpdateState(uid, component);
    }

    public void RefreshIFFState()
    {
        var turrets = _console.GetAllTurrets();
        var query = AllEntityQuery<RadarConsoleComponent>();
        while (query.MoveNext(out var uid, out var console))
        {
            if (console.LastUpdatedState == null || console.LastUpdatedState.IFFState == null)
            {
                continue;
            }

            console.LastUpdatedState.IFFState.Turrets = turrets;
        }
    }

    protected override void UpdateState(EntityUid uid, RadarConsoleComponent component)
    {
        var xform = Transform(uid);
        var onGrid = xform.ParentUid == xform.GridUid;
        EntityCoordinates? coordinates = onGrid ? xform.Coordinates : null;
        Angle? angle = onGrid ? xform.LocalRotation : null;
        
        // Frontier - For handheld mass scanner, PR 484
        if (HasComp<PowerCellDrawComponent>(uid))
        {
            coordinates = new EntityCoordinates(uid, Vector2.Zero);
            angle = Angle.Zero + MathHelper.DegreesToRadians(180);
        }

        if (component.FollowEntity)
        {
            coordinates = new EntityCoordinates(uid, Vector2.Zero);
            angle = Angle.Zero;
        }

        if (_uiSystem.HasUi(uid, RadarConsoleUiKey.Key))
        {
            NavInterfaceState state;
            var docks = _console.GetAllDocks();

            if (coordinates != null && angle != null)
            {
                state = _console.GetNavState(uid, docks, coordinates.Value, angle.Value);
            }
            else
            {
                state = _console.GetNavState(uid, docks);
            }

            component.LastUpdatedState = new NavBoundUserInterfaceState(state);
            _uiSystem.SetUiState(uid, RadarConsoleUiKey.Key, component.LastUpdatedState);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<RadarConsoleComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var console, out var transform))
        {
            if (console.LastUpdatedState == null || !_uiSystem.IsUiOpen(uid, RadarConsoleUiKey.Key))
            {
                continue;
            }

            var turrets = console.LastUpdatedState.IFFState?.Turrets;
            var iffState = _console.GetIFFState(uid, transform, turrets);
            var state = new NavBoundUserInterfaceState(console.LastUpdatedState);
            state.IFFState = iffState;

            if (state.DirtyFlags < NavBoundUserInterfaceState.StateDirtyFlags.IFF)
            {
                state.DirtyFlags |= NavBoundUserInterfaceState.StateDirtyFlags.IFF;
            }
            else if (state.DirtyFlags > NavBoundUserInterfaceState.StateDirtyFlags.IFF)
            {
                state.DirtyFlags = NavBoundUserInterfaceState.StateDirtyFlags.IFF;
            }

            console.LastUpdatedState = state;
            _uiSystem.SetUiState(uid, RadarConsoleUiKey.Key, state);
        }
    }
}
