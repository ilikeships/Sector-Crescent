using Robust.Shared.GameStates;

namespace Content.Shared.PointCannons;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TargetingConsoleComponent : Component
{
    public string CurrentGroupName = "all";

    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public Dictionary<string, List<NetEntity>> CannonGroups = new() { { "all", new() } };

    public List<NetEntity> CurrentGroup => CannonGroups[CurrentGroupName];
}