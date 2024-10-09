using Robust.Shared.GameStates;

namespace Content.Shared._Crescent.Intelligence;

[RegisterComponent]
public sealed partial class IntelligenceThresholdComponent : Component
{
    /// <summary>
    /// Minimum IQ required to interact with an entity
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public byte Required;

    /// <summary>
    /// Invert requirement, anyone above threshold is denied
    /// </summary>
    [DataField]
    public bool Inverted;
}
