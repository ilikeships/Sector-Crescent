using System.Linq;
using System.Numerics;
using Content.Server.Administration;
using Content.Server.Popups;
using Content.Server.Shuttles.Systems;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Crescent.Radar;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.PointCannons;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;

namespace Content.Server.PointCannons;

public sealed class PointCannonSystem : EntitySystem
{
    [Dependency] private readonly ISharedPlayerManager _playerMan = default!;
    [Dependency] private readonly TransformSystem _formSys = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _uiSys = default!;
    [Dependency] private readonly PopupSystem _popSys = default!;
    [Dependency] private readonly QuickDialogSystem _dialogSys = default!;
    [Dependency] private readonly GunSystem _gunSys = default!;
    [Dependency] private readonly ShuttleConsoleSystem _shuttleConSys = default!;

    private const int MaxCollisionCheckDistance = 10;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TargetingConsoleComponent, ComponentStartup>(OnConsoleStartup);
        SubscribeLocalEvent<TargetingConsoleComponent, TargetingConsoleFireMessage>(OnConsoleFire);
        SubscribeLocalEvent<TargetingConsoleComponent, TargetingConsoleGroupChangedMessage>(OnConsoleGroupChanged);

        SubscribeLocalEvent<PointCannonComponent, ComponentShutdown>(OnCannonShutdown);

        SubscribeLocalEvent<PointCannonLinkToolComponent, UseInHandEvent>(OnLinkToolHandUse);
        SubscribeLocalEvent<PointCannonComponent, InteractUsingEvent>(OnLinkToolUse);
    }

    private void OnConsoleStartup(Entity<TargetingConsoleComponent> uid, ref ComponentStartup args)
    {
        UpdateConsoleState(uid, uid.Comp);
    }

    private void OnCannonShutdown(Entity<PointCannonComponent> uid, ref ComponentShutdown args)
    {
        EntityUid? gridUid = Transform(uid).GridUid;
        if (gridUid == null)
            return;

        var query = EntityQueryEnumerator<TransformComponent, TargetingConsoleComponent>();
        while (query.MoveNext(out var consoleUid, out var form, out var console))
        {
            if (form.GridUid == gridUid)
                UnlinkCannon(uid, consoleUid, console);
        }
    }

    private void OnLinkToolUse(Entity<PointCannonComponent> uid, ref InteractUsingEvent args)
    {
        if (!TryComp<PointCannonLinkToolComponent>(args.Used, out var linkTool))
            return;

        EntityUid? gridUid = Transform(uid).GridUid;
        if (gridUid == null)
            return;

        var query = EntityQueryEnumerator<TransformComponent, TargetingConsoleComponent>();
        while (query.MoveNext(out var consoleUid, out var form, out var console))
        {
            if (form.GridUid == gridUid)
                LinkCannon(uid, consoleUid, console, linkTool.GroupName);
        }

        _popSys.PopupEntity($"Added to {linkTool.GroupName}", args.User);
    }

    public void LinkCannon(EntityUid cannonUid, EntityUid consoleUid, TargetingConsoleComponent console, string group)
    {
        NetEntity cannonNetUid = GetNetEntity(cannonUid);

        if (!console.CannonGroups.ContainsKey(group))
            console.CannonGroups[group] = [];

        if (console.CannonGroups[group].Contains(cannonNetUid))
            return;

        console.CannonGroups[group].Add(cannonNetUid);
        if (group != "all" && !console.CannonGroups["all"].Contains(cannonNetUid))
            console.CannonGroups["all"].Add(cannonNetUid);

        UpdateConsoleState(consoleUid, Comp<TargetingConsoleComponent>(consoleUid));
        Dirty(consoleUid, console);
    }

    public void UnlinkCannon(EntityUid cannonUid, EntityUid consoleUid, TargetingConsoleComponent console)
    {
        NetEntity cannonNetUid = GetNetEntity(cannonUid);

        foreach (string group in console.CannonGroups.Keys.ToList())
        {
            console.CannonGroups[group].Remove(cannonNetUid);
            if (console.CannonGroups[group].Count == 0)
                console.CannonGroups.Remove(group);
        }

        UpdateConsoleState(consoleUid, Comp<TargetingConsoleComponent>(consoleUid));
        Dirty(consoleUid, console);
    }

    private void OnLinkToolHandUse(Entity<PointCannonLinkToolComponent> uid, ref UseInHandEvent args)
    {
        if (!_playerMan.TryGetSessionByEntity(args.User, out var session))
            return;

        _dialogSys.OpenDialog(session, "Group name", "Name (case insensitive)", (string name) =>
        {
            uid.Comp.GroupName = name == "" ? "all" : name.ToLower();
        });
    }

    public void UpdateConsoleState(EntityUid uid, TargetingConsoleComponent console)
    {
        NavInterfaceState navState = _shuttleConSys.GetNavState(uid, _shuttleConSys.GetAllDocks());
        IFFInterfaceState iffState = _shuttleConSys.GetIFFState(uid, null, null);
        TargetingConsoleBoundUserInterfaceState consoleState = new(navState, iffState);
        _uiSys.SetUiState(uid, TargetingConsoleUiKey.Key, consoleState);
    }

    private void OnConsoleFire(EntityUid uid, TargetingConsoleComponent console, TargetingConsoleFireMessage ev)
    {
        for (int i = 0; i < console.CurrentGroup.Count;)
        {
            EntityUid cannonUid = GetEntity(console.CurrentGroup[i]);
            if (Deleted(cannonUid))
            {
                console.CurrentGroup.RemoveAt(i);
                continue;
            }

            TryFireCannon(cannonUid, ev.Coordinates);
            i++;
        }
    }

    private void OnConsoleGroupChanged(Entity<TargetingConsoleComponent> uid, ref TargetingConsoleGroupChangedMessage args)
    {
        uid.Comp.CurrentGroupName = args.GroupName;
    }

    private bool TryFireCannon(
        EntityUid uid,
        Vector2 pos,
        TransformComponent? form = null,
        GunComponent? gun = null,
        PointCannonComponent? cannon = null)
    {
        if (!Resolve(uid, ref form) || !Resolve(uid, ref gun) || !Resolve(uid, ref cannon))
            return false;

        if (form.MapUid == null || !_gunSys.CanShoot(gun))
            return false;

        Vector2 cannonPos = _formSys.GetWorldPosition(form);
        _formSys.SetWorldRotation(uid, Angle.FromWorldVec(pos - cannonPos));

        if (!SafetyCheck(form.LocalRotation, cannon))
            return false;

        EntityCoordinates entPos = new(form.MapUid.Value, pos);
        _gunSys.AttemptShoot(uid, uid, gun, entPos);
        return true;
    }

    public bool SafetyCheck(Angle ang, PointCannonComponent cannon)
    {
        foreach ((Angle start, Angle width) in cannon.ObstructedRanges)
        {
            if (CrescentHelpers.AngInSector(ang, start, width))
                return false;
        }

        return true;
    }

    public void RefreshFiringRanges(EntityUid uid, TransformComponent? form = null, GunComponent? gun = null, PointCannonComponent? cannon = null)
    {
        if (!Resolve(uid, ref form) || !Resolve(uid, ref gun) || !Resolve(uid, ref cannon))
            return;

        cannon.ObstructedRanges = CalculateFiringRanges(uid, form, gun);
        Dirty(uid, cannon);
    }

    private List<(Angle, Angle)> CalculateFiringRanges(EntityUid uid, TransformComponent form, GunComponent gun)
    {
        if (form.GridUid == null)
            return new();

        TransformComponent gridForm = Transform(form.GridUid.Value);
        List<(Angle, Angle)> ranges = new();

        foreach (EntityUid childUid in gridForm.ChildEntities)
        {
            //checking if obstacle is not too far/close to the cannon
            TransformComponent otherForm = Transform(childUid);
            Vector2 dir = otherForm.LocalPosition - Transform(uid).LocalPosition;
            float dist = dir.Length();
            if (dist > MaxCollisionCheckDistance || dist < 1)
                continue;

            //checking that obstacle is anchored and solid
            if (!otherForm.Anchored || !TryComp<PhysicsComponent>(childUid, out var body) || !body.Hard)
                continue;

            //calculating circular sector that obstacle occupies relative to the cannon
            (Angle start0, Angle width0) = GetObstacleSector(dir);
            ranges.Add((start0, width0));

            (Angle start2, Angle width2) = (start0, width0);

            //checking whether new sector overlaps with any existing ones and combining them if so
            List<(Angle, Angle)> overlaps = new();
            foreach ((Angle start1, Angle width1) in ranges)
            {
                if (CrescentHelpers.AngSectorsOverlap(start0, width0, start1, width1))
                {
                    (start2, width2) = CrescentHelpers.AngCombinedSector(start2, width2, start1, width1);
                    overlaps.Add((start1, width1));
                }
            }

            foreach ((Angle start1, Angle width1) in overlaps)
            {
                ranges.Remove((start1, width1));
            }
            ranges.Add((start2, width2));
        }

        //subtracting spread from every range
        Angle maxSpread = gun.MaxAngle + Angle.FromDegrees(10);
        for (int i = 0; i < ranges.Count; i++)
        {
            ranges[i] = (CrescentHelpers.AngNormal(ranges[i].Item1 - maxSpread / 2), ranges[i].Item2 + maxSpread);
        }

        return ranges;
    }

    //delta between cannon's and obstacle's position
    private (Angle, Angle) GetObstacleSector(Vector2 delta)
    {
        Angle dirAngle = CrescentHelpers.AngNormal(new Angle(delta));
        Vector2 a, b;

        //this can be done without ugly conditional below, by rotating tile's square by delta's angle and finding left- and rightmost points,
        //but this certainly will be heavier and less clear
        if (dirAngle % (Math.PI * 0.5) == 0)
        {
            switch (dirAngle.Theta)
            {
                case 0:
                case Math.Tau:
                    a = delta - Vector2Helpers.Half;
                    b = new Vector2(delta.X - 0.5f, delta.Y + 0.5f);
                    break;
                case Math.PI * 0.5:
                    a = delta - Vector2Helpers.Half;
                    b = new Vector2(delta.X + 0.5f, delta.Y - 0.5f);
                    break;
                case Math.PI:
                    a = delta + Vector2Helpers.Half;
                    b = new Vector2(delta.X + 0.5f, delta.Y - 0.5f);
                    break;
                case Math.PI * 1.5:
                    a = delta + Vector2Helpers.Half;
                    b = new Vector2(delta.X - 0.5f, delta.Y + 0.5f);
                    break;
                default:
                    return (double.NaN, double.NaN);
            }
        }
        else if (dirAngle > 0 && dirAngle < Math.PI * 0.5 || dirAngle > Math.PI && dirAngle < Math.PI * 1.5)
        {
            a = new Vector2(delta.X - 0.5f, delta.Y + 0.5f);
            b = new Vector2(delta.X + 0.5f, delta.Y - 0.5f);
        }
        else
        {
            a = delta + Vector2Helpers.Half;
            b = delta - Vector2Helpers.Half;
        }

        Angle start = CrescentHelpers.AngNormal(new Angle(a));
        Angle end = CrescentHelpers.AngNormal(new Angle(b));
        Angle width = Angle.ShortestDistance(start, end);
        if (width < 0)
        {
            start = end;
            width = -width;
        }

        return (start, width);
    }
}
