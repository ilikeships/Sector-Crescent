using Content.Shared.Access;
using Robust.Shared.Prototypes;

namespace Content.Shared._Crescent;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class GridDynamicAccesComponent : Component
{
    public List<ProtoId<AccessLevelPrototype>> dynamicAccesCodes = new();
}
