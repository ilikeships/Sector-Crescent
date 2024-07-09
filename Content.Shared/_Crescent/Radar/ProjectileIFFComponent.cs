using Content.Shared.Shuttles.Components;
using Robust.Shared.GameStates;

namespace Content.Shared.Crescent.Radar;

/// <summary>
/// Handles what a projectile should look like on radar.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ProjectileIFFComponent : Component
{
    /// <summary>
    /// Default color to use for IFF if no component is found.
    /// </summary>
    public static readonly Color DefaultColor = Color.Red;
}
