using Robust.Shared.GameStates;

namespace Content.Shared.PointCannons;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PointCannonComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public List<(Angle, Angle)> ObstructedRanges = new();

    /// <summary>
    /// Since projectiles vary in size and it's kinda hard to estimate how much more clearance is needed
    /// to prevent large projectiles from colliding with walls, you should set this manually
    /// Only used when generating safety ranges
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Angle ClearanceAngle = 0;

    [DataField,ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? LinkedConsoleId;
}
