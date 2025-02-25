using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Crescent.DynamicCodes;

/// <summary>
/// Uses the mapping to add the dynamic codes to each relevant entity along with the component needed
/// </summary>
[RegisterComponent]
public sealed partial class DynamicAccesGridInitializer : Component
{
    [DataField("accesMapping", customTypeSerializer: typeof(PrototypeIdSerializer<ShipDynamicAccesMappingPrototype>))]
    public ProtoId<ShipDynamicAccesMappingPrototype> accesMapping = default!;
}
