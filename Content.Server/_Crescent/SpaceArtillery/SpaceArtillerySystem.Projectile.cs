using Content.Shared.Projectiles;
using Content.Shared.Camera;
using Robust.Shared.Player;

namespace Content.Server._Crescent.SpaceArtillery;
public abstract partial class SpaceArtillerySystem
{
    [Dependency] private readonly SharedCameraRecoilSystem _recoilSystem = default!;
    [Dependency] private readonly SharedTransformSystem _xformSystem = default!;
    protected virtual void InitializeProjectile()
    {
        SubscribeLocalEvent<SpaceArtilleryComponent, ProjectileHitEvent>(OnProjectileHit);
    }

    private void OnProjectileHit(EntityUid uid, SpaceArtilleryComponent component, ProjectileHitEvent hitEvent)
    {
        var grid = Transform(hitEvent.Target).GridUid;
        if (grid == null)
            return;

        var players = Filter.Empty();
        players.AddInGrid((EntityUid) grid);

        foreach (var player in players.Recipients)
        {
            if (player.AttachedEntity is not EntityUid playerEnt)
                continue;

            var vector = _xformSystem.GetWorldPosition(uid) - _xformSystem.GetWorldPosition(playerEnt);

            _recoilSystem.KickCamera(playerEnt, -vector.Normalized() * 0.5f);
        }
    }
}
