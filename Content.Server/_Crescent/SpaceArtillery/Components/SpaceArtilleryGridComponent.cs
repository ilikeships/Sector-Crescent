using Content.Shared.Shuttles.Components;

namespace Content.Server._Crescent.SpaceArtillery;

/// <summary>
/// Component dictates if grid is allowed to utilise space artillery armaments at all
/// </summary>
[RegisterComponent]
public sealed partial class SpaceArtilleryGridComponent : Component
{
    /// <summary>
    /// Whether the grid has fully activated the armaments
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)] public bool IsActive = false;

    /// <summary>
    /// Whether the grid started activating the armaments, safety delay
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)] public bool IsCharging = false;

    [ViewVariables]
    public TimeSpan LastActivationTime;

    [ViewVariables]
    public TimeSpan ChargeUpDuration = TimeSpan.FromSeconds(30);

    [ViewVariables]
    public TimeSpan ChargeUpEndTime;

    [ViewVariables]
    public TimeSpan CooldownDuration = TimeSpan.FromSeconds(120);

    [ViewVariables]
    public TimeSpan CooldownEndTime;

    /// <summary>
    /// Default color to use for IFF if no component is found.
    /// </summary>
    public static readonly Color IFFColor = Color.Aquamarine;

    /// <summary>
    /// Default color to use for activated armament IFF if no component is found.
    /// </summary>
    public static readonly Color IFFArmedColor = Color.Red;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public IFFFlags Flags = IFFFlags.None;

    /// <summary>
    /// Color for this to show up on IFF.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public Color Color = IFFColor;

    /// <summary>
    /// Color for armed vessel to show up on IFF.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public Color ArmedColor = IFFArmedColor;
}
