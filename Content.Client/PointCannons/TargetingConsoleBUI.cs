using System.Threading;
using Content.Shared.PointCannons;
using Timer = Robust.Shared.Timing.Timer;
using JetBrains.Annotations;
using System.Numerics;
using Robust.Shared.Map;
using Robust.Client.GameObjects;

namespace Content.Client.PointCannons;

[UsedImplicitly]
public sealed class TargetingConsoleBoundUserInterface : BoundUserInterface
{
    private IEntityManager _entMan;
    private IMapManager _mapMan;
    private TransformSystem _formSys;

    private TargetingConsoleWindow? _window;
    private bool _isFiring;
    private Vector2 _coords;
    private CancellationTokenSource _updTimerTok = new();
    private TargetingConsoleComponent _console;

    public TargetingConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _entMan = IoCManager.Resolve<IEntityManager>();
        _mapMan = IoCManager.Resolve<IMapManager>();
        _formSys = _entMan.System<TransformSystem>();
        Timer.SpawnRepeating(100, Update, _updTimerTok.Token);
        _console = _entMan.GetComponent<TargetingConsoleComponent>(owner);
    }

    private void Update()
    {
        if (_isFiring)
            SendMessage(new TargetingConsoleFireMessage(_coords));
    }

    protected override void Open()
    {
        base.Open();
        _window = new TargetingConsoleWindow();
        _window.OpenCentered();
        _window.OnClose += Close;

        _window.Radar.OnRadarClick += (coords) =>
        {
            _coords = _formSys.ToMapCoordinates(coords).Position;
            SendMessage(new TargetingConsoleFireMessage(_coords));
            _isFiring = true;
        };

        _window.Radar.OnRadarRelease += () =>
        {
            _isFiring = false;
        };

        _window.Radar.OnRadarMouseMove += (coords) =>
        {
            _coords = _formSys.ToMapCoordinates(coords).Position;
        };

        _window.OnCannonGroupChange += (groupName) =>
        {
            SendMessage(new TargetingConsoleGroupChangedMessage(groupName));
        };
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _updTimerTok.Cancel();
            _window?.Dispose();
        }
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not TargetingConsoleBoundUserInterfaceState consoleState)
            return;

        _window?.UpdateState(consoleState, _console);
    }
}
