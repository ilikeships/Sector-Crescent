using Content.Shared.Shuttles.Components;
using Content.Server.Shuttles.Components;

namespace Content.Server._Crescent.SpaceArtillery;

public abstract partial class SpaceArtillerySystem
{
    protected virtual void InitializeGrid()
    {
        ///TODO Integrate vessel armament deactivation event
        SubscribeLocalEvent<SpaceArtilleryGridComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SpaceArtilleryGridComponent, SpaceArtilleryGridActivationEvent>(OnActivationEvent);

        //This is to ensure proper operation of armed vessel
        SubscribeLocalEvent<IFFConsoleComponent, ComponentInit>(OnIFFInit);
    }

    /// Armed vessels handling
    /// TODO Code it much much better
    /// TODO Hang yourself

    /// Prevents built IFF console from being capable of changing armed vessel's IFF settings
    private void OnIFFInit(EntityUid uid, IFFConsoleComponent iffComponent, ComponentInit args)
    {
        if (TryComp<TransformComponent>(uid, out var transformComponent))
        {
            var xformGridUid = transformComponent.GridUid;

            if (xformGridUid is { Valid: true } gridUid)
            {
                if (TryComp<SpaceArtilleryGridComponent>(gridUid, out var artyComp))
                {
                    if (artyComp.IsActive == true || artyComp.IsCharging == true)
                    {
                        var oldFlags = iffComponent.AllowedFlags;
                        var newFlags = iffComponent.AccessableAllowedFlags;

                        iffComponent.AllowedFlags = newFlags;
                        iffComponent.AccessableAllowedFlags = oldFlags;

                        iffComponent.IsDisabled = true;

                        var ev = new AnchorStateChangedEvent(transformComponent);
                        RaiseLocalEvent(uid, ref ev, false);
                    }
                }
            }
        }
    }

    private void OnActivationEvent(EntityUid gridUid, SpaceArtilleryGridComponent componentGrid, ref SpaceArtilleryGridActivationEvent args)
    {

        if (componentGrid.IsActive == true)
        {
            if (_gameTiming.CurTime >= componentGrid.CooldownEndTime)
            {
                componentGrid.IsActive = false;

                if (TryComp<IFFComponent>(gridUid, out var iffComp))
                {
                    //IffComponent.Color = componentGrid.Color;
                    _shuttleSystem.SetIFFColor(gridUid, componentGrid.Color, iffComp);
                    //_shuttleSystem.AddIFFFlag(GridUid, IFFFlags.Hide);
                    //_shuttleSystem.AddIFFFlag(GridUid, IFFFlags.HideLabel);

                    var query = EntityQueryEnumerator<IFFConsoleComponent>();
                    while (query.MoveNext(out var uid, out var comp))
                    {

                        if (Transform(uid).GridUid == gridUid && comp.IsDisabled == true)
                        {
                            var oldFlags = comp.AllowedFlags;
                            var newFlags = comp.AccessableAllowedFlags;

                            comp.AllowedFlags = newFlags;
                            comp.AccessableAllowedFlags = oldFlags;

                            comp.IsDisabled = false;

                            var ev = new AnchorStateChangedEvent(Transform(uid));
                            RaiseLocalEvent(uid, ref ev, false);
                        }
                    }
                }
            }
        }
        else if (componentGrid.IsCharging == true)
        {
            if (_gameTiming.CurTime >= componentGrid.ChargeUpEndTime)
            {
                componentGrid.IsCharging = false;
                componentGrid.IsActive = true;
            }
        }
        else
        {
            componentGrid.IsCharging = true;

            componentGrid.LastActivationTime = _gameTiming.CurTime;
            componentGrid.ChargeUpEndTime = componentGrid.LastActivationTime + componentGrid.ChargeUpDuration;
            componentGrid.CooldownEndTime = componentGrid.LastActivationTime + componentGrid.CooldownDuration;

            if (TryComp<IFFComponent>(gridUid, out var iffComp))
            {
                //IffComponent.Color = componentGrid.ArmedColor;
                //IffComponent.Flags = componentGrid.Flags;
                ///TODO have it affect IFF consoles and disable their ability
                _shuttleSystem.SetIFFColor(gridUid, componentGrid.ArmedColor, iffComp);
                _shuttleSystem.RemoveIFFFlag(gridUid, IFFFlags.Hide);
                _shuttleSystem.RemoveIFFFlag(gridUid, IFFFlags.HideLabel);

                var query = EntityQueryEnumerator<IFFConsoleComponent>();
                while (query.MoveNext(out var uid, out var comp))
                {
                    if (Transform(uid).GridUid == gridUid && comp.IsDisabled == false)
                    {
                        var oldFlags = comp.AllowedFlags;
                        var newFlags = comp.AccessableAllowedFlags;

                        comp.AllowedFlags = newFlags;
                        comp.AccessableAllowedFlags = oldFlags;

                        comp.IsDisabled = true;

                        var ev = new AnchorStateChangedEvent(Transform(uid));
                        RaiseLocalEvent(uid, ref ev, false);
                    }
                }
            }
        }
    }
    private void OnMapInit(EntityUid uid, SpaceArtilleryGridComponent componentGrid, MapInitEvent args)
    {
        componentGrid.LastActivationTime = _gameTiming.CurTime;
        componentGrid.CooldownEndTime = componentGrid.LastActivationTime + componentGrid.CooldownDuration;
    }
}
