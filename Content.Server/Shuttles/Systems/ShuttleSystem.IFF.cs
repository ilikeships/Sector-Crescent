using Content.Server.Shuttles.Components;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Events;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Content.Server.Shuttles.Systems;

public sealed partial class ShuttleSystem
{
    private List<IFFhandler> _activeComponents = new();

    private sealed class IFFhandler
    {
        public readonly IFFConsoleComponent Component;
        public readonly EntityUid ComponentOwner;
        public bool Hiding = false;

        public IFFhandler(IFFConsoleComponent comp, EntityUid owner, bool hide)
        {
            Component = comp;
            ComponentOwner = owner;
            Hiding = hide;
        }
    }

    public void RemoveActiveComponent(EntityUid uid)
    {
        foreach (var cloaker in _activeComponents)
        {
            if (cloaker.ComponentOwner == uid)
            {
                _activeComponents.Remove(cloaker);
                break;
            }
        }
    }

    private IFFhandler? FetchActiveComponent(EntityUid uid)
    {
        foreach (var cloaker in _activeComponents)
        {
            if (cloaker.ComponentOwner == uid)
            {
                return cloaker;
            }
        }
        return null;
    }
    private void InitializeIFF()
    {
        SubscribeLocalEvent<IFFConsoleComponent, AnchorStateChangedEvent>(OnIFFConsoleAnchor);
        SubscribeLocalEvent<IFFConsoleComponent, IFFShowIFFMessage>(OnIFFShow);
        SubscribeLocalEvent<IFFConsoleComponent, IFFShowVesselMessage>(OnIFFShowVessel);
    }

    private void OnIFFShow(EntityUid uid, IFFConsoleComponent component, IFFShowIFFMessage args)
    {
        if (!TryComp<TransformComponent>(uid, out var xform) || xform.GridUid == null ||
            (component.AllowedFlags & IFFFlags.HideLabel) == 0x0)
        {
            return;
        }

        if (!args.Show)
        {
            AddIFFFlag(xform.GridUid.Value, IFFFlags.HideLabel);
        }
        else
        {
            RemoveIFFFlag(xform.GridUid.Value, IFFFlags.HideLabel);
        }
    }

    private void OnIFFShowVessel(EntityUid uid, IFFConsoleComponent component, IFFShowVesselMessage args)
    {
        if (!TryComp<TransformComponent>(uid, out var xform) || xform.GridUid == null ||
            (component.AllowedFlags & IFFFlags.Hide) == 0x0)
        {
            return;
        }

        if (!args.Show)
        {
            if (component.HeatCapacity - component.CurrentHeat < component.HeatGeneration)
                return;
            AddIFFFlag(xform.GridUid.Value, IFFFlags.Hide);
            IFFhandler? handle = FetchActiveComponent(uid);
            if(handle is null)
                _activeComponents.Add(new IFFhandler(component, uid, true));
            else
            {
                handle.Hiding = true;
            }
        }
        else
        {
            RemoveIFFFlag(xform.GridUid.Value, IFFFlags.Hide);
            IFFhandler? handle = FetchActiveComponent(uid);
            if(handle is not null)
                handle.Hiding = false;

        }
    }

    private void OnIFFConsoleAnchor(EntityUid uid, IFFConsoleComponent component, ref AnchorStateChangedEvent args)
    {
        // If we anchor / re-anchor then make sure flags up to date.
        if (!args.Anchored ||
            !TryComp<TransformComponent>(uid, out var xform) ||
            !TryComp<IFFComponent>(xform.GridUid, out var iff))
        {
            _uiSystem.SetUiState(uid, IFFConsoleUiKey.Key, new IFFConsoleBoundUserInterfaceState()
            {
                AllowedFlags = component.AllowedFlags,
                Flags = IFFFlags.None,
                HeatCapacity = component.HeatCapacity,
                CurrentHeat = component.CurrentHeat,
            }) ;
        }
        else
        {
            _uiSystem.SetUiState(uid, IFFConsoleUiKey.Key, new IFFConsoleBoundUserInterfaceState()
            {
                AllowedFlags = component.AllowedFlags,
                Flags = iff.Flags,
                HeatCapacity = component.HeatCapacity,
                CurrentHeat = component.CurrentHeat,
            });

        }
    }

    protected override void UpdateIFFInterfaces(EntityUid gridUid, IFFComponent component)
    {
        base.UpdateIFFInterfaces(gridUid, component);

        var query = AllEntityQuery<IFFConsoleComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (xform.GridUid != gridUid)
                continue;

            _uiSystem.SetUiState(uid, IFFConsoleUiKey.Key, new IFFConsoleBoundUserInterfaceState()
            {
                AllowedFlags = comp.AllowedFlags,
                Flags = component.Flags,
                HeatCapacity = comp.HeatCapacity,
                CurrentHeat = comp.CurrentHeat,
            });
        }

    }

    void UpdateCloakers()
    {
        foreach(var handler in _activeComponents)
        {
            IFFConsoleComponent cloaker = handler.Component;
            if (handler.Hiding)
            {
                cloaker.CurrentHeat += cloaker.HeatGeneration;
                if (cloaker.CurrentHeat > cloaker.HeatCapacity)
                {
                    if (!TryComp<TransformComponent>(handler.ComponentOwner, out var xform) || xform.GridUid == null)
                    {
                        return;
                    }
                    RemoveIFFFlag(xform.GridUid.Value, IFFFlags.Hide);
                    handler.Hiding = false;
                }
            }
            else
            {
                cloaker.CurrentHeat = Math.Max(0f, cloaker.CurrentHeat - cloaker.HeatDissipation);
                if (cloaker.CurrentHeat == 0f)
                    RemoveActiveComponent(handler.ComponentOwner);

            }
            if (TryComp<TransformComponent>(handler.ComponentOwner, out var xform2) && TryComp<IFFComponent>(xform2.GridUid, out var comp))
            {
                _uiSystem.SetUiState(handler.ComponentOwner, IFFConsoleUiKey.Key, new IFFConsoleBoundUserInterfaceState()
                {
                    AllowedFlags = cloaker.AllowedFlags,
                    Flags = comp.Flags,
                    HeatCapacity = cloaker.HeatCapacity,
                    CurrentHeat = cloaker.CurrentHeat,
                });
            }
        }
    }

}
