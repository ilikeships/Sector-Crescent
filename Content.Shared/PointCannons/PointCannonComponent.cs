using Robust.Shared.GameStates;

namespace Content.Shared.PointCannons;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PointCannonComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public List<(Angle, Angle)> ObstructedRanges = new();
}