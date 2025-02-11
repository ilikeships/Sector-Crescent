using Content.Client.Shuttles.UI;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Events;
using JetBrains.Annotations;
using Robust.Shared.Map;
using Content.Shared.NamedModules.Components;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Systems;

namespace Content.Client.Shuttles.BUI;

[UsedImplicitly]
public sealed class ShuttleConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private ShuttleConsoleWindow? _window;



    public ShuttleConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = new ShuttleConsoleWindow();
        _window.OpenCentered();
        _window.OnClose += Close;


        _window.RequestFTL += OnFTLRequest;
        _window.RequestBeaconFTL += OnFTLBeaconRequest;
        _window.DockRequest += OnDockRequest;
        _window.UndockRequest += OnUndockRequest;

        _window.idSlotButtonPressed += _ => SendMessage(new ItemSlotButtonPressedEvent(SharedShuttleConsoleComponent.IdSlotName));
        _window.crewHudVisChange += _ => SendMessage(new SwitchedToCrewHudMessage(_));
        _window.employToggleButtonClicked += args => SendMessage(new TryMakeEmployeeMessage(args));
        
        _window.OnGroup1Pressed += () => SendMessage(new NavConsoleGroupPressedMessage(1));
        _window.OnGroup2Pressed += () => SendMessage(new NavConsoleGroupPressedMessage(2));
        _window.OnGroup3Pressed += () => SendMessage(new NavConsoleGroupPressedMessage(3));
        _window.OnGroup4Pressed += () => SendMessage(new NavConsoleGroupPressedMessage(4));
        _window.OnGroup5Pressed += () => SendMessage(new NavConsoleGroupPressedMessage(5));
        _window.MouseMove += (args) => SendMessage(new SetTargetPositionFace() {TargetAngle = args});
        _window.OnRename += OnModuleRename;
        
    }

    private void OnModuleRename(List<string> newNames)
    {
        SendMessage(new ModuleNamingChangeEvent(newNames));
    }

    private void OnUndockRequest(NetEntity entity)
    {
        SendMessage(new UndockRequestMessage()
        {
            DockEntity = entity,
        });
    }

    private void OnDockRequest(NetEntity entity, NetEntity target)
    {
        SendMessage(new DockRequestMessage()
        {
            DockEntity = entity,
            TargetDockEntity = target,
        });
    }

    private void OnFTLBeaconRequest(NetEntity ent, Angle angle)
    {
        SendMessage(new ShuttleConsoleFTLBeaconMessage()
        {
            Beacon = ent,
            Angle = angle,
        });
    }

    private void OnFTLRequest(MapCoordinates obj, Angle angle)
    {
        SendMessage(new ShuttleConsoleFTLPositionMessage()
        {
            Coordinates = obj,
            Angle = angle,
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _window?.Dispose();
        }
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not ShuttleBoundUserInterfaceState cState)
            return;

        _window?.UpdateState(Owner, cState);
    }
}
