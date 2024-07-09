using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.Crescent.Radar;

/// <summary>
/// State of each individual docking port for interface purposes
/// </summary>
[Serializable, NetSerializable]
public sealed class ProjectileState
{
    public NetCoordinates Coordinates;
}
