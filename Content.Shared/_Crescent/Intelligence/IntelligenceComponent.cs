using Robust.Shared.GameStates;

namespace Content.Shared._Crescent.Intelligence;

/// <summary>
/// Governs character intelligence
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class IntelligenceComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public byte Intelligence;
}
