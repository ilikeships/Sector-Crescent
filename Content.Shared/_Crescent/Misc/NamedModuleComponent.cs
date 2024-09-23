using Content.Shared.Access.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.NamedModules.Components;

[RegisterComponent, NetworkedComponent,AutoGenerateComponentState]
public sealed partial class NamedModulesComponent : Component
{
    [AutoNetworkedField]
    [DataField("buttonNames")]
    public Dictionary<int, string> ButtonNames = new();

}

[NetSerializable, Serializable]
public sealed class ModuleNamingChangeEvent : BoundUserInterfaceMessage
{

    public readonly Dictionary<int, string> NewNames;

    public ModuleNamingChangeEvent(Dictionary<int, string> names)
    {

        NewNames = names;
    }
}

