using Robust.Shared.GameStates;

namespace Content.Shared._Crescent;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DynamicCodeHolderComponent : Component
{
    [AutoNetworkedField]
    public HashSet<int> codes = new();

    [AutoNetworkedField]
    public Dictionary<string, HashSet<int>> mappedCodes = new();

    [AutoNetworkedField]
    public Dictionary<int, string> keyToMap = new();
}
