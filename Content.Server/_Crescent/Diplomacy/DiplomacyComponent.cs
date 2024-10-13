namespace Content.Server._Crescent.Diplomacy;

[RegisterComponent]
[Access(typeof(DiplomacySystem))]
public sealed partial class DiplomacyComponent : Component
{
    public Relations[,]? DiplomaticSituation = null;
    public Dictionary<string, int> DiplomacyIndicies = new();
}

public enum Relations
{
    Ally,
    Neutral,
    ColdWar,
    War
}
