using Robust.Shared.GameStates;

namespace Content.Shared._Crescent.Hardpoints;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HardpointAnchorableOnlyComponent : Component
{
    public EntityUid? anchoredTo;

    public weaponTypes CompatibleTypes = weaponTypes.Ballistic;
    public weaponSizes CompatibleSizes = weaponSizes.Small;
}

