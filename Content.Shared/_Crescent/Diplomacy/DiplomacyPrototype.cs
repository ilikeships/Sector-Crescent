using Robust.Shared.Prototypes;

namespace Content.Shared._Crescent.Diplomacy;

[Prototype("diplomacy")]
public sealed partial class DiplomacyPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public Dictionary<string, Relations>? Relations = default!;
}

public enum Relations
{
    Ally,
    Neutral,
    ColdWar,
    War
}
