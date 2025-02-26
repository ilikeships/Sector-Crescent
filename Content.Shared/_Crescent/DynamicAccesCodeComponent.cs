using Robust.Shared.GameStates;

namespace Content.Shared._Crescent;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DynamicCodeHolderComponent : Component
{
    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public HashSet<int> codes = new();

    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<string, HashSet<int>> mappedCodes = new();

}
