using Content.Shared.Access;
using Content.Shared.Shuttles.BUIStates;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Crescent;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GridDynamicAccesComponent : Component
{
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadOnly)]
    public List<ProtoId<AccessLevelPrototype>> dynamicAccesCodes = new();

    /// <summary>
    /// Storage for any system that adds these acces levels to the grid . Use the format of shortened entitySystem + unique Hash as the string to
    /// prevent conflicts please.
    /// </summary>
    ///
    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<string, ProtoId<AccessLevelPrototype>> keyToAccesMapping = new();
}
