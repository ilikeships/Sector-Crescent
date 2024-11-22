using Content.Client._Crescent.Broadcaster.UI;
using Content.Client.Bank.UI;
using Content.Shared._Crescent.Broadcaster;
using Content.Shared.Bank.BUI;
using Content.Shared.Bank.Events;
using Robust.Client.GameObjects;

namespace Content.Client._Crescent.Broadcaster.BUI;

public sealed class BroadcasterBUI : BoundUserInterface
{
    private BroadcasterUI? _menu;

    public BroadcasterBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        _menu = new BroadcasterUI();
        _menu.ClickBroadcast += OnTryPlayBroadcast;
        _menu.OnClose += Close;
        _menu.OpenCentered();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _menu?.Dispose();
        }
    }

    private void OnTryPlayBroadcast(int index)
    {
        SendMessage(new SharedBroadcasterSystem.BroadcasterBroadcastMessage(index));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not SharedBroadcasterSystem.BroadcasterConsoleState bankState)
            return;

    }
}
