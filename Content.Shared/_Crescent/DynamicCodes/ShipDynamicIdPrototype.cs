using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Shared._Crescent.DynamicCodes;

/// <summary>
/// This is a prototype for...
/// </summary>
[Prototype("ShipDynamicAccesMapping")]
public sealed partial class ShipDynamicAccesMappingPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField]
    public Dictionary<string, HashSet<ProtoId<EntityPrototype>>> accesIdentifierToEntity = default!;
}
