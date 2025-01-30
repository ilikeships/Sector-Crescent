using Content.Shared._Crescent.Hardpoints;
using Content.Shared.Construction.Components;

namespace Content.Server._Crescent.Hardpoint;

/// <summary>
/// This handles...
/// </summary>
public sealed class HardpointSystem : SharedHardpointSystem
{

    /// <inheritdoc/>
    public override void Initialize()
    {
        
    }

    public override void OnAnchorTry(EntityUid uid, HardpointAnchorableOnlyComponent component,
        ref AnchorAttemptEvent args)
    {
        base(uid, component, ref args);
    }
}
