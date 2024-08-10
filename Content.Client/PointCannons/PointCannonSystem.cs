using Content.Shared.PointCannons;
using Robust.Client.GameObjects;

namespace Content.Client.PointCannons;

public sealed class PointCannonSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSys = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TargetingConsoleComponent, AfterAutoHandleStateEvent>(OnCompUpdate);
    }

    private void OnCompUpdate(Entity<TargetingConsoleComponent> uid, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp<UserInterfaceComponent>(uid, out var ui))
            return;

        _uiSys.SetUiState(new Entity<UserInterfaceComponent?>(uid, ui), TargetingConsoleUiKey.Key, new TargetingConsoleBoundUserInterfaceState(null, null));
    }
}