using Robust.Shared.Prototypes;

namespace Content.Shared._Crescent.Diplomacy;

[Prototype("diplomacy")]
public sealed partial class DiplomacyPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;
}
