using Robust.Shared.Serialization;

namespace Content.Shared.Crescent.Radar;

[Serializable, NetSerializable]
public sealed class IFFInterfaceState
{
    public List<ProjectileState> Projectiles;

    public IFFInterfaceState(List<ProjectileState> projectiles)
    {
        Projectiles = projectiles;
    }
}
