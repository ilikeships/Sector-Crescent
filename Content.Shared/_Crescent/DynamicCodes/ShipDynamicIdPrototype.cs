using Robust.Shared.Prototypes;

namespace Content.Shared._Crescent.DynamicCodes;

/// <summary>
/// This is a prototype for...
/// </summary>
[Prototype("ShipDynamicIdPreset")]
public sealed partial class ShipDynamicIdPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    /// This is a list of the mapping identifiers for the ship's preset.
    /// These will be given a generated key on initialization and any
    /// matching CodeHolderComponents with the mapping identifier preset
    /// will have their key replaced with the newly generated one
    /// </summary>
    [DataField]
    public List<string> ShipIds = default!;
}
